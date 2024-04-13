using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace DemoConsole
{
    public class Cell
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public string LaunchInfo { get; set; }
        public string Status { get; set; }
        public string Dimensions { get; set; }
        public string Weight { get; set; }
        public string SIM { get; set; }
        public string DisplayType { get; set; }
        public string DisplaySize { get; set; }
        public string Resolution { get; set; }
        public string OS { get; set; }

        public Cell(string brand, string model, string launchInfo, string status, string dimensions, string weight, string sim, string displayType, string displaySize, string resolution, string os)
        {
            Brand = brand;
            Model = model;
            LaunchInfo = launchInfo;
            Status = status;
            Dimensions = dimensions;
            Weight = weight;
            SIM = sim;
            DisplayType = displayType;
            DisplaySize = displaySize;
            Resolution = resolution;
            OS = os;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string relativePath = Path.Combine(baseDirectory, "cells.csv");
            string csvPath = Path.GetFullPath(Path.Combine(baseDirectory, relativePath));

            var cells = new List<Cell>();

            try
            {
                using (var reader = new StreamReader(csvPath))
                {
                    bool isFirstLine = true;
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (isFirstLine)
                        {
                            isFirstLine = false;
                            continue; // Skip the header line
                        }
                        var values = line.Split(',');

                        // Parsing and creating a Cell object, assuming each cell phone data has 11 columns.
                        if (values.Length >= 11)
                        {
                            var cell = new Cell(values[0].Trim(), values[1].Trim(), values[2].Trim(), values[3].Trim(),
                                                values[4].Trim(), values[5].Trim(), values[6].Trim(), values[7].Trim(),
                                                values[8].Trim(), values[9].Trim(), values[10].Trim());
                            cells.Add(cell);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading from CSV file: " + ex.Message);
            }

            // Displaying the data
            foreach (var cell in cells)
            {
                Console.WriteLine($"Brand: {cell.Brand}, Model: {cell.Model}, Launch Info: {cell.LaunchInfo}, Status: {cell.Status}");
            }
        }
    }
}
