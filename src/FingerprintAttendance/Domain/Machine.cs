namespace FingerprintAttendance.Domain
{
    public sealed class Machine
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string SerialNumber { get; set; }
        public bool IsEnabled { get; set; }
    }
}
