using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FingerprintAttendance.Data;
using FingerprintAttendance.Domain;

namespace FingerprintAttendance.UI
{
    internal sealed class OverviewPage : AppPageControl
    {
        public OverviewPage(AppDatabase database, Action openMachines, Action openImport)
            : base("Ringkasan", "Data yang tersimpan di komputer ini.")
        {
            TableLayoutPanel content = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = Theme.Ground,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 102));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 190));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 136));
            content.Height = 464;
            content.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            content.Width = Math.Max(400, Body.ClientSize.Width);
            Body.Controls.Add(content);
            Body.Resize += delegate { content.Width = Math.Max(400, Body.ClientSize.Width); };

            Panel metrics = CreateOutlinedPanel();
            metrics.Dock = DockStyle.Fill;
            TableLayoutPanel metricLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Theme.Surface,
                Padding = new Padding(18, 12, 18, 10)
            };
            for (int i = 0; i < 3; i++)
            {
                metricLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
            }
            metricLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            metricLayout.Controls.Add(MakeMetric("Karyawan aktif", database.CountEmployees(true)), 0, 0);
            metricLayout.Controls.Add(MakeMetric("Mesin terdaftar", database.CountMachines()), 1, 0);
            metricLayout.Controls.Add(MakeMetric("Record lokal", database.CountAttendanceRecords()), 2, 0);
            metrics.Controls.Add(metricLayout);
            content.Controls.Add(metrics, 0, 0);

            Panel start = CreateOutlinedPanel();
            start.Dock = DockStyle.Fill;
            TableLayoutPanel startLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Theme.Surface,
                Padding = new Padding(22, 20, 22, 18)
            };
            startLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            startLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 168));
            startLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            startLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            Label startTitle = Theme.MakeLabel(
                database.CountMachines() == 0 ? "Mulai dengan mesin fingerprint" : "Mesin dikelola dari satu tempat",
                13f, FontStyle.Bold, Theme.Ink);
            startLayout.Controls.Add(startTitle, 0, 0);

            Label startDetail = Theme.MakeLabel(
                database.CountMachines() == 0
                    ? "Tambahkan mesin yang berada di LAN. Pada tahap ini aplikasi menyimpan konfigurasinya saja."
                    : "Konfigurasi mesin tersimpan lokal. Uji koneksi SDK sudah tersedia; pembacaan data dan unduh record masuk tahap berikutnya.",
                9.5f, FontStyle.Regular, Theme.Muted);
            startDetail.MaximumSize = new Size(720, 55);
            startDetail.Margin = new Padding(0, 10, 12, 0);
            startLayout.Controls.Add(startDetail, 0, 1);

            Button addMachine = Theme.MakeButton(
                database.CountMachines() == 0 ? "Tambah mesin" : "Kelola mesin",
                ButtonKind.Primary);
            addMachine.Width = 146;
            addMachine.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            addMachine.Margin = new Padding(0, 2, 0, 0);
            addMachine.Click += delegate { openMachines(); };
            startLayout.SetRowSpan(addMachine, 2);
            startLayout.Controls.Add(addMachine, 1, 0);
            start.Controls.Add(startLayout);
            content.Controls.Add(start, 0, 2);

            Panel import = CreateOutlinedPanel();
            import.Dock = DockStyle.Fill;
            TableLayoutPanel importLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Theme.Surface,
                Padding = new Padding(22, 18, 22, 16)
            };
            importLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            importLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 168));
            importLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            importLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            importLayout.Controls.Add(
                Theme.MakeLabel("Bawa data yang sudah tersedia", 12f, FontStyle.Bold, Theme.Ink), 0, 0);
            Label importDetail = Theme.MakeLabel(
                "Impor database Att atau masukkan daftar karyawan melalui template.",
                9.5f, FontStyle.Regular, Theme.Muted);
            importDetail.Margin = new Padding(0, 8, 12, 0);
            importLayout.Controls.Add(importDetail, 0, 1);
            Button importButton = Theme.MakeButton("Buka impor", ButtonKind.Secondary);
            importButton.Width = 130;
            importButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            importButton.Click += delegate { openImport(); };
            importLayout.SetRowSpan(importButton, 2);
            importLayout.Controls.Add(importButton, 1, 0);
            import.Controls.Add(importLayout);
            content.Controls.Add(import, 0, 4);
        }

        private static Control MakeMetric(string label, long value)
        {
            Panel cell = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Surface,
                Padding = new Padding(8, 8, 8, 2)
            };
            Label name = Theme.MakeLabel(label, 9f, FontStyle.Regular, Theme.Muted);
            name.Location = new Point(8, 4);
            cell.Controls.Add(name);
            Label number = Theme.MakeLabel(value.ToString("N0"), 18f, FontStyle.Regular, Theme.Ink);
            number.Location = new Point(8, 30);
            cell.Controls.Add(number);
            return cell;
        }
    }

    internal sealed class EmployeesPage : AppPageControl
    {
        private readonly AppDatabase _database;
        private readonly Action _onChanged;
        private readonly DataGridView _grid;
        private readonly Panel _emptyState;
        private readonly TextBox _search;
        private readonly Button _edit;
        private readonly Button _toggle;
        private readonly Button _delete;

        public EmployeesPage(AppDatabase database, Action onChanged)
            : base("Karyawan", "Data karyawan di aplikasi. Perubahan di sini tidak mengubah data pada mesin.")
        {
            _database = database;
            _onChanged = onChanged;

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Theme.Ground,
                Margin = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Body.Controls.Add(layout);

            FlowLayoutPanel toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Theme.Ground,
                Padding = new Padding(0, 2, 0, 8),
                Margin = new Padding(0)
            };

            Label searchLabel = Theme.MakeLabel("Cari", 9f, FontStyle.Regular, Theme.Muted);
            searchLabel.Margin = new Padding(0, 9, 8, 0);
            toolbar.Controls.Add(searchLabel);

            _search = new TextBox
            {
                Width = 235,
                Height = 32,
                Font = new Font("Segoe UI", 9.5f),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 12, 0),
                AccessibleName = "Cari kode, nama, atau departemen"
            };
            _search.TextChanged += delegate { RefreshRows(); };
            toolbar.Controls.Add(_search);

            Button add = Theme.MakeButton("Tambah karyawan", ButtonKind.Primary);
            add.Width = 154;
            add.Click += delegate { AddEmployee(); };
            toolbar.Controls.Add(add);

            _edit = Theme.MakeButton("Edit", ButtonKind.Secondary);
            _edit.Enabled = false;
            _edit.Click += delegate { EditSelected(); };
            toolbar.Controls.Add(_edit);

            _toggle = Theme.MakeButton("Nonaktifkan", ButtonKind.Secondary);
            _toggle.Enabled = false;
            _toggle.Click += delegate { ToggleSelected(); };
            toolbar.Controls.Add(_toggle);

            _delete = Theme.MakeButton("Hapus", ButtonKind.Danger);
            _delete.Enabled = false;
            _delete.Margin = new Padding(0);
            _delete.Click += delegate { DeleteSelected(); };
            toolbar.Controls.Add(_delete);

            layout.Controls.Add(toolbar, 0, 0);

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
            AddEmployeeColumns();
            gridHost.Controls.Add(_grid);

            _emptyState = MakeCenteredEmptyState(
                "Belum ada data karyawan",
                "Tambahkan karyawan atau gunakan template pada halaman Impor data.",
                null);
            gridHost.Controls.Add(_emptyState);
            layout.Controls.Add(gridHost, 0, 1);

            RefreshRows();
        }

        private void AddEmployeeColumns()
        {
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "code", HeaderText = "Nomor karyawan", FillWeight = 20, MinimumWidth = 130
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "name", HeaderText = "Nama karyawan", FillWeight = 34, MinimumWidth = 190
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "department", HeaderText = "Departemen", FillWeight = 20, MinimumWidth = 120
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "position", HeaderText = "Jabatan", FillWeight = 18, MinimumWidth = 110
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "active", HeaderText = "Status", FillWeight = 12, MinimumWidth = 95
            });
        }

        private void RefreshRows()
        {
            List<Employee> employees = _database.GetEmployees(_search.Text);
            _grid.Rows.Clear();
            foreach (Employee employee in employees)
            {
                int index = _grid.Rows.Add(
                    employee.Code,
                    employee.FullName,
                    String.IsNullOrWhiteSpace(employee.Department) ? "—" : employee.Department,
                    String.IsNullOrWhiteSpace(employee.Position) ? "—" : employee.Position,
                    employee.IsActive ? "Aktif" : "Nonaktif");
                _grid.Rows[index].Tag = employee;
            }

            bool hasRows = employees.Count > 0;
            _grid.Visible = hasRows;
            _emptyState.Visible = !hasRows;
            UpdateActions();
        }

        private Employee SelectedEmployee()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                return null;
            }
            return _grid.SelectedRows[0].Tag as Employee;
        }

        private void UpdateActions()
        {
            Employee employee = SelectedEmployee();
            _edit.Enabled = employee != null;
            _toggle.Enabled = employee != null;
            _delete.Enabled = employee != null;
            if (employee != null)
            {
                _toggle.Text = employee.IsActive ? "Nonaktifkan" : "Aktifkan";
            }
        }

        private void AddEmployee()
        {
            using (EmployeeDialog dialog = new EmployeeDialog(null))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    _database.AddEmployee(dialog.EmployeeValue);
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
            Employee selected = SelectedEmployee();
            if (selected == null)
            {
                return;
            }

            using (EmployeeDialog dialog = new EmployeeDialog(selected))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                dialog.EmployeeValue.Id = selected.Id;
                try
                {
                    _database.UpdateEmployee(dialog.EmployeeValue);
                    _onChanged();
                }
                catch (Exception ex)
                {
                    ShowSaveError(ex);
                }
            }
        }

        private void ToggleSelected()
        {
            Employee selected = SelectedEmployee();
            if (selected == null)
            {
                return;
            }
            _database.SetEmployeeActive(selected.Id, !selected.IsActive);
            _onChanged();
        }

        private void DeleteSelected()
        {
            Employee selected = SelectedEmployee();
            if (selected == null)
            {
                return;
            }

            string message = "Karyawan " + selected.FullName + " (" + selected.Code + ") akan dihapus dari aplikasi. " +
                "Riwayat absensi dan koreksi yang tersimpan di aplikasi ikut dihapus. Data pengguna, sidik jari, " +
                "dan record pada mesin fingerprint tidak diubah.";
            if (ConfirmDialog.Show(this, "Hapus karyawan dari aplikasi?", message, "Hapus dari aplikasi", true))
            {
                _database.DeleteEmployee(selected.Id);
                _onChanged();
            }
        }

        private void ShowSaveError(Exception ex)
        {
            string message = ex.Message;
            if (message.IndexOf("UNIQUE", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                message = "Nomor karyawan tersebut sudah digunakan. Periksa kembali nomor yang dimasukkan.";
            }
            MessageBox.Show(this, message, "Data belum tersimpan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}

