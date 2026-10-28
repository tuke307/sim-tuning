// swift-tools-version: 6.0
import PackageDescription

let package = Package(
    name: "SimTuningKit",
    defaultLocalization: "de",
    platforms: [
        .iOS(.v18),
        .macOS(.v15),
    ],
    products: [
        .library(
            name: "SimTuningKit",
            targets: ["SimTuningLogic", "SimTuningAPI", "SimTuningStore"]
        ),
        .library(name: "SimTuningLogic", targets: ["SimTuningLogic"]),
        .library(name: "SimTuningAPI", targets: ["SimTuningAPI"]),
        .library(name: "SimTuningStore", targets: ["SimTuningStore"]),
    ],
    targets: [
        .target(name: "SimTuningLogic"),
        .target(name: "SimTuningAPI"),
        .target(
            name: "SimTuningStore",
            dependencies: ["SimTuningAPI"]
        ),
        .testTarget(
            name: "SimTuningLogicTests",
            dependencies: ["SimTuningLogic"],
            resources: [.copy("Fixtures")]
        ),
        .testTarget(
            name: "SimTuningAPITests",
            dependencies: ["SimTuningAPI"]
        ),
        .testTarget(
            name: "SimTuningStoreTests",
            dependencies: ["SimTuningStore", "SimTuningAPI"]
        ),
    ],
    swiftLanguageModes: [.v6]
)
