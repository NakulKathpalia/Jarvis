using System.Text.Json;
using System.Text.Json.Serialization;
using Jarvis.Memory;
using Jarvis.Models;
using Jarvis.Repositories;
using Jarvis.Users;

namespace Jarvis.Ingestion;

public sealed class IngestionService
{
    public const long MaxFileSizeBytes = 25L * 1024 * 1024;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".png", ".jpg", ".jpeg", ".webp"
    };

    private readonly string _uploadDirectory;
    private readonly string _metadataPath;
    private readonly IIngestionRepository? _repository;
    private readonly JarvisUserContext _userContext;
    private readonly IReadOnlyList<ITextExtractionService> _extractors;
    private readonly MemoryCandidateService _candidateService;
    private readonly MemoryService _memoryService;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };
    private readonly List<IngestionJob> _jobs = [];
    private readonly SemaphoreSlim _gate = new(1, 1);

    public IngestionService(
        string uploadDirectory,
        string metadataPath,
        MemoryService memoryService,
        IIngestionRepository? repository = null,
        JarvisUserContext? userContext = null,
        IEnumerable<ITextExtractionService>? extractors = null,
        MemoryCandidateService? candidateService = null)
    {
        _uploadDirectory = uploadDirectory;
        _metadataPath = metadataPath;
        _memoryService = memoryService;
        _repository = repository;
        _userContext = userContext ?? new JarvisUserContext();
        _extractors = (extractors ?? [new PdfTextExtractionService()]).ToList();
        _candidateService = candidateService ?? new MemoryCandidateService();
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
        Directory.CreateDirectory(_uploadDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(_metadataPath) ?? ".");
    }

    public IReadOnlyList<IngestionJob> Jobs => _jobs;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (_repository is not null)
        {
            var storedJobs = await _repository.GetForUserAsync(_userContext.UserId, cancellationToken);
            _jobs.Clear();
            _jobs.AddRange(storedJobs.Select(NormalizeJob).OrderByDescending(job => job.UpdatedAtUtc));
            return;
        }

        if (!File.Exists(_metadataPath))
        {
            await SaveAsync(cancellationToken);
            return;
        }

        var json = await File.ReadAllTextAsync(_metadataPath, cancellationToken);
        var jobs = JsonSerializer.Deserialize<List<IngestionJob>>(json, _jsonOptions) ?? [];
        _jobs.Clear();
        _jobs.AddRange(jobs.Select(NormalizeJob).OrderByDescending(job => job.UpdatedAtUtc));
    }

    public async Task<IngestionJob> UploadAsync(
        string fileName,
        Stream content,
        long length,
        CancellationToken cancellationToken = default)
    {
        ValidateUpload(fileName, length);
        var safeFileName = Path.GetFileName(fileName).Trim();
        var extension = Path.GetExtension(safeFileName).ToLowerInvariant();
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var storedPath = Path.Combine(_uploadDirectory, storedFileName);
        var fullUploadDirectory = Path.GetFullPath(_uploadDirectory);
        var fullStoredPath = Path.GetFullPath(storedPath);
        if (!fullStoredPath.StartsWith(fullUploadDirectory, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid upload path.");
        }

        await using (var fileStream = File.Create(fullStoredPath))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        var now = DateTime.UtcNow;
        var job = new IngestionJob
        {
            Id = Guid.NewGuid().ToString("N"),
            UserId = _userContext.UserId,
            FileName = safeFileName,
            FileType = extension.TrimStart('.').ToUpperInvariant(),
            StoredPath = fullStoredPath,
            SourceType = extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase)
                ? IngestionSourceType.Pdf
                : IngestionSourceType.Image,
            Status = IngestionStatus.Uploaded,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        await UpsertInMemoryAsync(job, cancellationToken);
        return job;
    }

    public async Task<IngestionJob?> ExtractAsync(string id, CancellationToken cancellationToken = default)
    {
        var job = Find(id);
        if (job is null)
        {
            return null;
        }

        var extractor = _extractors.FirstOrDefault(service => service.CanHandle(job.SourceType));
        if (extractor is null)
        {
            job.Status = IngestionStatus.ExtractionFailed;
            job.ErrorMessage = "No text extraction service is configured for this file type.";
            await UpsertInMemoryAsync(job, cancellationToken);
            return job;
        }

        var result = await extractor.ExtractAsync(job, cancellationToken);
        job.Status = result.Status;
        job.ExtractedText = result.ExtractedText;
        job.TextBlocks = result.TextBlocks.ToList();
        job.ErrorMessage = result.ErrorMessage;
        job.UpdatedAtUtc = DateTime.UtcNow;

        await UpsertInMemoryAsync(job, cancellationToken);
        return job;
    }

    public async Task<IngestionJob?> GenerateCandidatesAsync(string id, CancellationToken cancellationToken = default)
    {
        var job = Find(id);
        if (job is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(job.ExtractedText))
        {
            job.ErrorMessage = "Extracted text is required before generating memory candidates.";
            await UpsertInMemoryAsync(job, cancellationToken);
            return job;
        }

        job.Candidates = _candidateService.CreateCandidates(job).ToList();
        job.Status = IngestionStatus.CandidatesGenerated;
        job.ErrorMessage = string.Empty;
        job.UpdatedAtUtc = DateTime.UtcNow;
        await UpsertInMemoryAsync(job, cancellationToken);
        return job;
    }

    public async Task<(IngestionJob? Job, IngestionMemoryCandidate? Candidate, MemoryItem? Memory)> ApproveCandidateAsync(
        string candidateId,
        string? content = null,
        string? category = null,
        MemoryType? memoryType = null,
        int? importance = null,
        int? confidence = null,
        CancellationToken cancellationToken = default)
    {
        var (job, candidate) = FindCandidate(candidateId);
        if (job is null || candidate is null)
        {
            return (null, null, null);
        }

        candidate.Content = string.IsNullOrWhiteSpace(content) ? candidate.Content : content.Trim();
        candidate.SuggestedCategory = string.IsNullOrWhiteSpace(category) ? candidate.SuggestedCategory : category.Trim();
        candidate.SuggestedMemoryType = memoryType ?? candidate.SuggestedMemoryType;
        candidate.SuggestedImportance = Math.Clamp(importance ?? candidate.SuggestedImportance, 1, 10);
        candidate.SuggestedConfidence = Math.Clamp(confidence ?? candidate.SuggestedConfidence, 1, 10);
        candidate.ReviewStatus = MemoryReviewStatus.Approved;
        candidate.UpdatedAtUtc = DateTime.UtcNow;

        var memory = await _memoryService.AddAsync(
            candidate.Content,
            candidate.SuggestedCategory,
            tags: ["ingested"],
            importance: candidate.SuggestedImportance,
            confidence: candidate.SuggestedConfidence,
            source: $"Ingestion:{candidate.SourceFile}",
            memoryType: candidate.SuggestedMemoryType == MemoryType.TemporaryContext ? MemoryType.TemporaryContext : MemoryType.PermanentMemory,
            reviewStatus: MemoryReviewStatus.Approved,
            cancellationToken: cancellationToken);

        candidate.ApprovedMemoryId = memory.Id;
        if (!job.SuggestedMemoryIds.Contains(memory.Id, StringComparer.OrdinalIgnoreCase))
        {
            job.SuggestedMemoryIds.Add(memory.Id);
        }

        if (job.Candidates.Count > 0 && job.Candidates.All(item => item.ReviewStatus != MemoryReviewStatus.Pending))
        {
            job.Status = IngestionStatus.Completed;
        }

        await UpsertInMemoryAsync(job, cancellationToken);
        return (job, candidate, memory);
    }

    public async Task<(IngestionJob? Job, IngestionMemoryCandidate? Candidate)> RejectCandidateAsync(
        string candidateId,
        CancellationToken cancellationToken = default)
    {
        var (job, candidate) = FindCandidate(candidateId);
        if (job is null || candidate is null)
        {
            return (null, null);
        }

        candidate.ReviewStatus = MemoryReviewStatus.Rejected;
        candidate.UpdatedAtUtc = DateTime.UtcNow;
        if (job.Candidates.Count > 0 && job.Candidates.All(item => item.ReviewStatus != MemoryReviewStatus.Pending))
        {
            job.Status = IngestionStatus.Completed;
        }

        await UpsertInMemoryAsync(job, cancellationToken);
        return (job, candidate);
    }

    public IngestionJob? Get(string id) => Find(id);

    public async Task<IngestionJob?> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var index = _jobs.FindIndex(job => job.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return null;
            }

            var removed = _jobs[index];
            _jobs.RemoveAt(index);
            if (_repository is not null)
            {
                await _repository.DeleteAsync(_userContext.UserId, id, cancellationToken);
            }
            else
            {
                await SaveAsync(cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(removed.StoredPath) && File.Exists(removed.StoredPath))
            {
                File.Delete(removed.StoredPath);
            }

            return removed;
        }
        finally
        {
            _gate.Release();
        }
    }

    private static void ValidateUpload(string fileName, long length)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new InvalidOperationException("File name is required.");
        }

        if (fileName.Contains("..", StringComparison.Ordinal) || fileName.Contains('/') || fileName.Contains('\\'))
        {
            throw new InvalidOperationException("Invalid file name.");
        }

        if (length <= 0)
        {
            throw new InvalidOperationException("Uploaded file is empty.");
        }

        if (length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException("File is larger than the 25 MB limit.");
        }

        var extension = Path.GetExtension(fileName);
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Unsupported file type.");
        }
    }

    private IngestionJob? Find(string id) =>
        _jobs.FirstOrDefault(job => job.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    private (IngestionJob? Job, IngestionMemoryCandidate? Candidate) FindCandidate(string candidateId)
    {
        foreach (var job in _jobs)
        {
            var candidate = job.Candidates.FirstOrDefault(item => item.Id.Equals(candidateId, StringComparison.OrdinalIgnoreCase));
            if (candidate is not null)
            {
                return (job, candidate);
            }
        }

        return (null, null);
    }

    private async Task UpsertInMemoryAsync(IngestionJob job, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            job = NormalizeJob(job);
            var index = _jobs.FindIndex(item => item.Id.Equals(job.Id, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _jobs[index] = job;
            }
            else
            {
                _jobs.Insert(0, job);
            }

            _jobs.Sort((left, right) => right.UpdatedAtUtc.CompareTo(left.UpdatedAtUtc));
            await SaveAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    private Task SaveAsync(CancellationToken cancellationToken)
    {
        if (_repository is not null)
        {
            return Task.WhenAll(_jobs.Select(job => _repository.UpsertAsync(_userContext.UserId, job, cancellationToken)));
        }

        var json = JsonSerializer.Serialize(_jobs, _jsonOptions);
        return File.WriteAllTextAsync(_metadataPath, json, cancellationToken);
    }

    private IngestionJob NormalizeJob(IngestionJob job)
    {
        job.UserId = string.IsNullOrWhiteSpace(job.UserId) ? _userContext.UserId : job.UserId;
        job.FileName = Path.GetFileName(job.FileName);
        job.CreatedAtUtc = job.CreatedAtUtc == default ? DateTime.UtcNow : job.CreatedAtUtc;
        job.UpdatedAtUtc = DateTime.UtcNow;
        job.Candidates ??= [];
        job.TextBlocks ??= [];
        job.SuggestedMemoryIds ??= [];
        foreach (var candidate in job.Candidates)
        {
            candidate.UserId = string.IsNullOrWhiteSpace(candidate.UserId) ? job.UserId : candidate.UserId;
            candidate.CreatedAtUtc = candidate.CreatedAtUtc == default ? job.CreatedAtUtc : candidate.CreatedAtUtc;
            candidate.UpdatedAtUtc = candidate.UpdatedAtUtc == default ? job.UpdatedAtUtc : candidate.UpdatedAtUtc;
            candidate.SuggestedCategory = MemoryCategories.Normalize(candidate.SuggestedCategory);
            candidate.SuggestedImportance = Math.Clamp(candidate.SuggestedImportance, 1, 10);
            candidate.SuggestedConfidence = Math.Clamp(candidate.SuggestedConfidence, 1, 10);
        }

        return job;
    }
}
