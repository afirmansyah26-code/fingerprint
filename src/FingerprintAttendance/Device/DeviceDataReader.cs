using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using FingerprintAttendance.Domain;

namespace FingerprintAttendance.Device
{
    internal sealed class DeviceAttendanceRecord
    {
        public string UserId { get; set; }
        public DateTime RecordedAt { get; set; }
        public int VerifyMode { get; set; }
        public int InOutMode { get; set; }
        public int WorkCode { get; set; }
    }

    internal sealed class DeviceAttendanceReadResult
    {
        public DeviceAttendanceReadResult()
        {
            Records = new List<DeviceAttendanceRecord>();
        }

        public List<DeviceAttendanceRecord> Records { get; private set; }
        public int InvalidRecordsSkipped { get; set; }
    }

    internal static class DeviceDataReader
    {
        private const string ProgId = "zkemkeeper.ZKEM.1";
        private const int SdkMachineNumber = 1;

        public static DeviceAttendanceReadResult ReadAttendance(Machine machine)
        {
            if (machine == null)
            {
                throw new ArgumentNullException("machine");
            }

            Type sdkType = Type.GetTypeFromProgID(ProgId, false);
            if (sdkType == null)
            {
                throw new InvalidOperationException(
                    "SDK zkemkeeper 32-bit belum terdaftar pada komputer ini.");
            }

            object sdk = Activator.CreateInstance(sdkType);
            bool connected = false;
            try
            {
                object connectResult = Invoke(sdkType, sdk, "Connect_Net",
                    new object[] { machine.Host, machine.Port }, null);
                connected = Convert.ToBoolean(connectResult, CultureInfo.InvariantCulture);
                if (!connected)
                {
                    throw new InvalidOperationException(
                        "Koneksi ke mesin gagal. Periksa alamat, port, dan jaringan lokal.");
                }

                object readResult = Invoke(sdkType, sdk, "ReadAllGLogData",
                    new object[] { SdkMachineNumber }, null);
                if (!Convert.ToBoolean(readResult, CultureInfo.InvariantCulture))
                {
                    throw new InvalidOperationException(
                        "SDK tidak dapat memuat daftar record dari mesin.");
                }

                DeviceAttendanceReadResult result = new DeviceAttendanceReadResult();
                while (true)
                {
                    object[] arguments = new object[]
                    {
                        SdkMachineNumber,
                        String.Empty,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0
                    };
                    object nextResult = Invoke(sdkType, sdk, "SSR_GetGeneralLogData",
                        arguments, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
                    if (!Convert.ToBoolean(nextResult, CultureInfo.InvariantCulture))
                    {
                        break;
                    }

                    string userId = Convert.ToString(arguments[1], CultureInfo.InvariantCulture);
                    if (String.IsNullOrWhiteSpace(userId))
                    {
                        result.InvalidRecordsSkipped++;
                        continue;
                    }

                    try
                    {
                        DateTime recordedAt = new DateTime(
                            Convert.ToInt32(arguments[4], CultureInfo.InvariantCulture),
                            Convert.ToInt32(arguments[5], CultureInfo.InvariantCulture),
                            Convert.ToInt32(arguments[6], CultureInfo.InvariantCulture),
                            Convert.ToInt32(arguments[7], CultureInfo.InvariantCulture),
                            Convert.ToInt32(arguments[8], CultureInfo.InvariantCulture),
                            Convert.ToInt32(arguments[9], CultureInfo.InvariantCulture),
                            DateTimeKind.Unspecified);

                        result.Records.Add(new DeviceAttendanceRecord
                        {
                            UserId = userId.Trim(),
                            RecordedAt = recordedAt,
                            VerifyMode = Convert.ToInt32(arguments[2], CultureInfo.InvariantCulture),
                            InOutMode = Convert.ToInt32(arguments[3], CultureInfo.InvariantCulture),
                            WorkCode = Convert.ToInt32(arguments[10], CultureInfo.InvariantCulture)
                        });
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        result.InvalidRecordsSkipped++;
                    }
                    catch (FormatException)
                    {
                        result.InvalidRecordsSkipped++;
                    }
                }

                return result;
            }
            finally
            {
                if (connected)
                {
                    try
                    {
                        Invoke(sdkType, sdk, "Disconnect", new object[0], null);
                    }
                    catch
                    {
                        // Kegagalan melepas koneksi tidak boleh menutupi hasil pembacaan.
                    }
                }

                if (Marshal.IsComObject(sdk))
                {
                    Marshal.ReleaseComObject(sdk);
                }
            }
        }

        private static object Invoke(
            Type sdkType,
            object sdk,
            string methodName,
            object[] arguments,
            int[] outputParameterIndexes)
        {
            ParameterModifier[] modifiers = null;
            if (outputParameterIndexes != null && outputParameterIndexes.Length > 0)
            {
                ParameterModifier modifier = new ParameterModifier(arguments.Length);
                foreach (int index in outputParameterIndexes)
                {
                    modifier[index] = true;
                }
                modifiers = new ParameterModifier[] { modifier };
            }

            return sdkType.InvokeMember(
                methodName,
                BindingFlags.InvokeMethod,
                null,
                sdk,
                arguments,
                modifiers,
                CultureInfo.InvariantCulture,
                null);
        }
    }
}