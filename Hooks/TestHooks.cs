
using Microsoft.Playwright;
using Reqnroll;
using System.Text.RegularExpressions;
using TfL.Automation.Drivers;

namespace TfL.Automation.Hooks;

[Binding]
public class TestHooks
{
    private readonly PlaywrightDriver _driver;
    private readonly ScenarioContext _scenarioContext;

    public TestHooks(
        PlaywrightDriver driver,
        ScenarioContext scenarioContext)
    {
        _driver = driver;
        _scenarioContext = scenarioContext;
    }

    [BeforeScenario("playwright")]
    public async Task BeforeScenario()
    {
        await _driver.StartAsync();
    }

    [AfterScenario("playwright")]
    public async Task AfterScenario()
    {
        try
        {
            if (_driver.Page is not null)
            {
                // Identify test result
                string status =
                    _scenarioContext.TestError == null
                        ? "Passed"
                        : "Failed";

                // Generate a safe screenshot filename
                string scenarioName = Regex.Replace(
                    _scenarioContext.ScenarioInfo.Title,
                    @"[^\w-]+",
                    "_");

                string timestamp =
                    DateTime.UtcNow.ToString(
                        "yyyyMMdd_HHmmss_fff");

                // Find project Reports folder
                string projectDirectory = Path.GetFullPath(
                    Path.Combine(
                        AppContext.BaseDirectory,
                        "..", "..", ".."));

                string reportsDirectory = Path.Combine(
                    projectDirectory,
                    "Reports",
                    status);

                Directory.CreateDirectory(reportsDirectory);

                string screenshotPath = Path.Combine(
                    reportsDirectory,
                    $"{scenarioName}_{timestamp}.png");

                // Capture current browser state
                await _driver.Page.ScreenshotAsync(
                    new PageScreenshotOptions
                    {
                        Path = screenshotPath,
                        FullPage = true
                    });

                Console.WriteLine(
                    $"{status} screenshot saved: {screenshotPath}");
            }
        }
        catch (Exception ex)
        {
            // Screenshot issues should not hide
            // the original test failure
            Console.WriteLine(
                $"Screenshot capture failed: {ex.Message}");
        }
        finally
        {
            // Always close the browser
            await _driver.StopAsync();
        }
    }
}
