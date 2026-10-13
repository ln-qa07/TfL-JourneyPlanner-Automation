
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TfL.Automation.Config;
using TfL.Automation.Drivers;

namespace TfL.Automation.Pages;

public class SeleniumJourneyPlannerPage
{
    private readonly SeleniumDriver _selenium;

    private IWebDriver Driver => _selenium.Driver;

    public SeleniumJourneyPlannerPage(SeleniumDriver driver)
    {
        _selenium = driver;
    }

    public void Open()
    {
        // 1. Navigate to TfL Journey Planner
        Driver.Navigate().GoToUrl(
            TestSettings.JourneyPlannerUrl);

        var wait = new WebDriverWait(
            Driver, TimeSpan.FromSeconds(30));

        wait.PollingInterval =
            TimeSpan.FromMilliseconds(500);

        wait.IgnoreExceptionTypes(
            typeof(StaleElementReferenceException));

        // 2. Wait for Journey Planner form
        var fromField = By.Id("InputFrom");

        wait.Until(d =>
            d.FindElements(fromField)
                .Any(e => e.Displayed));

        Console.WriteLine(
            "Selenium: Journey Planner loaded.");

        // 3. Locate cookie consent button
        var cookieButton = By.XPath(
            "//button[normalize-space(.)=" +
            "'Accept only essential cookies']");

        bool cookieAccepted = false;

        try
        {
            var cookieWait = new WebDriverWait(
                Driver, TimeSpan.FromSeconds(15));

            cookieWait.PollingInterval =
                TimeSpan.FromMilliseconds(500);

            cookieWait.IgnoreExceptionTypes(
                typeof(StaleElementReferenceException));

            // Wait until cookie button is clickable
            var button = cookieWait.Until(d =>
                d.FindElements(cookieButton)
                    .FirstOrDefault(e =>
                        e.Displayed && e.Enabled));

            button!.Click();

            cookieAccepted = true;

            Console.WriteLine(
                "Selenium: Cookie consent accepted.");
        }
        catch (WebDriverTimeoutException)
        {
            if (IsCookieOverlayVisible())
            {
                throw new InvalidOperationException(
                    "Cookie overlay is visible but consent button could not be found.");
            }

            Console.WriteLine(
                "Selenium: Cookie popup not displayed.");
        }

        // 4. Refresh once after accepting cookies
        if (cookieAccepted)
        {
            Driver.Navigate().Refresh();

            Console.WriteLine(
                "Selenium: Page refreshed after cookie consent.");
        }

        // 5. Handle cookie overlay if it reappears after refresh
        for (int attempt = 1; attempt <= 2; attempt++)
        {
            if (!IsCookieOverlayVisible())
            {
                break;
            }

            Console.WriteLine(
                $"Selenium: Cookie overlay detected. Retry {attempt}/2.");

            var retryWait = new WebDriverWait(
                Driver, TimeSpan.FromSeconds(10));

            retryWait.IgnoreExceptionTypes(
                typeof(StaleElementReferenceException));

            // Find and accept cookies again
            var retryButton = retryWait.Until(d =>
                d.FindElements(cookieButton)
                    .FirstOrDefault(e =>
                        e.Displayed && e.Enabled));

            retryButton!.Click();

            Console.WriteLine(
                "Selenium: Cookie consent accepted again.");

            try
            {
                // Verify overlay disappears
                var overlayWait = new WebDriverWait(
                    Driver, TimeSpan.FromSeconds(8));

                overlayWait.Until(d =>
                    !IsCookieOverlayVisible());

                break;
            }
            catch (WebDriverTimeoutException)
            {
                if (attempt == 2)
                {
                    throw new InvalidOperationException(
                        "Cookie overlay remains visible after two retry attempts.");
                }
            }
        }

        Console.WriteLine(
            "Selenium: Cookie overlay cleared.");

        // 6. Wait for the form after refresh
        var input = wait.Until(d =>
            d.FindElements(fromField)
                .FirstOrDefault(e =>
                    e.Displayed && e.Enabled));

        // 7. Verify input can be clicked
        input!.Click();

        Console.WriteLine(
            "Selenium: Journey Planner ready for automation.");
    }

    private bool IsCookieOverlayVisible()
    {
        return Driver
            .FindElements(By.Id("cb-cookieoverlay"))
            .Any(e => e.Displayed);
    }

    public void SubmitEmptyJourney()
    {
        var wait = new WebDriverWait(
            Driver, TimeSpan.FromSeconds(15));

        var submitButton = wait.Until(d =>
            d.FindElements(By.XPath(
                "//button[normalize-space(.)='Plan my journey']" +
                " | //input[@type='submit' and " +
                "@value='Plan my journey']"))
            .FirstOrDefault(e =>
                e.Displayed && e.Enabled));

        submitButton!.Click();

        Console.WriteLine(
            "Selenium: Submitted empty journey.");
    }

    public void VerifyRequiredFieldErrors()
    {
        var wait = new WebDriverWait(
            Driver, TimeSpan.FromSeconds(15));

        wait.Until(d =>
        {
            string text = d.FindElement(
                By.TagName("body")).Text;

            return text.Contains(
                       "The From field is required.")
                && text.Contains(
                       "The To field is required.");
        });

        Assert.That(
            Driver.FindElement(By.Id("InputFrom"))
                .GetAttribute("value"),
            Is.Empty);

        Assert.That(
            Driver.FindElement(By.Id("InputTo"))
                .GetAttribute("value"),
            Is.Empty);

        Assert.That(
            Driver.FindElements(By.XPath(
                "//h1[normalize-space(.)='Journey results']")),
            Is.Empty);

        Console.WriteLine(
            "Selenium: Both validation messages verified.");
    }
}
