
using Reqnroll;
using TfL.Automation.Drivers;
using TfL.Automation.Pages;

namespace TfL.Automation.StepDefinitions;

[Binding]
public class SeleniumJourneySteps
{
    private readonly SeleniumJourneyPlannerPage _page;

    public SeleniumJourneySteps(SeleniumDriver driver)
    {
        _page = new SeleniumJourneyPlannerPage(driver);
    }

    [Given("I open the TfL Journey Planner using ChromeDriver")]
    public void GivenIOpenJourneyPlannerUsingChromeDriver()
    {
        _page.Open();
    }

    [When("I submit the journey without entering either location")]
    public void WhenISubmitEmptyJourney()
    {
        _page.SubmitEmptyJourney();
    }

    [Then("Selenium should display both required location errors")]
    public void ThenSeleniumShouldDisplayRequiredErrors()
    {
        _page.VerifyRequiredFieldErrors();
    }
}
