import Foundation
import Testing

/// Minimal JSON tree decoded with `JSONDecoder`, whose number parsing is correctly rounded
/// (`JSONSerialization` can be off by one ulp, e.g. for `0.008253968253968255`).
enum JSONValue: Decodable, Sendable {
    case number(Double)
    case string(String)
    case bool(Bool)
    case array([JSONValue])
    case object([String: JSONValue])
    case null

    init(from decoder: Decoder) throws {
        let container = try decoder.singleValueContainer()
        if container.decodeNil() { self = .null }
        else if let value = try? container.decode(Bool.self) { self = .bool(value) }
        else if let value = try? container.decode(Double.self) { self = .number(value) }
        else if let value = try? container.decode(String.self) { self = .string(value) }
        else if let value = try? container.decode([JSONValue].self) { self = .array(value) }
        else { self = .object(try container.decode([String: JSONValue].self)) }
    }

    var double: Double {
        guard case let .number(value) = self else { fatalError("not a number: \(self)") }
        return value
    }

    var array: [JSONValue] {
        guard case let .array(value) = self else { fatalError("not an array: \(self)") }
        return value
    }

    var object: [String: JSONValue] {
        guard case let .object(value) = self else { fatalError("not an object: \(self)") }
        return value
    }
}

/// Loads the shared v2 golden fixtures (byte-identical copies of
/// `packages/common/src/__fixtures__`), so .NET v2, TypeScript and Swift are pinned to the same
/// values (3-way parity).
enum Golden {
    static func url(_ name: String) -> URL {
        Bundle.module.url(forResource: name, withExtension: nil, subdirectory: "Fixtures")!
    }

    static let values: [String: JSONValue] = {
        let data = try! Data(contentsOf: url("v2-golden.json"))
        return try! JSONDecoder().decode([String: JSONValue].self, from: data)
    }()

    static func number(_ key: String) -> Double { values[key]!.double }

    static func numbers(_ key: String) -> [Double] { values[key]!.array.map(\.double) }

    static func pairs(_ key: String) -> [[Double]] { values[key]!.array.map { $0.array.map(\.double) } }

    static func dict(_ key: String) -> [String: Double] { values[key]!.object.mapValues(\.double) }

    static func ints(_ key: String) -> [Int] { numbers(key).map { Int($0) } }
}

/// Same helper as the TS spec: `|a − e| ≤ max(1e-9, |e|·relTol)`.
func expectClose(_ actual: Double, _ expected: Double, relTol: Double = 1e-12,
                 sourceLocation: SourceLocation = #_sourceLocation) {
    #expect(abs(actual - expected) <= max(1e-9, abs(expected) * relTol),
            "\(actual) ≉ \(expected)", sourceLocation: sourceLocation)
}

/// Same helper as the TS audio spec: `|a − e| ≤ |e|·tol + 1e-9`.
func expectRelClose(_ actual: Double, _ expected: Double, _ tol: Double, _ label: String = "",
                    sourceLocation: SourceLocation = #_sourceLocation) {
    #expect(abs(actual - expected) <= abs(expected) * tol + 1e-9,
            "\(label) \(actual) ≉ \(expected)", sourceLocation: sourceLocation)
}
