using System;
using System.ComponentModel;
using System.Windows.Forms;
using FingerprintAttendance.Device;
using FingerprintAttendance.Domain;

namespace FingerprintAttendance.UI
{
    internal sealed partial class MachinesPage
    {
        private Button _downloadButton;
        private BackgroundWorker _downloadWorker;

        private sealed class DownloadSummary
        {
            public int ReadCount;
            public int StoredCount;
            public int InvalidCount;
        }

        private void AddDownloadAction(FlowLayoutPanel toolbar)
        {
            _downloadButton = Theme.MakeButton("Unduh record", ButtonKind.Secondary);
            _downloadButton.Width = 126;
            _downloadButton.Enabled = false;
            _downloadButton.Click += delegate { StartDownloadRecords(); };
            toolbar.Controls.Add(_downloadButton);
        }

        private void StartDownloadRecords()
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
            _grid.Rows[rowIndex].Cells["state"].Value = "Mengunduh…";

            _downloadWorker = new BackgroundWorker();
            _downloadWorker.DoWork += delegate(object sender, DoWorkEventArgs e)
            {
                Machine machine = (Machine)e.Argument;
                DeviceAttendanceReadResult read = DeviceDataReader.ReadAttendance(machine);
                int stored = _database.SaveAttendanceRecords(machine, read.Records);
                e.Result = new DownloadSummary
                {
                    ReadCount = read.Records.Count,
                    StoredCount = stored,
                    InvalidCount = read.InvalidRecordsSkipped
                };
            };
            _downloadWorker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                _probeRunning = false;
                if (_downloadWorker != null)
                {
                    _downloadWorker.Dispose();
                    _downloadWorker = null;
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
                        "Unduh record gagal",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                DownloadSummary summary = (DownloadSummary)e.Result;
                _grid.Rows[rowIndex].Cells["state"].Value = "Terhubung";
                string message = summary.ReadCount.ToString("N0") +
                    " record dibaca; " + summary.StoredCount.ToString("N0") +
                    " record baru disimpan. Data di mesin tetap utuh.";
                if (summary.InvalidCount > 0)
                {
                    message += "\r\n" + summary.InvalidCount.ToString("N0") +
                        " data tidak valid dilewati.";
                }
                MessageBox.Show(this, message, "Unduh record selesai",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                _onChanged();
            };
            _downloadWorker.RunWorkerAsync(selected);
        }
    }
}