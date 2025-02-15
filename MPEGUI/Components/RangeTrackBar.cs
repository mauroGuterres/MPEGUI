using System;
using System.Drawing;
using System.Windows.Forms;

namespace MPEGUI.Components
{
    public class RangeTrackBar : Control
    {
        private int _minimum = 0;
        private int _maximum = 100;
        private int _lowerValue = 0;
        private int _upperValue = 100;
        private bool _dragLower = false;
        private bool _dragUpper = false;

        public int Minimum
        {
            get => _minimum;
            set { _minimum = value; Invalidate(); }
        }
        public int Maximum
        {
            get => _maximum;
            set { _maximum = value; Invalidate(); }
        }
        public int LowerValue
        {
            get => _lowerValue;
            set { _lowerValue = Math.Max(_minimum, Math.Min(value, _upperValue)); Invalidate(); OnRangeChanged(); }
        }
        public int UpperValue
        {
            get => _upperValue;
            set { _upperValue = Math.Min(_maximum, Math.Max(value, _lowerValue)); Invalidate(); OnRangeChanged(); }
        }

        public event EventHandler RangeChanged;
        protected virtual void OnRangeChanged()
        {
            RangeChanged?.Invoke(this, EventArgs.Empty);
        }

        public RangeTrackBar()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            this.MinimumSize = new Size(100, 30);
            this.Height = 30;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            int trackHeight = 6;
            int thumbWidth = 10;
            int thumbHeight = 20;
            int trackY = (this.Height - trackHeight) / 2;
            Rectangle trackRect = new Rectangle(thumbWidth, trackY, this.Width - 2 * thumbWidth, trackHeight);

            // Draw background track
            g.FillRectangle(Brushes.LightGray, trackRect);

            // Scale factor to convert value to pixel position.
            float scale = (float)trackRect.Width / (Maximum - Minimum);
            int lowerX = trackRect.Left + (int)((LowerValue - Minimum) * scale);
            int upperX = trackRect.Left + (int)((UpperValue - Minimum) * scale);

            // Draw selected range
            Rectangle selectedRect = new Rectangle(lowerX, trackY, upperX - lowerX, trackHeight);
            g.FillRectangle(Brushes.Blue, selectedRect);

            // Draw lower thumb
            Rectangle lowerThumb = new Rectangle(lowerX - thumbWidth / 2, (this.Height - thumbHeight) / 2, thumbWidth, thumbHeight);
            g.FillRectangle(Brushes.DarkGray, lowerThumb);
            g.DrawRectangle(Pens.Black, lowerThumb);

            // Draw upper thumb
            Rectangle upperThumb = new Rectangle(upperX - thumbWidth / 2, (this.Height - thumbHeight) / 2, thumbWidth, thumbHeight);
            g.FillRectangle(Brushes.DarkGray, upperThumb);
            g.DrawRectangle(Pens.Black, upperThumb);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            int thumbWidth = 10;
            int trackLeft = thumbWidth;
            int trackWidth = this.Width - 2 * thumbWidth;
            float scale = (float)trackWidth / (Maximum - Minimum);
            int lowerX = trackLeft + (int)((LowerValue - Minimum) * scale);
            int upperX = trackLeft + (int)((UpperValue - Minimum) * scale);
            Rectangle lowerThumb = new Rectangle(lowerX - thumbWidth / 2, (this.Height - 20) / 2, thumbWidth, 20);
            Rectangle upperThumb = new Rectangle(upperX - thumbWidth / 2, (this.Height - 20) / 2, thumbWidth, 20);

            if (lowerThumb.Contains(e.Location))
                _dragLower = true;
            else if (upperThumb.Contains(e.Location))
                _dragUpper = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_dragLower || _dragUpper)
            {
                int thumbWidth = 10;
                int trackLeft = thumbWidth;
                int trackWidth = this.Width - 2 * thumbWidth;
                float scale = (float)(Maximum - Minimum) / trackWidth;
                int value = Minimum + (int)((e.X - trackLeft) * scale);
                if (_dragLower)
                {
                    LowerValue = Math.Min(value, UpperValue);
                }
                else if (_dragUpper)
                {
                    UpperValue = Math.Max(value, LowerValue);
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _dragLower = false;
            _dragUpper = false;
        }
    }
}
