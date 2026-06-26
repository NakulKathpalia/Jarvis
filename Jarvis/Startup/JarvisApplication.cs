using System.Text.Json.Serialization;
using Jarvis.Auth;
using Jarvis.Commands;
using Jarvis.ConnectedApps;
using Jarvis.Core;
using Jarvis.Data;
using Jarvis.Ingestion;
using Jarvis.Knowledge;
using Jarvis.Memory;
using Jarvis.Migrations;
using Jarvis.Mongo;
using Jarvis.Repositories;
using Jarvis.Repositories.Mongo;
using Jarvis.Security;
using Jarvis.Services;
using Jarvis.Users;
using Jarvis.Voice;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Startup;

public static class JarvisApplication
{
    public static JarvisApplicationBuilder CreateBuilder(string[] args)
    {
        var runtime = CreateRuntimeAsync(args).GetAwaiter().GetResult();
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.UseUrls("http://localhost:5055");
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.Services.AddSingleton(runtime);
        return new JarvisApplicationBuilder(args, builder, runtime);
    }

    private static async Task<JarvisRuntime> CreateRuntimeAsync(string[] args)
    {
        var basePath = AppContext.BaseDirectory;
        var pathResolver = new PathResolver(basePath, ResolveWebRootPath(basePath));
        var platformService = new PlatformService();
        var userContext = new JarvisUserContext();

        MongoDocumentConventions.Register();
        IMemoryRepository? memoryRepository = null;
        IChatRepository? chatRepository = null;
        IChatHistoryRepository? chatHistoryRepository = null;
        ISettingsRepository? settingsRepository = null;
        IAuditLogRepository? auditLogRepository = null;
        ICommandHistoryRepository? commandHistoryRepository = null;
        IVoiceHistoryRepository? voiceHistoryRepository = null;
        IConnectedAppRepository? connectedAppRepository = null;
        IDeviceRepository? deviceRepository = null;
        IIngestionRepository? ingestionRepository = null;
        IKnowledgeRepository? knowledgeRepository = null;
        IReadOnlyCollection<ConnectedAppInfo>? connectedApps = null;

        var mongoOptions = new MongoOptions
        {
            ConnectionString = Environment.GetEnvironmentVariable("JARVIS_MONGODB_URI") ?? "mongodb://localhost:27017",
            DatabaseName = Environment.GetEnvironmentVariable("JARVIS_MONGODB_DATABASE") ?? "jarvis_local"
        };

        try
        {
            var mongoContext = new MongoContext(mongoOptions);
            if (await mongoContext.IsAvailableAsync())
            {
                await new MongoInitializer(mongoContext).InitializeAsync();

                memoryRepository = new MongoMemoryRepository(mongoContext);
                chatRepository = new MongoChatRepository(mongoContext);
                chatHistoryRepository = new MongoChatHistoryRepository(mongoContext);
                settingsRepository = new MongoSettingsRepository(mongoContext);
                auditLogRepository = new MongoAuditLogRepository(mongoContext);
                commandHistoryRepository = new MongoCommandHistoryRepository(mongoContext);
                voiceHistoryRepository = new MongoVoiceHistoryRepository(mongoContext);
                connectedAppRepository = new MongoConnectedAppRepository(mongoContext);
                deviceRepository = new MongoDeviceRepository(mongoContext);
                ingestionRepository = new MongoIngestionRepository(mongoContext);
                knowledgeRepository = new MongoKnowledgeRepository(mongoContext);

                var migrationRepository = new MongoMigrationRepository(mongoContext);
                var migrationService = new FileStorageMigrationService(
                    new FileStorageMigrationPaths(
                        pathResolver.MemoryPath,
                        pathResolver.ChatHistoryPath,
                        pathResolver.ChatSessionsPath,
                        pathResolver.SettingsPath,
                        pathResolver.InteractionLogPath,
                        pathResolver.CommandLogPath,
                        pathResolver.VoiceHistoryPath),
                    userContext,
                    migrationRepository,
                    memoryRepository,
                    chatRepository,
                    chatHistoryRepository,
                    settingsRepository,
                    auditLogRepository,
                    commandHistoryRepository,
                    voiceHistoryRepository);
                await migrationService.RunAsync();

                connectedApps = await connectedAppRepository.GetForUserAsync(userContext.UserId);
                if (connectedApps.Count == 0)
                {
                    foreach (var connectedApp in ConnectedAppService.GetDefaultApps())
                    {
                        await connectedAppRepository.UpsertAsync(userContext.UserId, connectedApp);
                    }

                    connectedApps = await connectedAppRepository.GetForUserAsync(userContext.UserId);
                }

                await deviceRepository.UpsertAsync(new DeviceRecord
                {
                    Id = userContext.DeviceId,
                    UserId = userContext.UserId,
                    DeviceName = Environment.MachineName,
                    DeviceType = userContext.SessionType.ToString(),
                    Trusted = userContext.TrustedDevice,
                    LastSeenAtUtc = DateTime.UtcNow
                });

                Console.WriteLine($"MongoDB storage enabled: {mongoOptions.DatabaseName}");
            }
            else
            {
                Console.WriteLine("MongoDB is not reachable. Falling back to JSON file storage.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MongoDB storage disabled: {ex.Message}");
            Console.WriteLine("Falling back to JSON file storage.");
        }

        var settingsService = new SettingsService(pathResolver.SettingsPath, settingsRepository, userContext);
        await settingsService.LoadAsync();

        var memoryService = new MemoryService(pathResolver.MemoryPath, memoryRepository, userContext);
        await memoryService.LoadAsync();
        var knowledgeService = new KnowledgeService(pathResolver.KnowledgePath, knowledgeRepository, userContext);
        await knowledgeService.LoadAsync();
        var imageOcrService = new ImageOcrService(settingsService);
        var ingestionService = new IngestionService(
            pathResolver.IngestionUploadDirectory,
            pathResolver.IngestionJobsPath,
            memoryService,
            ingestionRepository,
            userContext,
            [new PdfTextExtractionService(), imageOcrService]);
        await ingestionService.LoadAsync();
        var memoryRankingService = new MemoryRankingService();
        var memoryRetrievalService = new MemoryRetrievalService(memoryService, memoryRankingService);
        var memoryContextBuilder = new MemoryContextBuilder();

        var chatHistoryService = new ChatHistoryService(pathResolver.ChatHistoryPath, chatHistoryRepository, userContext);
        await chatHistoryService.LoadAsync();

        var chatSessionService = new ChatSessionService(pathResolver.ChatSessionsPath, chatRepository, userContext);
        await chatSessionService.LoadAsync();

        var commandLogService = new CommandLogService(pathResolver.CommandLogPath, commandHistoryRepository, userContext);
        await commandLogService.LoadAsync();

        var interactionLogService = new InteractionLogService(pathResolver.InteractionLogPath, auditLogRepository, userContext);
        await interactionLogService.LoadAsync();

        var voiceHistoryService = new VoiceHistoryService(pathResolver.VoiceHistoryPath, voiceHistoryRepository, userContext);
        await voiceHistoryService.LoadAsync();

        var httpClient = new HttpClient();
        var ollamaService = new OllamaService(httpClient, settingsService);
        var voiceSettingsService = new VoiceSettingsService(settingsService);
        var speechToTextService = new SpeechToTextService(voiceSettingsService);
        var voiceActivityDetector = new VoiceActivityDetector();
        var whisperService = new WhisperService(settingsService, speechToTextService);
        var piperService = new PiperService(settingsService, pathResolver.GeneratedAudioDirectory);
        var textToSpeechService = new TextToSpeechService(settingsService, piperService, pathResolver.GeneratedAudioDirectory);
        var jarvisPersonalityService = new JarvisPersonalityService(settingsService);
        var wakeWordService = new Jarvis.Services.WakeWordService(settingsService, whisperService);
        var fileIndexService = new FileIndexService(settingsService);
        var pcCommandParser = new PcCommandParser();
        var commandSafetyService = new CommandSafetyService();
        var commandRiskClassifier = new CommandRiskClassifier();
        var settingsValidationService = new SettingsValidationService(settingsService);
        var auditLogger = new AuditLogger(pathResolver.SecurityAuditLogPath);
        var permissionService = new PermissionService(userContext);
        var securityService = new SecurityService(
            new InputValidator(),
            permissionService,
            auditLogger);
        var pcControlService = new WindowsPcControlService(pathResolver.ScreenshotDirectory);
        var pcCommandService = new PcCommandService(
            pcCommandParser,
            commandSafetyService,
            commandRiskClassifier,
            securityService,
            commandLogService,
            pcControlService,
            jarvisPersonalityService,
            interactionLogService);
        var voiceCommandProcessor = new VoiceCommandProcessor(pcCommandParser, pcCommandService);
        var voiceCommandService = new VoiceCommandService(voiceCommandProcessor);
        var classifierService = new ClassifierService();
        IAuthService authService = new LocalAuthService();
        IConnectedAppService connectedAppService = new ConnectedAppService(connectedApps);

        var commandManager = global::Jarvis.Commands.Commands.Create(
            memoryService,
            settingsService,
            fileIndexService,
            pcCommandService);

        var assistant = new Assistant(
            ollamaService,
            settingsService,
            memoryService,
            chatHistoryService,
            jarvisPersonalityService,
            memoryRetrievalService,
            memoryContextBuilder);

        var voicePipelineService = new VoicePipelineService(
            voiceSettingsService,
            voiceActivityDetector,
            speechToTextService,
            voiceCommandProcessor,
            assistant,
            voiceHistoryService,
            interactionLogService,
            textToSpeechService);

        var router = new CommandRouter(classifierService, commandManager, assistant);

        return new JarvisRuntime
        {
            PathResolver = pathResolver,
            PlatformService = platformService,
            SettingsService = settingsService,
            MemoryService = memoryService,
            KnowledgeService = knowledgeService,
            IngestionService = ingestionService,
            ImageOcrService = imageOcrService,
            MemoryRetrievalService = memoryRetrievalService,
            MemoryContextBuilder = memoryContextBuilder,
            ChatHistoryService = chatHistoryService,
            ChatSessionService = chatSessionService,
            CommandLogService = commandLogService,
            InteractionLogService = interactionLogService,
            VoiceHistoryService = voiceHistoryService,
            JarvisPersonalityService = jarvisPersonalityService,
            VoiceSettingsService = voiceSettingsService,
            SpeechToTextService = speechToTextService,
            VoiceActivityDetector = voiceActivityDetector,
            VoiceCommandProcessor = voiceCommandProcessor,
            PermissionService = permissionService,
            OllamaService = ollamaService,
            WhisperService = whisperService,
            PiperService = piperService,
            TextToSpeechService = textToSpeechService,
            WakeWordService = wakeWordService,
            FileIndexService = fileIndexService,
            PcCommandParser = pcCommandParser,
            PcCommandService = pcCommandService,
            VoiceCommandService = voiceCommandService,
            SettingsValidationService = settingsValidationService,
            AuthService = authService,
            ConnectedAppService = connectedAppService,
            CommandManager = commandManager,
            Assistant = assistant,
            VoicePipelineService = voicePipelineService,
            Router = router,
            HttpClient = httpClient
        };
    }

    private static string ResolveWebRootPath(string basePath)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var projectWebRoot = Path.Combine(currentDirectory, "Jarvis", "wwwroot");
        if (Directory.Exists(projectWebRoot) || File.Exists(Path.Combine(currentDirectory, "Jarvis", "Jarvis.csproj")))
        {
            return projectWebRoot;
        }

        var contentWebRoot = Path.Combine(currentDirectory, "wwwroot");
        if (Directory.Exists(contentWebRoot) || File.Exists(Path.Combine(currentDirectory, "Jarvis.csproj")))
        {
            return contentWebRoot;
        }

        return Path.Combine(basePath, "wwwroot");
    }
}
