
using Microsoft.Playwright;

namespace TfL.Automation.Drivers;

public class PlaywrightDriver
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;

    public IPage Page { get; private set; } = null!;

    public async Task StartAsync()
    {
        _playwright = await Playwright.CreateAsync();

        _browser = await _playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions
            {
                Headless = false
            });

        _context = await _browser.NewContextAsync();

        Page = await _context.NewPageAsync();
    }

    public async Task StopAsync()
    {
        if (_context != null)
            await _context.CloseAsync();

        if (_browser != null)
            await _browser.CloseAsync();

        _playwright?.Dispose();
    }
}
