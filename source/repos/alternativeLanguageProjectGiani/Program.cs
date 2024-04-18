using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

public class DisplaySizeConverter : ITypeConverter
{
    public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        // Using Regex to extract the first numeric part of the string
        var match = Regex.Match(text, @"\d+(\.\d+)?");
        if (match.Success && float.TryParse(match.Value, out float size))
            return size;

        return null;  // Return null if the conversion fails or if no numbers are found
    }

    public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
    {
        return value?.ToString() ?? string.Empty;
    }
}

public class LaunchAnnouncedConverter : ITypeConverter
{
    public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        // Using Regex to extract the first occurrence of four consecutive digits
        var match = Regex.Match(text, @"\b\d{4}\b");
        if (match.Success && int.TryParse(match.Value, out int year))
            return year;

        return null;  // Return null if no valid year is found
    }

    public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
    {
        return value?.ToString() ?? string.Empty;
    }
}
public class WeightConverter : ITypeConverter
{
    public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        // Using Regex to extract the first numeric part of the string
        var match = Regex.Match(text, @"\d+(\.\d+)?");
        if (match.Success && float.TryParse(match.Value, out float weight))
            return weight;

        return null;  // Return null if the conversion fails or if no numbers are found
    }

    public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
    {
        return value?.ToString() ?? string.Empty;
    }
}
public class Cell
{
    public string OEM { get; set; }
    public string Model { get; set; }
    public int? LaunchAnnounced { get; set; }
    public string LaunchStatus { get; set; }
    public string BodyDimensions { get; set; }
    public float? BodyWeight { get; set; }
    public string BodySIM { get; set; }
    public string DisplayType { get; set; }
    public float? DisplaySize { get; set; }
    public string DisplayResolution { get; set; }
    public string FeaturesSensors { get; set; }
    public string PlatformOS { get; set; }

    // Cell constructor and methods...
    // Methods to parse and validate different fields...
    public override string ToString()
    {
        return $"{OEM} {Model} ({LaunchAnnounced})";
    }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(OEM) && !string.IsNullOrEmpty(Model) && LaunchAnnounced.HasValue;
    }

    public bool HasValidBodyWeight()
    {
        return BodyWeight.HasValue && BodyWeight.Value > 0;
    }

    public bool HasValidDisplaySize()
    {
        return DisplaySize.HasValue && DisplaySize.Value > 0;
    }

    public bool HasValidLaunchStatus()
    {
        return !string.IsNullOrEmpty(LaunchStatus) && (LaunchStatus == "Discontinued" || LaunchStatus == "Cancelled" ||
                                                                 (int.TryParse(LaunchStatus, out int year) && year >= 1999));
    }

    public bool HasValidFeaturesSensors()
    {
        return !string.IsNullOrEmpty(FeaturesSensors) && FeaturesSensors.Split(',').All(f => !string.IsNullOrEmpty(f));
    }

    public bool HasValidPlatformOS()
    {
        return !string.IsNullOrEmpty(PlatformOS);
    }

    public bool HasValidDisplayResolution()
    {
        return !string.IsNullOrEmpty(DisplayResolution) && Regex.IsMatch(DisplayResolution, @"^\d+x\d+$");
    }

    //  this additional function to extract release year from LaunchStatus if available
    public int? GetReleaseYear()
    {
        var match = Regex.Match(LaunchStatus, @"\b\d{4}\b");
        if (match.Success && int.TryParse(match.Value, out int releaseYear))
            return releaseYear;
        return null;
    }
}

public sealed class CellMap : ClassMap<Cell>
{
    public CellMap()
    {
        Map(m => m.OEM).Name("oem");
        Map(m => m.Model).Name("model");
        Map(m => m.LaunchAnnounced).Name("launch_announced").TypeConverter<LaunchAnnouncedConverter>();
        Map(m => m.LaunchStatus).Name("launch_status");
        Map(m => m.BodyDimensions).Name("body_dimensions");
        Map(m => m.BodyWeight).Name("body_weight").TypeConverter<WeightConverter>();
        Map(m => m.BodySIM).Name("body_sim");
        Map(m => m.DisplayType).Name("display_type");
        Map(m => m.DisplaySize).Name("display_size").TypeConverter<DisplaySizeConverter>();
        Map(m => m.DisplayResolution).Name("display_resolution");
        Map(m => m.FeaturesSensors).Name("features_sensors");
        Map(m => m.PlatformOS).Name("platform_os");
    }
}

class Program
{
    static void Main(string[] args)
    {
        // Read the CSV file
        List<Cell> phones = ReadCSVFile("cells.csv");

        // Highest average weight by OEM
        var highestAvgWeightOem = GetHighestAverageWeightOem(phones);
        Console.WriteLine($"OEM with the highest average weight: {highestAvgWeightOem.Key} with an average weight of {highestAvgWeightOem.Value} grams");

        // Phones announced in one year and released in another
        var phonesDifferentYear = PhonesAnnouncedReleasedDifferentYear(phones);
        Console.WriteLine("Phones announced in one year and released in another:");
        foreach (var phone in phonesDifferentYear)
        {
            Console.WriteLine($"{phone.OEM} {phone.Model}");
        }

        // Number of phones with only one feature sensor
        int countOneFeatureSensor = CountPhonesWithOneFeatureSensor(phones);
        Console.WriteLine($"Number of phones with only one feature sensor: {countOneFeatureSensor}");

        // Year with the most phones launched after 1999
        int mostPhonesLaunchedYear = MostPhonesLaunchedYear(phones);
        Console.WriteLine($"Year with the most phones launched after 1999: {mostPhonesLaunchedYear}");
    }

    static KeyValuePair<string, float> GetHighestAverageWeightOem(List<Cell> phones)
    {
        return phones
            .Where(p => p.BodyWeight.HasValue)
            .GroupBy(p => p.OEM)
            .Select(g => new { OEM = g.Key, AverageWeight = g.Average(p => p.BodyWeight.Value) })
            .OrderByDescending(g => g.AverageWeight)
            .Select(g => new KeyValuePair<string, float>(g.OEM, g.AverageWeight))
            .FirstOrDefault();
    }

    static List<Cell> PhonesAnnouncedReleasedDifferentYear(List<Cell> phones)
    {
        return phones.Where(p =>
            p.LaunchAnnounced.HasValue &&
            p.GetReleaseYear().HasValue &&
            p.LaunchAnnounced.Value != p.GetReleaseYear().Value
        ).ToList();
    }

    static int ExtractYearFromStatus(string status)
    {
        var match = Regex.Match(status, @"\b\d{4}\b");
        if (match.Success && int.TryParse(match.Value, out int year))
            return year;
        return -1;  // Return -1 if no valid year is found, assuming no year can be -1
    }

    static int CountPhonesWithOneFeatureSensor(List<Cell> phones)
    {
        return phones.Count(p => p.FeaturesSensors.Split(',').Length == 1);
    }

    static int MostPhonesLaunchedYear(List<Cell> phones)
    {
        return phones
            .Where(p => p.LaunchAnnounced.HasValue && p.LaunchAnnounced > 1999)
            .GroupBy(p => p.LaunchAnnounced)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key.Value)
            .FirstOrDefault();
    }

    static List<Cell> ReadCSVFile(string filePath)
    {
        List<Cell> phones = new List<Cell>();
        try
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.ToLower(),
                MissingFieldFound = null
            }))
            {
                csv.Context.RegisterClassMap<CellMap>();
                phones = csv.GetRecords<Cell>().ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading CSV file: {ex.Message}");
        }
        return phones;
    }
}

