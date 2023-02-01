using LAMA.Extensions;
using LAMA;
using LAMA.Models;
using LAMA.Views;
using SkiaSharp;
using System;
using Xamarin.Forms;

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
        public LarpActivity Activity { get; private set; }
        enum EditState { Left, Right, Move }
        private EditState _editState;
        public ActivityButton(LarpActivity activity, ActivityGraph activityGraph)
        {
            Activity = activity;
            _activityGraph = activityGraph;
            Text = activity.name;
            VerticalOptions = LayoutOptions.Start;
            HorizontalOptions = LayoutOptions.Start;
            _editState = EditState.Move;
            activity.duration = 60;// TODO: DELETE When actual duration works

            // Clicking the button displays the activity
            Clicked += (object sender, EventArgs e) => Navigation.PushAsync(new DisplayActivityPage(activity));
        }

        /// <summary>
        /// Updates position and visual of the button
        /// </summary>
        public void Update()
        {
            var now = DateTime.Now;
            //var time = new DateTime(now.Year, now.Month, _activity.day, _activity.start.hours, _activity.start.minutes, 0);
            var start = DateTimeExtension.UnixTimeStampMillisecondsToDateTime(Activity.start);
            //var time = new DateTime(now.Year, now.Month, 1, start.Hour, start.Minute, 0);
            var span = start - _activityGraph.TimeOffset;

            Activity.GraphY = Math.Max(0, Activity.GraphY);
            TranslationX = _activityGraph.FromPixels((float)span.TotalMinutes * _activityGraph.MinuteWidth * _activityGraph.Zoom);
            TranslationY = _activityGraph.FromPixels((float)Activity.GraphY * _activityGraph.Zoom + _activityGraph.OffsetY);
            WidthRequest = _activityGraph.FromPixels(Activity.duration * _activityGraph.MinuteWidth * _activityGraph.Zoom);

            IsVisible = TranslationY >= - Height / 3;
            TextColor = Color.Black;
            Text = Activity.name;
            BackgroundColor = GetColor(Activity.status);
            CornerRadius = GetCornerRadius(Activity.eventType);
        }

        /// <summary>
        /// Draws indicators for resizing activity.
        /// </summary>
        /// <param name="canvas"></param>
        public void DrawBoders(SKCanvas canvas)
        {
            var paint = new SKPaint();
            paint.Color = SKColors.Green.WithAlpha(125);
            float x = _activityGraph.ToPixels((float)TranslationX);
            float y = _activityGraph.ToPixels((float)TranslationY);
            float w = _activityGraph.ToPixels((float)Width);
            float h = _activityGraph.ToPixels((float)Height);

            var lRect = new SKRect(x, y, x + _sideWidth, y + h);
            var rRect = new SKRect(x + w - _sideWidth, y, x + w, y + h);

            paint.Color = (_editState == EditState.Left) ? SKColors.Red : SKColors.Green;
            canvas.DrawRect(lRect, paint);

            paint.Color = (_editState == EditState.Right) ? SKColors.Red : SKColors.Green;
            canvas.DrawRect(rRect, paint);
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

            if (yRel < 0 || yRel > Height || xRel < 0 || yRel > Width)
                return;

            float sideWidthXam = _activityGraph.FromPixels(_sideWidth);
            if (xRel < sideWidthXam) _editState = EditState.Left;
            if (xRel > Width - sideWidthXam) _editState = EditState.Right;
        }

        /// <summary>
        /// Release button in edit mode.
        /// </summary>
        public void ReleaseEdit()
        {
            _editState = EditState.Move;
        }

        /// <summary>
        /// Edits the activity according to mouse move and button state.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void MoveEdit(float x, float y)
        {
            const long minimalDuration = 10;
            if (_editState == EditState.Left)
            {
                DateTime at = _activityGraph.ToTime(x);
                long duration = (long)(DateTimeExtension.UnixTimeStampMillisecondsToDateTime(Activity.start) - at).TotalMinutes + Activity.duration;

                if (duration > minimalDuration)
                {
                    Activity.duration = duration;
                    Activity.start = at.ToUnixTimeMilliseconds();
                }
            }

            if (_editState == EditState.Right)
            {
                DateTime at = _activityGraph.ToTime(x);
                long duration = (long)(at - DateTimeExtension.UnixTimeStampMillisecondsToDateTime(Activity.start)).TotalMinutes;
                if (duration >= minimalDuration)
                    Activity.duration = duration;
            }

            if (_editState == EditState.Move)
            {
                // X -> time
                DateTime newTime = _activityGraph.ToTime(x - (float)Width / 2);
                Activity.start = newTime.ToUnixTimeMilliseconds();
                Activity.day = newTime.Day;

                // Y
                y = _activityGraph.ToPixels(y - (float)Height / 2 - _activityGraph.XamOffset);
                Activity.GraphY = y / _activityGraph.Zoom - _activityGraph.OffsetY / _activityGraph.Zoom;
            }
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
                requiredItems: new EventList<Pair<int, int>>(),
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
