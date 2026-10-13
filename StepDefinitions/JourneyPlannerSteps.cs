
using Reqnroll;
using TfL.Automation.Drivers;
using TfL.Automation.Pages;

namespace TfL.Automation.StepDefinitions;

[Binding]
public class JourneyPlannerSteps
{
    private readonly JourneyPlannerPage _journeyPage;
    private readonly JourneyResultsPage _resultsPage;

    public JourneyPlannerSteps(PlaywrightDriver driver)
    {
        _journeyPage = new JourneyPlannerPage(driver);
        _resultsPage = new JourneyResultsPage(driver);
    }

    [Given("I am on the TfL Journey Planner page")]
    public async Task GivenIAmOnJourneyPlannerPage()
    {
        await _journeyPage.OpenAsync();
    }

    [Then("the Journey Planner page should load successfully")]
    public async Task ThenJourneyPlannerPageShouldLoad()
    {
        await _journeyPage.VerifyPageLoadedAsync();
    }

    [When("I select {string} as the departure station")]
    public async Task WhenISelectDeparture(string station)
    {
        await _journeyPage.SelectStationAsync(
            "#InputFrom",
            station);
    }

    [When("I select {string} as the destination station")]
    public async Task WhenISelectDestination(string station)
    {
        await _journeyPage.SelectStationAsync(
            "#InputTo",
            station);
    }

    [Then("both stations should be selected from autocomplete")]
    public async Task ThenStationsShouldBeSelected()
    {
        await _journeyPage.VerifyStationsSelectedAsync();
    }

    [When("I click Plan my journey")]
    public async Task WhenIClickPlanMyJourney()
    {
        await _journeyPage.PlanJourneyAsync();
    }

    [Then("journey results should be displayed")]
    public async Task ThenJourneyResultsShouldBeDisplayed()
    {
        await _journeyPage.VerifyJourneyResultsAsync();
        await _journeyPage.CaptureJourneyResultsAsync();
    }

    [Then("walking and cycling journey times should be displayed")]
    public async Task ThenWalkingAndCyclingTimesShouldBeDisplayed()
    {
        await _resultsPage.VerifyWalkingAndCyclingTimesAsync();
    }

    [When("I update journey preferences to least walking")]
    public async Task WhenIUpdateJourneyPreferences()
    {
        await _resultsPage.UpdateJourneyWithLeastWalkingAsync();
    }

    [Then("the least walking preference should be applied")]
    public async Task ThenLeastWalkingPreferenceShouldBeApplied()
    {
        await _resultsPage.VerifyLeastWalkingPreferenceAsync();
    }

    [Then("the updated journey time should be displayed")]
    public async Task ThenUpdatedJourneyTimeShouldBeDisplayed()
    {
        await _resultsPage.VerifyUpdatedJourneyTimeAsync();
    }

    [When("I view the journey details")]
    public async Task WhenIViewJourneyDetails()
    {
        await _resultsPage.ViewJourneyDetailsAsync();
    }

    [When("I expand accessibility details")]
    public async Task WhenIExpandAccessibilityDetails()
    {
        await _resultsPage.ExpandAccessibilityDetailsAsync();
    }

    [Then("journey accessibility information should be displayed")]
    public async Task ThenAccessibilityInformationShouldBeDisplayed()
    {
        await _resultsPage.VerifyAccessibilityDetailsAsync();
    }

    [Then("Covent Garden station access indicators should be displayed")]
    public async Task ThenCoventGardenStationAccessShouldBeDisplayed()
    {
        await _resultsPage.VerifyCoventGardenStationAccessAsync();
    }

    [When("I enter {string} as the invalid departure location")]
    public async Task WhenIEnterInvalidDeparture(string location)
    {
        await _journeyPage.EnterInvalidDepartureAsync(location);
    }

    [Then("the invalid journey should be rejected")]
    public async Task ThenInvalidJourneyShouldBeRejected()
    {
        await _resultsPage.VerifyInvalidJourneyRejectedAsync();
    }

    [Then("the journey should not be planned without locations")]
    public async Task ThenEmptyJourneyShouldBeRejected()
    {
        await _journeyPage.VerifyEmptyJourneyRejectedAsync();
    }
}
