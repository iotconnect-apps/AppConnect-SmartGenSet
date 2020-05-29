namespace iot.solution.entity.Response
{
    public class GeneratorDetailResponse
    {
        public int TotalCurrent { get; set; }
        public int TotalFuelUsed { get; set; }
        public int AvgVolt { get; set; }
        public int AvgRPM { get; set; }
        public int LatestBattlevel { get; set; }
        public int LatestFuelLevel { get; set; }
    }

   
}
