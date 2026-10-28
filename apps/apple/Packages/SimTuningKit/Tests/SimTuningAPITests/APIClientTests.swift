import Foundation
import Testing
@testable import SimTuningAPI

/// URLProtocol stub so the client can be exercised without a server.
final class StubURLProtocol: URLProtocol, @unchecked Sendable {
    struct Response: Sendable {
        let status: Int
        let body: String
    }

    nonisolated(unsafe) static var handler: (@Sendable (URLRequest) -> Response)?
    nonisolated(unsafe) static var requests: [URLRequest] = []
    static let lock = NSLock()

    override class func canInit(with request: URLRequest) -> Bool { true }
    override class func canonicalRequest(for request: URLRequest) -> URLRequest { request }

    override func startLoading() {
        var request = request
        if request.httpBody == nil, let stream = request.httpBodyStream {
            stream.open()
            var data = Data()
            var buffer = [UInt8](repeating: 0, count: 4096)
            while stream.hasBytesAvailable {
                let read = stream.read(&buffer, maxLength: buffer.count)
                if read <= 0 { break }
                data.append(buffer, count: read)
            }
            stream.close()
            request.httpBody = data
        }
        Self.lock.lock()
        Self.requests.append(request)
        let handler = Self.handler
        Self.lock.unlock()
        let response = handler?(request) ?? Response(status: 500, body: "{}")
        let http = HTTPURLResponse(url: request.url!, statusCode: response.status, httpVersion: nil, headerFields: nil)!
        client?.urlProtocol(self, didReceive: http, cacheStoragePolicy: .notAllowed)
        client?.urlProtocol(self, didLoad: Data(response.body.utf8))
        client?.urlProtocolDidFinishLoading(self)
    }

    override func stopLoading() {}
}

@Suite("APIClient", .serialized)
struct APIClientTests {
    func makeClient(tokens: AuthTokensDto?) -> APIClient {
        let configuration = URLSessionConfiguration.ephemeral
        configuration.protocolClasses = [StubURLProtocol.self]
        StubURLProtocol.requests = []
        return APIClient(baseURL: URL(string: "https://example.test")!,
                         tokenStore: InMemoryTokenStore(tokens: tokens, userId: "u1"),
                         session: URLSession(configuration: configuration))
    }

    @Test func unwrapsEnvelopeAndSendsBearer() async throws {
        StubURLProtocol.handler = { _ in
            .init(status: 200, body: #"{"success":true,"data":[{"id":"e1","createdAt":"a","updatedAt":"b","systemKey":null,"isSystem":false,"name":"Track","temperaturT":20,"luftdruckP":null}]}"#)
        }
        let client = makeClient(tokens: AuthTokensDto(accessToken: "FAKE-ACCESS", refreshToken: "FAKE-REFRESH",
                                                      expiresAt: "2030-01-01T00:00:00.000Z"))
        let environments = try await client.listEnvironments()
        #expect(environments.first?.name == "Track")
        #expect(environments.first?.luftdruckP == nil)
        let request = try #require(StubURLProtocol.requests.first)
        #expect(request.value(forHTTPHeaderField: "Authorization") == "Bearer FAKE-ACCESS")
        #expect(request.url?.path == "/api/environments")
    }

    @Test func rotatesRefreshTokenOnceAndRetries() async throws {
        StubURLProtocol.handler = { request in
            if request.url?.path == "/api/auth/refresh-token" {
                return .init(status: 200, body: #"{"success":true,"data":{"success":true,"tokens":{"accessToken":"FAKE-NEW","refreshToken":"FAKE-NEW-REFRESH","expiresAt":"x"}}}"#)
            }
            if request.value(forHTTPHeaderField: "Authorization") == "Bearer FAKE-NEW" {
                return .init(status: 200, body: #"{"success":true,"data":null}"#)
            }
            return .init(status: 401, body: #"{"success":false,"message":"expired"}"#)
        }
        let client = makeClient(tokens: AuthTokensDto(accessToken: "FAKE-OLD", refreshToken: "FAKE-OLD-REFRESH",
                                                      expiresAt: "x"))
        let dyno = try await client.getDyno(vehicleId: "veh/../1")
        #expect(dyno == nil)
        #expect(client.tokenStore.loadTokens()?.refreshToken == "FAKE-NEW-REFRESH")
        let refresh = try #require(StubURLProtocol.requests.first { $0.url?.path == "/api/auth/refresh-token" })
        let body = try JSONDecoder().decode([String: String].self, from: refresh.httpBody ?? Data())
        #expect(body["refreshToken"] == "FAKE-OLD-REFRESH")
        // Path segments are percent-encoded, so ids cannot traverse paths.
        #expect(StubURLProtocol.requests.first?.url?.absoluteString.contains("veh%2F..%2F1") == true)
    }

    @Test func clearsTokensWhenRefreshFails() async throws {
        StubURLProtocol.handler = { _ in .init(status: 401, body: "{}") }
        let client = makeClient(tokens: AuthTokensDto(accessToken: "FAKE", refreshToken: "FAKE", expiresAt: "x"))
        await #expect(throws: APIClientError.unauthorized) { _ = try await client.listVehicles() }
        #expect(client.tokenStore.loadTokens() == nil)
    }

    @Test func encodesPartialInputsWithNullVsAbsent() throws {
        var motor = MotorInputDto()
        motor.hubL = .some(nil)
        motor.bohrungD = 38
        let json = try String(decoding: JSONEncoder().encode(VehicleInputDto(motor: motor)), as: UTF8.self)
        #expect(json.contains(#""hubL":null"#))
        #expect(json.contains(#""bohrungD":38"#))
        #expect(!json.contains("pleulL"))
        #expect(!json.contains(#""name""#))
    }

    @Test func redactsTokensFromDescriptions() {
        let tokens = AuthTokensDto(accessToken: "FAKE-SECRET", refreshToken: "FAKE-SECRET2", expiresAt: "x")
        #expect(!"\(tokens)".contains("FAKE-SECRET"))
        #expect(!String(reflecting: tokens).contains("FAKE-SECRET"))
    }

    @Test func acceptsOnlyHttpsOrLocalhostBaseURLs() {
        #expect(APIClient.validatedBaseURL("https://simtuning.tony-luke.de") != nil)
        #expect(APIClient.validatedBaseURL("http://localhost:3001") != nil)
        #expect(APIClient.validatedBaseURL("http://evil.example") == nil)
        #expect(APIClient.validatedBaseURL("file:///etc/passwd") == nil)
    }

    @Test func formatsIsoDatesLikeJavaScript() throws {
        let date = Date(timeIntervalSince1970: 1_767_225_600.5)
        #expect(ISO8601.string(from: date) == "2026-01-01T00:00:00.500Z")
        #expect(ISO8601.parse("2026-01-01T00:00:00.000Z") != nil)
        #expect(ISO8601.parse("2026-01-01T00:00:00Z") != nil)
        #expect(ISO8601.parse("not a date") == nil)
    }

    @Test func validatesPasswordRules() {
        #expect(PasswordRules.isValid("abcdefg1"))
        #expect(!PasswordRules.isValid("abcdefgh"))
        #expect(!PasswordRules.isValid("1234567"))
    }
}
