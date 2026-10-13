
using System.Text.Json;

namespace TfL.Automation.Config;

public static class TestSettings
{
    public static string JourneyPlannerUrl
    {
        get
        {
            string path = Path.Combine(
                AppContext.BaseDirectory,
                "appsettings.json");

            using var document = JsonDocument.Parse(
                File.ReadAllText(path));

            return document.RootElement
                .GetProperty("JourneyPlannerUrl")
                .GetString()
                ?? throw new InvalidOperationException(
                    "JourneyPlannerUrl is missing.");
        }
    }
}
