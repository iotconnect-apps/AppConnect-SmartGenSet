using System;
using System.Collections.Generic;
using System.Text;

namespace iot.solution.entity.Structs.Routes
{
    public class ChartRoute
    {
        public struct Name
        {
            public const string GeneratorUsage = "chart.generatorusage";
            public const string FuelUsage = "chart.fuelused";
            public const string GeneratorBatteryStatus = "chart.generatorbatterystatus";
            public const string EnergyGenerated = "chart.energygenerated";
            public const string EnergyUsage = "chart.getenergyusage";
        }

        public struct Route
        {
            public const string Global = "api/chart";
            public const string GeneratorUsage = "getgeneratorusage";
            public const string FuelUsage = "getfuelused";
            public const string GeneratorBatteryStatus = "getgeneratorbatterystatus";
            public const string EnergyGenerated = "getenergygenerated";
        }
    }
}
