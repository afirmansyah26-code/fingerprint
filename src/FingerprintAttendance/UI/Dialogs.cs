using System;
using System.Drawing;
using System.Windows.Forms;
using FingerprintAttendance.Domain;

namespace FingerprintAttendance.UI
{
    internal sealed class EmployeeDialog : Form
    {
        private readonly TextBox _code;
        private readonly TextBox _name;
        private readonly TextBox _department;
        private readonly TextBox _position;
        private readonly CheckBox _active;

        public Employee EmployeeValue { get; private set; }

        public EmployeeDialog(Employee employee)
        {
            bool editing = employee != null;
            Text = editing ? "Edit karyawan" : "Tambah karyawan";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(530, 520);
            BackColor = Theme.Ground;
            Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);

            Label title = Theme.MakeLabel(editing ? "Edit karyawan" : "Tambah karyawan",
                17f, FontStyle.Bold, Theme.Ink);
            title.Location = new Point(26, 22);
            Controls.Add(title);
            Label subtitle = Theme.MakeLabel("Data ini disimpan di aplikasi.", 9.5f, FontStyle.Regular, Theme.Muted);
            subtitle.Location = new Point(27, 56);
            Controls.Add(subtitle);

            _code = AddField("Nomor karyawan / NIP", 94);
            _name = AddField("Nama lengkap", 168);
            _department = AddField("Departemen", 242);
            _position = AddField("Jabatan", 316);

            _active = new CheckBox
            {
                Text = "Karyawan aktif",
                Checked = employee == null || employee.IsActive,
                AutoSize = true,
                Location = new Point(27, 400),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Theme.Ink,
                AccessibleName = "Status karyawan aktif"
            };
            Controls.Add(_active);

            if (employee != null)
            {
                _code.Text = employee.Code;
                _name.Text = employee.FullName;
                _department.Text = employee.Department;
                _position.Text = employee.Position;
            }

            Panel footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 66,
                BackColor = Theme.Ground,
                Padding = new Padding(20, 10, 20, 10)
            };
            Controls.Add(footer);
            Panel rule = Theme.MakeRule();
            rule.Dock = DockStyle.Top;
            footer.Controls.Add(rule);

            Button cancel = Theme.MakeButton("Batal", ButtonKind.Secondary);
            cancel.Text = "Batal";
            cancel.Width = 92;
            cancel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            cancel.Location = new Point(286, 18);
            cancel.DialogResult = DialogResult.Cancel;
            footer.Controls.Add(cancel);

            Button save = Theme.MakeButton("Simpan", ButtonKind.Primary);
            save.Width = 104;
            save.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            save.Location = new Point(386, 18);
            save.Click += delegate { SaveEmployee(); };
            footer.Controls.Add(save);

            AcceptButton = save;
            CancelButton = cancel;
        }

        private TextBox AddField(string label, int top)
        {
            Label caption = Theme.MakeLabel(label, 9f, FontStyle.Regular, Theme.Muted);
            caption.Location = new Point(27, top);
            Controls.Add(caption);

            TextBox input = new TextBox
            {
                Location = new Point(26, top + 21),
                Width = 476,
                Height = 32,
                Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point),
                BorderStyle = BorderStyle.FixedSingle,
                AccessibleName = label
            };
            Controls.Add(input);
            return input;
        }

        private void SaveEmployee()
        {
            if (String.IsNullOrWhiteSpace(_code.Text))
            {
                MessageBox.Show(this, "Nomor karyawan wajib diisi.", "Periksa data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _code.Focus();
                return;
            }
            if (String.IsNullOrWhiteSpace(_name.Text))
            {
                MessageBox.Show(this, "Nama karyawan wajib diisi.", "Periksa data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _name.Focus();
                return;
            }

            EmployeeValue = new Employee
            {
                Code = _code.Text.Trim(),
                FullName = _name.Text.Trim(),
                Department = _department.Text.Trim(),
                Position = _position.Text.Trim(),
                IsActive = _active.Checked
            };
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    internal sealed class MachineDialog : Form
    {
        private readonly TextBox _name;
        private readonly TextBox _host;
        private readonly NumericUpDown _port;
        private readonly TextBox _serial;
        private readonly CheckBox _enabled;

        public Machine MachineValue { get; private set; }

        public MachineDialog(Machine machine)
        {
            bool editing = machine != null;
            Text = editing ? "Edit konfigurasi mesin" : "Tambah mesin fingerprint";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(530, 535);
            BackColor = Theme.Ground;
            Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);

            Label title = Theme.MakeLabel(editing ? "Edit mesin" : "Tambah mesin",
                17f, FontStyle.Bold, Theme.Ink);
            title.Location = new Point(26, 22);
            Controls.Add(title);
            Label subtitle = Theme.MakeLabel(
                "Konfigurasi hanya disimpan di aplikasi; belum ada perintah ke mesin.",
                9.5f, FontStyle.Regular, Theme.Muted);
            subtitle.Location = new Point(27, 56);
            Controls.Add(subtitle);

            _name = AddTextField("Nama mesin", 94, 476);
            _host = AddTextField("Alamat IP atau hostname", 168, 476);

            Label portLabel = Theme.MakeLabel("Port", 9f, FontStyle.Regular, Theme.Muted);
            portLabel.Location = new Point(27, 242);
            Controls.Add(portLabel);
            _port = new NumericUpDown
            {
                Location = new Point(26, 263),
                Width = 155,
                Height = 32,
                Minimum = 1,
                Maximum = 65535,
                Value = 4370,
                Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point),
                AccessibleName = "Port mesin"
            };
            Controls.Add(_port);

            _serial = AddTextField("Nomor seri (opsional)", 316, 476);
            _enabled = new CheckBox
            {
                Text = "Mesin aktif di aplikasi",
                Checked = machine == null || machine.IsEnabled,
                AutoSize = true,
                Location = new Point(27, 390),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Theme.Ink
            };
            Controls.Add(_enabled);

            if (machine != null)
            {
                _name.Text = machine.Name;
                _host.Text = machine.Host;
                _port.Value = Math.Max(_port.Minimum, Math.Min(_port.Maximum, machine.Port));
                _serial.Text = machine.SerialNumber;
                _enabled.Checked = machine.IsEnabled;
            }

            Panel footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 66,
                BackColor = Theme.Ground,
                Padding = new Padding(20, 10, 20, 10)
            };
            Controls.Add(footer);
            Panel rule = Theme.MakeRule();
            rule.Dock = DockStyle.Top;
            footer.Controls.Add(rule);

            Button cancel = Theme.MakeButton("Batal", ButtonKind.Secondary);
            cancel.Width = 92;
            cancel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            cancel.Location = new Point(286, 18);
            cancel.DialogResult = DialogResult.Cancel;
            footer.Controls.Add(cancel);

            Button save = Theme.MakeButton("Simpan", ButtonKind.Primary);
            save.Width = 104;
            save.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            save.Location = new Point(386, 18);
            save.Click += delegate { SaveMachine(); };
            footer.Controls.Add(save);

            AcceptButton = save;
            CancelButton = cancel;
        }

        private TextBox AddTextField(string label, int top, int width)
        {
            Label caption = Theme.MakeLabel(label, 9f, FontStyle.Regular, Theme.Muted);
            caption.Location = new Point(27, top);
            Controls.Add(caption);

            TextBox input = new TextBox
            {
                Location = new Point(26, top + 21),
                Width = width,
                Height = 32,
                Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point),
                BorderStyle = BorderStyle.FixedSingle,
                AccessibleName = label
            };
            Controls.Add(input);
            return input;
        }

        private void SaveMachine()
        {
            string host = _host.Text.Trim();
            UriHostNameType hostType = Uri.CheckHostName(host);
            if (String.IsNullOrWhiteSpace(_name.Text))
            {
                MessageBox.Show(this, "Nama mesin wajib diisi.", "Periksa data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _name.Focus();
                return;
            }
            if (hostType == UriHostNameType.Unknown)
            {
                MessageBox.Show(this, "Masukkan alamat IP atau hostname yang valid.", "Periksa alamat mesin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _host.Focus();
                return;
            }

            MachineValue = new Machine
            {
                Name = _name.Text.Trim(),
                Host = host,
                Port = Decimal.ToInt32(_port.Value),
                SerialNumber = _serial.Text.Trim(),
                IsEnabled = _enabled.Checked
            };
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    internal static class ConfirmDialog
    {
        public static bool Show(IWin32Window owner, string title, string message, string confirmText, bool destructive)
        {
            using (Form dialog = new Form())
            {
                dialog.Text = title;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.ClientSize = new Size(520, 245);
                dialog.BackColor = Theme.Ground;
                dialog.Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);

                Label heading = Theme.MakeLabel(title, 15f, FontStyle.Bold, Theme.Ink);
                heading.Location = new Point(24, 20);
                dialog.Controls.Add(heading);

                Label body = Theme.MakeLabel(message, 9.5f, FontStyle.Regular, Theme.Muted);
                body.AutoSize = false;
                body.Location = new Point(25, 60);
                body.Size = new Size(470, 104);
                body.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                dialog.Controls.Add(body);

                Panel footer = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 64,
                    Padding = new Padding(18, 8, 18, 8),
                    BackColor = Theme.Ground
                };
                dialog.Controls.Add(footer);
                Panel rule = Theme.MakeRule();
                rule.Dock = DockStyle.Top;
                footer.Controls.Add(rule);

                Button cancel = Theme.MakeButton("Kembali", ButtonKind.Secondary);
                cancel.Width = 100;
                cancel.Location = new Point(286, 16);
                cancel.DialogResult = DialogResult.Cancel;
                footer.Controls.Add(cancel);

                Button confirm = Theme.MakeButton(confirmText, destructive ? ButtonKind.Danger : ButtonKind.Primary);
                confirm.Width = Math.Max(130, TextRenderer.MeasureText(confirmText, confirm.Font).Width + 28);
                confirm.Location = new Point(392, 16);
                confirm.DialogResult = DialogResult.Yes;
                footer.Controls.Add(confirm);

                dialog.AcceptButton = confirm;
                dialog.CancelButton = cancel;
                return dialog.ShowDialog(owner) == DialogResult.Yes;
            }
        }
    }
}
