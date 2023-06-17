using LAMA.Extensions;
using LAMA;
using LAMA.Models;
using LAMA.Views;
using SkiaSharp;
using System;
using Xamarin.Forms;
using System.Diagnostics;
using LAMA.Themes;

namespace LAMA.ActivityGraphLib
{
    /// <summary>
    /// Represents Activity on the graph with duration as start.
    /// Horizontal attributes X and Width represent start and duration.
    /// Vertical attributes are purely for visual clarity.
    /// </summary>
    public class ActivityButton
    {
        private float _sideWidth = 10;
        private bool _isVisible;
        private INavigation _navigation;
        private float _sideWidthXam => _graph.FromPixels(_sideWidth);
        private enum EditState { None, Left, Right, Move, Connect, Disconnect }
        private EditState _editState;

        private ActivityGraph _graph;
        private float _offsetX;
        private float _offsetY;

        public LarpActivity Activity;
        public const float DEFAULT_HEIGHT = 0.05f;

        /// <summary>
        /// Represents start of the activity.
        /// </summary>
        public float X { get; private set; }
        public float Y { get; private set; }
        /// <summary>
        /// Represents duration.
        /// </summary>
        public float Width { get; private set; }
        /// <summary>
        /// Percentual of Graph Height.
        /// </summary>
        public float Height => DEFAULT_HEIGHT * (_graph.Height * (1 - DEFAULT_HEIGHT));

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
                    float mX = mouseX - _offsetX;
                    float mY = mouseY - _offsetY;

                    paint.IsAntialias = true;
                    paint.Color = SKColors.Orange.WithAlpha(125);
                    canvas.DrawRect(mX, mY, Width, Height * _graph.Zoom, paint);
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 15f, 10f }, -_graph.OffsetY);
                    canvas.DrawLine(mX, mY, mX, 0, paint);
                    canvas.DrawLine(mX + Width, mY, mX + Width, 0, paint);
                }

            if (_editState == EditState.Connect)
                _graph.DrawConnection(canvas, this, mouseX, mouseY);

            if (_editState == EditState.Disconnect)
                _graph.DrawConnection(canvas, this, mouseX, mouseY, SKColors.Red);
        }

        /// <summary>
        /// Call the actual functionality of the button.
        /// </summary>
        public void Click()
        {
            _navigation.PushAsync(new ActivityDetailsPage(Activity));
        }

        /// <summary>
        /// Click on button in edit mode to edit the activity.
        /// </summary>
        /// <param name="x">In pixel coordinates.</param>
        /// <param name="y">In pixel coordinates.</param>
        public void ClickEdit(float x, float y)
        {
            _offsetX = x - X;
            _offsetY = y - Y;

            _editState = EditState.Move;
            if (_graph.Mode == ActivityGraph.EditingMode.Connect) _editState = EditState.Connect;
            if (_graph.Mode == ActivityGraph.EditingMode.Disconnect) _editState = EditState.Disconnect;

            // Clicked inside of the button - no adjusting width
            if (_offsetY < 0 || _offsetY > Height * _graph.Zoom || _offsetX < -_sideWidth || _offsetX > Width + _sideWidth)
                return;

            // Adjusting width
            if (_offsetX < 0) _editState = EditState.Left;
            if (_offsetX > Width) _editState = EditState.Right;
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
                DateTime newTime = _graph.ToLocalTime(x - _offsetX);
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
                y = (y - _offsetY) / _graph.Zoom;
                Activity.GraphY = (y - _graph.OffsetY / _graph.Zoom) / (_graph.Height - Height);
            }

            if (_editState == EditState.Connect)
            {
                ActivityButton b = _graph.GetButtonAt(x, y);
                if (b != null && b != this && !b.Activity.prerequisiteIDs.Contains(Activity.ID))
                    b.Activity.prerequisiteIDs.Add(Activity.ID);
            }

            if (_editState == EditState.Disconnect)
            {
                ActivityButton b = _graph.GetButtonAt(x, y);
                if (b != null && b != this && b.Activity.prerequisiteIDs.Contains(Activity.ID))
                    b.Activity.prerequisiteIDs.Remove(Activity.ID);
            }

            _editState = EditState.None;
        }

        /// <summary>
        /// Moves only in y-axis.
        /// </summary>
        /// <param name="y">In Pixel coordinates.</param>
        public void MoveY(float y) => Activity.GraphY = y - _graph.OffsetY;

        /// <summary>
        /// Bounds on the canvas. Is wider when editing, with added bars on sides.
        /// </summary>
        /// <param name="editMode"></param>
        /// <returns></returns>
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
        /// Preparation is round.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
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
