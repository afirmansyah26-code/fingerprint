using System.Drawing;
using System.Windows.Forms;

namespace FingerprintAttendance.UI
{
    internal abstract class AppPageControl : UserControl
    {
        protected readonly Panel Body;

        protected AppPageControl(string title, string description)
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.Ground;
            Padding = new Padding(30, 26, 30, 22);
            Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Theme.Ground,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);

            Panel header = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Ground };
            Label titleLabel = Theme.MakeLabel(title, 18f, FontStyle.Bold, Theme.Ink);
            titleLabel.Location = new Point(0, 0);
            header.Controls.Add(titleLabel);

            Label descriptionLabel = Theme.MakeLabel(description, 9.5f, FontStyle.Regular, Theme.Muted);
            descriptionLabel.Location = new Point(1, 36);
            header.Controls.Add(descriptionLabel);
            layout.Controls.Add(header, 0, 0);

            Body = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Ground,
                Padding = new Padding(0),
                AutoScroll = true
            };
            layout.Controls.Add(Body, 0, 1);
        }

        protected Panel CreateOutlinedPanel()
        {
            return new Panel
            {
                BackColor = Theme.Surface,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0)
            };
        }

        protected static void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor = Theme.Surface;
            grid.BorderStyle = BorderStyle.None;
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
            grid.ColumnHeadersHeight = 42;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.RowTemplate.Height = 44;
            grid.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Theme.Surface;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Theme.Muted;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Theme.Surface;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Theme.Muted;
            grid.DefaultCellStyle.BackColor = Theme.Surface;
            grid.DefaultCellStyle.ForeColor = Theme.Ink;
            grid.DefaultCellStyle.SelectionBackColor = Theme.Active;
            grid.DefaultCellStyle.SelectionForeColor = Theme.Ink;
            grid.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            grid.RowTemplate.DefaultCellStyle.SelectionBackColor = Theme.Active;
            grid.RowTemplate.DefaultCellStyle.SelectionForeColor = Theme.Ink;
            grid.RowTemplate.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
        }

        protected static Panel MakeCenteredEmptyState(string title, string detail, Button action)
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Surface,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(28)
            };

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Theme.Surface
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
            panel.Controls.Add(layout);

            Label heading = Theme.MakeLabel(title, 13f, FontStyle.Bold, Theme.Ink);
            heading.Anchor = AnchorStyles.None;
            layout.Controls.Add(heading, 0, 1);

            Label description = Theme.MakeLabel(detail, 9.5f, FontStyle.Regular, Theme.Muted);
            description.AutoSize = false;
            description.TextAlign = ContentAlignment.MiddleCenter;
            description.Width = 560;
            description.Height = 42;
            description.Anchor = AnchorStyles.None;
            layout.Controls.Add(description, 0, 2);

            if (action != null)
            {
                layout.RowStyles[0] = new RowStyle(SizeType.Percent, 35);
                layout.RowStyles[3] = new RowStyle(SizeType.Percent, 65);
                action.Anchor = AnchorStyles.None;
                action.Margin = new Padding(0, 18, 0, 0);
                layout.Controls.Add(action, 0, 3);
            }

            return panel;
        }
    }
}

