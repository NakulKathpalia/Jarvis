type AudioContextWindow = Window &
  typeof globalThis & {
    webkitAudioContext?: typeof AudioContext;
  };

export type VoiceCaptureSession = {
  stop: () => Promise<Blob>;
  cancel: () => void;
};

export async function startVoiceCapture(): Promise<VoiceCaptureSession> {
  if (!navigator.mediaDevices?.getUserMedia) {
    throw new Error("Microphone is not supported in this browser.");
  }

  const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
  const audioWindow = window as AudioContextWindow;
  const AudioContextClass = audioWindow.AudioContext || audioWindow.webkitAudioContext;

  if (!AudioContextClass) {
    stopMediaStream(stream);
    throw new Error("Audio recording is not supported in this browser.");
  }

  const audioContext = new AudioContextClass();
  const source = audioContext.createMediaStreamSource(stream);
  const processor = audioContext.createScriptProcessor(4096, 1, 1);
  const samples: Float32Array[] = [];
  const sourceSampleRate = audioContext.sampleRate;

  processor.onaudioprocess = (event) => {
    const input = event.inputBuffer.getChannelData(0);
    samples.push(new Float32Array(input));
  };

  source.connect(processor);
  processor.connect(audioContext.destination);

  function cleanup() {
    processor.disconnect();
    source.disconnect();
    stopMediaStream(stream);
    void audioContext.close();
  }

  return {
    async stop() {
      cleanup();
      return encodeWav(samples, sourceSampleRate, 16000);
    },
    cancel() {
      cleanup();
    }
  };
}

function stopMediaStream(stream: MediaStream | null) {
  stream?.getTracks().forEach((track) => track.stop());
}

function encodeWav(chunks: Float32Array[], sourceSampleRate: number, targetSampleRate: number) {
  const merged = mergeFloat32(chunks);
  const downsampled = downsampleBuffer(merged, sourceSampleRate, targetSampleRate);
  const dataLength = downsampled.length * 2;
  const buffer = new ArrayBuffer(44 + dataLength);
  const view = new DataView(buffer);

  writeAscii(view, 0, "RIFF");
  view.setUint32(4, 36 + dataLength, true);
  writeAscii(view, 8, "WAVE");
  writeAscii(view, 12, "fmt ");
  view.setUint32(16, 16, true);
  view.setUint16(20, 1, true);
  view.setUint16(22, 1, true);
  view.setUint32(24, targetSampleRate, true);
  view.setUint32(28, targetSampleRate * 2, true);
  view.setUint16(32, 2, true);
  view.setUint16(34, 16, true);
  writeAscii(view, 36, "data");
  view.setUint32(40, dataLength, true);

  floatTo16BitPcm(view, 44, downsampled);
  return new Blob([view], { type: "audio/wav" });
}

function mergeFloat32(chunks: Float32Array[]) {
  const length = chunks.reduce((total, chunk) => total + chunk.length, 0);
  const result = new Float32Array(length);
  let offset = 0;

  for (const chunk of chunks) {
    result.set(chunk, offset);
    offset += chunk.length;
  }

  return result;
}

function downsampleBuffer(buffer: Float32Array, sourceSampleRate: number, targetSampleRate: number) {
  if (targetSampleRate === sourceSampleRate) {
    return buffer;
  }

  const ratio = sourceSampleRate / targetSampleRate;
  const newLength = Math.round(buffer.length / ratio);
  const result = new Float32Array(newLength);
  let offsetResult = 0;
  let offsetBuffer = 0;

  while (offsetResult < result.length) {
    const nextOffsetBuffer = Math.round((offsetResult + 1) * ratio);
    let accumulator = 0;
    let count = 0;

    for (let i = offsetBuffer; i < nextOffsetBuffer && i < buffer.length; i++) {
      accumulator += buffer[i];
      count++;
    }

    result[offsetResult] = count > 0 ? accumulator / count : 0;
    offsetResult++;
    offsetBuffer = nextOffsetBuffer;
  }

  return result;
}

function floatTo16BitPcm(view: DataView, offset: number, input: Float32Array) {
  for (let i = 0; i < input.length; i++, offset += 2) {
    const sample = Math.max(-1, Math.min(1, input[i]));
    view.setInt16(offset, sample < 0 ? sample * 0x8000 : sample * 0x7fff, true);
  }
}

function writeAscii(view: DataView, offset: number, value: string) {
  for (let i = 0; i < value.length; i++) {
    view.setUint8(offset + i, value.charCodeAt(i));
  }
}
