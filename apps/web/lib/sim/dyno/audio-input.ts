import { floatToPcm16, readWavMono, type PcmAudio } from '@sim-tuning/common/audio';

/** Loaded audio the dyno works on (kept in memory only — audio never leaves the device). */
export interface DynoAudio extends PcmAudio {
  /** Where it came from, for display. */
  source: 'file' | 'recording' | 'demo';
  label: string;
}

/** Upload limit; 50 MB ≈ 9 min of 16-bit stereo WAV — far more than a dyno run needs. */
export const MAX_AUDIO_FILE_BYTES = 50 * 1024 * 1024;

export const AUDIO_FILE_ACCEPT = '.wav,.mp3,.m4a,.aac,.ogg,.webm,audio/*';
const AUDIO_EXTENSIONS = ['wav', 'mp3', 'm4a', 'aac', 'ogg', 'oga', 'webm', 'mp4'];

export type AudioFileError = 'tooLarge' | 'unsupportedType' | 'empty';

/** Validates a user-picked file before reading it into memory. */
export function validateAudioFile(file: {
  name: string;
  size: number;
  type: string;
}): AudioFileError | null {
  if (file.size === 0) return 'empty';
  if (file.size > MAX_AUDIO_FILE_BYTES) return 'tooLarge';
  const ext = file.name.toLowerCase().split('.').pop() ?? '';
  if (!file.type.startsWith('audio/') && !AUDIO_EXTENSIONS.includes(ext)) {
    return 'unsupportedType';
  }
  return null;
}

/**
 * MediaRecorder container preference: Chrome/Firefox record webm/opus, Safari only mp4/aac.
 * Returns `undefined` to let the browser pick its default.
 */
export function pickRecorderMimeType(
  isTypeSupported: (type: string) => boolean,
): string | undefined {
  const candidates = [
    'audio/webm;codecs=opus',
    'audio/webm',
    'audio/mp4;codecs=mp4a.40.2',
    'audio/mp4',
    'audio/aac',
    'audio/ogg;codecs=opus',
  ];
  return candidates.find((type) => {
    try {
      return isTypeSupported(type);
    } catch {
      return false;
    }
  });
}

/** Microphone constraints: raw signal, no voice processing (it would filter engine harmonics). */
export const RECORDING_CONSTRAINTS: MediaStreamConstraints = {
  audio: {
    echoCancellation: false,
    noiseSuppression: false,
    autoGainControl: false,
    channelCount: 1,
  },
};

/**
 * Turns any audio file into 16-bit-range mono samples. Uncompressed 16-bit WAV goes through the
 * v2 reader (`readWavMono`) for exact v2 parity; everything else (MP3, M4A, recordings, other WAV
 * flavours) is decoded by the browser and its left channel converted with `floatToPcm16`.
 * Note: `decodeAudioData` resamples to the context's rate, which is reported as `sampleRate`.
 */
export async function decodeToPcm(buffer: ArrayBuffer): Promise<PcmAudio> {
  try {
    return readWavMono(buffer);
  } catch {
    // Not a 16-bit PCM WAV → let the browser decode it.
  }
  const Ctx =
    typeof window === 'undefined'
      ? undefined
      : (window.AudioContext ??
        (window as unknown as { webkitAudioContext?: typeof AudioContext }).webkitAudioContext);
  if (!Ctx) throw new Error('Web Audio is not supported');
  const context = new Ctx();
  try {
    // `slice` because some browsers detach the buffer they decode.
    const decoded = await context.decodeAudioData(buffer.slice(0));
    return { sampleRate: decoded.sampleRate, samples: floatToPcm16(decoded.getChannelData(0)) };
  } finally {
    void context.close();
  }
}

/** Builds a safe download file name from user text (no path separators / control chars). */
export function safeFileName(base: string, extension: string): string {
  const cleaned = base
    .normalize('NFKD')
    .replace(/[^\w\- ]+/g, '')
    .trim()
    .replace(/\s+/g, '-')
    .slice(0, 60);
  return `${cleaned || 'dyno'}.${extension}`;
}

/** Triggers a client-side download of a blob (nothing is uploaded). */
export function downloadBlob(blob: Blob, fileName: string): void {
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = fileName;
  anchor.rel = 'noopener';
  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
