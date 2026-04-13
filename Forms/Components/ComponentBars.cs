using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GenieClient
{
    public partial class ComponentBars
    {
        public ComponentBars()
        {
            InitializeComponent();
            DoubleBuffered = true;
            Disposed += (s, e) =>
            {
                m_BorderColor.Dispose();
                m_BorderColorGrayScale.Dispose();
            };
        }

        private int m_CurrentValue = 0;
        private string m_Text = string.Empty;
        private Color m_BackgroundColor = Color.Black;
        private Color m_ForegroundColor = Color.Gray;
        private Pen m_BorderColor = new Pen(Color.Gray);
        private Pen m_BorderColorGrayScale = new Pen(Color.Gray);

        private void PanelBar_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int panelW = PanelBar.Width;
            int panelH = PanelBar.Height;
            int radius = Math.Min(panelH / 2, 5);

            Pen borderPen = m_IsConnected ? m_BorderColor : m_BorderColorGrayScale;

            Color fillColor = m_IsConnected
                ? m_ForegroundColor
                : Genie.ColorCode.ColorToGrayscale(m_ForegroundColor);

            // Empty section: a bright pastel tint derived from the fill color (20% fill + 80% white).
            // This is computed from the fill rather than relying on the preset BgColor, so it always
            // looks correct regardless of what is saved in presets.cfg.
            Color emptyColor = Color.FromArgb(
                fillColor.R * 2 / 5,
                fillColor.G * 2 / 5,
                fillColor.B * 2 / 5);
            g.Clear(emptyColor);

            if (m_CurrentValue > 0)
            {

                int w = (int)Math.Round(panelW / 100.0 * m_CurrentValue);
                w = Math.Max(w, 1);

                using var fillPath = RoundedRect(new Rectangle(0, 0, w - 1, panelH - 1), radius);

                // 3D gradient: lighter at top, darker at bottom for a convex/bubble look
                Color topColor = ControlPaint.Light(fillColor, 0.5f);
                Color bottomColor = ControlPaint.Dark(fillColor, 0.3f);
                using var mainBrush = new LinearGradientBrush(
                    new Rectangle(0, 0, Math.Max(w, 1), Math.Max(panelH, 1)),
                    topColor, bottomColor, LinearGradientMode.Vertical);
                g.FillPath(mainBrush, fillPath);

                // Gloss highlight: semi-transparent white shimmer across the top ~45%, clipped to the fill shape
                int glossH = Math.Max((int)(panelH * 0.45), 1);
                using var glossBrush = new LinearGradientBrush(
                    new Rectangle(0, 0, Math.Max(w, 1), glossH + 1),
                    Color.FromArgb(130, 255, 255, 255),
                    Color.FromArgb(0, 255, 255, 255),
                    LinearGradientMode.Vertical);
                g.SetClip(fillPath);
                g.FillRectangle(glossBrush, 0, 0, w, glossH);
                g.ResetClip();
            }

            // Rounded border drawn last so it sits cleanly over the fill
            using var borderPath = RoundedRect(new Rectangle(0, 0, panelW - 1, panelH - 1), radius);
            g.DrawPath(borderPen, borderPath);
        }

        private bool m_IsConnected = false;

        public bool IsConnected
        {
            get => m_IsConnected;
            set
            {
                m_IsConnected = value;
                PanelBar.BackColor = m_IsConnected
                    ? m_BackgroundColor
                    : Genie.ColorCode.ColorToGrayscale(m_BackgroundColor);
                Invalidate();
            }
        }

        public string BarText
        {
            get => m_Text;
            set
            {
                m_Text = value;
                Invalidate();
            }
        }

        public Color BackgroundColor
        {
            get => m_BackgroundColor;
            set
            {
                m_BackgroundColor = value;
                PanelBar.BackColor = m_IsConnected
                    ? m_BackgroundColor
                    : Genie.ColorCode.ColorToGrayscale(m_BackgroundColor);
                Invalidate();
            }
        }

        public Color ForegroundColor
        {
            get => m_ForegroundColor;
            set
            {
                m_ForegroundColor = value;
                Invalidate();
            }
        }

        public Color BorderColor
        {
            get => m_BorderColor.Color;
            set
            {
                m_BorderColor.Dispose();
                m_BorderColorGrayScale.Dispose();
                m_BorderColor = new Pen(value);
                m_BorderColorGrayScale = new Pen(Genie.ColorCode.ColorToGrayscale(value));
                Invalidate();
            }
        }

        public int Value
        {
            get => m_CurrentValue;
            set
            {
                if (value < 0) value = 0;
                if (value > 100) value = 100;
                m_CurrentValue = value;
                LabelValue.Text = m_Text;
                Invalidate();
            }
        }

        // Clips the UserControl itself to the rounded shape so the parent background
        // shows through the corners naturally rather than leaving square remnants
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Width > 0 && Height > 0)
            {
                int radius = Math.Min(Height / 2, 5);
                using var path = RoundedRect(new Rectangle(0, 0, Width, Height), radius);
                Region = new Region(path);
            }
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            diameter = Math.Min(diameter, Math.Min(bounds.Width, bounds.Height));
            var path = new GraphicsPath();
            if (diameter <= 0 || bounds.Width <= 0 || bounds.Height <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
