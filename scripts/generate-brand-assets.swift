// Renders every brand raster (iOS app icons, web favicons / PWA icons) from the vector sources in
// `assets/brand/` and copies the vector logos where the apps consume them.
// Usage (repo root): swift scripts/generate-brand-assets.swift
import AppKit
import Foundation

let root = URL(fileURLWithPath: FileManager.default.currentDirectoryPath)
let brand = root.appendingPathComponent("assets/brand")
let iosAssets = root.appendingPathComponent("apps/apple/SimTuningIOS/Assets.xcassets")
let webPublic = root.appendingPathComponent("apps/web/public")
let webApp = root.appendingPathComponent("apps/web/app")

func render(_ source: String, size: Int, opaque: Bool, to output: URL) throws {
    guard let image = NSImage(contentsOf: brand.appendingPathComponent(source)) else {
        fatalError("cannot load \(source)")
    }
    let alpha: CGImageAlphaInfo = opaque ? .noneSkipLast : .premultipliedLast
    guard let context = CGContext(
        data: nil, width: size, height: size, bitsPerComponent: 8, bytesPerRow: 0,
        space: CGColorSpace(name: CGColorSpace.sRGB)!, bitmapInfo: alpha.rawValue
    ) else { fatalError("no context") }
    context.interpolationQuality = .high
    NSGraphicsContext.saveGraphicsState()
    NSGraphicsContext.current = NSGraphicsContext(cgContext: context, flipped: false)
    image.draw(in: NSRect(x: 0, y: 0, width: size, height: size))
    NSGraphicsContext.restoreGraphicsState()
    guard let cgImage = context.makeImage(),
          let png = NSBitmapImageRep(cgImage: cgImage).representation(using: .png, properties: [:])
    else { fatalError("no png") }
    try FileManager.default.createDirectory(at: output.deletingLastPathComponent(), withIntermediateDirectories: true)
    try png.write(to: output)
    print("wrote \(output.path.replacingOccurrences(of: root.path + "/", with: "")) (\(size)px)")
}

func copy(_ source: String, to output: URL) throws {
    try FileManager.default.createDirectory(at: output.deletingLastPathComponent(), withIntermediateDirectories: true)
    if FileManager.default.fileExists(atPath: output.path) { try FileManager.default.removeItem(at: output) }
    try FileManager.default.copyItem(at: brand.appendingPathComponent(source), to: output)
    print("copied \(source) → \(output.path.replacingOccurrences(of: root.path + "/", with: ""))")
}

// iOS: App Store icons must be opaque; dark and tinted are the iOS 18 appearance variants.
let appIcon = iosAssets.appendingPathComponent("AppIcon.appiconset")
try render("app-icon.svg", size: 1024, opaque: true, to: appIcon.appendingPathComponent("AppIcon-1024.png"))
try render("app-icon-dark.svg", size: 1024, opaque: true, to: appIcon.appendingPathComponent("AppIcon-1024-dark.png"))
try render("app-icon-tinted.svg", size: 1024, opaque: true, to: appIcon.appendingPathComponent("AppIcon-1024-tinted.png"))
// iOS in-app logos stay vector (asset catalog SVG with preserved vector data).
try copy("logo.svg", to: iosAssets.appendingPathComponent("Logo.imageset/logo.svg"))
try copy("logo-mark.svg", to: iosAssets.appendingPathComponent("LogoMark.imageset/logo-mark.svg"))
// Launch screens show images at their natural point size, so the launch copy is sized to 160 pt.
let launchSVG = try String(contentsOf: brand.appendingPathComponent("logo.svg"), encoding: .utf8)
    .replacingOccurrences(of: #"width="1024" height="1024""#, with: #"width="160" height="160""#)
let launchLogo = iosAssets.appendingPathComponent("LaunchLogo.imageset/launch-logo.svg")
try FileManager.default.createDirectory(at: launchLogo.deletingLastPathComponent(), withIntermediateDirectories: true)
try launchSVG.write(to: launchLogo, atomically: true, encoding: .utf8)
print("wrote apps/apple/SimTuningIOS/Assets.xcassets/LaunchLogo.imageset/launch-logo.svg (160pt)")

// Web: SVG favicon, PNG apple-touch icon (Safari ignores SVG there) and PNG PWA icons.
try copy("logo-mark.svg", to: webApp.appendingPathComponent("icon.svg"))
try copy("logo.svg", to: webPublic.appendingPathComponent("logo.svg"))
try copy("logo-mark.svg", to: webPublic.appendingPathComponent("logo-mark.svg"))
try render("app-icon.svg", size: 180, opaque: true, to: webPublic.appendingPathComponent("apple-touch-icon.png"))
try render("logo.svg", size: 192, opaque: false, to: webPublic.appendingPathComponent("icon-192.png"))
try render("logo.svg", size: 512, opaque: false, to: webPublic.appendingPathComponent("icon-512.png"))
try render("app-icon.svg", size: 512, opaque: true, to: webPublic.appendingPathComponent("icon-maskable-512.png"))
