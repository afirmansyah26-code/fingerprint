using System;

namespace FingerprintAttendance.Domain
{
    public sealed class AttendanceRecord
    {
        public string MachineName { get; set; }
        public string DeviceUserId { get; set; }
        public DateTime RecordedAt { get; set; }
        public int VerifyMode { get; set; }
        public int InOutMode { get; set; }
        public int WorkCode { get; set; }
    }
}