﻿using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using TouchTracking;
using TouchTracking.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using LAMA.Models;
using System.Collections.Generic;

namespace LAMA.Views
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
            IsVisible = TranslationY >= _activityGraph.XamOffset- Height / 3;

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

        public static LarpActivity CreateActivity(int durationMinutes, int startMinutes, int day, LarpActivity.Status status = LarpActivity.Status.readyToLaunch, LarpActivity.EventType type = LarpActivity.EventType.normal ,string name = "test", string description = "Test description")
        {
            return new LarpActivity(
                ID:0,
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

    public class ActivityGraph
    {
        // private
        private float _width;
        private float _height;
        public float OffsetY { get; private set; }
        public float XamOffset => (float)_canvasView.TranslationY;
        private float _columnWidth => _width / 24;
        private float _maxOffsetY => -_height * Zoom + _height - 200;

        private Label[] _timeLabels;
        private Label _dateLabel;
        private List<ActivityButton> _activityButtons;
        private SKCanvasView _canvasView;
        private Button _leftButton;
        private Button _rightButton;

        // Public
        private float _zoom;
        public float Zoom { get { return _zoom; } set { _zoom = Math.Max(1, value); } }
        public static DateTime TimeOffset = DateTime.Now;
        public float MinuteWidth => _columnWidth / 60;

        public ActivityGraph(SKCanvasView canvasView, Label[] timeLabels, Label dateLabel, Button leftButton, Button rightButton)
        {
            _canvasView = canvasView;
            Zoom = 2;
            OffsetY = 0;
            //TimeOffset = DateTime.Now;
            _timeLabels = timeLabels;
            _dateLabel = dateLabel;
            _activityButtons = new List<ActivityButton>();
            _leftButton = leftButton;
            _rightButton = rightButton;
        }

        public float FromPixels(float x) => x * (float)_canvasView.Width / _width;
        public float ToPixels(float x) => x * _width / (float)_canvasView.Width;

        public void Update(SKPaintSurfaceEventArgs args)
        {
            _width = args.Info.Width;
            _height = args.Info.Height;
        }

        public void Move(float dx, float dy)
        {
            dx = ToPixels(dx);
            dx /= _columnWidth * Zoom / 60;
            TimeOffset = TimeOffset.AddMinutes(-dx);
            OffsetY += ToPixels(dy);
            OffsetY = Math.Min(OffsetY, 0);
            OffsetY = Math.Max(_maxOffsetY, OffsetY);
        }

        public void Draw(SKCanvas canvas)
        {
            canvas.Clear(SKColors.Black);
            foreach (ActivityButton button in _activityButtons)
                button.Update();

            SKPaint paint = new SKPaint();
            paint.Color = SKColors.Blue;
            paint.StrokeWidth = 1;
            canvas.DrawLine(0, 5, _width, 5, paint);

            int columnCount = (int)Math.Round(_width / _columnWidth / Zoom);
            float minutePosition = _columnWidth * Zoom - TimeOffset.Minute * (_columnWidth * Zoom / 60);
            float columnOffset = minutePosition % (_columnWidth * Zoom);
            if (columnOffset < 0) columnOffset += _columnWidth * Zoom;

            foreach (Label label in _timeLabels) label.IsVisible = false;

            _dateLabel.TranslationX = _leftButton.Width * 2;
            _dateLabel.FontAttributes = FontAttributes.Bold;
            _dateLabel.Text = $"{TimeOffset.Day:00}.{TimeOffset.Month:00}.{TimeOffset.Year:0000}";
            _dateLabel.TranslationY = _canvasView.TranslationY - _dateLabel.Height - _timeLabels[0].Height;
           

            for (int i = 0; i < columnCount; i++)
            {
                int time = (TimeOffset.Hour + i) % 24;
                if (TimeOffset.Minute > 0) time = (time + 1) % 24;
                _timeLabels[i].IsVisible = true;
                _timeLabels[i].Text = $"{time:00}:00";
                _timeLabels[i].TranslationX = FromPixels(_columnWidth * Zoom * i + columnOffset);
                _timeLabels[i].TranslationY = _canvasView.TranslationY - _timeLabels[i].Height;

                if (time == 0)
                {
                    var tomorrow = TimeOffset.AddDays(1);
                    _dateLabel.Text = $"{tomorrow.Day:00}.{tomorrow.Month:00}.{tomorrow.Year:0000}";
                    _dateLabel.TranslationX = FromPixels(_columnWidth * Zoom * i + columnOffset);
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

            if (_dateLabel.TranslationX < _leftButton.Width*2)
                _dateLabel.TranslationX = _leftButton.Width*2;
            if (_dateLabel.TranslationX > Application.Current.MainPage.Width - _rightButton.Width*2 - _dateLabel.Width)
                _dateLabel.TranslationX = Application.Current.MainPage.Width - _rightButton.Width*2 - _dateLabel.Width;

            _leftButton.TranslationY = _dateLabel.TranslationY + _dateLabel.Height / 2 - _rightButton.Height / 2;
            _rightButton.TranslationY = _dateLabel.TranslationY + _dateLabel.Height / 2 - _rightButton.Height / 2;
            _leftButton.TranslationX = _dateLabel.TranslationX - _leftButton.Width;
            _rightButton.TranslationX = _dateLabel.TranslationX + _dateLabel.Width;

            paint.PathEffect = null;
            paint.Color = SKColors.Blue;
            paint.StrokeWidth = 1;
            canvas.DrawLine(0, _height * Zoom + OffsetY, _width, _height * Zoom + OffsetY, paint);

            paint.Color = SKColors.Gray;
            float offset = 20;
            canvas.DrawLine(5, offset, 5, _height - 200 + offset, paint);
            canvas.DrawRoundRect(0, OffsetY / _maxOffsetY * (_height - 200 - 20) + offset, 10, 20, 5, 5, paint);
            paint.StrokeWidth = 3;
            canvas.DrawLine(0, offset, 10, offset, paint);
            canvas.DrawLine(0, offset + _height - 200, 10, offset + _height - 200, paint);

            // For Testing
            {
                paint.Color = SKColors.WhiteSmoke;
                _activityButtons[0].DrawConnection(canvas, _activityButtons[1]);
                _activityButtons[1].DrawConnection(canvas, _activityButtons[2]);
                _activityButtons[0].DrawConnection(canvas, _activityButtons[3]);

                _activityButtons[5].DrawConnection(canvas, _activityButtons[9]);
                _activityButtons[6].DrawConnection(canvas, _activityButtons[4]);
                _activityButtons[7].DrawConnection(canvas, _activityButtons[5]);

            }
        }

        public void SwitchEditMode(bool edit)
        {
            foreach(ActivityButton button in _activityButtons)
            {
                button.IsEnabled = !edit;
            }
        }

        public ActivityButton GetButtonAt(float x, float y)
        {
            foreach (ActivityButton button in _activityButtons)
            {
                if (button.Bounds.Offset(button.TranslationX, button.TranslationY).Contains(x, y))
                    return button;
            }

            return null;
        }

        public ActivityButton AddActivity(LarpActivity activity)
        {
            var button = new ActivityButton(activity, this);
            _activityButtons.Add(button);
            return button;
        }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ActivityGraphPage : ContentPage
    {
        private ActivityGraph _graph;
        private SKCanvasView _canvasView;
        private Switch _editSwitch;
        private Button _plusButton;
        private Button _minusButton;
        private Button _leftButton;
        private Button _rightButton;
        private Button _calendarButton;
        private ActivityButton _draggedButton;
        private TouchTrackingPoint _mousePos;
        private bool _mouseDown = false;

        public ActivityGraphPage()
        {
            InitializeComponent();

            var tEffect = new TouchEffect();
            tEffect.Capture = true;
            tEffect.TouchAction += OnTouchEffectAction;

            var children = new List<View>();

            float offsetY = 100;
            _canvasView = new SKCanvasView();
            _canvasView.PaintSurface += OnCanvasViewPaintSurface;
            _canvasView.TranslationX = 0;
            _canvasView.TranslationY = offsetY;
            _canvasView.HeightRequest = Application.Current.MainPage.Height - offsetY;

            var grid = new Grid
            {
                Effects = {tEffect},
                Children = { _canvasView }
            };

            var timeLabels = new Label[24];
            for (int i = 0; i < 24; i++)
            {
                timeLabels[i] = new Label();
                timeLabels[i].Text = $"00:00";
                timeLabels[i].VerticalOptions = LayoutOptions.Start;
                timeLabels[i].HorizontalOptions = LayoutOptions.Start;

                grid.Children.Add(timeLabels[i]);
            }

            var now = DateTime.Now;
            var dateLabel = new Label
            {
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.Red,
                Text = $"{now.Day:00}.{now.Month:00}.{now.Year:0000}"
            };
            grid.Children.Add(dateLabel);

            _leftButton = new Button();
            _leftButton.Text = "<";
            _leftButton.FontSize = 20;
            _leftButton.FontAttributes = FontAttributes.Bold;
            _leftButton.VerticalOptions = LayoutOptions.Start;
            _leftButton.HorizontalOptions = LayoutOptions.Start;
            _leftButton.TextColor = Color.Blue;
            _leftButton.BackgroundColor = Color.FromRgba(0, 0, 0, 0);
            _leftButton.Clicked += (object sender, EventArgs args) => { ActivityGraph.TimeOffset = ActivityGraph.TimeOffset.AddDays(-1); _canvasView.InvalidateSurface(); };
            grid.Children.Add(_leftButton);

            _rightButton = new Button();
            _rightButton.Text = ">";
            _rightButton.FontSize = 20;
            _rightButton.FontAttributes = FontAttributes.Bold;
            _rightButton.VerticalOptions = LayoutOptions.Start;
            _rightButton.HorizontalOptions = LayoutOptions.Start;
            _rightButton.TextColor = Color.Blue;
            _rightButton.BackgroundColor = Color.FromRgba(0, 0, 0, 0);
            _rightButton.Clicked += (object sender, EventArgs args) => {
                ActivityGraph.TimeOffset = ActivityGraph.TimeOffset.AddDays(1); _canvasView.InvalidateSurface(); };
            grid.Children.Add(_rightButton);

            _graph = new ActivityGraph(_canvasView, timeLabels, dateLabel, _leftButton, _rightButton);

            _plusButton = new Button();
            _plusButton.Text = "+";
            _plusButton.VerticalOptions = LayoutOptions.Start;
            _plusButton.HorizontalOptions = LayoutOptions.Start;
            _plusButton.TranslationX = 50;
            _plusButton.Clicked += (object sender, EventArgs args) => { _graph.Zoom += 0.25f; _canvasView.InvalidateSurface(); };
            grid.Children.Add(_plusButton);

            _minusButton = new Button();
            _minusButton.Text = "-";
            _minusButton.VerticalOptions = LayoutOptions.Start;
            _minusButton.HorizontalOptions = LayoutOptions.Start;
            _minusButton.TranslationX = 10;
            _minusButton.Clicked += (object sender, EventArgs args) => { _graph.Zoom -= 0.25f; _canvasView.InvalidateSurface(); };
            grid.Children.Add(_minusButton);

            _calendarButton = new Button();
            _calendarButton.Text = "Calendar";
            _calendarButton.VerticalOptions = LayoutOptions.Start;
            _calendarButton.HorizontalOptions= LayoutOptions.Start;
            _calendarButton.TranslationX = 100;
            _calendarButton.Clicked += (object sender, EventArgs args) => { Navigation.PushModalAsync(new CalendarPage()); };
            grid.Children.Add(_calendarButton);

            _editSwitch = new Switch();
            _editSwitch.IsToggled = false;
            _editSwitch.VerticalOptions = LayoutOptions.Start;
            _editSwitch.HorizontalOptions = LayoutOptions.Start;
            _editSwitch.TranslationX = Application.Current.MainPage.Width - 100;
            _editSwitch.TranslationY = 20;
            _editSwitch.Toggled += (object sender, ToggledEventArgs e) => { _graph.SwitchEditMode(e.Value); };
            grid.Children.Add(_editSwitch);

            // just for testing
            {
                var rnd = new Random(0);

                for (int i = 0; i < 10; i++)
                {
                    var button = _graph.AddActivity(ActivityButton.CreateActivity(
                        60,
                        rnd.Next(10 * 60) + 7*60,
                        ActivityGraph.TimeOffset.Day,
                        type: rnd.Next(2) == 0 ? LarpActivity.EventType.normal : LarpActivity.EventType.preparation,
                        status: (LarpActivity.Status)rnd.Next(5)
                        )) ;
                    button.MoveY(rnd.Next(500));
                    grid.Children.Add(button);
                }
            }

            Content = grid;
            _canvasView.InvalidateSurface();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            if (args.Type == TouchActionType.Pressed)
            {
                _mouseDown = true;
                _mousePos = args.Location;
                _draggedButton = _graph.GetButtonAt(args.Location.X, args.Location.Y);
            }

            if (args.Type == TouchActionType.Released)
            {
                _mouseDown = false;
                _mousePos = args.Location;
                _draggedButton = null;
            }

            if (!_mouseDown)
                return;

            float diffX = args.Location.X - _mousePos.X;
            float diffY = args.Location.Y - _mousePos.Y;
            _mousePos = args.Location;

            if (_draggedButton != null)
                _draggedButton.Move(_mousePos.X, _mousePos.Y);
            else
                _graph.Move(diffX, diffY);

            _canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            _graph.Update(args);
            _graph.Draw(args.Surface.Canvas);
        }
    }
}