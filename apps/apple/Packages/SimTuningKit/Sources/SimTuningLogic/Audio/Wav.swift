import Foundation

/// Mono 16-bit PCM audio as raw sample values (−32768…32767), like v2 `WavFile`.
public struct PcmAudio: Sendable, Equatable {
    public var sampleRate: Int
    public var samples: [Double]

    public init(sampleRate: Int, samples: [Double]) {
        self.sampleRate = sampleRate
        self.samples = samples
    }
}

public enum WavError: Error, Equatable, LocalizedError {
    case missingHeader
    case unsupportedFmtChunk
    case notPCM
    case unsupportedChannels(Int)
    case unsupportedBitDepth
    case noDataChunk

    public var errorDescription: String? {
        switch self {
        case .missingHeader: return "Unsupported WAV format (missing RIFF/WAVE header)"
        case .unsupportedFmtChunk: return "Unsupported WAV format (expect 16 byte 'fmt' chunk)"
        case .notPCM: return "Unsupported WAV format (audio format must be uncompressed PCM)"
        case let .unsupportedChannels(count):
            return "Unsupported WAV format (must be 1 or 2 channel, file has \(count))"
        case .unsupportedBitDepth: return "Only 16-bit WAV files are supported"
        case .noDataChunk: return "Unsupported WAV format (no 'data' chunk found)"
        }
    }
}

/// Port of `packages/common/src/audio/wav.ts` (v2 `Spectrogram.WavFile.ReadMono`).
public enum Wav {
    private static func u8(_ bytes: [UInt8], _ offset: Int) -> UInt8 { bytes[offset] }

    private static func u16(_ bytes: [UInt8], _ offset: Int) -> Int {
        Int(bytes[offset]) | Int(bytes[offset + 1]) << 8
    }

    private static func u32(_ bytes: [UInt8], _ offset: Int) -> Int {
        Int(bytes[offset]) | Int(bytes[offset + 1]) << 8 | Int(bytes[offset + 2]) << 16
            | Int(bytes[offset + 3]) << 24
    }

    private static func i16(_ bytes: [UInt8], _ offset: Int) -> Double {
        Double(Int16(bitPattern: UInt16(bytes[offset]) | UInt16(bytes[offset + 1]) << 8))
    }

    private static func chunkId(_ bytes: [UInt8], _ offset: Int) -> String {
        String(decoding: bytes[offset..<offset + 4], as: Unicode.ASCII.self)
    }

    /// Reads uncompressed 16-bit PCM, mono or stereo (left channel is returned).
    public static func readMono(_ data: Data) throws -> PcmAudio {
        let bytes = [UInt8](data)
        guard bytes.count >= 44, chunkId(bytes, 0) == "RIFF", chunkId(bytes, 8) == "WAVE" else {
            throw WavError.missingHeader
        }
        guard chunkId(bytes, 12) == "fmt ", u32(bytes, 16) == 16 else { throw WavError.unsupportedFmtChunk }
        guard u16(bytes, 20) == 1 else { throw WavError.notPCM }
        let channels = u16(bytes, 22)
        guard (1...2).contains(channels) else { throw WavError.unsupportedChannels(channels) }
        let sampleRate = u32(bytes, 24)
        let blockSize = u16(bytes, 32)
        guard u16(bytes, 34) == 16 else { throw WavError.unsupportedBitDepth }

        var position = 36
        var dataStart = 0
        var dataBytes = 0
        var index = 0
        while index < 42, position + 8 <= bytes.count {
            let length = u32(bytes, position + 4)
            if chunkId(bytes, position) == "data" {
                dataStart = position + 8
                dataBytes = min(length, bytes.count - dataStart)
                break
            }
            position += length + 8
            index += 1
        }
        guard dataStart != 0, dataBytes != 0, blockSize > 0 else { throw WavError.noDataChunk }

        let count = dataBytes / blockSize
        var samples = [Double](repeating: 0, count: count)
        for i in 0..<count {
            samples[i] = i16(bytes, dataStart + i * blockSize)
        }
        return PcmAudio(sampleRate: sampleRate, samples: samples)
    }

    /// Converts float samples (−1…1, e.g. from AVAudioEngine) into the 16-bit value range the v2
    /// pipeline (and its intensity threshold of 255) was tuned for.
    public static func floatToPcm16(_ channel: [Float]) -> [Double] {
        channel.map { value in
            let s = max(-1, min(1, Double(value)))
            return (s < 0 ? s * 32768 : s * 32767).rounded(.towardZero)
        }
    }

    /// Encodes mono 16-bit PCM as a WAV file (used to persist recordings in the v2 format).
    public static func encodeMono(_ samples: [Double], sampleRate: Int) -> Data {
        let dataBytes = samples.count * 2
        var out = Data(capacity: 44 + dataBytes)
        func ascii(_ text: String) { out.append(contentsOf: Array(text.utf8)) }
        func le32(_ value: Int) { withUnsafeBytes(of: UInt32(truncatingIfNeeded: value).littleEndian) { out.append(contentsOf: $0) } }
        func le16(_ value: Int) { withUnsafeBytes(of: UInt16(truncatingIfNeeded: value).littleEndian) { out.append(contentsOf: $0) } }
        ascii("RIFF")
        le32(36 + dataBytes)
        ascii("WAVE")
        ascii("fmt ")
        le32(16)
        le16(1)
        le16(1)
        le32(sampleRate)
        le32(sampleRate * 2)
        le16(2)
        le16(16)
        ascii("data")
        le32(dataBytes)
        for sample in samples {
            let clamped = Int16(max(-32768, min(32767, sample.isFinite ? sample.rounded(.towardZero) : 0)))
            withUnsafeBytes(of: clamped.littleEndian) { out.append(contentsOf: $0) }
        }
        return out
    }
}
