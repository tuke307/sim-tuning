import Foundation
import Security

public protocol TokenStoring: Sendable {
    func saveTokens(_ tokens: AuthTokensDto, userId: String?) throws
    func loadTokens() -> AuthTokensDto?
    func loadUserId() -> String?
    func clearTokens()
}

public struct KeychainError: Error, Equatable {
    public let status: OSStatus
}

/// Tokens live only in the Keychain (`kSecAttrAccessibleAfterFirstUnlockThisDeviceOnly`, never
/// synced, never in `UserDefaults` or plain files). Tokens are never logged.
public final class KeychainTokenStore: TokenStoring, @unchecked Sendable {
    private let service: String
    private let lock = NSLock()

    public init(service: String = "de.tonyluke.simtuning.auth") {
        self.service = service
    }

    public func saveTokens(_ tokens: AuthTokensDto, userId: String?) throws {
        lock.lock()
        defer { lock.unlock() }
        try save(JSONEncoder().encode(tokens), account: "auth_tokens")
        if let userId { try save(Data(userId.utf8), account: "auth_user_id") }
    }

    public func loadTokens() -> AuthTokensDto? {
        lock.lock()
        defer { lock.unlock() }
        guard let data = read(account: "auth_tokens") else { return nil }
        return try? JSONDecoder().decode(AuthTokensDto.self, from: data)
    }

    public func loadUserId() -> String? {
        lock.lock()
        defer { lock.unlock() }
        return read(account: "auth_user_id").flatMap { String(data: $0, encoding: .utf8) }
    }

    public func clearTokens() {
        lock.lock()
        defer { lock.unlock() }
        delete(account: "auth_tokens")
        delete(account: "auth_user_id")
    }

    private func query(_ account: String) -> [String: Any] {
        [
            kSecClass as String: kSecClassGenericPassword,
            kSecAttrService as String: service,
            kSecAttrAccount as String: account,
        ]
    }

    private func save(_ data: Data, account: String) throws {
        delete(account: account)
        var item = query(account)
        item[kSecValueData as String] = data
        item[kSecAttrAccessible as String] = kSecAttrAccessibleAfterFirstUnlockThisDeviceOnly
        let status = SecItemAdd(item as CFDictionary, nil)
        guard status == errSecSuccess else { throw KeychainError(status: status) }
    }

    private func read(account: String) -> Data? {
        var item = query(account)
        item[kSecReturnData as String] = true
        item[kSecMatchLimit as String] = kSecMatchLimitOne
        var result: AnyObject?
        guard SecItemCopyMatching(item as CFDictionary, &result) == errSecSuccess else { return nil }
        return result as? Data
    }

    private func delete(account: String) {
        SecItemDelete(query(account) as CFDictionary)
    }
}

/// In-memory store for tests and UI tests (`--uitesting`).
public final class InMemoryTokenStore: TokenStoring, @unchecked Sendable {
    private let lock = NSLock()
    private var tokens: AuthTokensDto?
    private var userId: String?

    public init(tokens: AuthTokensDto? = nil, userId: String? = nil) {
        self.tokens = tokens
        self.userId = userId
    }

    public func saveTokens(_ tokens: AuthTokensDto, userId: String?) throws {
        lock.lock()
        defer { lock.unlock() }
        self.tokens = tokens
        if let userId { self.userId = userId }
    }

    public func loadTokens() -> AuthTokensDto? {
        lock.lock()
        defer { lock.unlock() }
        return tokens
    }

    public func loadUserId() -> String? {
        lock.lock()
        defer { lock.unlock() }
        return userId
    }

    public func clearTokens() {
        lock.lock()
        defer { lock.unlock() }
        tokens = nil
        userId = nil
    }
}
