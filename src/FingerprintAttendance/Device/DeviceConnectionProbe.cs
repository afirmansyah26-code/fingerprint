using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using FingerprintAttendance.Domain;

namespace FingerprintAttendance.Device
{
    internal sealed class DeviceConnectionProbeResult
    {
        public bool Connected { get; set; }
        public string SerialNumber { get; set; }
        public string FirmwareVersion { get; set; }
        public string ErrorMessage { get; set; }
    }

    internal static class DeviceConnectionProbe
    {
        private const string ProgId = "zkemkeeper.ZKEM.1";
        private const int SdkMachineNumber = 1;

        public static DeviceConnectionProbeResult Run(Machine machine)
        {
            if (machine == null)
            {
                throw new ArgumentNullException("machine");
            }

            Type sdkType = Type.GetTypeFromProgID(ProgId, false);
            if (sdkType == null)
            {
                throw new InvalidOperationException(
                    "SDK zkemkeeper 32-bit belum terdaftar pada komputer ini. Installer perlu memasang dependensi SDK yang sesuai.");
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
                    return new DeviceConnectionProbeResult
                    {
                        Connected = false,
                        ErrorMessage = "Mesin tidak menerima koneksi pada alamat dan port tersebut."
                    };
                }

                return new DeviceConnectionProbeResult
                {
                    Connected = true,
                    SerialNumber = ReadText(sdkType, sdk, "GetSerialNumber"),
                    FirmwareVersion = ReadText(sdkType, sdk, "GetFirmwareVersion")
                };
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
                        // Hasil koneksi tetap dipertahankan jika SDK gagal melepas koneksi.
                    }
                }

                if (Marshal.IsComObject(sdk))
                {
                    Marshal.ReleaseComObject(sdk);
                }
            }
        }

        private static string ReadText(Type sdkType, object sdk, string methodName)
        {
            object[] arguments = new object[] { SdkMachineNumber, String.Empty };
            object result = Invoke(sdkType, sdk, methodName, arguments, new int[] { 1 });
            if (!Convert.ToBoolean(result, CultureInfo.InvariantCulture))
            {
                return String.Empty;
            }
            return Convert.ToString(arguments[1], CultureInfo.InvariantCulture);
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