using LAMA.Models;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Xamarin.Forms;
using LAMA.ActivityGraphLib;
using LAMA.Extensions;

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
        private List<ActivityButton> ActivityButtons;
        private INavigation _navigation;

        // Public
        //===============================================

        /// <summary>
        /// Vertical offset of graph view while zoomed.
        /// </summary>
        public float OffsetY
        {
            get => _offsetY * _maxOffsetY;
            private set => _offsetY = value / _maxOffsetY;
        }
        private float _offsetY;

        /// <summary>
        /// Allows for editing the buttons and such.
        /// </summary>
        public bool EditMode { get; set; }

        /// <summary>
        /// Max height of the graph.
        /// </summary>
        public float Height => _height;

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

        public enum EditingMode { None, Create, Connect, Disconnect }
        public EditingMode Mode { get; set; }

        public ActivityButton SelectedButton { get; set; } = null;


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
        /// <param name="x">Pixel horizontal location on screen.</param>
        /// <returns></returns>
        public DateTime ToLocalTime(float x)
        {
            float minutes = x / MinuteWidth / Zoom;
            return TimeOffset.AddMinutes(minutes).ToLocalTime();
        }

        /// <summary>
        /// Converts time to mouse cursor horizontal location.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public float FromTime(DateTime time)
        {
            TimeSpan difference = time - TimeOffset;
            return (float)difference.TotalMinutes * MinuteWidth * Zoom;
        }

        public float RoundToFiveMinutes(float x)
        {
            var time = ToLocalTime(x);
            time = new DateTime(
                time.Year,
                time.Month,
                time.Day,
                time.Hour,
                time.Minute - time.Minute % 5,
                0);
            return FromTime(time);
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

        public ActivityGraph(Layout<View> canvasGrid, INavigation navigation)
        {
            ActivityButtons = new List<ActivityButton>();
            Zoom = 2;
            OffsetY = 0;
            _canvasLayout = canvasGrid;
            _navigation = navigation;
            TimeOffset = DateTime.Now;
            Mode = EditingMode.None;
            ReloadActivities();
            _canvasView.InvalidateSurface();

            SQLEvents.created += SQLEvents_created;
        }

        private void SQLEvents_created(Serializable created)
        {
            LarpActivity activity = created as LarpActivity;
            if (activity == null) return;

            AddActivity(activity);
            InvalidateSurface();
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

            // Tested at these values
            float baseSize = 15;
            float baseWidth = 1520;

            // Adjust to arbitrary width
            foreach (var label in TimeLabels)
                label.FontSize = baseSize * Zoom * _canvasView.Width / baseWidth;
        }

        /// <summary>
        /// Scrolls through the graff in x and y axis.
        /// x represents time.
        /// </summary>
        /// <param name="dx">In pixel coordinates.</param>
        /// <param name="dy">In pixel coordinates.</param>
        public void Move(float dx, float dy)
        {
            dx /= _columnWidth * Zoom / 60;
            TimeOffset = TimeOffset.AddMinutes(-dx);
            OffsetY += dy;
            OffsetY = Math.Min(OffsetY, 0);
            OffsetY = Math.Max(_maxOffsetY, OffsetY);
        }

        /// <summary>
        /// Draws the entire graph onto the canvas.
        /// </summary>
        /// <param name="canvas"></param>
        public void Draw(SKCanvas canvas)
        {
            // Background Color
            canvas.Clear(new SKColor(20, 20, 20));

            SKPaint paint = new SKPaint();
            paint.Color = SKColors.Blue;
            paint.StrokeWidth = 1;

            // Topmost horizontal line
            canvas.DrawLine(0, 5, _width, 5, paint);

            // Columns
            int columnCount = (int)Math.Round(_width / _columnWidth / Zoom);
            float minutePosition = _columnWidth * Zoom - TimeOffset.Minute * (_columnWidth * Zoom / 60);
            float columnOffset = minutePosition % (_columnWidth * Zoom);
            if (columnOffset < 0) columnOffset += _columnWidth * Zoom;

            // Times for columns
            if (TimeLabels != null)
                foreach (Label label in TimeLabels)
                    label.IsVisible = false;

            if (_dateLabel != null)
                _dateLabel.Text = $"{TimeOffset.Day:00}.{TimeOffset.Month:00}.{TimeOffset.Year:0000}";

            // Columns + day separators
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
                paint.Color = (time == 0) ? SKColors.LightGreen : SKColors.Blue;
                paint.Color = paint.Color.WithAlpha(125);
                paint.StrokeWidth = (time == 0) ? 3 : 1;

                canvas.DrawLine(
                    _columnWidth * Zoom * i + columnOffset,
                    0,
                    _columnWidth * Zoom * i + columnOffset,
                    _height * Zoom + OffsetY,
                    paint
                    );

                paint.Color = SKColors.Red;
                paint.StrokeWidth = 3;

                float x = FromTime(DateTime.Now);
                canvas.DrawLine(
                    x, 0, x, _height * Zoom + OffsetY,
                    paint);
            }

            if (DateView?.TranslationX < 0)
                DateView.TranslationX = 0;

            if (DateView?.TranslationX > Application.Current.MainPage.Width - DateView?.Width)
                DateView.TranslationX = Application.Current.MainPage.Width - DateView.Width;

            paint.PathEffect = null;
            paint.Color = SKColors.Blue;
            paint.StrokeWidth = 1;
            canvas.DrawLine(0, _height * Zoom + OffsetY, _width, _height * Zoom + OffsetY, paint);

            // Scroll Indicator
            paint.Color = SKColors.Gray;
            float offset = 0;
            canvas.DrawLine(5, offset, 5, _height - 200 + offset, paint);
            canvas.DrawRoundRect(0, OffsetY / _maxOffsetY * (_height - 200 - 20) + offset, 10, 20, 5, 5, paint);
            paint.StrokeWidth = 3;
            canvas.DrawLine(0, offset, 10, offset, paint);
            canvas.DrawLine(0, offset + _height - 200, 10, offset + _height - 200, paint);

            // Indicator for adding activities
            paint.Color = SKColors.Green;
            if (Mode == EditingMode.Create && EditMode)
            {
                float height = ActivityButton.DEFAULT_HEIGHT * (Height * (1 - ActivityButton.DEFAULT_HEIGHT));
                float hour = MinuteWidth * 60 * Zoom;
                float left = _mouseX - hour / 2;
                float top = _mouseY - height / 2;
                float width = hour;
                height *= Zoom;
                canvas.DrawRect(left, top, width, height, paint);
            }

            // Buttons
            DrawConnections(canvas);

            foreach (ActivityButton button in ActivityButtons)
            {
                button.Update();
                button.Draw(canvas, _mouseX, _mouseY, EditMode);
            }
        }

        /// <summary>
        /// Reload ActivityButtons with new activities from database at current span of days.
        /// </summary>
        public void ReloadActivities()
        {
            ActivityButtons.Clear();

            var rememberedList = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList;

            TimeSpan maxDifference = new TimeSpan(days: 9, 0, 0, 0);
            for (int i = 0; i < rememberedList.Count; i++)
            {
                LarpActivity activity = rememberedList[i];
                DateTime start = DateTimeExtension.UnixTimeStampMillisecondsToDateTime(activity.start).ToLocalTime();

                if ((start - TimeOffset).Duration() < maxDifference)
                    AddActivity(activity);
            }
        }

        /// <summary>
        /// Calculates GraphY for the LarpActivity from position on the graph.
        /// </summary>
        /// <param name="y">Pixel coordinates.</param>
        /// <param name="height">Pixel coordinates.</param>
        /// <returns></returns>
        public float CalculateGraphY(float y, float height = ActivityButton.DEFAULT_HEIGHT)
        {
            y = (y - height / 2) / Zoom;
            return (y - OffsetY / Zoom) / (Height - height);
        }

        /// <summary>
        /// Connects ActivityButtons on graph that serve as prerequisites to another activity.
        /// </summary>
        /// <param name="canvas"></param>
        public void DrawConnections(SKCanvas canvas)
        {
            foreach (ActivityButton button in ActivityButtons)
                foreach (ActivityButton prerequisite in ActivityButtons)
                    if (button.Activity.prerequisiteIDs.Contains(prerequisite.Activity.ID))
                        DrawConnection(canvas, prerequisite, button);
        }

        /// <summary>
        /// Loops over all ActivityButtons and returns first match in touch location x,y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public ActivityButton GetButtonAt(float x, float y)
        {
            foreach (ActivityButton button in ActivityButtons)
            {
                if (button.GetHitbox(EditMode).Contains(x, y))
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
            var button = new ActivityButton(activity, this, _navigation);
            ActivityButtons.Add(button);

            if (activity.GraphY >= 0)
                return button;

            long startX = activity.start;
            long endX = activity.start + activity.duration;
            double height = ActivityButton.DEFAULT_HEIGHT;
            double startY = 0;
            double endY = startY + height;

            var rememberedList = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList;
            var ySorted = new SortedDictionary<double, LarpActivity>();


            for (int i = 0; i < rememberedList.Count; i++)
            {
                var activity2 = rememberedList[i];
                if (activity2 == activity || activity2.GraphY < 0) continue;
                ySorted.Add(activity2.GraphY, activity2);
            }

            foreach (var activity2 in ySorted.Values)
            {
                if (activity2 == activity) continue;

                long start2X = activity2.start;
                long end2X = activity2.start + activity2.duration;
                double start2Y = activity2.GraphY;
                double end2Y = start2Y + height;

                if (end2X > startX && start2X < endX && end2Y > startY && start2Y < endY)
                {
                    startY = end2Y;
                    endY += start2Y + height;
                }
            }

            button.Activity.GraphY = startY;
            return button;
        }

        public void RemoveActivity(ActivityButton button)
        {
            ActivityButtons.Remove(button);
        }

        /// <summary>
        /// Invalidates CanvasView to redraw the graph.
        /// </summary>
        public void InvalidateSurface()
        {
            _canvasView.InvalidateSurface();
        }

        /// <summary>
        /// Offsets the graph, so the activity is on the screen.
        /// </summary>
        /// <param name="activity"></param>
        public void FocusOnActivity(LarpActivity activity)
        {
            TimeOffset = DateTimeExtension.UnixTimeStampMillisecondsToDateTime(activity.start).ToLocalTime();
            _offsetY = (float)activity.GraphY;
        }

        /// <summary>
        /// Draws a bezier curve between 2 buttons.
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="color"></param>
        public void DrawConnection(SKCanvas canvas, ActivityButton a, ActivityButton b, SKColor? color = null)
        {
            var aRect = new SKRect(
                a.X,
                a.Y,
                a.X + a.Width,
                a.Y + a.Height * Zoom);

            var bRect = new SKRect(
                b.X,
                b.Y,
                b.X + b.Width,
                b.Y + b.Height * Zoom);

            DrawConnection(canvas, aRect.MidX, aRect.MidY, bRect.MidX, bRect.MidY, color);
        }

        /// <summary>
        /// Draws a bezier curve between a button and a point on the graph.
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="a"></param>
        /// <param name="toX"></param>
        /// <param name="toY"></param>
        /// <param name="color"></param>
        public void DrawConnection(SKCanvas canvas, ActivityButton a, float toX, float toY, SKColor? color = null)
        {
            var aRect = new SKRect(
                a.X,
                a.Y,
                a.X + a.Width,
                a.Y + a.Height * Zoom);

            DrawConnection(canvas, aRect.MidX, aRect.MidY, toX, toY, color);
        }

        /// <summary>
        /// Draws a bezier curve between 2 points on the graph.
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="fromX"></param>
        /// <param name="fromY"></param>
        /// <param name="toX"></param>
        /// <param name="toY"></param>
        /// <param name="color"></param>
        public void DrawConnection(SKCanvas canvas, float fromX, float fromY, float toX, float toY, SKColor? color = null)
        {
            var paint = new SKPaint();
            paint.Color = color ?? SKColors.WhiteSmoke;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 2;
            paint.IsAntialias = true;

            // Draw the curve
            //===========================
            SKPoint p0 = new SKPoint(fromX, fromY);
            SKPoint p1 = new SKPoint((toX - fromX) / 2 + fromX, toY);
            SKPoint p2 = new SKPoint(toX, toY);

            SKPath path = new SKPath();
            path.MoveTo(p0);
            path.QuadTo(p1, p2);
            canvas.DrawPath(path, paint);

            // Draw the arrow
            //===========================
            float t = 0.5f;
            float v = 1 - t;

            SKPoint midPoint = new SKPoint(
                // Computation of quadratic bezier at t
                p1.X + v * v * (p0.X - p1.X) + t * t * (p2.X - p1.X),
                p1.Y + v * v * (p0.Y - p1.Y) + t * t * (p2.Y - p1.Y));

            SKPoint difference = new SKPoint(
                // Computation of the derivative of quadratic bezier at t
                2 * v * (p1.X - p0.X) + 2 * t * (p2.X - p1.X),
                2 * v * (p1.Y - p0.Y) + 2 * t * (p2.Y - p1.Y));

            float distance = difference.Length;
            SKPoint direction = new SKPoint(difference.X / distance, difference.Y / distance);
            SKPoint normal = new SKPoint(direction.Y, -direction.X);
            float length = 20;

            SKPoint arrowPoint1 = new SKPoint(
                midPoint.X - direction.X * length + normal.X * length / 2,
                midPoint.Y - direction.Y * length + normal.Y * length / 2);

            SKPoint arrowPoint2 = new SKPoint(
                midPoint.X - direction.X * length - normal.X * length / 2,
                midPoint.Y - direction.Y * length - normal.Y * length / 2);

            canvas.DrawLine(midPoint, arrowPoint1, paint);
            canvas.DrawLine(midPoint, arrowPoint2, paint);
        }
    }
}