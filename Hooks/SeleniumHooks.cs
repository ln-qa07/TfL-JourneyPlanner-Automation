
using OpenQA.Selenium;
using Reqnroll;
using System.Text.RegularExpressions;
using TfL.Automation.Drivers;

namespace TfL.Automation.Hooks;

[Binding]
public class SeleniumHooks
{
    private readonly SeleniumDriver _driver;
    private readonly ScenarioContext _scenarioContext;

    public SeleniumHooks(
        SeleniumDriver driver,
        ScenarioContext scenarioContext)
    {
        _driver = driver;
        _scenarioContext = scenarioContext;
    }

    [BeforeScenario("selenium")]
    public void BeforeScenario()
    {
        _driver.Start();
        Console.WriteLine("ChromeDriver browser started.");
    }

    [AfterScenario("selenium")]
    public void AfterScenario()
    {
        try
        {
            if (_driver.Driver is ITakesScreenshot screenshotDriver)
            {
                string status = _scenarioContext.TestError == null
                    ? "Passed"
                    : "Failed";

                string scenarioName = Regex.Replace(
                    _scenarioContext.ScenarioInfo.Title,
                    @"[^\w-]+",
                    "_");

                string projectDirectory = Path.GetFullPath(
                    Path.Combine(
                        AppContext.BaseDirectory,
                        "..", "..", ".."));

                string reportsDirectory = Path.Combine(
                    projectDirectory,
                    "Reports",
                    status);

                Directory.CreateDirectory(reportsDirectory);

                string fileName =
                    $"Selenium_{scenarioName}_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}.png";

                string screenshotPath = Path.Combine(
                    reportsDirectory,
                    fileName);

                screenshotDriver.GetScreenshot()
                    .SaveAsFile(screenshotPath);

                Console.WriteLine(
                    $"Selenium screenshot saved: {screenshotPath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Screenshot capture failed: {ex.Message}");
        }
        finally
        {
            _driver.Stop();
        }
    }
}
