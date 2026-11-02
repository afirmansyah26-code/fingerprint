using System;
using System.Windows.Forms;
using FingerprintAttendance.Data;
using FingerprintAttendance.UI;

namespace FingerprintAttendance
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                AppDatabase database = new AppDatabase();
                database.Initialize();
                Application.Run(new MainForm(database));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Absensi tidak dapat dibuka.\r\n\r\n" + ex.Message,
                    "Kesalahan saat memulai",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
