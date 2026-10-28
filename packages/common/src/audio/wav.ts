export interface PcmAudio {
  sampleRate: number;
  /** Left / mono channel as raw 16-bit sample values (−32768…32767), like v2 `WavFile`. */
  samples: Float64Array;
}

function chunkId(view: DataView, offset: number): string {
  return String.fromCharCode(
    view.getUint8(offset),
    view.getUint8(offset + 1),
    view.getUint8(offset + 2),
    view.getUint8(offset + 3),
  );
}

/**
 * Port of v2 `Spectrogram.WavFile.ReadMono`: uncompressed 16-bit PCM, mono or stereo (left
 * channel is returned). Throws on anything else so callers can fall back to `decodeAudioData`.
 */
export function readWavMono(buffer: ArrayBuffer): PcmAudio {
  const view = new DataView(buffer);
  if (view.byteLength < 44 || chunkId(view, 0) !== 'RIFF' || chunkId(view, 8) !== 'WAVE') {
    throw new Error('Unsupported WAV format (missing RIFF/WAVE header)');
  }
  if (chunkId(view, 12) !== 'fmt ' || view.getUint32(16, true) !== 16) {
    throw new Error("Unsupported WAV format (expect 16 byte 'fmt' chunk)");
  }
  if (view.getUint16(20, true) !== 1) {
    throw new Error('Unsupported WAV format (audio format must be uncompressed PCM)');
  }
  const channels = view.getUint16(22, true);
  if (channels < 1 || channels > 2) {
    throw new Error(`Unsupported WAV format (must be 1 or 2 channel, file has ${channels})`);
  }
  const sampleRate = view.getUint32(24, true);
  const blockSize = view.getUint16(32, true);
  if (view.getUint16(34, true) !== 16) {
    throw new Error('Only 16-bit WAV files are supported');
  }

  let position = 36;
  let dataStart = 0;
  let dataBytes = 0;
  for (let i = 0; i < 42 && position + 8 <= view.byteLength; i++) {
    const length = view.getUint32(position + 4, true);
    if (chunkId(view, position) === 'data') {
      dataStart = position + 8;
      dataBytes = Math.min(length, view.byteLength - dataStart);
      break;
    }
    position += length + 8;
  }
  if (dataStart === 0 || dataBytes === 0) {
    throw new Error("Unsupported WAV format (no 'data' chunk found)");
  }

  const count = Math.floor(dataBytes / blockSize);
  const samples = new Float64Array(count);
  for (let i = 0; i < count; i++) {
    samples[i] = view.getInt16(dataStart + i * blockSize, true);
  }
  return { sampleRate, samples };
}

/**
 * Converts Web Audio float samples (−1…1, e.g. from `AudioBuffer.getChannelData`) into the 16-bit
 * value range the v2 pipeline (and its intensity threshold of 255) was tuned for.
 */
export function floatToPcm16(channel: Float32Array): Float64Array {
  const out = new Float64Array(channel.length);
  for (let i = 0; i < channel.length; i++) {
    const s = Math.max(-1, Math.min(1, channel[i] ?? 0));
    out[i] = Math.trunc(s < 0 ? s * 32768 : s * 32767);
  }
  return out;
}

/** Encodes mono 16-bit PCM as a WAV file (used to persist recordings in the v2 format). */
export function encodeWavMono(samples: ArrayLike<number>, sampleRate: number): ArrayBuffer {
  const dataBytes = samples.length * 2;
  const buffer = new ArrayBuffer(44 + dataBytes);
  const view = new DataView(buffer);
  const write = (offset: number, text: string) => {
    for (let i = 0; i < text.length; i++) view.setUint8(offset + i, text.charCodeAt(i));
  };
  write(0, 'RIFF');
  view.setUint32(4, 36 + dataBytes, true);
  write(8, 'WAVE');
  write(12, 'fmt ');
  view.setUint32(16, 16, true);
  view.setUint16(20, 1, true);
  view.setUint16(22, 1, true);
  view.setUint32(24, sampleRate, true);
  view.setUint32(28, sampleRate * 2, true);
  view.setUint16(32, 2, true);
  view.setUint16(34, 16, true);
  write(36, 'data');
  view.setUint32(40, dataBytes, true);
  for (let i = 0; i < samples.length; i++) {
    view.setInt16(44 + i * 2, Math.max(-32768, Math.min(32767, Math.trunc(samples[i] ?? 0))), true);
  }
  return buffer;
}
