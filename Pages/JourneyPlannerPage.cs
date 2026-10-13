
using Microsoft.Playwright;
using TfL.Automation.Config;
using TfL.Automation.Drivers;

namespace TfL.Automation.Pages;

public class JourneyPlannerPage
{
    private readonly PlaywrightDriver _driver;

    public JourneyPlannerPage(PlaywrightDriver driver)
    {
        _driver = driver;
    }

    // 1. Open TfL Journey Planner
    public async Task OpenAsync()
    {
        var page = _driver.Page;

        // 1. Navigate to TfL Journey Planner
        await page.GotoAsync(
            TestSettings.JourneyPlannerUrl,
            new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 60000
            });

        // 2. Wait until the journey form is loaded
        var fromInput = page.Locator("#InputFrom");

        await Assertions.Expect(fromInput)
            .ToBeVisibleAsync(new() { Timeout = 30000 });

        // 3. Locate cookie consent controls
        var cookieOverlay = page.Locator("#cb-cookieoverlay");

        var acceptButton = page.GetByRole(
            AriaRole.Button,
            new()
            {
                Name = "Accept only essential cookies",
                Exact = true
            });

        bool cookieAccepted = false;

        // 4. Accept cookies when the popup appears
        try
        {
            await acceptButton.WaitForAsync(
                new()
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 8000
                });

            await acceptButton.ClickAsync(
                new() { Timeout = 15000 });

            cookieAccepted = true;

            Console.WriteLine("Cookie consent accepted.");
        }
        catch (System.TimeoutException)
        {
            Console.WriteLine("Cookie consent button not displayed.");
        }

        // 5. Refresh once after accepting cookies,
        // or if an overlay is still blocking the page
        if (cookieAccepted || await cookieOverlay.IsVisibleAsync())
        {
            Console.WriteLine("Refreshing page to clear cookie overlay.");

            await page.ReloadAsync(
                new PageReloadOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 60000
                });
        }

        // 6. Verify cookie overlay has disappeared
        await Assertions.Expect(cookieOverlay)
            .ToBeHiddenAsync(new() { Timeout = 15000 });

        // 7. Verify journey form is ready for interaction
        await Assertions.Expect(fromInput)
            .ToBeVisibleAsync(new() { Timeout = 30000 });

        await fromInput.ClickAsync(
            new() { Timeout = 10000 });

        Console.WriteLine("Journey Planner ready for automation.");
    }

    // 2. Verify Journey Planner page loaded
    public async Task VerifyPageLoadedAsync()
    {
        await Assertions.Expect(
            _driver.Page.GetByRole(
                AriaRole.Heading,
                new() { Name = "Plan a journey" }
            ).First
        ).ToBeVisibleAsync();
    }

    // 3. Select station from autocomplete
    public async Task SelectStationAsync(
        string fieldId,
        string partialName)
    {
        var input = _driver.Page.Locator(fieldId);

        await Assertions.Expect(input)
            .ToBeVisibleAsync();

        // Clear existing input
        await input.FillAsync("");

        // Type partial station name
        await input.PressSequentiallyAsync(
            partialName,
            new() { Delay = 100 });

        Console.WriteLine(
            $"Entered station: {partialName}");

        // Select autocomplete suggestion
        string fullName =
            $"{partialName} Underground Station";

        var suggestion = _driver.Page
            .Locator(".tt-suggestion")
            .Filter(new() { HasText = fullName })
            .First;

        await Assertions.Expect(suggestion)
            .ToBeVisibleAsync(
                new() { Timeout = 15000 });

        await suggestion.ClickAsync();

        Console.WriteLine(
            $"Selected station: {fullName}");
    }

    // 4. Verify selected stations
    public async Task VerifyStationsSelectedAsync()
    {
        await Assertions.Expect(
            _driver.Page.Locator("#InputFrom")
        ).ToHaveValueAsync(
            "Leicester Square Underground Station");

        await Assertions.Expect(
            _driver.Page.Locator("#InputTo")
        ).ToHaveValueAsync(
            "Covent Garden Underground Station");
    }

    public async Task PlanJourneyAsync()
    {
        await _driver.Page.GetByRole(
            AriaRole.Button,
            new() { Name = "Plan my journey" }
        ).ClickAsync();
    }

    public async Task VerifyJourneyResultsAsync()
    {
        var page = _driver.Page;

        // Verify results page is displayed
        await Assertions.Expect(
            page.GetByRole(
                AriaRole.Heading,
                new() { Name = "Journey results" })
        ).ToBeVisibleAsync(
            new() { Timeout = 30000 });

        // Wait until the loading indicator disappears
        await Assertions.Expect(
            page.GetByText(
                "Fetching results",
                new() { Exact = true })
        ).ToBeHiddenAsync(
            new() { Timeout = 60000 });

        // Verify journey options have loaded
        await Assertions.Expect(
            page.GetByText(
                "Walking and cycling",
                new() { Exact = false }).First
        ).ToBeVisibleAsync(
            new() { Timeout = 30000 });

        Console.WriteLine(
            "Journey results loaded successfully.");
    }

    public async Task CaptureJourneyResultsAsync()
    {
        // Locate the project root directory
        string projectDirectory = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", ".."
            )
        );

        // Create the Reports folder path
        string reportsDirectory = Path.Combine(
            projectDirectory,
            "Reports"
        );

        // Ensure the Reports folder exists
        Directory.CreateDirectory(reportsDirectory);

        // Set the screenshot file path
        string screenshotPath = Path.Combine(
            reportsDirectory,
            "journey-results.png"
        );

        // Capture the full page
        await _driver.Page.ScreenshotAsync(
            new PageScreenshotOptions
            {
                Path = screenshotPath,
                FullPage = true
            }
        );

        Console.WriteLine(
            $"Journey results screenshot saved: {screenshotPath}"
        );
    }

    public async Task EnterInvalidDepartureAsync(string location)
    {
        var fromInput = _driver.Page.Locator("#InputFrom");

        // Verify departure field is available
        await Assertions.Expect(fromInput)
            .ToBeVisibleAsync();

        // Enter an invalid station name
        await fromInput.FillAsync(location);

        // Move focus away from the field
        await fromInput.PressAsync("Tab");

        Console.WriteLine(
            $"Invalid departure entered: {location}");
    }

    public async Task VerifyEmptyJourneyRejectedAsync()
    {
        var page = _driver.Page;

        // Verify From field is empty
        await Assertions.Expect(
            page.Locator("#InputFrom")
        ).ToHaveValueAsync("");

        // Verify To field is empty
        await Assertions.Expect(
            page.Locator("#InputTo")
        ).ToHaveValueAsync("");

        // Verify From validation message
        await Assertions.Expect(
            page.GetByText(
                "The From field is required.",
                new() { Exact = true })
        ).ToBeVisibleAsync();

        // Verify To validation message
        await Assertions.Expect(
            page.GetByText(
                "The To field is required.",
                new() { Exact = true })
        ).ToBeVisibleAsync();

        // Verify the journey results page is not displayed
        await Assertions.Expect(
            page.GetByRole(
                AriaRole.Heading,
                new() { Name = "Journey results" })
        ).ToHaveCountAsync(0);

        Console.WriteLine(
            "Both mandatory field validation messages verified.");

        Console.WriteLine(
            "Journey planning correctly prevented for empty locations.");
    }
}
