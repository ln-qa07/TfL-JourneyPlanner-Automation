
using Microsoft.Playwright;
using NUnit.Framework;
using System.Text.RegularExpressions;
using TfL.Automation.Drivers;

namespace TfL.Automation.Pages;

public class JourneyResultsPage
{
    private readonly IPage _page;

    public JourneyResultsPage(PlaywrightDriver driver)
    {
        _page = driver.Page;
    }

    public async Task VerifyWalkingAndCyclingTimesAsync()
    {
        // Verify the walking and cycling section is visible
        await Assertions.Expect(
            _page.GetByText(
                "Walking and cycling",
                new() { Exact = true })
        ).ToBeVisibleAsync();

        // Read the journey results text
        string pageText = await _page.Locator("body")
            .InnerTextAsync();

        int start = pageText.IndexOf(
            "Walking and cycling",
            StringComparison.OrdinalIgnoreCase);

        Assert.That(start, Is.GreaterThanOrEqualTo(0));

        string resultsText = pageText[
            (start + "Walking and cycling".Length)..];

        // Keep only the walking and cycling section
        int end = resultsText.IndexOf(
            "Earlier journeys",
            StringComparison.OrdinalIgnoreCase);

        if (end >= 0)
        {
            resultsText = resultsText[..end];
        }

        // Extract time values for both options
        int cyclingMinutes = ExtractMinutes(
            resultsText, "Cycling");

        int walkingMinutes = ExtractMinutes(
            resultsText, "Walking");

        // Validate positive journey durations
        Assert.That(cyclingMinutes, Is.GreaterThan(0));
        Assert.That(walkingMinutes, Is.GreaterThan(0));

        Console.WriteLine(
            $"Cycling journey: {cyclingMinutes} minutes");

        Console.WriteLine(
            $"Walking journey: {walkingMinutes} minutes");
    }

    public async Task UpdateJourneyWithLeastWalkingAsync()
    {
        // Open journey preferences
        await _page.GetByText(
            "Edit preferences",
            new() { Exact = true }
        ).ClickAsync();

        // Find the least walking option
        var leastWalking = _page.GetByText(
            "Routes with least walking",
            new() { Exact = true }
        );

        await Assertions.Expect(leastWalking)
            .ToBeVisibleAsync();

        // Select least walking
        await leastWalking.ClickAsync();

        Console.WriteLine("Least walking preference selected.");

        // Apply the new preferences
        await _page.GetByRole(
            AriaRole.Button,
            new()
            {
                Name = "Update journey",
                Exact = true
            }
        ).ClickAsync();

        Console.WriteLine("Journey preferences updated.");

        // Wait until results finish loading
        await Assertions.Expect(
            _page.GetByText(
                "Fetching results",
                new() { Exact = true }
            )
        ).ToBeHiddenAsync(
            new() { Timeout = 60000 });

        // Verify the journey results page remains visible
        await Assertions.Expect(
            _page.GetByRole(
                AriaRole.Heading,
                new() { Name = "Journey results" }
            )
        ).ToBeVisibleAsync();
    }

    public async Task VerifyLeastWalkingPreferenceAsync()
    {
        await Assertions.Expect(
            _page.GetByText(
                "Showing routes with least walking",
                new() { Exact = true })
        ).ToBeVisibleAsync(new() { Timeout = 30000 });

        Console.WriteLine("Least walking preference verified.");
    }

    public async Task VerifyUpdatedJourneyTimeAsync()
    {
        // Identify the first public transport journey
        var firstOption = _page.GetByRole(
            AriaRole.Button,
            new()
            {
                NameRegex = new Regex(
                    @"^Option 1:",
                    RegexOptions.IgnoreCase)
            }).First;

        await Assertions.Expect(firstOption)
            .ToBeVisibleAsync(new() { Timeout = 30000 });

        string optionText = await firstOption.InnerTextAsync();

        // Extract the total journey duration
        var match = Regex.Match(
            optionText,
            @"Total\s*time\s*:\s*(\d+)\s*mins?",
            RegexOptions.IgnoreCase);

        Assert.That(
            match.Success,
            Is.True,
            $"Journey duration not found: {optionText}");

        int minutes = int.Parse(match.Groups[1].Value);

        Assert.That(minutes, Is.GreaterThan(0));

        Console.WriteLine(
            $"Updated journey duration: {minutes} minutes");
    }

    public async Task ViewJourneyDetailsAsync()
    {
        // Locate View details using its CSS classes
        var viewDetailsButton = _page
            .Locator("button.show-detailed-results.view-hide-details")
            .Filter(new() { HasText = "View details" })
            .First;

        // Verify button is visible
        await Assertions.Expect(viewDetailsButton)
            .ToBeVisibleAsync(
                new() { Timeout = 15000 });

        Console.WriteLine("View details button found.");

        // Click the button
        await viewDetailsButton.ClickAsync();

        Console.WriteLine("View details clicked successfully.");
    }

    public async Task ExpandAccessibilityDetailsAsync()
    {
        // Locate accessibility accordion by ID
        var accessibilityHeading = _page.Locator(
            "#option-1-accessibility-heading");

        // Verify accordion heading is visible
        await Assertions.Expect(accessibilityHeading)
            .ToBeVisibleAsync(
                new() { Timeout = 15000 });

        Console.WriteLine("Accessibility accordion found.");

        // Expand accessibility details
        await accessibilityHeading.ClickAsync();

        Console.WriteLine("Accessibility details expanded.");
    }

    public async Task VerifyAccessibilityDetailsAsync()
    {
        var heading = _page.Locator(
            "#option-1-accessibility-heading");

        // Find the parent details element
        var details = heading.Locator(
            "xpath=ancestor::details[1]");

        // Confirm accordion is expanded
        await Assertions.Expect(details)
            .ToHaveAttributeAsync("open", "");

        // Read displayed accessibility information
        string information = await details.InnerTextAsync();

        Assert.That(information,
            Does.Contain("Total walking time:"));

        Assert.That(
            Regex.IsMatch(
                information,
                @"\b\d+\s*minutes?\b",
                RegexOptions.IgnoreCase),
            Is.True,
            "A walking duration should be displayed.");

        Assert.That(information,
            Does.Contain("stairs").IgnoreCase);

        Assert.That(information,
            Does.Contain("escalator").IgnoreCase);

        Assert.That(information,
            Does.Contain("level walkway").IgnoreCase);

        Console.WriteLine(
            "Accessibility information verified successfully.");
    }

    public async Task VerifyCoventGardenStationAccessAsync()
    {
        // Locate the expanded first journey
        var firstJourney = _page.GetByRole(
            AriaRole.Region,
            new()
            {
                NameRegex = new Regex(@"^Option 1:")
            });

        // Identify the access information containing the lift
        var stationAccess = firstJourney
            .Locator("div.access-information")
            .Filter(new()
            {
                Has = _page.Locator("a.up-lift[aria-label='Up lift']")
            }).First;

        await Assertions.Expect(stationAccess)
            .ToBeVisibleAsync();

        // Verify stairs information
        await Assertions.Expect(
            stationAccess.Locator(
                "a.up-stairs[aria-label='Up stairs']")
        ).ToHaveCountAsync(1);

        // Verify lift information
        await Assertions.Expect(
            stationAccess.Locator(
                "a.up-lift[aria-label='Up lift']")
        ).ToHaveCountAsync(1);

        // Verify level walkway information
        await Assertions.Expect(
            stationAccess.Locator(
                "a.level-walkway[aria-label='Level walkway']")
        ).ToHaveCountAsync(1);

        Console.WriteLine(
            "Station access indicators verified: " +
            "Up stairs, Up lift, Level walkway.");
    }

    public async Task VerifyInvalidJourneyRejectedAsync()
    {
        // Verify location warning is displayed
        var warning = _page.GetByText(
            new Regex(
                @"We found more than one location matching",
                RegexOptions.IgnoreCase)
        ).First;

        await Assertions.Expect(warning)
            .ToBeVisibleAsync(
                new() { Timeout = 30000 });

        Console.WriteLine(
            "Invalid location warning displayed.");

        // Verify no public transport journeys exist
        var journeyOptions = _page.GetByRole(
            AriaRole.Button,
            new()
            {
                NameRegex = new Regex(
                    @"^Option\s+\d+:",
                    RegexOptions.IgnoreCase)
            });

        await Assertions.Expect(journeyOptions)
            .ToHaveCountAsync(0);

        // Verify no walking/cycling results exist
        await Assertions.Expect(
            _page.GetByRole(
                AriaRole.Heading,
                new() { Name = "Walking and cycling" })
        ).ToHaveCountAsync(0);

        Console.WriteLine(
            "Invalid journey rejected successfully.");
    }

    private static int ExtractMinutes(
        string text,
        string journeyType)
    {
        var match = Regex.Match(
            text,
            $@"\b{Regex.Escape(journeyType)}\b.{{0,250}}?\b(\d+)\s*mins?\b",
            RegexOptions.IgnoreCase |
            RegexOptions.Singleline);

        Assert.That(
            match.Success,
            Is.True,
            $"No duration found for {journeyType} journey");

        return int.Parse(match.Groups[1].Value);
    }
}
