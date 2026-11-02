namespace FingerprintAttendance.Domain
{
    public sealed class Employee
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string FullName { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public bool IsActive { get; set; }
    }
}
