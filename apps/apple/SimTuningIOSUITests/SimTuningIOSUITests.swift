import XCTest

/// Guest-mode smoke tests: launch, tab navigation and one Motor calculation. The app runs with
/// `--uitesting` (in-memory storage, no network) and a fixed German locale.
final class SimTuningIOSUITests: XCTestCase {
    override func setUp() {
        continueAfterFailure = false
    }

    @MainActor
    private func launch(light: Bool = false) -> XCUIApplication {
        let app = XCUIApplication()
        app.launchArguments = ["--uitesting", "-AppleLanguages", "(de)", "-AppleLocale", "de_DE"]
            + (light ? ["--uitesting-light"] : [])
        app.launch()
        return app
    }

    /// Keeps a screenshot of the current screen in the result bundle (design review). With
    /// `TEST_RUNNER_SIMTUNING_SCREENSHOT_DIR` set for xcodebuild, it is also written there as PNG.
    @MainActor
    private func attachScreenshot(_ name: String) {
        let screenshot = XCUIScreen.main.screenshot()
        let attachment = XCTAttachment(screenshot: screenshot)
        attachment.name = name
        attachment.lifetime = .keepAlways
        add(attachment)
        if let dir = ProcessInfo.processInfo.environment["SIMTUNING_SCREENSHOT_DIR"], !dir.isEmpty {
            let url = URL(fileURLWithPath: dir, isDirectory: true).appendingPathComponent("\(name).png")
            try? screenshot.pngRepresentation.write(to: url)
        }
    }

    /// Small, controlled scroll step (a full swipe can fling a field past the navigation bar).
    @MainActor
    private func scrollStep(_ app: XCUIApplication, up: Bool) {
        let start = app.coordinate(withNormalizedOffset: CGVector(dx: 0.5, dy: up ? 0.55 : 0.35))
        let end = app.coordinate(withNormalizedOffset: CGVector(dx: 0.5, dy: up ? 0.35 : 0.55))
        start.press(forDuration: 0.05, thenDragTo: end)
    }

    @MainActor
    private func selectStarterVehicle(_ app: XCUIApplication) {
        XCTAssertTrue(app.navigationBars["Fahrzeuge"].waitForExistence(timeout: 10))
        let vehicle = app.staticTexts["S51 B1-4"]
        var attempts = 0
        // Large Dynamic Type sizes push the starter vehicles below the fold.
        while !(vehicle.exists && vehicle.isHittable) && attempts < 15 {
            scrollStep(app, up: true)
            attempts += 1
        }
        XCTAssertTrue(vehicle.waitForExistence(timeout: 5))
        vehicle.tap()
    }

    /// Scrolls the form until the field is reachable, then types into it.
    @MainActor
    private func fill(_ app: XCUIApplication, _ identifier: String, _ text: String) {
        let field = app.textFields[identifier]
        var attempts = 0
        while !(field.exists && field.isHittable) && attempts < 15 {
            // Above the visible area → scroll back down; otherwise (below / behind keyboard) → up.
            let above = field.exists && field.frame.minY < app.frame.height * 0.2
            scrollStep(app, up: !above)
            attempts += 1
        }
        XCTAssertTrue(field.isHittable, "Field '\(identifier)' not reachable")
        field.tap()
        field.typeText(text)
    }

    @MainActor
    private func scrollToTop(_ app: XCUIApplication) {
        for _ in 0..<6 { app.collectionViews.firstMatch.swipeDown(velocity: .fast) }
    }

    @MainActor
    private func checkSteuerdiagramm(light: Bool) {
        let app = launch(light: light)
        selectStarterVehicle(app)
        openTab(app, "Motor")
        let link = app.buttons["motor.steuerdiagramm"].firstMatch
        XCTAssertTrue(link.waitForExistence(timeout: 5))
        attachScreenshot("motor-\(light ? "light" : "dark")")
        link.tap()
        XCTAssertTrue(app.descendants(matching: .any)["schriftfeld.2"].waitForExistence(timeout: 5))
        let drawing = app.descendants(matching: .any)["steuer.zeichnung"].firstMatch
        XCTAssertTrue(drawing.waitForExistence(timeout: 5))
        XCTAssertTrue((drawing.value as? String ?? "").contains("Einlass 135"))
        attachScreenshot("steuerdiagramm-\(light ? "light" : "dark")")
        let firstItem = app.descendants(matching: .any)["stueckliste.1"].firstMatch
        var attempts = 0
        repeat {
            scrollStep(app, up: true)
            attempts += 1
        } while !(firstItem.exists && firstItem.isHittable) && attempts < 15
        XCTAssertTrue(firstItem.waitForExistence(timeout: 5))
        attachScreenshot("steuerdiagramm-stueckliste-\(light ? "light" : "dark")")
    }

    @MainActor
    private func checkAuspuff(light: Bool) {
        let app = launch(light: light)
        selectStarterVehicle(app)
        openTab(app, "Auslass")
        let link = app.buttons["auslass.anwendung"].firstMatch
        XCTAssertTrue(link.waitForExistence(timeout: 5))
        link.tap()
        // S51 B1-4 provides resonance rpm and exhaust timing; the chamber needs the rest.
        for (id, text) in [("auspuff.durchmesser", "26"), ("auspuff.flaeche", "400"), ("auspuff.abgasT", "550"),
                           ("auspuff.kruemmerF", "10"), ("auspuff.kruemmerW", "2"), ("auspuff.diffusorW", "7,5"),
                           ("auspuff.mittelteilF", "2,9"), ("auspuff.gegenkonusW", "8"),
                           ("auspuff.endrohrL", "295"), ("auspuff.endrohrD", "27")] {
            fill(app, id, text)
        }
        scrollToTop(app)
        XCTAssertTrue(app.descendants(matching: .any)["auspuff.zeichnung"].waitForExistence(timeout: 5))
        attachScreenshot("auspuff-\(light ? "light" : "dark")")
        let resonanz = app.descendants(matching: .any)["auspuff.resonanz"].firstMatch
        var attempts = 0
        while !(resonanz.exists && resonanz.isHittable) && attempts < 20 {
            scrollStep(app, up: true)
            attempts += 1
        }
        XCTAssertTrue(resonanz.exists)
        attachScreenshot("auspuff-stueckliste-\(light ? "light" : "dark")")
    }

    @MainActor
    func testSteuerdiagrammDrawingDark() { checkSteuerdiagramm(light: false) }

    @MainActor
    func testSteuerdiagrammDrawingLight() { checkSteuerdiagramm(light: true) }

    @MainActor
    func testAuspuffDrawingDark() { checkAuspuff(light: false) }

    @MainActor
    func testAuspuffDrawingLight() { checkAuspuff(light: true) }

    /// iPhone shows at most five tabs; the rest live behind "Mehr".
    @MainActor
    private func openTab(_ app: XCUIApplication, _ label: String) {
        let tab = app.tabBars.buttons[label]
        if tab.waitForExistence(timeout: 3) {
            tab.tap()
            return
        }
        let more = app.tabBars.buttons["Mehr"]
        XCTAssertTrue(more.waitForExistence(timeout: 5), "Expected tab '\(label)' or 'Mehr'")
        more.tap()
        let entry = app.cells.staticTexts[label].firstMatch
        XCTAssertTrue(entry.waitForExistence(timeout: 5), "Expected '\(label)' in Mehr")
        entry.tap()
    }

    @MainActor
    func testLaunchShowsStarterVehicles() {
        let app = launch()
        XCTAssertTrue(app.navigationBars["Fahrzeuge"].waitForExistence(timeout: 10))
        XCTAssertTrue(app.staticTexts["S51 B1-4"].waitForExistence(timeout: 5))
        XCTAssertTrue(app.staticTexts["Startfahrzeuge"].exists || app.staticTexts["STARTFAHRZEUGE"].exists)
    }

    @MainActor
    func testNavigatesAllTabs() {
        let app = launch()
        XCTAssertTrue(app.navigationBars["Fahrzeuge"].waitForExistence(timeout: 10))
        for (tab, title) in [("Motor", "Motor"), ("Einlass", "Einlass"), ("Auslass", "Auslass"), ("Dyno", "Dyno"),
                             ("Profil", "Profil"), ("Fahrzeuge", "Fahrzeuge")] {
            openTab(app, tab)
            XCTAssertTrue(app.navigationBars[title].waitForExistence(timeout: 5), "Expected screen '\(title)'")
        }
    }

    @MainActor
    func testMotorCompressionCalculationInGuestMode() {
        let app = launch()
        XCTAssertTrue(app.navigationBars["Fahrzeuge"].waitForExistence(timeout: 10))
        openTab(app, "Motor")
        let link = app.buttons["motor.verdichtung"].firstMatch
        XCTAssertTrue(link.waitForExistence(timeout: 5))
        link.tap()

        // Same input as the v2 golden `compression2`: 49 900 mm³ / 5 880 mm³ → 9,5 : 1.
        let hubraum = app.textFields["verdichtung.hubraum"]
        XCTAssertTrue(hubraum.waitForExistence(timeout: 5))
        hubraum.tap()
        hubraum.typeText("49,9")
        let brennraum = app.textFields["verdichtung.brennraum"]
        brennraum.tap()
        brennraum.typeText("5,88")

        let result = app.descendants(matching: .any)["verdichtung.result"].firstMatch
        XCTAssertTrue(result.waitForExistence(timeout: 5))
        let predicate = NSPredicate(format: "label CONTAINS %@", "9,5 : 1")
        expectation(for: predicate, evaluatedWith: result)
        waitForExpectations(timeout: 5)
    }
}
