using LAMA.Models;
using LAMA.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ActivityGraphLib
{
    public class ActivityButton : Button
    {
        private LarpActivity _activity;
        private ActivityGraph _activityGraph;
        private float _yPos;
        public ActivityButton(LarpActivity activity, ActivityGraph activityGraph)
        {
            _activity = activity;
            _activityGraph = activityGraph;
            Text = activity.name;
            VerticalOptions = LayoutOptions.Start;
            HorizontalOptions = LayoutOptions.Start;
            _yPos = 20;
            Clicked += (object sender, EventArgs e) => { Navigation.PushAsync(new DisplayActivityPage(activity)); };
        }

        public void Update()
        {
            var now = DateTime.Now;
            var time = new DateTime(now.Year, now.Month, _activity.day, _activity.start.hours, _activity.start.minutes, 0);
            var span = time - ActivityGraph.TimeOffset;

            TranslationX = _activityGraph.FromPixels((float)span.TotalMinutes * _activityGraph.MinuteWidth * _activityGraph.Zoom);
            TranslationY = _activityGraph.FromPixels(_yPos * _activityGraph.Zoom + _activityGraph.OffsetY) + _activityGraph.XamOffset;
            IsVisible = TranslationY >= _activityGraph.XamOffset - Height / 3;

            TextColor = Color.Black;
            BackgroundColor = GetColor(_activity.status);
            CornerRadius = GetCornerRadius(_activity.eventType);
        }


        public void Move(float x, float y)
        {
            var now = DateTime.Now;
            x = _activityGraph.ToPixels(x - (float)Width / 2);
            y = _activityGraph.ToPixels(y - (float)Height / 2 - _activityGraph.XamOffset);
            _yPos = y / _activityGraph.Zoom - _activityGraph.OffsetY / _activityGraph.Zoom;

            float minutes = x / _activityGraph.MinuteWidth / _activityGraph.Zoom;
            DateTime newTime = ActivityGraph.TimeOffset.AddMinutes(minutes);
            _activity.start.setRawMinutes(newTime.Hour * 60 + newTime.Minute);
            _activity.day = newTime.Day;
        }
        public void MoveY(float y) => _yPos = y;

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

        public void DrawConnection(SKCanvas canvas, ActivityButton b)
        {
            ActivityButton a = this;

            float ax = _activityGraph.ToPixels((float)a.TranslationX);
            float ay = _activityGraph.ToPixels((float)a.TranslationY - _activityGraph.XamOffset);
            float aWidth = _activityGraph.ToPixels((float)a.Width);
            float aHeight = _activityGraph.ToPixels((float)a.Height);
            var aRect = new SKRect(ax, ay, ax + aWidth, ay + aHeight);

            float bx = _activityGraph.ToPixels((float)b.TranslationX);
            float by = _activityGraph.ToPixels((float)b.TranslationY - _activityGraph.XamOffset);
            float bWidth = _activityGraph.ToPixels((float)b.Width);
            float bHeight = _activityGraph.ToPixels((float)b.Height);
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
