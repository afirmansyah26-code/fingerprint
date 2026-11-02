using System;
using System.ComponentModel;
using System.Windows.Forms;
using FingerprintAttendance.Device;
using FingerprintAttendance.Domain;

namespace FingerprintAttendance.UI
{
    internal sealed partial class MachinesPage
    {
        private Button _probeButton;
        private bool _probeRunning;
        private BackgroundWorker _probeWorker;

        private void AddProbeAction(FlowLayoutPanel toolbar)
        {
            _probeButton = Theme.MakeButton("Uji koneksi", ButtonKind.Secondary);
            _probeButton.Width = 116;
            _probeButton.Enabled = false;
            _probeButton.Click += delegate { StartConnectionProbe(); };
            toolbar.Controls.Add(_probeButton);
        }

        private void UpdateProbeActionState()
        {
            Machine selected = SelectedMachine();
            bool available = selected != null && selected.IsEnabled && !_probeRunning;
            if (_probeButton != null)
            {
                _probeButton.Enabled = available;
            }
            if (_downloadButton != null)
            {
                _downloadButton.Enabled = available;
            }
        }

        private void StartConnectionProbe()
        {
            Machine selected = SelectedMachine();
            if (selected == null || !selected.IsEnabled || _probeRunning)
            {
                return;
            }

            int rowIndex = _grid.SelectedRows[0].Index;
            _probeRunning = true;
            _grid.Enabled = false;
            UpdateActions();
            _edit.Enabled = false;
            _delete.Enabled = false;
            _grid.Rows[rowIndex].Cells["state"].Value = "Menghubungkan…";

            _probeWorker = new BackgroundWorker();
            _probeWorker.DoWork += delegate(object sender, DoWorkEventArgs e)
            {
                e.Result = DeviceConnectionProbe.Run((Machine)e.Argument);
            };
            _probeWorker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                _probeRunning = false;
                if (_probeWorker != null)
                {
                    _probeWorker.Dispose();
                    _probeWorker = null;
                }
                if (IsDisposed || !Visible || _grid.IsDisposed)
                {
                    return;
                }

                _grid.Enabled = true;
                UpdateActions();

                if (e.Error != null)
                {
                    _grid.Rows[rowIndex].Cells["state"].Value = "Gagal";
                    MessageBox.Show(this,
                        e.Error.GetBaseException().Message,
                        "Uji koneksi gagal",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                DeviceConnectionProbeResult result = (DeviceConnectionProbeResult)e.Result;
                if (!result.Connected)
                {
                    _grid.Rows[rowIndex].Cells["state"].Value = "Tidak terhubung";
                    MessageBox.Show(this,
                        result.ErrorMessage,
                        "Uji koneksi gagal",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                _grid.Rows[rowIndex].Cells["state"].Value = "Terhubung";
                string details = "Koneksi ke mesin berhasil.";
                string serialSaveError = String.Empty;
                if (!String.IsNullOrWhiteSpace(result.SerialNumber))
                {
                    try
                    {
                        _database.UpdateMachineSerialNumber(selected.Id, result.SerialNumber);
                        selected.SerialNumber = result.SerialNumber;
                        _grid.Rows[rowIndex].Cells["serial"].Value = result.SerialNumber;
                        details += "\r\nNomor seri: " + result.SerialNumber;
                    }
                    catch (Exception ex)
                    {
                        serialSaveError = ex.Message;
                    }
                }
                if (!String.IsNullOrWhiteSpace(result.FirmwareVersion))
                {
                    details += "\r\nFirmware: " + result.FirmwareVersion;
                }
                if (!String.IsNullOrEmpty(serialSaveError))
                {
                    details += "\r\nNomor seri terbaca tetapi belum tersimpan di aplikasi: " + serialSaveError;
                }
                MessageBox.Show(this, details, "Mesin terhubung", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            _probeWorker.RunWorkerAsync(selected);
        }
    }
}