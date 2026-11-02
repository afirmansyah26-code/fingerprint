using System.Drawing;
using System.Windows.Forms;
using FingerprintAttendance.Data;
using FingerprintAttendance.Import;

namespace FingerprintAttendance.UI
{
    public enum AppPage
    {
        Overview,
        Employees,
        Machines,
        Attendance,
        Rules,
        Reports,
        Import
    }

    public sealed class MainForm : Form
    {
        private readonly AppDatabase _database;
        private readonly Panel _pageHost;
        private readonly Button[] _navigationButtons;
        private AppPage _currentPage;

        public MainForm(AppDatabase database)
        {
            _database = database;
            _navigationButtons = new Button[7];

            Text = "Absensi";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1120, 700);
            Size = new Size(1380, 860);
            Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);
            BackColor = Theme.Ground;
            ForeColor = Theme.Ink;
            AutoScaleMode = AutoScaleMode.Dpi;
            KeyPreview = true;

            TableLayoutPanel frame = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Theme.Ground,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            frame.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 232));
            frame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            frame.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(frame);

            Panel sidebar = BuildSidebar();
            frame.Controls.Add(sidebar, 0, 0);

            _pageHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Ground,
                Padding = new Padding(0),
                AutoScroll = false
            };
            frame.Controls.Add(_pageHost, 1, 0);

            Navigate(AppPage.Overview);
        }

        private Panel BuildSidebar()
        {
            Panel sidebar = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Sidebar,
                Padding = new Padding(16, 18, 16, 14)
            };

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Theme.Sidebar,
                Margin = new Padding(0)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            sidebar.Controls.Add(layout);

            Panel identity = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Sidebar };
            FingerprintMark mark = new FingerprintMark
            {
                Location = new Point(0, 7),
                AccessibleName = "Tanda fingerprint"
            };
            identity.Controls.Add(mark);

            Label appName = Theme.MakeLabel("Absensi", 13f, FontStyle.Bold, Theme.Ink);
            appName.Location = new Point(44, 6);
            identity.Controls.Add(appName);

            Label appKind = Theme.MakeLabel("Fingerprint", 9f, FontStyle.Regular, Theme.Muted);
            appKind.Location = new Point(44, 29);
            identity.Controls.Add(appKind);
            layout.Controls.Add(identity, 0, 0);

            FlowLayoutPanel menu = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Theme.Sidebar,
                Padding = new Padding(0, 14, 0, 0),
                Margin = new Padding(0)
            };

            Label groupData = Theme.MakeLabel("Pengelolaan", 9f, FontStyle.Regular, Theme.Muted);
            groupData.Margin = new Padding(8, 2, 0, 8);
            groupData.Width = 184;
            menu.Controls.Add(groupData);

            AddNav(menu, 0, "Ringkasan", AppPage.Overview);
            AddNav(menu, 1, "Karyawan", AppPage.Employees);
            AddNav(menu, 2, "Mesin fingerprint", AppPage.Machines);

            Label groupWork = Theme.MakeLabel("Absensi", 9f, FontStyle.Regular, Theme.Muted);
            groupWork.Margin = new Padding(8, 18, 0, 8);
            groupWork.Width = 184;
            menu.Controls.Add(groupWork);

            AddNav(menu, 3, "Record absensi", AppPage.Attendance);
            AddNav(menu, 4, "Aturan kerja", AppPage.Rules);
            AddNav(menu, 5, "Laporan", AppPage.Reports);

            Label groupTools = Theme.MakeLabel("Data", 9f, FontStyle.Regular, Theme.Muted);
            groupTools.Margin = new Padding(8, 18, 0, 8);
            groupTools.Width = 184;
            menu.Controls.Add(groupTools);
            AddNav(menu, 6, "Impor data", AppPage.Import);

            layout.Controls.Add(menu, 0, 1);

            Panel footer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Sidebar,
                Padding = new Padding(8, 8, 0, 0)
            };
            footer.Controls.Add(Theme.MakeRule());
            Label footerText = Theme.MakeLabel("Data tersimpan di komputer ini", 8.5f, FontStyle.Regular, Theme.Muted);
            footerText.Location = new Point(0, 20);
            footer.Controls.Add(footerText);
            layout.Controls.Add(footer, 0, 2);

            return sidebar;
        }

        private void AddNav(FlowLayoutPanel menu, int index, string label, AppPage page)
        {
            Button button = new Button
            {
                Text = label,
                TextAlign = ContentAlignment.MiddleLeft,
                Width = 198,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = Theme.Sidebar,
                ForeColor = Theme.Muted,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point),
                Padding = new Padding(10, 0, 8, 0),
                Margin = new Padding(0, 0, 0, 3),
                Cursor = Cursors.Hand,
                Tag = page,
                AccessibleName = label,
                AccessibleRole = AccessibleRole.PageTab
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(234, 234, 230);
            button.Click += delegate { Navigate(page); };
            menu.Controls.Add(button);
            _navigationButtons[index] = button;
        }

        private void Navigate(AppPage page)
        {
            _currentPage = page;
            for (int i = 0; i < _navigationButtons.Length; i++)
            {
                if (_navigationButtons[i] == null)
                {
                    continue;
                }
                bool active = (AppPage)_navigationButtons[i].Tag == page;
                _navigationButtons[i].BackColor = active ? Theme.Active : Theme.Sidebar;
                _navigationButtons[i].ForeColor = active ? Theme.Ink : Theme.Muted;
                _navigationButtons[i].Font = new Font("Segoe UI", 9.5f,
                    active ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Point);
            }

            _pageHost.Controls.Clear();
            UserControl view;
            switch (page)
            {
                case AppPage.Employees:
                    view = new EmployeesPage(_database, RefreshCurrentPage);
                    break;
                case AppPage.Machines:
                    view = new MachinesPage(_database, RefreshCurrentPage);
                    break;
                case AppPage.Attendance:
                    view = new AttendancePage(_database, delegate { Navigate(AppPage.Machines); });
                    break;
                case AppPage.Import:
                    view = new ImportPage(_database, delegate { Navigate(AppPage.Employees); });
                    break;
                case AppPage.Rules:
                    view = new PlaceholderPage("Aturan kerja",
                        "Pengaturan shift, jam masuk dan pulang, toleransi keterlambatan, hari libur, izin, dan lembur.");
                    break;
                case AppPage.Reports:
                    view = new PlaceholderPage("Laporan",
                        "Laporan harian dan bulanan per karyawan atau departemen, untuk dicetak atau diekspor.");
                    break;
                default:
                    view = new OverviewPage(_database, delegate { Navigate(AppPage.Machines); },
                        delegate { Navigate(AppPage.Import); });
                    break;
            }

            view.Dock = DockStyle.Fill;
            _pageHost.Controls.Add(view);
            view.BringToFront();
        }

        private void RefreshCurrentPage()
        {
            Navigate(_currentPage);
        }
    }
}

