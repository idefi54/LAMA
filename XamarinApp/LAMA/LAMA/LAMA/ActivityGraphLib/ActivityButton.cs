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
        private float _yPos;
        public LarpActivity Activity { get; private set; }
        public ActivityButton(LarpActivity activity, ActivityGraph activityGraph)
        {
            Activity = activity;
            _activityGraph = activityGraph;
            Text = activity.name;
            VerticalOptions = LayoutOptions.Start;
            HorizontalOptions = LayoutOptions.Start;
            _yPos = 20; // TODO: Make load from button database when available

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
            var time = new DateTime(now.Year, now.Month, 1, Activity.start.hours, Activity.start.minutes, 0);
            var span = time - _activityGraph.TimeOffset;

            _yPos = Math.Max(0, _yPos);
            TranslationX = _activityGraph.FromPixels((float)span.TotalMinutes * _activityGraph.MinuteWidth * _activityGraph.Zoom);
            TranslationY = _activityGraph.FromPixels(_yPos * _activityGraph.Zoom + _activityGraph.OffsetY) + _activityGraph.XamOffset;
            IsVisible = TranslationY >= - Height / 3;

            TextColor = Color.Black;
            Text = Activity.name;
            BackgroundColor = GetColor(Activity.status);
            CornerRadius = GetCornerRadius(Activity.eventType);
        }

        /// <summary>
        /// Moves the button to (x,y) and updates LarpActivity accordingly.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Move(float x, float y)
        {
            // X -> time
            DateTime newTime = _activityGraph.ToTime(x - (float)Width / 2);
            Activity.start.setRawMinutes(newTime.Hour * 60 + newTime.Minute);
            Activity.day = newTime.Day;

            // Y
            y = y - _activityGraph.XamOffset;
            y = _activityGraph.ToPixels(y - (float)Height / 2 - _activityGraph.XamOffset);
            _yPos = y / _activityGraph.Zoom - _activityGraph.OffsetY / _activityGraph.Zoom;
        }

        /// <summary>
        /// Moves only in y-axis.
        /// </summary>
        /// <param name="y"></param>
        public void MoveY(float y) => _yPos = y - _activityGraph.XamOffset;

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
                prerequisiteIDs: new EventList<int>(),
                duration: new Time(durationMinutes),
                day: day,
                start: new Time(startMinutes),
                place: new Pair<double, double>(0.0, 0.0),
                status: status,
                requiredItems: new EventList<Pair<int, int>>(),
                roles: new EventList<Pair<string, int>>(),
                registrations: new EventList<Pair<int, string>>()
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
