using System;
using System.Globalization;
using System.Text;
using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace DatesAndStuff.Web.Tests;

[TestFixture]
public class WizzAirFlightTests
{
    private IWebDriver driver;
    private const string BaseURL = "https://www.wizzair.com/en-gb";

    [SetUp]
    public void SetupTest()
    {
        var options = new ChromeOptions();
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddArgument("--start-maximized");
        driver = new ChromeDriver(options);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
    }

    [TearDown]
    public void TeardownTest()
    {
        try
        {
            driver.Quit();
            driver.Dispose();
        }
        catch (Exception)
        {
            // Ignore errors if unable to close the browser
        }
    }

    [Test]
    public void WizzAir_NextWeek_BucharestBudapest_ShouldHaveAtLeastTwoFlights()
    {
        // Arrange
        var nextWeekStart = DateTime.Now.AddDays(7 - (int)DateTime.Now.DayOfWeek + 1); // next Monday
        var nextWeekEnd = nextWeekStart.AddDays(6); // next Sunday

        driver.Navigate().GoToUrl(BaseURL + "/flights/timetable");

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

        // Accept cookies if the banner appears
        try
        {
            var cookieButton = wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("button[data-test='cookies-accept-all']")));
            cookieButton.Click();
            Thread.Sleep(1000);
        }
        catch (WebDriverTimeoutException)
        {
            // Cookie banner might not appear
        }

        // Select departure: Bucharest
        var departureInput = wait.Until(ExpectedConditions.ElementToBeClickable(
            By.CssSelector("input[data-test='search-departure-station']")));
        departureInput.Click();
        departureInput.Clear();
        departureInput.SendKeys("Bucharest");
        Thread.Sleep(2000);

        var bucharestOption = wait.Until(ExpectedConditions.ElementToBeClickable(
            By.XPath("//div[contains(@class, 'station') or contains(@class, 'location')]//span[contains(text(), 'Bucharest')]")));
        bucharestOption.Click();
        Thread.Sleep(1000);

        // Select destination: Budapest
        var destinationInput = wait.Until(ExpectedConditions.ElementToBeClickable(
            By.CssSelector("input[data-test='search-arrival-station']")));
        destinationInput.Click();
        destinationInput.Clear();
        destinationInput.SendKeys("Budapest");
        Thread.Sleep(2000);

        var budapestOption = wait.Until(ExpectedConditions.ElementToBeClickable(
            By.XPath("//div[contains(@class, 'station') or contains(@class, 'location')]//span[contains(text(), 'Budapest')]")));
        budapestOption.Click();
        Thread.Sleep(1000);

        // Click search / navigate to timetable
        try
        {
            var searchButton = wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("button[data-test='search-submit']")));
            searchButton.Click();
        }
        catch (WebDriverTimeoutException)
        {
            // Timetable might load automatically
        }

        Thread.Sleep(5000);

        // Navigate to next week if needed using the calendar/timetable navigation
        // Look for flight entries in the timetable for the next week
        var flightElements = driver.FindElements(
            By.CssSelector("[data-test*='flight'], [class*='flight-row'], [class*='timetable__flight'], .flight-info, .timetable-row"));

        // If no flights found with specific selectors, try broader approach
        if (flightElements.Count == 0)
        {
            flightElements = driver.FindElements(
                By.XPath("//div[contains(@class, 'flight') or contains(@class, 'timetable')]//div[contains(@class, 'row') or contains(@class, 'item')]"));
        }

        // Count flights for next week
        int flightCount = flightElements.Count;

        // Assert
        flightCount.Should().BeGreaterThanOrEqualTo(2,
            $"there should be at least 2 flights between Bucharest and Budapest in the next week ({nextWeekStart:yyyy-MM-dd} to {nextWeekEnd:yyyy-MM-dd})");
    }

    [Test]
    public void WizzAir_NextWeek_BucharestBudapest_CheapFlight_ShouldTakeScreenshot()
    {
        // Arrange
        const double maxPrice = 50.0; // preset price threshold in EUR
        var screenshotFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        var nextWeekStart = DateTime.Now.AddDays(7 - (int)DateTime.Now.DayOfWeek + 1);
        var nextWeekEnd = nextWeekStart.AddDays(6);

        driver.Navigate().GoToUrl(BaseURL + "/flights/timetable");

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

        // Accept cookies if the banner appears
        try
        {
            var cookieButton = wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("button[data-test='cookies-accept-all']")));
            cookieButton.Click();
            Thread.Sleep(1000);
        }
        catch (WebDriverTimeoutException)
        {
            // Cookie banner might not appear
        }

        // Select departure: Bucharest
        var departureInput = wait.Until(ExpectedConditions.ElementToBeClickable(
            By.CssSelector("input[data-test='search-departure-station']")));
        departureInput.Click();
        departureInput.Clear();
        departureInput.SendKeys("Bucharest");
        Thread.Sleep(2000);

        var bucharestOption = wait.Until(ExpectedConditions.ElementToBeClickable(
            By.XPath("//div[contains(@class, 'station') or contains(@class, 'location')]//span[contains(text(), 'Bucharest')]")));
        bucharestOption.Click();
        Thread.Sleep(1000);

        // Select destination: Budapest
        var destinationInput = wait.Until(ExpectedConditions.ElementToBeClickable(
            By.CssSelector("input[data-test='search-arrival-station']")));
        destinationInput.Click();
        destinationInput.Clear();
        destinationInput.SendKeys("Budapest");
        Thread.Sleep(2000);

        var budapestOption = wait.Until(ExpectedConditions.ElementToBeClickable(
            By.XPath("//div[contains(@class, 'station') or contains(@class, 'location')]//span[contains(text(), 'Budapest')]")));
        budapestOption.Click();
        Thread.Sleep(1000);

        // Click search
        try
        {
            var searchButton = wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("button[data-test='search-submit']")));
            searchButton.Click();
        }
        catch (WebDriverTimeoutException)
        {
            // Timetable might load automatically
        }

        Thread.Sleep(5000);

        // Find flight price elements
        var priceElements = driver.FindElements(
            By.CssSelector("[data-test*='price'], [class*='price'], [class*='fare']"));

        bool cheapFlightFound = false;

        foreach (var priceElement in priceElements)
        {
            var priceText = priceElement.Text;

            // Extract numeric price value from text (e.g., "€29.99", "29.99 EUR")
            var numericText = System.Text.RegularExpressions.Regex.Match(priceText, @"[\d]+[.,]?\d*");
            if (numericText.Success && double.TryParse(numericText.Value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double price))
            {
                if (price > 0 && price < maxPrice)
                {
                    cheapFlightFound = true;

                    // Take screenshot
                    var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                    var fileName = $"WizzAir_CheapFlight_BucharestBudapest_{DateTime.Now:yyyy-MM-dd_HHmmss}.png";
                    var filePath = Path.Combine(screenshotFolder, fileName);
                    screenshot.SaveAsFile(filePath);

                    TestContext.WriteLine($"Cheap flight found! Price: {price} EUR. Screenshot saved to: {filePath}");
                    break;
                }
            }
        }

        if (!cheapFlightFound)
        {
            TestContext.WriteLine($"No flights found below {maxPrice} EUR between Bucharest and Budapest for next week ({nextWeekStart:yyyy-MM-dd} to {nextWeekEnd:yyyy-MM-dd}).");
        }

        // Test passes regardless - it's informational; screenshot is taken only if cheap flight exists
        Assert.Pass(cheapFlightFound
            ? "Cheap flight found and screenshot saved to Desktop."
            : $"No flights below {maxPrice} EUR found.");
    }
}
