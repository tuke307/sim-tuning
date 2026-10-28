import XCTest

/// Guest-mode smoke tests: launch, tab navigation and one Motor calculation. The app runs with
/// `--uitesting` (in-memory storage, no network) and a fixed German locale.
final class SimTuningIOSUITests: XCTestCase {
    override func setUp() {
        continueAfterFailure = false
    }

    @MainActor
    private func launch() -> XCUIApplication {
        let app = XCUIApplication()
        app.launchArguments = ["--uitesting", "-AppleLanguages", "(de)", "-AppleLocale", "de_DE"]
        app.launch()
        return app
    }

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
