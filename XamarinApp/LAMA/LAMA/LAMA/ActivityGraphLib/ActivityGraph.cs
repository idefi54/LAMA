using LAMA.Models;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Xamarin.Forms;

namespace LAMA.ActivityGraphLib
{
    /// <summary>
    /// Holds all logic for drawing the graph.
    /// </summary>
    public class ActivityGraph
    {
        // private
        //===============================================

        private float _width;
        private float _height;
        private float _columnWidth => _width / 24;
        private float _maxOffsetY => -_height * Zoom + _height - 200;
        private Label _dateLabel => DateView?.Children[1] as Label;
        private SKCanvasView _canvasView => _canvasLayout.Children[0] as SKCanvasView;
        private float _zoom;
        private static DateTime loadPoint;
        private DateTime _timeOffset;
        private float _mouseX;
        private float _mouseY;
        private Layout<View> _canvasLayout;

        // Public
        //===============================================

        /// <summary>
        /// Vertical offset of graph view while zoomed.
        /// </summary>
        public float OffsetY { get; private set; }
        
        /// <summary>
        /// Vertical offset of the canvas layout containing the activity graph.
        /// </summary>
        public float XamOffset => (float)_canvasLayout.Y;

        /// <summary>
        /// 24 labels to be shown as description for hours of a day.
        /// </summary>
        public Label[] TimeLabels { get; set; }

        /// <summary>
        /// Xamarin UI for showing current date above the graph.
        /// </summary>
        public Layout<View> DateView { get; set; }

        /// <summary>
        /// Is in mode for creation new a activity.
        /// </summary>
        public bool ActivityCreationMode { get; private set; } = false;

        /// <summary>
        /// Larger value zooms in the graph.
        /// </summary>
        public float Zoom { get { return _zoom; } set { _zoom = Math.Max(1, value); } }

        /// <summary>
        /// Current time offset where the activity graph is positioned on the timeline.
        /// </summary>
        public DateTime TimeOffset
        {
            get { return _timeOffset; }
            set
            {
                _timeOffset = value;
                if (Math.Abs(_timeOffset.Subtract(loadPoint).Days) > 3)
                {
                    ReloadActivities();
                    loadPoint = _timeOffset;
                }

            }
        }

        /// <summary>
        /// Width in canvas pixels of one minte on the activity graph.
        /// </summary>
        public float MinuteWidth => _columnWidth / 60;

        /// <summary>
        /// Converts pixel coordinates of the canvas containing the activity graph to xamarin coordinates.
        /// </summary>
        /// <param name="x">Chosen (horizontal or vertical) coordinate you wish to convert.</param>
        /// <returns></returns>
        public float FromPixels(float x) => x * (float)_canvasView.Width / _width;


        /// <summary>
        /// Converts xamarin coordinates to pixel coordinates of the canvas containing the activity graph.
        /// </summary>
        /// <param name="x">Chosen (horizontal or vertical) coordinate you wish to convert.</param>
        /// <returns></returns>
        public float ToPixels(float x) => x * _width / (float)_canvasView.Width;

        /// <summary>
        /// Converts mouse cursor horizontal location to time on the current state of the activity graph.
        /// </summary>
        /// <param name="x">Xamarin horizontal location on screen.</param>
        /// <returns></returns>
        public DateTime ToTime(float x)
        {
            float minutes = ToPixels(x) / MinuteWidth / Zoom;
            return TimeOffset.AddMinutes(minutes - (minutes % 5));
        }

        /// <summary>
        /// Mouse location for some functions of the graph.
        /// Like for highlight of adding new activity button. (Activity creation mode)
        /// </summary>
        public float MouseX
        {
            get { return FromPixels(_mouseX); }
            set { _mouseX = ToPixels(value); }
        }

        /// <summary>
        /// Mouse location for some functions of the graph.
        /// Like for highlight of adding new activity button. (Activity creation mode)
        /// </summary>
        public float MouseY
        {
            get { return FromPixels(_mouseY); }
            set { _mouseY = ToPixels(value - XamOffset); }
        }

        public ActivityGraph(Layout<View> canvasGrid)
        {
            Zoom = 2;
            OffsetY = 0;
            _canvasLayout = canvasGrid;
            TimeOffset = DateTime.Now;
            ReloadActivities();
            _canvasView.InvalidateSurface();
        }

        /// <summary>
        /// Updates graph size and font sizes.
        /// </summary>
        /// <param name="args"></param>
        public void Update(SKPaintSurfaceEventArgs args)
        {
            _width = args.Info.Width;
            _height = args.Info.Height;

            if (TimeLabels == null)
                return;

            if (_width < 1000)
            {
                //_dateLabel.FontSize = 8;
                foreach (var label in TimeLabels)
                    label.FontSize = 8;

            } else
            {
                //_dateLabel.FontSize = 14;
                foreach (var label in TimeLabels)
                    label.FontSize = 14;
            }
        }

        /// <summary>
        /// Scrolls through the graff in x and y axis.
        /// x represents time.
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        public void Move(float dx, float dy)
        {
            dx = ToPixels(dx);
            dx /= _columnWidth * Zoom / 60;
            TimeOffset = TimeOffset.AddMinutes(-dx);
            OffsetY += ToPixels(dy);
            OffsetY = Math.Min(OffsetY, 0);
            OffsetY = Math.Max(_maxOffsetY, OffsetY);
        }

        /// <summary>
        /// Draws the entire graph onto the canvas.
        /// </summary>
        /// <param name="canvas"></param>
        public void Draw(SKCanvas canvas)
        {
            canvas.Clear(SKColors.Black);
            foreach (ActivityButton button in ActivityButtons())
                button.Update();

            SKPaint paint = new SKPaint();
            paint.Color = SKColors.Blue;
            paint.StrokeWidth = 1;
            canvas.DrawLine(0, 5, _width, 5, paint);

            int columnCount = (int)Math.Round(_width / _columnWidth / Zoom);
            float minutePosition = _columnWidth * Zoom - TimeOffset.Minute * (_columnWidth * Zoom / 60);
            float columnOffset = minutePosition % (_columnWidth * Zoom);
            if (columnOffset < 0) columnOffset += _columnWidth * Zoom;

            if (TimeLabels != null)
                foreach (Label label in TimeLabels)
                    label.IsVisible = false;

            if (_dateLabel != null)
                _dateLabel.Text = $"{TimeOffset.Day:00}.{TimeOffset.Month:00}.{TimeOffset.Year:0000}";

            for (int i = 0; i < columnCount; i++)
            {
                int time = (TimeOffset.Hour + i) % 24;
                if (TimeOffset.Minute > 0) time = (time + 1) % 24;

                if (TimeLabels != null)
                {
                    TimeLabels[i].IsVisible = true;
                    TimeLabels[i].Text = $"{time:00}:00";
                    TimeLabels[i].TranslationX = FromPixels(_columnWidth * Zoom * i + columnOffset);
                }

                if (time == 0 && _dateLabel != null)
                {
                    var t = TimeOffset;

                    // this if happens if we draw the line exactly at position 0 so it is not tommorrow yet
                    if (TimeOffset.Minute != 0 || TimeOffset.Hour != 0)
                        t = TimeOffset.AddDays(1);

                    _dateLabel.Text = $"{t.Day:00}.{t.Month:00}.{t.Year:0000}";
                    DateView.TranslationX = FromPixels(_columnWidth * Zoom * i + columnOffset);
                }

                paint.PathEffect = SKPathEffect.CreateDash(new float[] { 15f, 10f }, -OffsetY);
                paint.Color = (time == 0) ? SKColors.Red : SKColors.Blue;
                paint.Color = paint.Color.WithAlpha(125);
                paint.StrokeWidth = (time == 0) ? 3 : 1;

                canvas.DrawLine(
                    _columnWidth * Zoom * i + columnOffset,
                    0,
                    _columnWidth * Zoom * i + columnOffset,
                    _height * Zoom + OffsetY,
                    paint
                    );
            }

            if (DateView?.TranslationX < 0)
                DateView.TranslationX = 0;

            if (DateView?.TranslationX > Application.Current.MainPage.Width - DateView?.Width)
                DateView.TranslationX = Application.Current.MainPage.Width - DateView.Width;

            paint.PathEffect = null;
            paint.Color = SKColors.Blue;
            paint.StrokeWidth = 1;
            canvas.DrawLine(0, _height * Zoom + OffsetY, _width, _height * Zoom + OffsetY, paint);

            paint.Color = SKColors.Gray;
            float offset = 0;
            canvas.DrawLine(5, offset, 5, _height - 200 + offset, paint);
            canvas.DrawRoundRect(0, OffsetY / _maxOffsetY * (_height - 200 - 20) + offset, 10, 20, 5, 5, paint);
            paint.StrokeWidth = 3;
            canvas.DrawLine(0, offset, 10, offset, paint);
            canvas.DrawLine(0, offset + _height - 200, 10, offset + _height - 200, paint);

            paint.Color = SKColors.Green;
            if (ActivityCreationMode)
                canvas.DrawRect(_mouseX - 50, _mouseY - 25, 100, 50, paint);

            DrawConnections(canvas);
        }

        /// <summary>
        /// Disables ActivityButtons and makes it possible to move them with mouse.
        /// </summary>
        /// <param name="edit"></param>
        public void SwitchEditMode(bool edit)
        {
            foreach (ActivityButton button in ActivityButtons())
                button.IsEnabled = !edit;
        }

        /// <summary>
        /// Switches edit mode and draws location of the new would be button.
        /// </summary>
        /// <param name="active"></param>
        public void SwitchActivityCreationMode(bool active)
        {
            SwitchEditMode(active);
            ActivityCreationMode = active;
        }

        /// <summary>
        /// Reload ActivityButtons with new activities from database at current span of days.
        /// </summary>
        public void ReloadActivities()
        {
            // Remove all instances of ActivityButton
            int count = _canvasLayout.Children.Count;
            for (int i = 0; i < count - 1; i++)
                _canvasLayout.Children.RemoveAt(1);

            var rememberedList = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList;
            var activities = rememberedList.sqlConnection.ReadData();
            var currentActivities =
                            from activity in activities
                            where Math.Abs(activity.day - TimeOffset.Day) <= 9 ||
                            DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) - Math.Abs(activity.day - DateTime.Now.Day) <= 9
                            select activity;

            foreach (LarpActivity activity in currentActivities)
                _canvasLayout.Children.Add(new ActivityButton(activity, this));
        }

        /// <summary>
        /// Connects ActivityButtons on graph that serve as prerequisites to another activity.
        /// </summary>
        /// <param name="canvas"></param>
        public void DrawConnections(SKCanvas canvas)
        {
            foreach (ActivityButton button1 in ActivityButtons())
                foreach (ActivityButton button2 in ActivityButtons())
                    if (button1.Activity.prerequisiteIDs.Contains(button2.Activity.ID))
                        ActivityButton.DrawConnection(canvas, this, button1, button2);
        }

        /// <summary>
        /// Loops over all ActivityButtons and returns first match in touch location x,y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public ActivityButton GetButtonAt(float x, float y)
        {
            foreach (ActivityButton button in ActivityButtons())
            {
                if (button.Bounds.Offset(button.TranslationX, button.TranslationY + _canvasLayout.Y).Contains(x, y))
                    return button;
            }

            return null;
        }

        /// <summary>
        /// Creates and adds new ActivityButton to canvas layout.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public ActivityButton AddActivity(LarpActivity activity)
        {
            var button = new ActivityButton(activity, this);
            _canvasLayout.Children.Add(button);
            return button;
        }

        /// <summary>
        /// Invalidates CanvasView to redraw the graph.
        /// </summary>
        public void InvalidateSurface()
        {
            _canvasView.InvalidateSurface();
        }

        private IEnumerable<ActivityButton> ActivityButtons()
        {
            for (int i = 1; i < _canvasLayout.Children.Count; i++)
                yield return _canvasLayout.Children[i] as ActivityButton;
        }
    }
}