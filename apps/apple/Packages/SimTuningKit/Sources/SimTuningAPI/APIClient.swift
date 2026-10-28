import Foundation

public enum APIClientError: Error, LocalizedError, Sendable, Equatable {
    case invalidURL
    case unauthorized
    case forbidden(String)
    case notFound(String)
    case offline(String)
    case serverError(Int, String)
    case decodingError(String)

    public var errorDescription: String? {
        switch self {
        case .invalidURL: return "Invalid API URL"
        case .unauthorized: return "Authentication required"
        case let .forbidden(message), let .notFound(message), let .offline(message),
             let .serverError(_, message), let .decodingError(message):
            return message
        }
    }
}

private struct APIErrorDetail: Decodable {
    let message: String?
}

/// Standard `ApiResponse<T>` envelope (`{ success, data, message? }`).
private struct APIEnvelope<T: Decodable>: Decodable {
    let success: Bool?
    let data: T?
    let message: String?
    let error: APIErrorDetail?
}

private struct RefreshResponseDto: Decodable {
    let success: Bool?
    let tokens: AuthTokensDto?
}

private struct EmptyObject: Codable {}

/// Async URLSession client for the SimTuning API (`docs/sim-api.md` + `/api/auth/*`).
///
/// - Bearer auth from the Keychain-backed `TokenStoring`; tokens are never logged.
/// - On `401` it performs a single-flight refresh-token rotation (`POST /api/auth/refresh-token`
///   with the refresh token in the body) and retries once; if rotation fails, tokens are cleared.
/// - Cookies are disabled so the server never falls back to ambient cookie auth.
public actor APIClient {
    public nonisolated let baseURL: URL
    public nonisolated let tokenStore: any TokenStoring
    private let session: URLSession
    private var inFlightRefresh: Task<Bool, Never>?
    private var language: String = "de"

    public init(baseURL: URL = APIClient.defaultBaseURL,
                tokenStore: any TokenStoring = KeychainTokenStore(),
                session: URLSession? = nil) {
        self.baseURL = baseURL
        self.tokenStore = tokenStore
        if let session {
            self.session = session
        } else {
            let configuration = URLSessionConfiguration.ephemeral
            configuration.httpShouldSetCookies = false
            configuration.httpCookieAcceptPolicy = .never
            configuration.timeoutIntervalForRequest = 30
            self.session = URLSession(configuration: configuration)
        }
    }

    /// `SIMTUNING_API_BASE_URL` (environment) → Info.plist (xcconfig) → build default.
    public static var defaultBaseURL: URL {
        if let env = ProcessInfo.processInfo.environment["SIMTUNING_API_BASE_URL"],
           let url = validatedBaseURL(env) {
            return url
        }
        if let plist = Bundle.main.object(forInfoDictionaryKey: "SIMTUNING_API_BASE_URL") as? String,
           let url = validatedBaseURL(plist) {
            return url
        }
        #if DEBUG
        return URL(string: "http://localhost:3001")!
        #else
        return URL(string: "https://simtuning.tony-luke.de")!
        #endif
    }

    /// Only HTTPS is accepted, except plain HTTP to the local development host.
    public static func validatedBaseURL(_ text: String) -> URL? {
        guard let url = URL(string: text.trimmingCharacters(in: .whitespaces)), let host = url.host(),
              let scheme = url.scheme?.lowercased() else { return nil }
        if scheme == "https" { return url }
        if scheme == "http", ["localhost", "127.0.0.1", "::1"].contains(host) { return url }
        return nil
    }

    public func setLanguage(_ language: String) {
        self.language = language
    }

    public nonisolated func isAuthenticated() -> Bool {
        tokenStore.loadTokens() != nil
    }

    static func segment(_ id: String) -> String {
        var allowed = CharacterSet.urlPathAllowed
        allowed.remove(charactersIn: "/?#")
        return id.addingPercentEncoding(withAllowedCharacters: allowed) ?? ""
    }

    // MARK: - Auth

    private func storeSession(_ user: UserDto) {
        if let tokens = user.tokens { try? tokenStore.saveTokens(tokens, userId: user.id) }
    }

    public func login(email: String, password: String) async throws -> UserDto {
        let user: UserDto = try await request("/api/auth/login", method: "POST",
                                              body: LoginDto(email: email, password: password), requiresAuth: false)
        storeSession(user)
        return user
    }

    public func register(_ dto: RegisterDto) async throws -> UserDto {
        let user: UserDto = try await request("/api/auth/register", method: "POST", body: dto, requiresAuth: false)
        storeSession(user)
        return user
    }

    public func verifyEmail(token: String) async throws -> UserDto {
        struct Body: Encodable { let token: String }
        let user: UserDto = try await request("/api/auth/verify-email", method: "POST", body: Body(token: token),
                                              requiresAuth: false)
        storeSession(user)
        return user
    }

    public func resendConfirmation(email: String) async throws {
        struct Body: Encodable { let email: String }
        let _: SuccessResponseDto = try await request("/api/auth/resend-confirmation", method: "POST",
                                                      body: Body(email: email), requiresAuth: false)
    }

    public func forgotPassword(email: String) async throws {
        struct Body: Encodable { let email: String }
        let _: SuccessResponseDto = try await request("/api/auth/forgot-password", method: "POST",
                                                      body: Body(email: email), requiresAuth: false)
    }

    public func resetPassword(token: String, newPassword: String) async throws {
        struct Body: Encodable { let token: String; let newPassword: String }
        let _: SuccessResponseDto = try await request("/api/auth/reset-password", method: "POST",
                                                      body: Body(token: token, newPassword: newPassword),
                                                      requiresAuth: false)
    }

    public func me() async throws -> UserDto {
        try await request("/api/auth/me", method: "GET")
    }

    public func updatePreferences(_ dto: UpdatePreferencesDto) async throws -> UserDto {
        try await request("/api/auth/preferences", method: "PATCH", body: dto)
    }

    public func changePassword(_ dto: ChangePasswordDto) async throws {
        let _: SuccessResponseDto = try await request("/api/auth/change-password", method: "POST", body: dto)
    }

    public func requestAccountDeletion(password: String) async throws {
        struct Body: Encodable { let password: String }
        let _: SuccessResponseDto = try await request("/api/auth/request-delete-account", method: "POST",
                                                      body: Body(password: password))
    }

    public func confirmAccountDeletion(token: String) async throws {
        struct Body: Encodable { let token: String }
        let _: SuccessResponseDto = try await request("/api/auth/confirm-delete-account", method: "POST",
                                                      body: Body(token: token), requiresAuth: false)
        tokenStore.clearTokens()
    }

    public func logout() async {
        _ = try? await request("/api/auth/logout", method: "POST", body: EmptyObject()) as SuccessResponseDto
        tokenStore.clearTokens()
    }

    // MARK: - Vehicles

    public func listVehicles() async throws -> [VehicleDto] {
        try await request("/api/vehicles", method: "GET")
    }

    public func getVehicle(id: String) async throws -> VehicleDto {
        try await request("/api/vehicles/\(Self.segment(id))", method: "GET")
    }

    public func createVehicle(_ input: VehicleInputDto) async throws -> VehicleDto {
        try await request("/api/vehicles", method: "POST", body: input.validated(requireName: true))
    }

    public func updateVehicle(id: String, _ input: VehicleInputDto) async throws -> VehicleDto {
        try await request("/api/vehicles/\(Self.segment(id))", method: "PATCH",
                          body: input.validated(requireName: false))
    }

    public func deleteVehicle(id: String) async throws {
        try await requestVoid("/api/vehicles/\(Self.segment(id))", method: "DELETE")
    }

    public func duplicateVehicle(id: String, name: String? = nil) async throws -> VehicleDto {
        struct Body: Encodable { let name: String? }
        return try await request("/api/vehicles/\(Self.segment(id))/duplicate", method: "POST", body: Body(name: name))
    }

    // MARK: - Environments

    public func listEnvironments() async throws -> [EnvironmentDto] {
        try await request("/api/environments", method: "GET")
    }

    public func createEnvironment(_ input: EnvironmentInputDto) async throws -> EnvironmentDto {
        try await request("/api/environments", method: "POST", body: input)
    }

    public func updateEnvironment(id: String, _ input: EnvironmentInputDto) async throws -> EnvironmentDto {
        try await request("/api/environments/\(Self.segment(id))", method: "PATCH", body: input)
    }

    public func deleteEnvironment(id: String) async throws {
        try await requestVoid("/api/environments/\(Self.segment(id))", method: "DELETE")
    }

    // MARK: - Dyno

    public func getDyno(vehicleId: String) async throws -> DynoDto? {
        try await requestOptional("/api/vehicles/\(Self.segment(vehicleId))/dyno", method: "GET")
    }

    public func saveDyno(vehicleId: String, _ input: DynoInputDto) async throws -> DynoDto {
        try await request("/api/vehicles/\(Self.segment(vehicleId))/dyno", method: "PUT", body: input)
    }

    public func saveDynoData(dynoId: String, _ data: DynoDataInputDto) async throws -> DynoDto {
        try data.validate()
        return try await request("/api/dynos/\(Self.segment(dynoId))/data", method: "PUT", body: data)
    }

    public func deleteDyno(id: String) async throws {
        try await requestVoid("/api/dynos/\(Self.segment(id))", method: "DELETE")
    }

    public func exportDyno(id: String) async throws -> DynoExportDto {
        try await request("/api/dynos/\(Self.segment(id))/export", method: "GET")
    }

    public func importDyno(_ data: DynoExportDto) async throws -> VehicleDto {
        try await request("/api/dynos/import", method: "POST", body: data.validated())
    }

    // MARK: - Core request & single-flight refresh

    private func send(_ path: String, method: String, body: (any Encodable)?, requiresAuth: Bool,
                      allowRetry: Bool) async throws -> Data {
        guard let url = URL(string: path, relativeTo: baseURL) else { throw APIClientError.invalidURL }
        var req = URLRequest(url: url)
        req.httpMethod = method
        req.setValue("application/json", forHTTPHeaderField: "Content-Type")
        req.setValue("application/json", forHTTPHeaderField: "Accept")
        req.setValue("apple", forHTTPHeaderField: "X-Client-Platform")
        req.setValue(language, forHTTPHeaderField: "Accept-Language")
        if let tokens = tokenStore.loadTokens() {
            req.setValue("Bearer \(tokens.accessToken)", forHTTPHeaderField: "Authorization")
        } else if requiresAuth {
            throw APIClientError.unauthorized
        }
        if let body {
            req.httpBody = try JSONEncoder().encode(AnyEncodable(body))
        }

        let data: Data
        let response: URLResponse
        do {
            (data, response) = try await session.data(for: req)
        } catch {
            throw APIClientError.offline(error.localizedDescription)
        }
        guard let http = response as? HTTPURLResponse else {
            throw APIClientError.offline("Invalid HTTP response")
        }

        let isAuthRoute = path.hasPrefix("/api/auth/login") || path.hasPrefix("/api/auth/refresh-token")
        if http.statusCode == 401, allowRetry, !isAuthRoute, tokenStore.loadTokens() != nil {
            if await refreshSession() {
                return try await send(path, method: method, body: body, requiresAuth: requiresAuth, allowRetry: false)
            }
            tokenStore.clearTokens()
            throw APIClientError.unauthorized
        }

        guard (200...299).contains(http.statusCode) else {
            let envelope = try? JSONDecoder().decode(APIEnvelope<EmptyObject>.self, from: data)
            let message = envelope?.error?.message ?? envelope?.message ?? "HTTP \(http.statusCode)"
            switch http.statusCode {
            case 401: throw isAuthRoute ? APIClientError.serverError(401, message) : APIClientError.unauthorized
            case 403: throw APIClientError.forbidden(message)
            case 404: throw APIClientError.notFound(message)
            default: throw APIClientError.serverError(http.statusCode, message)
            }
        }
        return data
    }

    public func request<T: Decodable>(_ path: String, method: String, body: (any Encodable)? = nil,
                                      requiresAuth: Bool = true) async throws -> T {
        let data = try await send(path, method: method, body: body, requiresAuth: requiresAuth, allowRetry: true)
        let decoder = JSONDecoder()
        if let envelope = try? decoder.decode(APIEnvelope<T>.self, from: data), let unwrapped = envelope.data {
            return unwrapped
        }
        do {
            return try decoder.decode(T.self, from: data)
        } catch {
            throw APIClientError.decodingError(error.localizedDescription)
        }
    }

    /// For routes whose `data` may legitimately be `null` (e.g. `GET /vehicles/:id/dyno`).
    public func requestOptional<T: Decodable>(_ path: String, method: String) async throws -> T? {
        let data = try await send(path, method: method, body: nil, requiresAuth: true, allowRetry: true)
        do {
            return try JSONDecoder().decode(APIEnvelope<T>.self, from: data).data
        } catch {
            throw APIClientError.decodingError(error.localizedDescription)
        }
    }

    public func requestVoid(_ path: String, method: String, body: (any Encodable)? = nil) async throws {
        _ = try await send(path, method: method, body: body, requiresAuth: true, allowRetry: true)
    }

    /// Rotates the refresh token once even if many requests hit `401` concurrently.
    public func refreshSession() async -> Bool {
        if let inFlightRefresh { return await inFlightRefresh.value }
        let task = Task<Bool, Never> { await self.performRefresh() }
        inFlightRefresh = task
        let result = await task.value
        inFlightRefresh = nil
        return result
    }

    private func performRefresh() async -> Bool {
        guard let current = tokenStore.loadTokens(),
              let url = URL(string: "/api/auth/refresh-token", relativeTo: baseURL) else { return false }
        struct Body: Encodable { let refreshToken: String }
        var req = URLRequest(url: url)
        req.httpMethod = "POST"
        req.setValue("application/json", forHTTPHeaderField: "Content-Type")
        req.setValue("apple", forHTTPHeaderField: "X-Client-Platform")
        req.httpBody = try? JSONEncoder().encode(Body(refreshToken: current.refreshToken))
        guard let (data, response) = try? await session.data(for: req),
              let http = response as? HTTPURLResponse, (200...299).contains(http.statusCode) else {
            return false
        }
        let decoder = JSONDecoder()
        let tokens = (try? decoder.decode(APIEnvelope<RefreshResponseDto>.self, from: data))?.data?.tokens
            ?? (try? decoder.decode(RefreshResponseDto.self, from: data))?.tokens
        guard let tokens else { return false }
        try? tokenStore.saveTokens(tokens, userId: tokenStore.loadUserId())
        return true
    }
}

private struct AnyEncodable: Encodable {
    private let encodeValue: (Encoder) throws -> Void
    init(_ wrapped: any Encodable) { encodeValue = wrapped.encode(to:) }
    func encode(to encoder: Encoder) throws { try encodeValue(encoder) }
}
