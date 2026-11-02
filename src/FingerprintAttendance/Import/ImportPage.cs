using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FingerprintAttendance.Data;
using FingerprintAttendance.Domain;
using FingerprintAttendance.UI;

namespace FingerprintAttendance.Import
{
    internal sealed class ImportPage : AppPageControl
    {
        private readonly AppDatabase _database;
        private readonly Action _onImported;

        public ImportPage(AppDatabase database, Action onImported)
            : base("Impor data", "Pilih sumber data lama atau gunakan template untuk memasukkan karyawan.")
        {
            _database = database;
            _onImported = onImported;

            TableLayoutPanel content = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Theme.Ground,
                Margin = new Padding(0)
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 174));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 192));
            content.Height = 384;
            content.Width = Math.Max(400, Body.ClientSize.Width);
            content.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Body.Controls.Add(content);
            Body.Resize += delegate { content.Width = Math.Max(400, Body.ClientSize.Width); };

            Panel templatePanel = MakePanel();
            BuildTemplateSection(templatePanel);
            content.Controls.Add(templatePanel, 0, 0);

            Panel gap = new Panel { BackColor = Theme.Ground, Dock = DockStyle.Fill };
            content.Controls.Add(gap, 0, 1);

            Panel mdbPanel = MakePanel();
            BuildMdbSection(mdbPanel);
            content.Controls.Add(mdbPanel, 0, 2);
        }

        private Panel MakePanel()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Surface,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(22, 18, 22, 18)
            };
        }

        private void BuildTemplateSection(Panel panel)
        {
            Label title = Theme.MakeLabel("Template karyawan", 13f, FontStyle.Bold, Theme.Ink);
            title.Location = new Point(20, 18);
            panel.Controls.Add(title);

            Label detail = Theme.MakeLabel(
                "Gunakan CSV UTF-8 dengan pemisah titik koma. Template dapat dibuka dan diedit di Excel.",
                9.5f, FontStyle.Regular, Theme.Muted);
            detail.Location = new Point(21, 53);
            detail.MaximumSize = new Size(680, 44);
            panel.Controls.Add(detail);

            Label fields = Theme.MakeLabel(
                "Kolom wajib: employee_code dan full_name. Kolom opsional: department, position, is_active.",
                9f, FontStyle.Regular, Theme.Muted);
            fields.Location = new Point(21, 82);
            fields.MaximumSize = new Size(680, 40);
            panel.Controls.Add(fields);

            Button download = Theme.MakeButton("Unduh template", ButtonKind.Secondary);
            download.Width = 144;
            download.Location = new Point(21, 120);
            download.Click += delegate { DownloadTemplate(); };
            panel.Controls.Add(download);

            Button import = Theme.MakeButton("Impor CSV", ButtonKind.Primary);
            import.Width = 112;
            import.Location = new Point(174, 120);
            import.Click += delegate { ImportCsv(); };
            panel.Controls.Add(import);
        }

        private void BuildMdbSection(Panel panel)
        {
            Label title = Theme.MakeLabel("Database Att", 13f, FontStyle.Bold, Theme.Ink);
            title.Location = new Point(20, 18);
            panel.Controls.Add(title);

            Label state = Theme.MakeLabel("Impor .mdb dalam pengerjaan", 9f, FontStyle.Bold, Theme.Muted);
            state.Location = new Point(21, 52);
            panel.Controls.Add(state);

            Label detail = Theme.MakeLabel(
                "att2000.mdb akan dibaca secara read-only. Pemetaan karyawan dan riwayat absensi akan ditampilkan sebelum dipindahkan.",
                9.5f, FontStyle.Regular, Theme.Muted);
            detail.Location = new Point(21, 80);
            detail.MaximumSize = new Size(700, 48);
            panel.Controls.Add(detail);

            Label source = Theme.MakeLabel(
                "Jangan memilih database kerja Att yang sedang aktif. Gunakan salinan berkas sebagai sumber impor.",
                9f, FontStyle.Regular, Theme.Muted);
            source.Location = new Point(21, 128);
            source.MaximumSize = new Size(700, 38);
            panel.Controls.Add(source);
        }

        private void DownloadTemplate()
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "Simpan template karyawan";
                dialog.Filter = "CSV UTF-8 (*.csv)|*.csv";
                dialog.FileName = "template-karyawan.csv";
                dialog.OverwritePrompt = true;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    EmployeeCsvImporter.WriteTemplate(dialog.FileName);
                    MessageBox.Show(this,
                        "Template karyawan sudah disimpan.",
                        "Template siap digunakan",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Template tidak dapat disimpan",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void ImportCsv()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Pilih file karyawan";
                dialog.Filter = "CSV (*.csv)|*.csv";
                dialog.CheckFileExists = true;
                dialog.Multiselect = false;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                EmployeeImportResult result = EmployeeCsvImporter.Read(dialog.FileName, _database.GetEmployeeCodes());
                if (result.FileErrors.Count > 0)
                {
                    MessageBox.Show(this,
                        String.Join(Environment.NewLine, result.FileErrors.ToArray()),
                        "File belum dapat diimpor",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                using (ImportReviewDialog review = new ImportReviewDialog(result))
                {
                    if (review.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }
                }

                List<Employee> employees = new List<Employee>();
                foreach (EmployeeImportRow row in result.Rows)
                {
                    employees.Add(row.Employee);
                }

                try
                {
                    _database.ImportEmployees(employees);
                    MessageBox.Show(this,
                        employees.Count.ToString() + " karyawan berhasil diimpor.",
                        "Impor selesai",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    _onImported();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Impor tidak selesai",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }

    internal sealed class ImportReviewDialog : Form
    {
        public ImportReviewDialog(EmployeeImportResult result)
        {
            Text = "Pratinjau impor karyawan";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize = new Size(820, 480);
            Size = new Size(1000, 590);
            ShowInTaskbar = false;
            BackColor = Theme.Ground;
            Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(22, 20, 22, 16),
                BackColor = Theme.Ground
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            Controls.Add(layout);

            Panel header = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Ground };
            Label title = Theme.MakeLabel("Periksa data sebelum disimpan", 15f, FontStyle.Bold, Theme.Ink);
            title.Location = new Point(0, 0);
            header.Controls.Add(title);
            Label summary = Theme.MakeLabel(
                result.ValidCount.ToString() + " siap diimpor · " + result.ErrorCount.ToString() + " baris bermasalah",
                9f, FontStyle.Regular, Theme.Muted);
            summary.Location = new Point(1, 31);
            header.Controls.Add(summary);
            layout.Controls.Add(header, 0, 0);

            DataGridView grid = new DataGridView { Dock = DockStyle.Fill };
            AppPageGridStyle.Apply(grid);
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Baris", FillWeight = 7, MinimumWidth = 54 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nomor karyawan", FillWeight = 15, MinimumWidth = 110 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nama", FillWeight = 22, MinimumWidth = 145 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Departemen", FillWeight = 16, MinimumWidth = 100 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Jabatan", FillWeight = 15, MinimumWidth = 95 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", FillWeight = 10, MinimumWidth = 80 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Hasil pemeriksaan", FillWeight = 25, MinimumWidth = 170 });
            foreach (EmployeeImportRow row in result.Rows)
            {
                Employee employee = row.Employee;
                grid.Rows.Add(
                    row.RowNumber,
                    employee == null ? "" : employee.Code,
                    employee == null ? "" : employee.FullName,
                    employee == null ? "" : employee.Department,
                    employee == null ? "" : employee.Position,
                    employee == null ? "" : employee.IsActive ? "Aktif" : "Nonaktif",
                    row.Error);
            }
            layout.Controls.Add(grid, 0, 1);

            FlowLayoutPanel footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Theme.Ground,
                Padding = new Padding(0, 12, 0, 0)
            };
            Button cancel = Theme.MakeButton("Batal", ButtonKind.Secondary);
            cancel.Width = 92;
            cancel.DialogResult = DialogResult.Cancel;
            footer.Controls.Add(cancel);

            Button accept = Theme.MakeButton(
                "Impor " + result.ValidCount.ToString() + " karyawan",
                ButtonKind.Primary);
            accept.Width = 166;
            accept.Enabled = result.ErrorCount == 0 && result.ValidCount > 0;
            accept.DialogResult = DialogResult.OK;
            footer.Controls.Add(accept);
            layout.Controls.Add(footer, 0, 2);

            AcceptButton = accept;
            CancelButton = cancel;
        }
    }

    internal static class AppPageGridStyle
    {
        public static void Apply(DataGridView grid)
        {
            grid.BackgroundColor = Theme.Surface;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.EnableHeadersVisualStyles = false;
            grid.GridColor = Theme.Rule;
            grid.RowHeadersVisible = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.MultiSelect = false;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ColumnHeadersHeight = 38;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.RowTemplate.Height = 38;
            grid.Font = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Theme.Surface;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Theme.Muted;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular, GraphicsUnit.Point);
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Theme.Surface;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Theme.Muted;
            grid.DefaultCellStyle.BackColor = Theme.Surface;
            grid.DefaultCellStyle.ForeColor = Theme.Ink;
            grid.DefaultCellStyle.SelectionBackColor = Theme.Active;
            grid.DefaultCellStyle.SelectionForeColor = Theme.Ink;
            grid.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
        }
    }
}
