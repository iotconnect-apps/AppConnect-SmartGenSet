namespace iot.solution.entity
{
    //public class OverviewResponse
    //{
    //    public int TotalLocations{ get; set; }
    //    public int TotalGenerators { get; set; }
    //    public int TotalOnGenerators { get; set; }
    //    public int TotalOffGenerators { get; set; }
    //    public int TotalDisconnectedGenerators { get; set; }
    //    public int TotalEneryGenerated { get; set; }
    //    public int TotalFuelUsed { get; set; }
    //    public int TotalAlerts { get; set; }

    //}
    public class DashboardOverviewResponse
    {
        public int TotalLocations { get; set; }
        public int TotalGenerators { get; set; }
        public int TotalOnGenerators { get; set; }
        public int TotalOffGenerators { get; set; }
        public int TotalDisconnectedGenerators { get; set; }
        public int TotalEneryGenerated { get; set; }
        public int TotalFuelUsed { get; set; }
        public int TotalAlerts { get; set; }
    }

    public class LocationStaticsResponse
    {
        public int TotalGenerators { get; set; }
        public int TotalOnGenerators { get; set; }
        public int TotalOffGenerators { get; set; }
        public int TotalDisconnectedGenerators { get; set; }
        public int TotalEnergyGenerated { get; set; }
        public int TotalFuelUsed { get; set; }
        public int TotalAlerts { get; set; }
    }
}
