using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AcxynAPI.Model
{
    public class CustomTags
    {
        [JsonPropertyName("ARKit")]
        public string ARKit { get; set; }

        [JsonPropertyName("ARPDAU (Last Month, US)")]
        public string ARPDAULastMonthUS { get; set; }

        [JsonPropertyName("ARPDAU (Last Month, WW)")]
        public string ARPDAULastMonthWW { get; set; }

        [JsonPropertyName("Advertised on Any Network")]
        public string AdvertisedOnAnyNetwork { get; set; }

        [JsonPropertyName("Advertises with Banner Ads")]
        public string AdvertisesWithBannerAds { get; set; }

        [JsonPropertyName("Advertises with Full Screen Ads")]
        public string AdvertisesWithFullScreenAds { get; set; }

        [JsonPropertyName("Advertises with Playable Ads")]
        public string AdvertisesWithPlayableAds { get; set; }

        [JsonPropertyName("Advertises with Video Ads")]
        public string AdvertisesWithVideoAds { get; set; }

        [JsonPropertyName("Age (Last Quarter, US)")]
        public string AgeLastQuarterUS { get; set; }

        [JsonPropertyName("Age (Last Quarter, WW)")]
        public string AgeLastQuarterWW { get; set; }

        [JsonPropertyName("All Time Downloads (WW)")]
        public string AllTimeDownloadsWW { get; set; }

        [JsonPropertyName("All Time Publisher Downloads (WW)")]
        public string AllTimePublisherDownloadsWW { get; set; }

        [JsonPropertyName("All Time Publisher Revenue (WW)")]
        public string AllTimePublisherRevenueWW { get; set; }

        [JsonPropertyName("All Time Revenue (WW)")]
        public string AllTimeRevenueWW { get; set; }

        [JsonPropertyName("Apple Watch Support")]
        public string AppleWatchSupport { get; set; }

        [JsonPropertyName("Browser Downloads % (Last Q, US)")]
        public string BrowserDownloadsPercentageLastQUS { get; set; }

        [JsonPropertyName("Browser Downloads % (Last Q, WW)")]
        public string BrowserDownloadsPercentageLastQWW { get; set; }

        [JsonPropertyName("Changed Price")]
        public string ChangedPrice { get; set; }

        [JsonPropertyName("Content Rating")]
        public string ContentRating { get; set; }

        [JsonPropertyName("Current US Rating")]
        public string CurrentUSRating { get; set; }

        [JsonPropertyName("Day 1 Retention (Last Quarter, US)")]
        public string Day1RetentionLastQuarterUS { get; set; }

        [JsonPropertyName("Day 1 Retention (Last Quarter, WW)")]
        public string Day1RetentionLastQuarterWW { get; set; }

        [JsonPropertyName("Day 30 Retention (Last Quarter, US)")]
        public string Day30RetentionLastQuarterUS { get; set; }

        [JsonPropertyName("Day 30 Retention (Last Quarter, WW)")]
        public string Day30RetentionLastQuarterWW { get; set; }

        [JsonPropertyName("Day 60 Retention (Last Quarter, US)")]
        public string Day60RetentionLastQuarterUS { get; set; }

        [JsonPropertyName("Day 60 Retention (Last Quarter, WW)")]
        public string Day60RetentionLastQuarterWW { get; set; }

        [JsonPropertyName("Day 7 Retention (Last Quarter, US)")]
        public string Day7RetentionLastQuarterUS { get; set; }

        [JsonPropertyName("Day 7 Retention (Last Quarter, WW)")]
        public string Day7RetentionLastQuarterWW { get; set; }

        [JsonPropertyName("Downloads First 30 Days (WW)")]
        public string DownloadsFirst30DaysWW { get; set; }

        [JsonPropertyName("Earliest Release Date")]
        public string EarliestReleaseDate { get; set; }

        [JsonPropertyName("Editors' Choice")]
        public string EditorsChoice { get; set; }

        [JsonPropertyName("Free")]
        public string Free { get; set; }

        [JsonPropertyName("Game Art Style")]
        public string GameArtStyle { get; set; }

        [JsonPropertyName("Game Camera POV")]
        public string GameCameraPOV { get; set; }

        [JsonPropertyName("Game Class")]
        public string GameClass { get; set; }

        [JsonPropertyName("Game Genre")]
        public string GameGenre { get; set; }

        [JsonPropertyName("Game Product Model")]
        public string GameProductModel { get; set; }

        [JsonPropertyName("Game Setting")]
        public string GameSetting { get; set; }

        [JsonPropertyName("Game Sub-genre")]
        public string GameSubgenre { get; set; }

        [JsonPropertyName("Game Theme")]
        public string GameTheme { get; set; }

        [JsonPropertyName("Genders (Last Quarter, US)")]
        public string GendersLastQuarterUS { get; set; }

        [JsonPropertyName("Genders (Last Quarter, WW)")]
        public string GendersLastQuarterWW { get; set; }

        [JsonPropertyName("Global Rating Count")]
        public string GlobalRatingCount { get; set; }

        [JsonPropertyName("Has Video Trailer")]
        public string HasVideoTrailer { get; set; }

        [JsonPropertyName("In-App Events")]
        public string InAppEvents { get; set; }

        [JsonPropertyName("In-App Purchases")]
        public string InAppPurchases { get; set; }

        [JsonPropertyName("Inactive App")]
        public string InactiveApp { get; set; }

        [JsonPropertyName("Is Unified")]
        public string IsUnified { get; set; }

        [JsonPropertyName("Is a Game")]
        public string IsAGame { get; set; }

        [JsonPropertyName("Last 180 Days Downloads (WW)")]
        public string Last180DaysDownloadsWW { get; set; }

        [JsonPropertyName("Last 180 Days Revenue (WW)")]
        public string Last180DaysRevenueWW { get; set; }

        [JsonPropertyName("Last 30 Days Average DAU (US)")]
        public string Last30DaysAverageDAUUS { get; set; }

        [JsonPropertyName("Last 30 Days Average DAU (WW)")]
        public string Last30DaysAverageDAUWW { get; set; }

        [JsonPropertyName("Last 30 Days Downloads (WW)")]
        public string Last30DaysDownloadsWW { get; set; }

        [JsonPropertyName("Last 30 Days Revenue (WW)")]
        public string Last30DaysRevenueWW { get; set; }

        [JsonPropertyName("Last 4 Weeks Average WAU (US)")]
        public string Last4WeeksAverageWAUUS { get; set; }

        [JsonPropertyName("Last 4 Weeks Average WAU (WW)")]
        public string Last4WeeksAverageWAUWW { get; set; }

        [JsonPropertyName("Last Month Average MAU (US)")]
        public string LastMonthAverageMAUUS { get; set; }

        [JsonPropertyName("Last Month Average MAU (WW)")]
        public string LastMonthAverageMAUWW { get; set; }

        [JsonPropertyName("Latest Update Days Ago")]
        public string LatestUpdateDaysAgo { get; set; }

        [JsonPropertyName("Messages / Sticker Support")]
        public string MessagesStickerSupport { get; set; }

        [JsonPropertyName("Meta: Decoration / Renovation")]
        public string MetaDecorationRenovation { get; set; }

        [JsonPropertyName("Meta: Levels")]
        public string MetaLevels { get; set; }

        [JsonPropertyName("Meta: Narrative Stories")]
        public string MetaNarrativeStories { get; set; }

        [JsonPropertyName("Monetization: Ads")]
        public string MonetizationAds { get; set; }

        [JsonPropertyName("Monetization: Currency Bundles")]
        public string MonetizationCurrencyBundles { get; set; }

        [JsonPropertyName("Monetization: Free to Play")]
        public string MonetizationFreeToPlay { get; set; }

        [JsonPropertyName("Monetization: Live Ops")]
        public string MonetizationLiveOps { get; set; }

        [JsonPropertyName("Monetization: Starter Pack")]
        public string MonetizationStarterPack { get; set; }

        [JsonPropertyName("Most Popular Country by Downloads")]
        public string MostPopularCountryByDownloads { get; set; }

        [JsonPropertyName("Most Popular Country by Revenue")]
        public string MostPopularCountryByRevenue { get; set; }

        [JsonPropertyName("Most Popular Region by Downloads")]
        public string MostPopularRegionByDownloads { get; set; }

        [JsonPropertyName("Most Popular Region by Revenue")]
        public string MostPopularRegionByRevenue { get; set; }

        [JsonPropertyName("Organic Downloads % (Last Q, US)")]
        public string OrganicDownloadsPercentageLastQUS { get; set; }

        [JsonPropertyName("Organic Downloads % (Last Q, WW)")]
        public string OrganicDownloadsPercentageLastQWW { get; set; }

        [JsonPropertyName("Overall US Rating")]
        public string OverallUSRating { get; set; }

        [JsonPropertyName("Paid Downloads % (Last Q, US)")]
        public string PaidDownloadsPercentageLastQUS { get; set; }

        [JsonPropertyName("Paid Downloads % (Last Q, WW)")]
        public string PaidDownloadsPercentageLastQWW { get; set; }

        [JsonPropertyName("Primary Category")]
        public string PrimaryCategory { get; set; }

        [JsonPropertyName("Publisher Country")]
        public string PublisherCountry { get; set; }

        [JsonPropertyName("RPD (All Time, WW)")]
        public string RPDAllTimeWW { get; set; }

        [JsonPropertyName("Recent App Update")]
        public string RecentAppUpdate { get; set; }

        [JsonPropertyName("Release Date (JP)")]
        public string ReleaseDateJP { get; set; }

        [JsonPropertyName("Release Date (US)")]
        public string ReleaseDateUS { get; set; }

        [JsonPropertyName("Release Date (WW)")]
        public string ReleaseDateWW { get; set; }

        [JsonPropertyName("Released Days Ago (WW)")]
        public string ReleasedDaysAgoWW { get; set; }

        [JsonPropertyName("Revenue First 30 Days (WW)")]
        public string RevenueFirst30DaysWW { get; set; }

        [JsonPropertyName("Sexual Content or Nudity")]
        public string SexualContentOrNudity { get; set; }

        [JsonPropertyName("Soft Launch Date")]
        public string SoftLaunchDate { get; set; }

        [JsonPropertyName("Soft Launched Currently")]
        public string SoftLaunchedCurrently { get; set; }

        [JsonPropertyName("Stock Ticker")]
        public string StockTicker { get; set; }

        [JsonPropertyName("Storefront Game Subcategory")]
        public string StorefrontGameSubcategory { get; set; }

        [JsonPropertyName("Storefront Game Subcategory (Secondary)")]
        public string StorefrontGameSubcategorySecondary { get; set; }

        [JsonPropertyName("US Rating Count")]
        public string USRatingCount { get; set; }

        [JsonPropertyName("iOS App File Size")]
        public string iOSAppFileSize { get; set; }

        [JsonPropertyName("iOS Offerwalls: Tapjoy")]
        public string iOSOfferwallsTapjoy { get; set; }

        [JsonPropertyName("iOS Offerwalls: ironSource")]
        public string iOSOfferwallsIronSource { get; set; }
    }

    public class IOSData
    {
        [JsonPropertyName("app_id")]
        public long AppId { get; set; }

        [JsonPropertyName("current_units_value")]
        public int CurrentUnitsValue { get; set; }

        [JsonPropertyName("comparison_units_value")]
        public int ComparisonUnitsValue { get; set; }

        [JsonPropertyName("units_absolute")]
        public int UnitsAbsolute { get; set; }

        [JsonPropertyName("units_delta")]
        public int UnitsDelta { get; set; }

        [JsonPropertyName("units_transformed_delta")]
        public double UnitsTransformedDelta { get; set; }

        [JsonPropertyName("current_revenue_value")]
        public long CurrentRevenueValue { get; set; }

        [JsonPropertyName("comparison_revenue_value")]
        public long ComparisonRevenueValue { get; set; }

        [JsonPropertyName("revenue_absolute")]
        public long RevenueAbsolute { get; set; }

        [JsonPropertyName("revenue_delta")]
        public long RevenueDelta { get; set; }

        [JsonPropertyName("revenue_transformed_delta")]
        public double RevenueTransformedDelta { get; set; }

        [JsonPropertyName("absolute")]
        public int Absolute { get; set; }

        [JsonPropertyName("delta")]
        public int Delta { get; set; }

        [JsonPropertyName("transformed_delta")]
        public double TransformedDelta { get; set; }

        [JsonPropertyName("custom_tags")]
        public CustomTags CustomTags { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("country")]
        public object Country { get; set; }
    }

    public class IOSConfig
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public string AuthToken { get; set; }
        public int Category { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
    public class AppNameData
    {
        public int Id { get; set; }
        [JsonPropertyName("app_id")]
        public long AppId { get; set; }
        [JsonPropertyName("name")]
        public string AppName { get; set; }
    }
    public class AppNameResp
    {
        public List<AppNameData> apps { get; set; }
    }

    public class SensorTowerCsvData
    {
        public int Id { get; set; }
        public string AppId { get; set; }
        public string AppName { get; set; }
        public DateTime DataDate { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Revenue { get; set; }
        public string Type { get; set; }
        public decimal YoYGrowth { get; set; }
        public decimal NextYoYGrowth { get; set; }
    }

    public class SirePostData
    {
        public string id { get; set; }
        public string company_name { get; set; }
        public Array customer_focus { get; set; }
        public Array sectors { get; set; }
        public string revenue { get; set; }
        public string month { get; set; }
        public string year { get; set; }
        public string yoy_growth { get; set; }
        public string next_yoy_growth { get; set; }
    }

}
