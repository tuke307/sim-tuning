import Foundation

// Codable mirror of the auth DTOs in `packages/common/src/schemas/index.ts`.

public enum LanguagePreference: String, Codable, Sendable, CaseIterable {
    case system, en, de
}

public enum ThemePreference: String, Codable, Sendable, CaseIterable {
    case system, light, dark
}

/// Decimal places shown after a unit conversion (v2 `UnitSettings.RoundingAccuracy`).
public enum RoundingAccuracy {
    public static let min = 0
    public static let max = 6
    public static let `default` = 2
}

public enum PasswordRules {
    public static let minLength = 8

    /// `PasswordSchema`: ≥ 8 characters, at least one letter and one number.
    public static func isValid(_ password: String) -> Bool {
        password.count >= minLength
            && password.unicodeScalars.contains { CharacterSet.letters.contains($0) && $0.isASCII }
            && password.unicodeScalars.contains { ("0"..."9").contains(Character($0)) }
    }
}

public struct AuthTokensDto: Codable, Sendable, Equatable {
    public var accessToken: String
    public var refreshToken: String
    public var expiresAt: String

    public init(accessToken: String, refreshToken: String, expiresAt: String) {
        self.accessToken = accessToken
        self.refreshToken = refreshToken
        self.expiresAt = expiresAt
    }
}

/// Never print tokens: redact them from debug output.
extension AuthTokensDto: CustomStringConvertible, CustomDebugStringConvertible {
    public var description: String { "AuthTokensDto(expiresAt: \(expiresAt), tokens: <redacted>)" }
    public var debugDescription: String { description }
}

public struct UserPreferencesDto: Codable, Sendable, Equatable {
    public var id: String
    public var userId: String
    public var theme: String
    public var language: LanguagePreference?
    public var roundingAccuracy: Int
    public var roundOnUnitChange: Bool
}

public struct UserDto: Codable, Sendable, Equatable {
    public var id: String
    public var email: String
    public var name: String?
    public var emailVerified: Bool?
    public var lastLoginAt: String?
    public var createdAt: String?
    public var preferences: UserPreferencesDto?
    public var tokens: AuthTokensDto?
    /// Only on `RegisterResponseDto`.
    public var requiresEmailConfirmation: Bool?
    public var message: String?
}

public struct RegisterDto: Codable, Sendable, Equatable {
    public var email: String
    public var password: String
    public var name: String?

    public init(email: String, password: String, name: String? = nil) {
        self.email = email
        self.password = password
        self.name = name
    }
}

public struct LoginDto: Codable, Sendable, Equatable {
    public var email: String
    public var password: String

    public init(email: String, password: String) {
        self.email = email
        self.password = password
    }
}

public struct UpdatePreferencesDto: Codable, Sendable, Equatable {
    public var name: String?
    public var theme: ThemePreference?
    public var language: LanguagePreference?
    public var roundingAccuracy: Int?
    public var roundOnUnitChange: Bool?

    public init(name: String? = nil, theme: ThemePreference? = nil, language: LanguagePreference? = nil,
                roundingAccuracy: Int? = nil, roundOnUnitChange: Bool? = nil) {
        self.name = name
        self.theme = theme
        self.language = language
        self.roundingAccuracy = roundingAccuracy
        self.roundOnUnitChange = roundOnUnitChange
    }
}

public struct ChangePasswordDto: Codable, Sendable, Equatable {
    public var currentPassword: String
    public var newPassword: String

    public init(currentPassword: String, newPassword: String) {
        self.currentPassword = currentPassword
        self.newPassword = newPassword
    }
}

/// Generic `{ success, message? }` responses (logout, password reset, …).
public struct SuccessResponseDto: Codable, Sendable, Equatable {
    public var success: Bool?
    public var message: String?
}
