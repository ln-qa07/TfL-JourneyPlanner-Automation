
Feature: TfL Journey Planner
  As a TfL passenger
  I want to plan journeys between locations
  So that I can identify suitable travel routes


@playwright
Scenario: Verify valid journey planning
    Given I am on the TfL Journey Planner page
    When I select "Leicester Square" as the departure station
    And I select "Covent Garden" as the destination station
    And I click Plan my journey
    Then journey results should be displayed
    And walking and cycling journey times should be displayed


  @playwright
  Scenario: Journey Planner page opens
    Given I am on the TfL Journey Planner page
    Then the Journey Planner page should load successfully


  @playwright
  Scenario: Verify station autocomplete
    Given I am on the TfL Journey Planner page
    When I select "Leicester Square" as the departure station
    And I select "Covent Garden" as the destination station
    Then both stations should be selected from autocomplete

  

@playwright
Scenario: Verify journey with least walking preference
    Given I am on the TfL Journey Planner page
    When I select "Leicester Square" as the departure station
    And I select "Covent Garden" as the destination station
    And I click Plan my journey
    Then journey results should be displayed
    When I update journey preferences to least walking
    Then the least walking preference should be applied
    And the updated journey time should be displayed



@playwright
Scenario: Verify Covent Garden station accessibility information
    Given I am on the TfL Journey Planner page
    When I select "Leicester Square" as the departure station
    And I select "Covent Garden" as the destination station
    And I click Plan my journey
    Then journey results should be displayed
    When I update journey preferences to least walking
    Then the least walking preference should be applied
    And the updated journey time should be displayed
    When I view the journey details
    And I expand accessibility details
    Then journey accessibility information should be displayed
    And Covent Garden station access indicators should be displayed


@playwright
Scenario: Verify invalid journey is rejected
    Given I am on the TfL Journey Planner page
    When I enter "InvalidStationXYZ98765" as the invalid departure location
    And I select "Covent Garden" as the destination station
    And I click Plan my journey
    Then the invalid journey should be rejected


@playwright
Scenario: Verify empty journey locations are rejected
    Given I am on the TfL Journey Planner page
    When I click Plan my journey
    Then the journey should not be planned without locations


@selenium
Scenario: Verify empty fields using ChromeDriver
    Given I open the TfL Journey Planner using ChromeDriver
    When I submit the journey without entering either location
    Then Selenium should display both required location errors
