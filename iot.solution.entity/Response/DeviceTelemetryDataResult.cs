namespace iot.solution.entity
{
    public class DeviceTelemetryDataResult
    {
        public string templateAttributeGuid { get; set; }
        public string attributeName { get; set; }
        public string attributeValue { get; set; }
        public System.DateTime deviceUpdatedDate { get; set; }
        public int notificationCount { get; set; }
        public int aggregateType { get; set; }
        public string DataType { get; set; }
        public object aggregateTypeValues { get; set; }
    }
}
