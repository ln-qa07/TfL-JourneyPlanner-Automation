# TfL Journey Planner — UI Test Automation

A C#/.NET UI automation project for the Transport for London (TfL) Journey Planner, created for the Senior Test Analyst coding challenge. Scenarios are expressed in Gherkin with Reqnroll and executed through NUnit. The project uses both **Microsoft Playwright** and **Selenium ChromeDriver** to exercise the public TfL website.

## Technology stack

- **Language / runtime:** C# / .NET 10
- **BDD:** Reqnroll (Gherkin)
- **Test runner / assertions:** NUnit
- **UI automation:** Microsoft Playwright (.NET) and Selenium WebDriver / ChromeDriver
- **Selenium explicit waits:** Selenium.Support / `WebDriverWait`
- **Configuration:** `appsettings.json` (read by `Config/TestSettings.cs`)
- **Evidence:** scenario-end screenshots in `Reports/Passed` and `Reports/Failed`

## Automated coverage

| Scenario | Engine | Key verification |
| --- | --- | --- |
| Journey Planner page opens | Playwright | Journey Planner page loads |
| Verify valid journey planning | Playwright | Leicester Square → Covent Garden; walking and cycling durations displayed and positive |
| Verify station autocomplete | Playwright | Departure and destination chosen from suggestions |
| Verify journey with least walking preference | Playwright | Preference applied; updated journey with positive duration |
| Verify Covent Garden station accessibility information | Playwright | Expanded details, accessibility summary, and up-stairs / up-lift / level-walkway indicators |
| Verify invalid journey is rejected | Playwright | TfL shows a location-matching warning instead of a planned journey |
| Verify empty journey locations are rejected | Playwright | Both required-field errors displayed; journey not submitted |
| Verify empty fields using ChromeDriver | Selenium | Both required-field errors displayed in Chrome |

**Latest observed combined execution:** 8 tests passed, 0 failed (approximately 99 seconds on the development machine, 13 October 2026). This is a record of one local run, not a guarantee of future runs against a live service.

## Project layout

```text
TfL.Automation/
├── Config/                  # TestSettings.cs
├── Drivers/                 # PlaywrightDriver.cs, SeleniumDriver.cs
├── Features/                # JourneyPlanner.feature and generated files
├── Hooks/                   # Reqnroll browser lifecycle and screenshot hooks
├── Pages/                   # Playwright and Selenium page objects
├── StepDefinitions/         # Gherkin-to-C# step bindings
├── Reports/                 # Runtime evidence (gitignored)
├── appsettings.json         # TfL Journey Planner URL
└── TfL.Automation.csproj
```

## Prerequisites

1. Windows machine with the **.NET 10 SDK** (`dotnet --version`).
2. Google Chrome, compatible with the restored ChromeDriver dependency.
3. Internet access to the public TfL site and NuGet.
4. PowerShell (for the Playwright browser installation script).

## Setup

From the `TfL.Automation` project directory:

```powershell
dotnet restore
dotnet build
powershell -ExecutionPolicy Bypass -File .\bin\Debug\net10.0\playwright.ps1 install chromium
```

The URL is read from `appsettings.json`:

```json
{
  "JourneyPlannerUrl": "https://tfl.gov.uk/plan-a-journey/?cid=plan-a-journey"
}
```

The project file copies `appsettings.json` to the test output directory.

## Run tests

All eight BDD scenarios (Playwright and Selenium):

```powershell
dotnet test --filter "TestCategory=playwright|TestCategory=selenium" --logger "console;verbosity=minimal"
```

Only Playwright:

```powershell
dotnet test --filter "TestCategory=playwright"
```

Only Selenium ChromeDriver:

```powershell
dotnet test --filter "TestCategory=selenium"
```

Run an individual scenario:

```powershell
dotnet test --filter "Name~VerifyValidJourneyPlanning"
```

Show discovered test names:

```powershell
dotnet test --list-tests
```

Screenshots are captured after tagged scenarios and written under `Reports/Passed/` or `Reports/Failed/`. They are excluded from source control. Detailed console output and screenshots help diagnose failures.

## Design decisions

- **Behaviour-driven scenarios:** Gherkin states user-facing behaviour; Reqnroll bindings connect steps to automation.
- **Page Object Model:** Page objects encapsulate selectors, interactions and assertions instead of embedding browser automation in Gherkin.
- **Separate drivers and tagged hooks:** `@playwright` and `@selenium` scenarios each use their own browser lifecycle. This keeps the two automation engines independent.
- **Maintainable configuration:** The application URL is externalised in `appsettings.json` rather than repeated in tests.
- **Dynamic wait strategies:** Playwright assertions and Selenium explicit waits are used instead of unconditional sleeps; tests wait for loaded journey results before checking durations.
- **Non-brittle time checks:** Journey minutes are validated as positive values rather than enforcing a fixed live-service duration. Some station accessibility indicators are asserted against the specific journey observed during development.
- **Failure evidence:** Reqnroll `AfterScenario` hooks save screenshots for both passing and failing runs. Browser cleanup takes place after each scenario.
- **Cookie handling:** Consent is handled through the visible user interface; a controlled refresh and bounded recovery logic address occasional TfL cookie-overlay interference.

## Limitations and risks

- Tests run against the **live public TfL website**, so UI markup, journey availability, loading times and service schedules may change without notice.
- Cookie consent can occasionally reappear after a refresh and intercept UI interactions. Recovery logic has improved observed reliability but cannot guarantee absence of intermittent failures.
- Underground service and available transport modes vary by departure time. The tests avoid fixed travel times and transport-line assumptions where feasible.
- The invalid-location scenario documents TfL's currently observed *multiple location matches* response for the chosen invalid input; it is not a universal invalid-location error contract.
- Full performance, load, security, cross-browser and screen-reader accessibility testing are outside the implemented scope.

## Proposed additional functional scenarios (not automated)

| Area | Suggested validation |
| --- | --- |
| Departures | Now, future departure, arrival-by time, and date selection |
| Location entry | Origin and destination same, reversed journey, special characters, long names, partial matches and no matches |
| Preferences | Fewest changes, fastest route, walking-speed and maximum-walk settings |
| Accessibility | Step-free access, stairs/escalator preferences, keyboard navigation and labelled interactive controls |
| Transport availability | Off-peak / late-night routes where services differ, using controlled test dates and known timetables |
| Resilience | No routes available, slow responses, temporary network interruption, and error recovery |

## Proposed non-functional coverage (not automated)

- **Performance:** Measure initial widget responsiveness and route-result loading at representative network speeds; define acceptable thresholds with stakeholders.
- **Accessibility:** Keyboard-only operation, focus order, screen-reader labels, colour contrast, and WCAG checks. Review the visually interactive `View details` control observed with `aria-hidden="true"` as a potential accessibility concern.
- **Cross-browser / responsive:** Chromium, Firefox, WebKit and mobile viewport layouts where supported.
- **Reliability:** Repeated executions, clean browser contexts and third-party cookie-consent variability.
- **Security / privacy:** Confirm no sensitive data or secrets are written to logs, repository files or screenshots.

### Responsible AI-use Disclosure

Generative AI (ChatGPT) was used to assist with drafting and refining selected portions of the test automation framework, including C# code, Gherkin scenarios and documentation.
The implementation was reviewed, adapted and validated through local test execution and investigation of the TfL website's actual behaviour. AI output was not assumed to be correct without verification.

