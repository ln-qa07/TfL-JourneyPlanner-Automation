
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace TfL.Automation.Drivers;

public class SeleniumDriver
{
    public IWebDriver Driver { get; private set; } = null!;

    public void Start()
    {
        var options = new ChromeOptions();

        options.AddArgument("--start-maximized");

        Driver = new ChromeDriver(options);

        Console.WriteLine(
            "Selenium ChromeDriver started.");
    }

    public void Stop()
    {
        if (Driver != null)
        {
            Driver.Quit();
            Driver.Dispose();

            Console.WriteLine(
                "Selenium ChromeDriver closed.");
        }
    }
}
