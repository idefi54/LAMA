using LAMA.Extensions;
using LAMA;
using LAMA.Models;
using LAMA.Views;
using SkiaSharp;
using System;
using Xamarin.Forms;
using System.Diagnostics;
using LAMA.Colors;

namespace LAMA.ActivityGraphLib
{

    public class ActivityButton
    {
        private float X, Y;
        private float Width, Height = DEFAULT_HEIGHT;
        private float _sideWidth = 10;
        private bool _isVisible;
        private INavigation _navigation;
        private float _sideWidthXam => _graph.FromPixels(_sideWidth);
        private enum EditState { None, Left, Right, Move }
        private EditState _editState;

        private ActivityGraph _graph;

        public LarpActivity Activity;
        public const float DEFAULT_HEIGHT = 20;

        public ActivityButton(LarpActivity activity, ActivityGraph activityGraph, INavigation navigation)
        {
            Activity = activity;
            _graph = activityGraph;
            _navigation = navigation;
            _editState = EditState.None;

            // Clicking the button displays the activity
            //Clicked += (object sender, EventArgs e) => Navigation.PushAsync(new DisplayActivityPage(activity));

            SQLEvents.dataChanged += SQLEvents_dataChanged;
            SQLEvents.dataDeleted += SQLEvents_dataDeleted;
        }

        private void SQLEvents_dataDeleted(Serializable deleted)
        {
            if (deleted != Activity)
                return;

            _graph.RemoveActivity(this);
            _graph.InvalidateSurface();
        }

        private void SQLEvents_dataChanged(Serializable changed, int changedAttributeIndex)
        {
            if (changed != Activity)
                return;

            Update();
            _graph.InvalidateSurface();
        }

        /// <summary>
        /// Updates position and visual of the button
        /// </summary>
        public void Update()
        {
            var start = DateTimeExtension.UnixTimeStampMillisecondsToDateTime(Activity.start).ToLocalTime();
            var span = start - _graph.TimeOffset;

            double newGraphY = Math.Max(0, Math.Min(1, Activity.GraphY));
            if (newGraphY != Activity.GraphY) Activity.GraphY = newGraphY;
            float y = (float)Activity.GraphY * (_graph.Height - Height);

            long durationMinutes = Activity.duration / 1000 / 60;
            X = (float)span.TotalMinutes * _graph.MinuteWidth * _graph.Zoom;
            Y = y * _graph.Zoom + _graph.OffsetY;
            Width = durationMinutes * _graph.MinuteWidth * _graph.Zoom;

            _isVisible = y >= -Height / 3;
        }

        /// <summary>
        /// Draws indicators for resizing and moving an activity.
        /// </summary>
        /// <param name="canvas"></param>
        public void Draw(SKCanvas canvas, float mouseX, float mouseY, bool editing = false)
        {
            if (!_isVisible) return;

            // Draw the button
            //=======================
            using (var paint = new SKPaint())
            {
                // Fill
                paint.IsAntialias = true;
                paint.Color = Activity.GetGraphColor();
                paint.Style = SKPaintStyle.Fill;
                if (editing) paint.Color = paint.Color.WithAlpha(125);
                int radius = GetCornerRadius(Activity.eventType);
                canvas.DrawRoundRect(X, Y, Width, Height * _graph.Zoom, radius, radius, paint);

                // Outline
                paint.Color = SKColors.Black;
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = 1.5f;
                canvas.DrawRoundRect(X, Y, Width, Height * _graph.Zoom, radius, radius, paint);
            }

            // Text on the button
            using (var paint = new SKPaint())
            {
                // Commented out breaking the text.
                //int count = (int)textPaint.BreakText(Activity.name, Width, out float textWidth);
                //var text = Activity.name.Substring(0, count);

                // Text overflows instead.
                string text = Activity.name;
                float textWidth = paint.MeasureText(text);

                float tx = X + Width / 2 - textWidth / 2;
                float ty = Y + Height * _graph.Zoom * 0.6f;

                // Normal
                paint.TextSize = Height * _graph.Zoom * 0.4f;
                paint.IsAntialias = true;
                paint.FakeBoldText = true;
                paint.Color = SKColors.Black;
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = 3f;// * _graph.Zoom;
                canvas.DrawText(text, tx, ty, paint);

                // Normal
                //paint.FakeBoldText = false;
                paint.Color = SKColors.White;
                paint.Style = SKPaintStyle.Fill;
                canvas.DrawText(text, tx, ty, paint);
            }

            if (!editing)
                return;

            // Draw Left
            //=======================
            using (var paint = new SKPaint())
            {
                float lx = X;

                paint.IsAntialias = true;
                paint.Color = SKColors.Green;
                if (_editState == EditState.Left)
                {
                    lx = mouseX + _sideWidth / 2;
                    paint.Color = SKColors.Orange;
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 15f, 10f }, -_graph.OffsetY);
                    canvas.DrawLine(lx, Y, lx, 0, paint);
                    paint.PathEffect = null;
                }

                var lRect = new SKRect(lx - _sideWidth, Y, lx, Y + Height * _graph.Zoom);
                canvas.DrawRect(lRect, paint);
            }

            // Draw Right
            //=======================
            using (var paint = new SKPaint())
            {
                float rx = X + Width;
                paint.IsAntialias = true;
                paint.Color = SKColors.Green;
                if (_editState == EditState.Right)
                {
                    rx = mouseX - _sideWidth / 2;
                    paint.Color = SKColors.Orange;
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 15f, 10f }, -_graph.OffsetY);
                    canvas.DrawLine(rx, Y, rx, 0, paint);
                    paint.PathEffect = null;
                }

                var rRect = new SKRect(rx, Y, rx + _sideWidth, Y + Height * _graph.Zoom);
                canvas.DrawRect(rRect, paint);
            }

            // Draw Move
            //=======================
            using (var paint = new SKPaint())
                if (_editState == EditState.Move)
                {
                    float mX = mouseX - Width / 2;
                    float mY = mouseY - Height / 2;

                    paint.IsAntialias = true;
                    paint.Color = SKColors.Orange.WithAlpha(125);
                    canvas.DrawRect(mX, mY, Width, Height * _graph.Zoom, paint);
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 15f, 10f }, -_graph.OffsetY);
                    canvas.DrawLine(mX, mY, mX, 0, paint);
                    canvas.DrawLine(mX + Width, mY, mX + Width, 0, paint);
                }
        }

        /// <summary>
        /// Click on button in edit mode to edit the activity.
        /// </summary>
        /// <param name="x">In pixel coordinates.</param>
        /// <param name="y">In pixel coordinates.</param>
        public void ClickEdit(float x, float y)
        {
            float xRel = x - X;
            float yRel = y - Y;

            _editState = EditState.Move;
            if (yRel < 0 || yRel > Height * _graph.Zoom || xRel < -_sideWidth || xRel > Width + _sideWidth)
                return;

            if (xRel < 0) _editState = EditState.Left;
            if (xRel > Width) _editState = EditState.Right;
        }

        public void Click()
        {
            _navigation.PushAsync(new DisplayActivityPage(Activity));
        }

        /// <summary>
        /// Edits the activity according to mouse position and ActivityButton state.
        /// </summary>
        /// <param name="x">In pixel coordinates.</param>
        /// <param name="y">In pixel coordinates.</param>
        public void ReleaseEdit(float x, float y)
        {
            const long minimalDuration = 10 * 1000 * 60;

            if (_editState == EditState.Left)
            {
                long at = _graph.ToLocalTime(x).ToUnixTimeMilliseconds();
                at = at - (at % (60 * 1000 * 5)); // Milliseconds round to 5 minutes
                long duration = Activity.start - at + Activity.duration;

                if (duration > minimalDuration)
                {
                    Activity.duration = duration;
                    Activity.start = at;
                }
            }

            if (_editState == EditState.Right)
            {
                long at = _graph.ToLocalTime(x).ToUnixTimeMilliseconds();
                long duration = at - Activity.start;
                duration = duration - (duration % (60 * 1000 * 5)); // Milliseconds round to 5 minutes
                if (duration >= minimalDuration)
                    Activity.duration = duration;
            }

            if (_editState == EditState.Move)
            {
                // X -> time
                DateTime newTime = _graph.ToLocalTime(x - Width / 2);
                newTime = new DateTime(
                    newTime.Year,
                    newTime.Month,
                    newTime.Day,
                    newTime.Hour,
                    newTime.Minute - newTime.Minute % 5,
                    0);


                Activity.start = newTime.ToUnixTimeMilliseconds();
                Activity.day = newTime.Day;

                // Y
                y = (y - Height / 2) / _graph.Zoom;
                Activity.GraphY = (y - _graph.OffsetY / _graph.Zoom) / (_graph.Height - Height);
            }

            _editState = EditState.None;
        }

        /// <summary>
        /// Moves only in y-axis.
        /// </summary>
        /// <param name="y">In Pixel coordinates.</param>
        public void MoveY(float y) => Activity.GraphY = y - _graph.OffsetY;

        public SKRect GetHitbox(bool editMode)
        {
            if (!editMode)
                return new SKRect(X, Y, X + Width, Y + Height * _graph.Zoom);

            return new SKRect(
                X - _sideWidth,
                Y,
                X + Width + _sideWidth,
                Y + Height * _graph.Zoom
                );
        }

        /// <summary>
        /// Draws bezier curve between 2 buttons.
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="graph"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void DrawConnection(SKCanvas canvas, ActivityGraph graph, ActivityButton a, ActivityButton b)
        {
            var aRect = new SKRect(a.X,
                a.Y - graph.OffsetY,
                a.X + a.Width,
                a.Y - graph.OffsetY + a.Height);

            var bRect = new SKRect(
                b.X,
                b.Y - graph.OffsetY,
                b.X + b.Width,
                b.Y - graph.OffsetY + b.Height);

            var paint = new SKPaint();
            paint.Color = SKColors.WhiteSmoke;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 2;

            SKPath path = new SKPath();
            paint.IsAntialias = true;
            path.MoveTo(aRect.MidX, aRect.MidY);
            path.QuadTo(aRect.MidX, bRect.MidY, bRect.MidX, bRect.MidY);
            canvas.DrawPath(path, paint);
        }

        private int GetCornerRadius(LarpActivity.EventType type)
        {
            if (type == LarpActivity.EventType.preparation)
                return 20;

            if (type == LarpActivity.EventType.normal)
                return 0;

            return 0;
        }
    }
}
