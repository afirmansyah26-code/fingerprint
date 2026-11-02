using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FingerprintAttendance.UI
{
    internal static class Theme
    {
        public static readonly Color Ground = Color.FromArgb(246, 246, 243);
        public static readonly Color Surface = Color.White;
        public static readonly Color Sidebar = Color.FromArgb(241, 241, 237);
        public static readonly Color Active = Color.FromArgb(229, 229, 224);
        public static readonly Color Border = Color.FromArgb(225, 225, 220);
        public static readonly Color Rule = Color.FromArgb(237, 237, 233);
        public static readonly Color Ink = Color.FromArgb(24, 25, 25);
        public static readonly Color Muted = Color.FromArgb(91, 96, 99);
        public static readonly Color Accent = Color.FromArgb(185, 33, 39);
        public static readonly Color Danger = Color.FromArgb(157, 31, 36);
        public static readonly Color Disabled = Color.FromArgb(143, 146, 145);

        public static Label MakeLabel(string text, float size, FontStyle style, Color color)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font("Segoe UI", size, style, GraphicsUnit.Point),
                ForeColor = color,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };
        }

        public static Button MakeButton(string text, ButtonKind kind)
        {
            Button button = new Button
            {
                Text = text,
                AutoSize = false,
                Height = 36,
                MinimumSize = new Size(92, 36),
                Padding = new Padding(13, 0, 13, 0),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                Margin = new Padding(0, 0, 8, 0)
            };
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(217, 217, 212);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(239, 239, 235);

            if (kind == ButtonKind.Primary)
            {
                button.BackColor = Accent;
                button.ForeColor = Color.White;
                button.FlatAppearance.BorderColor = Accent;
                button.FlatAppearance.MouseOverBackColor = Color.FromArgb(160, 26, 32);
                button.FlatAppearance.MouseDownBackColor = Color.FromArgb(140, 22, 27);
            }
            else if (kind == ButtonKind.Danger)
            {
                button.BackColor = Color.White;
                button.ForeColor = Danger;
                button.FlatAppearance.BorderColor = Border;
            }
            else if (kind == ButtonKind.Text)
            {
                button.BackColor = Color.Transparent;
                button.ForeColor = Ink;
                button.FlatAppearance.BorderSize = 0;
                button.MinimumSize = new Size(68, 36);
                button.Padding = new Padding(8, 0, 8, 0);
            }
            else
            {
                button.BackColor = Color.White;
                button.ForeColor = Ink;
                button.FlatAppearance.BorderColor = Border;
            }

            return button;
        }

        public static Panel MakeRule()
        {
            return new Panel { Height = 1, BackColor = Rule, Margin = new Padding(0) };
        }

        public static Panel MakeSurface()
        {
            return new Panel
            {
                BackColor = Surface,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0)
            };
        }
    }

    internal enum ButtonKind
    {
        Primary,
        Secondary,
        Text,
        Danger
    }

    internal sealed class FingerprintMark : Control
    {
        public FingerprintMark()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(36, 36);
            MinimumSize = Size;
            AccessibleName = "Tanda aplikasi Absensi";
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (Pen pen = new Pen(Theme.Accent, 1.7f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                Rectangle[] bounds = new Rectangle[]
                {
                    new Rectangle(7, 4, 22, 28),
                    new Rectangle(10, 8, 16, 21),
                    new Rectangle(13, 12, 10, 13)
                };
                foreach (Rectangle rect in bounds)
                {
                    e.Graphics.DrawArc(pen, rect, 205, 130);
                }

                e.Graphics.DrawArc(pen, new Rectangle(9, 11, 18, 20), 45, 82);
                e.Graphics.DrawArc(pen, new Rectangle(6, 15, 23, 20), 35, 82);
                e.Graphics.DrawArc(pen, new Rectangle(12, 17, 12, 15), 205, 105);
            }
        }
    }
}
