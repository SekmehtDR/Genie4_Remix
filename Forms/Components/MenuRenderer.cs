using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;

namespace GenieClient.Forms.Components
{
    internal class MenuRenderer : ToolStripRenderer
    {
        private readonly Genie.Globals.Presets _presets;

        private Color BgColor       => _presets["ui.menu"].BgColor;
        private bool  IsDark        => BgColor.GetBrightness() < 0.5f;
        private Color FgColor       => _presets["ui.menu"].FgColor;
        private Color HoverColor    => _presets["ui.menu.highlight"].FgColor;
        private Color CheckColor    => _presets["ui.menu.checked"].FgColor;
        private Color DisabledColor => Color.FromArgb(100, 100, 105);

        private static readonly Color SeparatorColor = Color.FromArgb(70, 70, 75);
        private static readonly Color BorderColor    = Color.FromArgb(70, 70, 75);

        private static readonly Font _checkFont = new Font("Segoe UI", 8f, FontStyle.Bold);
        private static readonly StringFormat _checkFormat = new StringFormat
        {
            Alignment     = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        public MenuRenderer(Genie.Globals.Presets presets)
        {
            _presets = presets;
        }

        // Stamp dark colors onto the ToolStrip control itself so no light BackColor bleeds through
        protected override void Initialize(ToolStrip toolStrip)
        {
            base.Initialize(toolStrip);
            toolStrip.BackColor = BgColor;
            toolStrip.ForeColor = FgColor;
        }

        // Stamp dark colors onto every item as it is initialized
        protected override void InitializeItem(ToolStripItem item)
        {
            base.InitializeItem(item);
            item.BackColor = BgColor;
            item.ForeColor = FgColor;
        }

        // Fill the entire strip background
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using var brush = new SolidBrush(BgColor);
            e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }

        // Thin accent border on dropdowns only
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            if (e.ToolStrip is ToolStripDropDown)
            {
                using var pen = new Pen(BorderColor);
                e.Graphics.DrawRectangle(pen, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
            }
        }

        // Full-width item background — hover or normal
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            Color bg = e.Item.Selected || e.Item.Pressed ? HoverColor : BgColor;
            using var brush = new SolidBrush(bg);
            e.Graphics.FillRectangle(brush, new Rectangle(0, 0, e.Item.Width, e.Item.Height));
        }

        // Image/checkbox area — always fill with item background before drawing any image.
        // This eliminates the white placeholder box for unchecked checkable items.
        // In dark mode, MDI control-box icons (minimize/restore/close) are black system images
        // injected into the MenuStrip when a child is maximized — invert them so they are visible.
        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            Color bg = e.Item.Selected || e.Item.Pressed ? HoverColor : BgColor;
            using var brush = new SolidBrush(bg);
            e.Graphics.FillRectangle(brush, e.ImageRectangle);
            if (e.Image == null) return;

            if (IsDark && e.Item.GetCurrentParent() is MenuStrip)
            {
                // Invert dark system icons: multiply RGB by -1 and offset by +1, keep alpha.
                // Scoped to MenuStrip only — toolbar icon images are unaffected.
                var cm = new ColorMatrix(new float[][]
                {
                    new float[] { -1,  0,  0, 0, 0 },
                    new float[] {  0, -1,  0, 0, 0 },
                    new float[] {  0,  0, -1, 0, 0 },
                    new float[] {  0,  0,  0, 1, 0 },
                    new float[] {  1,  1,  1, 0, 1 }
                });
                using var ia = new ImageAttributes();
                ia.SetColorMatrix(cm);
                var r = e.ImageRectangle;
                e.Graphics.DrawImage(e.Image,
                    new Rectangle(r.X, r.Y, r.Width, r.Height),
                    0, 0, e.Image.Width, e.Image.Height,
                    GraphicsUnit.Pixel, ia);
            }
            else
            {
                base.OnRenderItemImage(e);
            }
        }

        // Icon column background
        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            using var brush = new SolidBrush(BgColor);
            e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }

        // Checked item — dark box with ✓ glyph
        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            var r = e.ImageRectangle;
            using var bgBrush = new SolidBrush(CheckColor);
            e.Graphics.FillRectangle(bgBrush, r);
            using var fgBrush = new SolidBrush(FgColor);
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            e.Graphics.DrawString("✓", _checkFont, fgBrush, (System.Drawing.RectangleF)r, _checkFormat);
        }

        // Text — always theme foreground, disabled items dimmed
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Enabled ? FgColor : DisabledColor;
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            base.OnRenderItemText(e);
        }

        // Split button (e.g. running script buttons) — fill background, divider, and arrow
        protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
        {
            var btn = (ToolStripSplitButton)e.Item;

            // Button area background
            Color buttonBg = btn.ButtonPressed || btn.ButtonSelected ? HoverColor : BgColor;
            using var buttonBrush = new SolidBrush(buttonBg);
            e.Graphics.FillRectangle(buttonBrush, btn.ButtonBounds);

            // Dropdown area — base color + adaptive tint (lighten on dark, darken on light)
            Color dropBg = btn.DropDownButtonPressed || btn.DropDownButtonSelected ? HoverColor : BgColor;
            using var dropBrush = new SolidBrush(dropBg);
            e.Graphics.FillRectangle(dropBrush, btn.DropDownButtonBounds);
            Color tint = IsDark ? Color.FromArgb(45, 255, 255, 255) : Color.FromArgb(35, 0, 0, 0);
            using var tintBrush = new SolidBrush(tint);
            e.Graphics.FillRectangle(tintBrush, btn.DropDownButtonBounds);

            // Vertical divider
            int splitX = btn.ButtonBounds.Right;
            using var pen = new Pen(IsDark ? Color.FromArgb(130, 130, 135) : Color.FromArgb(160, 160, 165));
            e.Graphics.DrawLine(pen, splitX, 3, splitX, e.Item.Height - 4);

            // Down arrow — explicit filled triangle so it shows on all themes
            var r = btn.DropDownButtonBounds;
            int cx = r.X + r.Width / 2;
            int cy = r.Y + r.Height / 2;
            var arrowPoints = new Point[]
            {
                new Point(cx - 4, cy - 2),
                new Point(cx + 4, cy - 2),
                new Point(cx,     cy + 3),
            };
            using var arrowBrush = new SolidBrush(btn.Enabled ? FgColor : DisabledColor);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.FillPolygon(arrowBrush, arrowPoints);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
        }

        // Submenu arrows only — split button arrow is drawn manually above
        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            if (e.Item is ToolStripSplitButton) return;
            e.ArrowColor = e.Item.Enabled ? FgColor : DisabledColor;
            base.OnRenderArrow(e);
        }

        // Flat 1px separator
        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            int y = e.Item.Height / 2;
            using var pen = new Pen(SeparatorColor);
            e.Graphics.DrawLine(pen, 4, y, e.Item.Width - 4, y);
        }

        // No grip dots
        protected override void OnRenderGrip(ToolStripGripRenderEventArgs e) { }
    }
}
