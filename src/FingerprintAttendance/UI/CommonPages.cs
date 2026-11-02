using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FingerprintAttendance.Data;
using FingerprintAttendance.Domain;
using FingerprintAttendance.Device;

namespace FingerprintAttendance.UI
{
    internal sealed partial class MachinesPage : AppPageControl
    {
        private readonly AppDatabase _database;
        private readonly Action _onChanged;
        private readonly DataGridView _grid;
        private readonly Panel _emptyState;
        private readonly Button _edit;
        private readonly Button _delete;

        public MachinesPage(AppDatabase database, Action onChanged)
            : base("Mesin fingerprint", "Satu komputer dapat menyimpan konfigurasi beberapa mesin di jaringan lokal.")
        {
            _database = database;
            _onChanged = onChanged;

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Theme.Ground,
                Margin = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Body.Controls.Add(layout);

            FlowLayoutPanel toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Theme.Ground,
                Padding = new Padding(0, 0, 0, 8)
            };
            Button add = Theme.MakeButton("Tambah mesin", ButtonKind.Primary);
            add.Width = 130;
            add.Click += delegate { AddMachine(); };
            toolbar.Controls.Add(add);
            _edit = Theme.MakeButton("Edit konfigurasi", ButtonKind.Secondary);
            _edit.Width = 142;
            _edit.Enabled = false;
            _edit.Click += delegate { EditSelected(); };
            toolbar.Controls.Add(_edit);
            _delete = Theme.MakeButton("Hapus dari aplikasi", ButtonKind.Danger);
            _delete.Width = 162;
            _delete.Enabled = false;
            _delete.Margin = new Padding(0);
            _delete.Click += delegate { DeleteSelected(); };
            toolbar.Controls.Add(_delete);
            AddProbeAction(toolbar);
            AddDownloadAction(toolbar);
            layout.Controls.Add(toolbar, 0, 0);

            Label note = Theme.MakeLabel(
                "Status koneksi belum diuji. Menyimpan atau menghapus konfigurasi di sini tidak mengubah mesin.",
                9f, FontStyle.Regular, Theme.Muted);
            note.Dock = DockStyle.Fill;
            note.TextAlign = ContentAlignment.MiddleLeft;
            note.Margin = new Padding(0, 0, 0, 7);
            layout.Controls.Add(note, 0, 1);

            Panel gridHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Surface,
                BorderStyle = BorderStyle.FixedSingle
            };
            _grid = new DataGridView { Dock = DockStyle.Fill };
            StyleGrid(_grid);
            _grid.SelectionChanged += delegate { UpdateActions(); };
            _grid.CellDoubleClick += delegate { EditSelected(); };
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "name", HeaderText = "Nama mesin", FillWeight = 22, MinimumWidth = 145
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "host", HeaderText = "Alamat LAN", FillWeight = 23, MinimumWidth = 155
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "port", HeaderText = "Port", FillWeight = 9, MinimumWidth = 80
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "serial", HeaderText = "Nomor seri", FillWeight = 20, MinimumWidth = 140
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "state", HeaderText = "Status koneksi", FillWeight = 16, MinimumWidth = 120
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "enabled", HeaderText = "Aktif di aplikasi", FillWeight = 12, MinimumWidth = 110
            });
            gridHost.Controls.Add(_grid);

            Button emptyAction = Theme.MakeButton("Tambah mesin", ButtonKind.Primary);
            emptyAction.Width = 130;
            emptyAction.Click += delegate { AddMachine(); };
            _emptyState = MakeCenteredEmptyState(
                "Belum ada mesin terdaftar",
                "Tambahkan alamat IP mesin fingerprint di jaringan lokal. Setelah terdaftar, uji koneksi SDK tersedia dari toolbar.",
                emptyAction);
            gridHost.Controls.Add(_emptyState);
            layout.Controls.Add(gridHost, 0, 2);

            RefreshRows();
        }

        private void RefreshRows()
        {
            List<Machine> machines = _database.GetMachines();
            _grid.Rows.Clear();
            foreach (Machine machine in machines)
            {
                int index = _grid.Rows.Add(
                    machine.Name,
                    machine.Host,
                    machine.Port.ToString(),
                    String.IsNullOrWhiteSpace(machine.SerialNumber) ? "—" : machine.SerialNumber,
                    "Belum diuji",
                    machine.IsEnabled ? "Ya" : "Tidak");
                _grid.Rows[index].Tag = machine;
            }

            bool hasRows = machines.Count > 0;
            _grid.Visible = hasRows;
            _emptyState.Visible = !hasRows;
            UpdateActions();
        }

        private Machine SelectedMachine()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                return null;
            }
            return _grid.SelectedRows[0].Tag as Machine;
        }

        private void UpdateActions()
        {
            bool selected = SelectedMachine() != null;
            _edit.Enabled = selected;
            _delete.Enabled = selected;
            UpdateProbeActionState();
        }

        private void AddMachine()
        {
            using (MachineDialog dialog = new MachineDialog(null))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    _database.AddMachine(dialog.MachineValue);
                    _onChanged();
                }
                catch (Exception ex)
                {
                    ShowSaveError(ex);
                }
            }
        }

        private void EditSelected()
        {
            Machine selected = SelectedMachine();
            if (selected == null)
            {
                return;
            }
            using (MachineDialog dialog = new MachineDialog(selected))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                dialog.MachineValue.Id = selected.Id;
                try
                {
                    _database.UpdateMachine(dialog.MachineValue);
                    _onChanged();
                }
                catch (Exception ex)
                {
                    ShowSaveError(ex);
                }
            }
        }

        private void DeleteSelected()
        {
            Machine selected = SelectedMachine();
            if (selected == null)
            {
                return;
            }

            string message = "Konfigurasi " + selected.Name + " (" + selected.Host + ":" + selected.Port +
                ") akan dihapus dari aplikasi. Tidak ada perintah yang dikirim ke mesin. Record lokal yang sudah diunduh tetap dipertahankan.";
            if (ConfirmDialog.Show(this, "Hapus konfigurasi mesin?", message, "Hapus konfigurasi", true))
            {
                _database.DeleteMachine(selected.Id);
                _onChanged();
            }
        }

        private void ShowSaveError(Exception ex)
        {
            string message = ex.Message;
            if (message.IndexOf("UNIQUE", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                message = "Mesin dengan alamat dan port tersebut sudah terdaftar.";
            }
            MessageBox.Show(this, message, "Konfigurasi belum tersimpan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    internal sealed class AttendancePage : AppPageControl
    {
        private readonly AppDatabase _database;
        private readonly Action _openMachines;
        private readonly DataGridView _grid;
        private readonly Panel _emptyState;
        private readonly Label _summary;

        public AttendancePage(AppDatabase database, Action openMachines)
            : base("Record absensi", "Record asli mesin yang sudah diunduh dan disimpan pada komputer ini.")
        {
            _database = database;
            _openMachines = openMachines;

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Theme.Ground,
                Margin = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Body.Controls.Add(layout);

            FlowLayoutPanel toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Theme.Ground,
                Padding = new Padding(0, 1, 0, 7),
                Margin = new Padding(0)
            };
            Button machines = Theme.MakeButton("Unduh dari mesin", ButtonKind.Primary);
            machines.Width = 148;
            machines.Click += delegate { _openMachines(); };
            toolbar.Controls.Add(machines);
            _summary = Theme.MakeLabel("Total lokal: 0", 9f, FontStyle.Regular, Theme.Muted);
            _summary.Margin = new Padding(8, 10, 0, 0);
            toolbar.Controls.Add(_summary);
            layout.Controls.Add(toolbar, 0, 0);

            Panel gridHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Surface,
                BorderStyle = BorderStyle.FixedSingle
            };
            _grid = new DataGridView { Dock = DockStyle.Fill };
            StyleGrid(_grid);
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "time", HeaderText = "Waktu", FillWeight = 22, MinimumWidth = 155
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "machine", HeaderText = "Mesin", FillWeight = 19, MinimumWidth = 135
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "user", HeaderText = "User ID mesin", FillWeight = 17, MinimumWidth = 125
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "verify", HeaderText = "Verifikasi", FillWeight = 14, MinimumWidth = 100
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "direction", HeaderText = "Mode", FillWeight = 12, MinimumWidth = 90
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "work", HeaderText = "Kode kerja", FillWeight = 12, MinimumWidth = 95
            });
            gridHost.Controls.Add(_grid);

            Button emptyAction = Theme.MakeButton("Pilih mesin", ButtonKind.Primary);
            emptyAction.Width = 120;
            emptyAction.Click += delegate { _openMachines(); };
            _emptyState = MakeCenteredEmptyState(
                "Belum ada record lokal",
                "Buka halaman mesin fingerprint dan pilih Unduh record. Proses ini hanya membaca; log tetap tersimpan pada mesin.",
                emptyAction);
            gridHost.Controls.Add(_emptyState);
            layout.Controls.Add(gridHost, 0, 1);

            RefreshRows();
        }

        private void RefreshRows()
        {
            List<AttendanceRecord> records = _database.GetAttendanceRecords(1000);
            long total = _database.CountAttendanceRecords();
            _grid.Rows.Clear();
            foreach (AttendanceRecord record in records)
            {
                _grid.Rows.Add(
                    record.RecordedAt.ToString("dd MMM yyyy HH:mm:ss",
                        System.Globalization.CultureInfo.GetCultureInfo("id-ID")),
                    String.IsNullOrWhiteSpace(record.MachineName) ? "—" : record.MachineName,
                    String.IsNullOrWhiteSpace(record.DeviceUserId) ? "—" : record.DeviceUserId,
                    record.VerifyMode.ToString(),
                    record.InOutMode.ToString(),
                    record.WorkCode.ToString());
            }

            _summary.Text = records.Count < total
                ? "Menampilkan " + records.Count.ToString("N0") + " dari " + total.ToString("N0") + " record"
                : "Total lokal: " + total.ToString("N0");
            _grid.Visible = records.Count > 0;
            _emptyState.Visible = records.Count == 0;
        }
    }

    internal sealed class PlaceholderPage : AppPageControl
    {
        public PlaceholderPage(string title, string description)
            : base(title, description)
        {
            Panel panel = CreateOutlinedPanel();
            panel.Dock = DockStyle.Top;
            panel.Height = 178;
            panel.Padding = new Padding(22);
            Body.Controls.Add(panel);

            Label heading = Theme.MakeLabel("Belum tersedia pada tahap ini", 13f, FontStyle.Bold, Theme.Ink);
            heading.Location = new Point(20, 20);
            panel.Controls.Add(heading);

            Label detail = Theme.MakeLabel(
                "Bagian ini sudah masuk rencana dan akan diimplementasikan setelah fondasi data serta koneksi mesin.",
                9.5f, FontStyle.Regular, Theme.Muted);
            detail.Location = new Point(20, 57);
            detail.MaximumSize = new Size(720, 44);
            panel.Controls.Add(detail);
        }
    }
}

