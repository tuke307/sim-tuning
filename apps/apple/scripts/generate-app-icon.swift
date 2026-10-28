// Renders the placeholder 1024×1024 app icon (gauge on a warm gradient).
// Usage: swift scripts/generate-app-icon.swift SimTuningIOS/Assets.xcassets/AppIcon.appiconset/AppIcon-1024.png
import AppKit
import CoreGraphics
import Foundation

let output = CommandLine.arguments.dropFirst().first ?? "AppIcon-1024.png"
let size = 1024
let colorSpace = CGColorSpaceCreateDeviceRGB()
guard let context = CGContext(data: nil, width: size, height: size, bitsPerComponent: 8, bytesPerRow: 0,
                              space: colorSpace, bitmapInfo: CGImageAlphaInfo.noneSkipLast.rawValue) else {
    fatalError("no context")
}
let s = CGFloat(size)
let gradient = CGGradient(colorsSpace: colorSpace, colors: [
    CGColor(red: 0.96, green: 0.46, blue: 0.16, alpha: 1),
    CGColor(red: 0.62, green: 0.12, blue: 0.10, alpha: 1),
] as CFArray, locations: [0, 1])!
context.drawLinearGradient(gradient, start: CGPoint(x: 0, y: s), end: CGPoint(x: s, y: 0), options: [])

let center = CGPoint(x: s / 2, y: s * 0.44)
let radius = s * 0.32
context.setStrokeColor(CGColor(red: 1, green: 1, blue: 1, alpha: 0.95))
context.setLineCap(.round)
context.setLineWidth(s * 0.055)
context.addArc(center: center, radius: radius, startAngle: .pi * 1.1, endAngle: -.pi * 0.1, clockwise: true)
context.strokePath()
context.setLineWidth(s * 0.02)
for i in 0...8 {
    let angle = CGFloat.pi * 1.1 - CGFloat(i) * (CGFloat.pi * 1.2 / 8)
    let inner = CGPoint(x: center.x + cos(angle) * radius * 0.72, y: center.y + sin(angle) * radius * 0.72)
    let outer = CGPoint(x: center.x + cos(angle) * radius * 0.86, y: center.y + sin(angle) * radius * 0.86)
    context.move(to: inner)
    context.addLine(to: outer)
}
context.strokePath()
let needle = CGFloat.pi * 0.28
context.setLineWidth(s * 0.035)
context.move(to: center)
context.addLine(to: CGPoint(x: center.x + cos(needle) * radius * 0.8, y: center.y + sin(needle) * radius * 0.8))
context.strokePath()
context.setFillColor(CGColor(red: 1, green: 1, blue: 1, alpha: 1))
context.fillEllipse(in: CGRect(x: center.x - s * 0.045, y: center.y - s * 0.045, width: s * 0.09, height: s * 0.09))

guard let image = context.makeImage() else { fatalError("no image") }
let rep = NSBitmapImageRep(cgImage: image)
guard let png = rep.representation(using: .png, properties: [:]) else { fatalError("no png") }
try png.write(to: URL(fileURLWithPath: output))
print("wrote \(output)")
