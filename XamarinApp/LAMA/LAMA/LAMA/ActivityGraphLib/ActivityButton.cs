﻿using LAMA.Extensions;
using LAMA;
using LAMA.Models;
using LAMA.Views;
using SkiaSharp;
using System;
using Xamarin.Forms;
using System.Diagnostics;

namespace LAMA.ActivityGraphLib
{
    /// <summary>
    /// Button containing LarpActivity and logic to be drawn on ActivityGraph.
    /// ActivityButton specific methods affect the activity itself.
    /// </summary>
    public class ActivityButton : Button
    {
        private ActivityGraph _activityGraph;
        private float _sideWidth = 10;
        private float _xamSideWidth => _activityGraph.FromPixels(_sideWidth);
        public LarpActivity Activity { get; private set; }
        public Rectangle ExtendedHitbox => new Rectangle(TranslationX - _xamSideWidth, TranslationY, Width + _xamSideWidth*2, Height);
       
        enum EditState { None, Left, Right, Move }
        private EditState _editState;
        public ActivityButton(LarpActivity activity, ActivityGraph activityGraph)
        {
            Activity = activity;
            _activityGraph = activityGraph;
            Text = activity.name;
            VerticalOptions = LayoutOptions.Start;
            HorizontalOptions = LayoutOptions.Start;
            _editState = EditState.None;

            // Clicking the button displays the activity
            Clicked += (object sender, EventArgs e) => Navigation.PushAsync(new DisplayActivityPage(activity));

            SQLEvents.dataChanged += SQLEvents_dataChanged;
            SQLEvents.dataDeleted += SQLEvents_dataDeleted;
        }

        private void SQLEvents_dataDeleted(Serializable deleted)
        {
            if (deleted != Activity)
                return;

            _activityGraph.RemoveActivity(this);
        }

        private void SQLEvents_dataChanged(Serializable changed, int changedAttributeIndex)
        {
            if (changed != Activity)
                return;

            Update();
        }

        /// <summary>
        /// Updates position and visual of the button
        /// </summary>
        public void Update()
        {
            var start = DateTimeExtension.UnixTimeStampMillisecondsToDateTime(Activity.start).ToLocalTime();
            var span = start - _activityGraph.TimeOffset;

            double newGraphY = Math.Max(0, Math.Min(1, Activity.GraphY));
            if (newGraphY != Activity.GraphY) Activity.GraphY = newGraphY;
            float y = (float)Activity.GraphY * (_activityGraph.Height - (float)Height);


            //Activity.GraphY = Math.Max(0, Activity.GraphY);
            //Activity.GraphY = Math.Min(_activityGraph.Height - Height, Activity.GraphY);
            long durationMinutes = Activity.duration / 1000 / 60;
            TranslationX = _activityGraph.FromPixels((float)span.TotalMinutes * _activityGraph.MinuteWidth * _activityGraph.Zoom);
            TranslationY = _activityGraph.FromPixels(y * _activityGraph.Zoom + _activityGraph.OffsetY);
            WidthRequest = _activityGraph.FromPixels(durationMinutes * _activityGraph.MinuteWidth * _activityGraph.Zoom);

            IsVisible = TranslationY >= - Height / 3;
            TextColor = Color.Black;
            Text = Activity.name;
            BackgroundColor = GetColor(Activity.status);
            CornerRadius = GetCornerRadius(Activity.eventType);
        }

        /// <summary>
        /// Draws indicators for resizing and moving an activity.
        /// </summary>
        /// <param name="canvas"></param>
        public void DrawIndicators(SKCanvas canvas, float mouseX, float mouseY)
        {
            var paint = new SKPaint();
            float x = _activityGraph.ToPixels((float)TranslationX);
            float y = _activityGraph.ToPixels((float)TranslationY);
            float w = _activityGraph.ToPixels((float)Width);
            float h = _activityGraph.ToPixels((float)Height);

            // Draw Left
            //=======================
            {
                float lx = x;
                paint.Color = SKColors.Green;
                if (_editState == EditState.Left)
                {
                    lx = mouseX + _sideWidth / 2;
                    paint.Color = SKColors.Orange;
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 15f, 10f }, -_activityGraph.OffsetY);
                    canvas.DrawLine(lx, y, lx, 0, paint);
                    paint.PathEffect = null;
                }

                var lRect = new SKRect(lx - _sideWidth, y, lx, y + h);
                canvas.DrawRect(lRect, paint);
            }

            // Draw Right
            //=======================
            {
                float rx = x + w;
                paint.Color = SKColors.Green;
                if (_editState == EditState.Right)
                {
                    rx = mouseX - _sideWidth / 2;
                    paint.Color = SKColors.Orange;
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 15f, 10f }, -_activityGraph.OffsetY);
                    canvas.DrawLine(rx, y, rx, 0, paint);
                    paint.PathEffect = null;
                }

                var rRect = new SKRect(rx, y, rx + _sideWidth, y + h);
                canvas.DrawRect(rRect, paint);
            }

            // Draw Move
            //=======================
            if (_editState == EditState.Move)
            {
                float mX = mouseX - w / 2;
                float mY = mouseY - h / 2;
                paint.Color = SKColors.Orange.WithAlpha(125);
                canvas.DrawRect(mX, mY, w, h, paint);
                paint.PathEffect = SKPathEffect.CreateDash(new float[] { 15f, 10f }, -_activityGraph.OffsetY);
                canvas.DrawLine(mX, mY, mX, 0, paint);
                canvas.DrawLine(mX + w, mY, mX + w, 0, paint);
            }

            // Debug
            //========================
            var r = new SKRect(
                _activityGraph.ToPixels((float)ExtendedHitbox.Left) - 1,
                _activityGraph.ToPixels((float)ExtendedHitbox.Top) - 1,
                _activityGraph.ToPixels((float)ExtendedHitbox.Right) + 1,
                _activityGraph.ToPixels((float)ExtendedHitbox.Bottom) + 1);

            canvas.DrawRect(r, paint);

        }

        /// <summary>
        /// Click on button in edit mode to edit the activity.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void ClickEdit(float x, float y)
        {
            float xRel = x - (float)TranslationX;
            float yRel = y - (float)TranslationY - _activityGraph.XamOffset;

            float xamSideWidth = _activityGraph.FromPixels(_sideWidth);
            _editState = EditState.Move;
            if (yRel < 0 || yRel > Height || xRel < -xamSideWidth || xRel > Width + xamSideWidth)
                return;

            if (xRel < 0) _editState = EditState.Left;
            if (xRel > Width) _editState = EditState.Right;
        }

        /// <summary>
        /// Edits the activity according to mouse position and ActivityButton state.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void ReleaseEdit(float x, float y)
        {
            const long minimalDuration = 10 * 1000 * 60;

            if (_editState == EditState.Left)
            {
                long at = _activityGraph.ToLocalTime(x).ToUnixTimeMilliseconds();
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
                long at = _activityGraph.ToLocalTime(x).ToUnixTimeMilliseconds();
                long duration = at - Activity.start;
                duration = duration - (duration % (60 * 1000 * 5)); // Milliseconds round to 5 minutes
                if (duration >= minimalDuration)
                    Activity.duration = duration;
            }

            if (_editState == EditState.Move)
            {
                // X -> time
                DateTime newTime = _activityGraph.ToLocalTime(x - (float)Width / 2);
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
                y = _activityGraph.ToPixels((y - (float)Height / 2 - _activityGraph.XamOffset) / _activityGraph.Zoom);
                Activity.GraphY = (y  - _activityGraph.OffsetY / _activityGraph.Zoom) / (_activityGraph.Height - (float)Height);
            }

            _editState = EditState.None;
        }

        /// <summary>
        /// Moves only in y-axis.
        /// </summary>
        /// <param name="y"></param>
        public void MoveY(float y) => Activity.GraphY = y - _activityGraph.XamOffset;

        private int GetCornerRadius(LarpActivity.EventType type)
        {
            if (type == LarpActivity.EventType.preparation)
                return 20;

            if (type == LarpActivity.EventType.normal)
                return 0;

            return 0;
        }

        private Color GetColor(LarpActivity.Status status)
        {
            switch (status)
            {
                case LarpActivity.Status.awaitingPrerequisites:
                    return Color.White;
                case LarpActivity.Status.readyToLaunch:
                    return Color.LightBlue;
                case LarpActivity.Status.launched:
                    return Color.LightGreen;
                case LarpActivity.Status.inProgress:
                    return Color.PeachPuff;
                case LarpActivity.Status.completed:
                    return Color.Gray;
                case LarpActivity.Status.cancelled:
                    return Color.Red;
                default:
                    return Color.White;
            }
        }

        /// <summary>
        /// Test Method for fast creating LarpActivities.
        /// TODO: Delete later.
        /// </summary>
        /// <param name="durationMinutes"></param>
        /// <param name="startMinutes"></param>
        /// <param name="day"></param>
        /// <param name="status"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static LarpActivity CreateActivity(int durationMinutes, int startMinutes, int day, LarpActivity.Status status = LarpActivity.Status.readyToLaunch, LarpActivity.EventType type = LarpActivity.EventType.normal, string name = "test", string description = "Test description")
        {
            return new LarpActivity(
                ID: 0,
                name: name,
                description: description,
                preparation: "",
                eventType: type,
                prerequisiteIDs: new EventList<long>(),
                duration: durationMinutes,
                day: day,
                start: startMinutes,
                place: new Pair<double, double>(0.0, 0.0),
                status: status,
                requiredItems: new EventList<Pair<long, int>>(),
                roles: new EventList<Pair<string, int>>(),
                registrations: new EventList<Pair<long, string>>()
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
            float ax = graph.ToPixels((float)a.TranslationX);
            float ay = graph.ToPixels((float)a.TranslationY - graph.XamOffset);
            float aWidth = graph.ToPixels((float)a.Width);
            float aHeight = graph.ToPixels((float)a.Height);
            var aRect = new SKRect(ax, ay, ax + aWidth, ay + aHeight);

            float bx = graph.ToPixels((float)b.TranslationX);
            float by = graph.ToPixels((float)b.TranslationY - graph.XamOffset);
            float bWidth = graph.ToPixels((float)b.Width);
            float bHeight = graph.ToPixels((float)b.Height);
            var bRect = new SKRect(bx, by, bx + bWidth, by + bHeight);

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
    }
}
