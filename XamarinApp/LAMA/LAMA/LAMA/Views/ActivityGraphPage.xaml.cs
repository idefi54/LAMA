using SkiaSharp;
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
            var span = time - _activityGraph.TimeOffset;

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
            DateTime newTime = _activityGraph.TimeOffset.AddMinutes(minutes);
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
        public DateTime TimeOffset;
        public float MinuteWidth => _columnWidth / 60;

        public ActivityGraph(SKCanvasView canvasView, Label[] timeLabels, Label dateLabel, Button leftButton, Button rightButton)
        {
            _canvasView = canvasView;
            Zoom = 2;
            OffsetY = 0;
            TimeOffset = DateTime.Now;
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

            _leftButton.TranslationY = _dateLabel.TranslationY;
            _rightButton.TranslationY = _dateLabel.TranslationY;
            _leftButton.TranslationX = _dateLabel.TranslationX - _leftButton.Width*2;
            _rightButton.TranslationX = _dateLabel.TranslationX + _rightButton.Width + _dateLabel.Width;

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

            var dateLabel = new Label
            {
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                TextColor = Color.Red,
                Text = DateTime.Now.ToShortDateString()
            };
            grid.Children.Add(dateLabel);

            _leftButton = new Button();
            _leftButton.Text = "<";
            _leftButton.VerticalOptions = LayoutOptions.Start;
            _leftButton.HorizontalOptions = LayoutOptions.Start;
            _leftButton.WidthRequest = 20;
            _leftButton.HeightRequest = 20;
            _leftButton.CornerRadius = 10;
            _leftButton.TextColor = Color.Black;
            _leftButton.Clicked += (object sender, EventArgs args) => { _graph.TimeOffset = _graph.TimeOffset.AddDays(-1); _canvasView.InvalidateSurface(); };
            grid.Children.Add(_leftButton);

            _rightButton = new Button();
            _rightButton.Text = ">";
            _rightButton.VerticalOptions = LayoutOptions.Start;
            _rightButton.HorizontalOptions = LayoutOptions.Start;
            _rightButton.WidthRequest = 20;
            _rightButton.HeightRequest = 20;
            _rightButton.CornerRadius = 10;
            _rightButton.TextColor = Color.Black;
            _rightButton.Clicked += (object sender, EventArgs args) => { _graph.TimeOffset = _graph.TimeOffset.AddDays(1); _canvasView.InvalidateSurface(); };
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
                        _graph.TimeOffset.Day,
                        type: rnd.Next(2) == 0 ? LarpActivity.EventType.normal : LarpActivity.EventType.preparation,
                        status: (LarpActivity.Status)rnd.Next(5)
                        )) ;
                    button.MoveY(rnd.Next(500));
                    grid.Children.Add(button);
                }
            }

            Content = grid;
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

/*/
namespace LAMA.Views
{
    class DrawSurface
    {
        public float CanvasScale;
        public float OffsetX;
        public float OffsetY;
        public float Width;
        public float Height;
        public float Zoom;
        public float PositionX;
        public float PositionY;

        public float ColumnWidth => Width / 24;
        public float TotalMinutes;
        public SKCanvas Canvas;

        public float XamOffsetX => FromPixels(OffsetX);
        public float XamOffsetY => FromPixels(OffsetY);
        public float XamWidth => FromPixels(Width);
        public float XamColumnWidth => FromPixels(ColumnWidth);
        public float XamHeight => FromPixels(Height);

        public float ToPixels(float x) => x * CanvasScale;
        public float FromPixels(float x) => x / CanvasScale;
    };

    class ActivityButton : Button
    {
        public LarpActivity Activity { get; private set; }
        public int Row { get; set; }
        public float posY;
        public ActivityButton(LarpActivity activity, INavigation navigation) : base()
        {
            Activity = activity;
            IsEnabled = true;
            Clicked += (object sender, EventArgs args) => navigation.PushAsync(new DisplayActivityPage(activity));
            Row = 0;
            Text = Activity.name;
            HeightRequest = 50;
        }

        public void Update(DrawSurface surface)
        {
            Text = Activity.name;
            WidthRequest = Activity.duration.getRawMinutes() / 60.0f * surface.XamColumnWidth * surface.Zoom;
            //HeightRequest = 50;
            TranslationX = Activity.start.getRawMinutes() / surface.TotalMinutes * surface.XamWidth * surface.Zoom + surface.XamOffsetX + surface.FromPixels(surface.PositionX);
            TranslationY = surface.FromPixels(posY * surface.Height * surface.Zoom + surface.PositionY) + surface.XamOffsetY;
            TranslationY = Math.Min(surface.XamHeight * surface.Zoom + surface.XamOffsetY - Height, TranslationY);
            TranslationY = Math.Max(surface.XamOffsetY, TranslationY);
            HorizontalOptions = LayoutOptions.Start;
            VerticalOptions = LayoutOptions.Start;
            TextColor = Color.Black;
            BackgroundColor = GetColor(Activity.status);
            CornerRadius = GetCornerRadius(Activity.eventType);

            IsVisible = TranslationX < surface.XamOffsetX + surface.XamWidth && TranslationY >= surface.XamOffsetY;
        }

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

        public void Move(DrawSurface surface, double x, double y)
        {
            posY = (surface.ToPixels((float)y - surface.XamOffsetY) - surface.PositionY) / surface.Height / surface.Zoom;
            posY = Math.Max(0, Math.Min(1, posY));
            TranslationY = Math.Min(surface.XamHeight*surface.Zoom + surface.XamOffsetY - Height, Math.Max(surface.XamOffsetY, y));
            double percentage = (x - surface.XamOffsetX - surface.FromPixels(surface.PositionX)) / surface.XamWidth / surface.Zoom;
            int minutes = (int)(percentage * surface.TotalMinutes);
            minutes = (int)Math.Max(Math.Min(minutes, surface.TotalMinutes - Activity.duration.getRawMinutes()), 0);
            minutes -= (minutes % 5);
            Activity.start = new Time(minutes);
        }

        public static void DrawConnection(DrawSurface surface, ActivityButton a, ActivityButton b)
        {
            if (!a.IsVisible) return;

            float ax = surface.ToPixels((float)a.TranslationX);
            float ay = surface.ToPixels((float)a.TranslationY);
            float aWidth = surface.ToPixels((float)a.Width);
            float aHeight = surface.ToPixels((float)a.Height);
            var aRect = new SKRect(ax, ay, ax + aWidth, ay + aHeight);

            float bx = surface.ToPixels((float)b.TranslationX);
            float by = surface.ToPixels((float)b.TranslationY);
            float bWidth = surface.ToPixels((float)b.Width);
            float bHeight = surface.ToPixels((float)b.Height);
            var bRect = new SKRect(bx, by, bx + bWidth, by + bHeight);

            var paint = new SKPaint();
            paint.Color = SKColors.WhiteSmoke;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 2;

            SKPath path = new SKPath();
            paint.IsAntialias = true;
            path.MoveTo(aRect.MidX, aRect.MidY);
            path.QuadTo(aRect.MidX, bRect.MidY, bRect.MidX, bRect.MidY);
            surface.Canvas.DrawPath(path, paint);
        }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ActivityGraphPage : ContentPage
    {
        private Button _editButton;
        private ActivityButton _draggedButton;
        private Label _timeLabel;
        private Label _buttonTimeLabel;
        private Label[] _segmentLabels;
        private const string EditText = "Edit";
        private const string FinishEditText = "Finish Edit";
        private DrawSurface _surface;
        private ActivityButton _button1;
        private ActivityButton _button2;
        private SKPoint mousePosition;
        private bool _mouseDown;


        public ActivityGraphPage()
        {
            InitializeComponent();
            _draggedButton = null;
            _surface = new DrawSurface
            {
                CanvasScale = canvasView.CanvasSize.Width / (float)canvasView.Width,
                OffsetX = 20,
                OffsetY = 100,
                PositionX = 300,
                PositionY = 0,
                Width = 1500,
                Height = 900,
                TotalMinutes = 24 * 60,
                Zoom = 1
            };

            Grid g = Content as Grid;
            SKCanvasView view = g.Children[0] as SKCanvasView;
            view.InputTransparent = true;
            

            var activity = new LarpActivity(
                ID: 1,
                name: "TestActivity",
                description: "Description",
                preparation: "No Preparation",
                eventType: LarpActivity.EventType.normal,
                prerequisiteIDs: new EventList<int>(),
                duration: new Time(60),
                day: 0,
                start: new Time(120),
                place: new Pair<double, double>(0, 0),
                status: LarpActivity.Status.launched,
                requiredItems: new EventList<Pair<int, int>>(),
                roles: new EventList<Pair<string, int>>(),
                registrations: new EventList<Pair<int, string>>()
                );

            _button1 = new ActivityButton(activity, Navigation);
            _button1.posY = 0.2f;
            g.Children.Add(_button1);

            var activity2 = new LarpActivity(
                ID: 1,
                name: "TestActivity",
                description: "Description",
                preparation: "No Preparation",
                eventType: LarpActivity.EventType.preparation,
                prerequisiteIDs: new EventList<int>(),
                duration: new Time(60),
                day: 0,
                start: new Time(240),
                place: new Pair<double, double>(0, 0),
                status: LarpActivity.Status.inProgress,
                requiredItems: new EventList<Pair<int, int>>(),
                roles: new EventList<Pair<string, int>>(),
                registrations: new EventList<Pair<int, string>>()
                );

            _button2 = new ActivityButton(activity2, Navigation);
            _button2.posY = 0.4f;
            g.Children.Add(_button2);

            _editButton = new Button
            {
                IsEnabled = true,
                Text = EditText,
                TranslationX = Application.Current.MainPage.Width - 100,
                TranslationY = 20,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                TextColor = Color.White,
            };
            _editButton.Clicked += Button_Clicked;
            g.Children.Add(_editButton);
            
            _timeLabel = new Label
            {
                Text = "Monday, 10.4.1995"
            };
            g.Children.Add(_timeLabel);

            _buttonTimeLabel = new Label();
            _buttonTimeLabel.IsVisible = false;
            g.Children.Add(_buttonTimeLabel);


            _segmentLabels = new Label[25];
            for (int i = 0; i <= 24; i++)
            {
                var label = new Label();
                label.Text = $"{i:00}:{00:00}";
                _segmentLabels[i] = label;
                g.Children.Add(label);
            }

            var pinch = new PinchGestureRecognizer();
            pinch.PinchUpdated += (object o, PinchGestureUpdatedEventArgs e) => {
                _surface.Zoom += _surface.ToPixels((float)e.Scale); canvasView.InvalidateSurface();
            };
            canvasView.GestureRecognizers.Add(pinch);
            g.GestureRecognizers.Add(pinch);
            
            _mouseDown = false;

            var plusButton = new Button
            {
                Text = "+",
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                TranslationX = 100
            };
            plusButton.Clicked += (object sender, EventArgs e) => { _surface.Zoom += 0.2f; canvasView.InvalidateSurface(); };

            var minusButton = new Button
            {
                Text = "-",
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start
            };
            minusButton.Clicked += (object sender, EventArgs e) => { _surface.Zoom -= 0.2f; canvasView.InvalidateSurface(); };
            g.Children.Add(plusButton);
            g.Children.Add(minusButton);
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            _editButton.Text = (_editButton.Text == EditText) ? FinishEditText : EditText;
            Grid grid = Content as Grid;
            foreach (var child in grid.Children)
            {
                var button = child as Button;
                if (button == null || button == _editButton)
                    continue;

                button.IsEnabled = _editButton.Text == EditText;
            }


        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            if (args.Type == TouchActionType.Released)
            {
                _draggedButton = null;
                _mouseDown = false;
                canvasView.InvalidateSurface();
                return;
            }

            if (args.Type == TouchActionType.Moved && _draggedButton == null && _mouseDown)
            {
                var pos = new SKPoint(args.Location.X, args.Location.Y);
                var distX = pos.X - mousePosition.X;
                var distY = pos.Y - mousePosition.Y;
                _surface.PositionX += _surface.ToPixels(distX);
                _surface.PositionY += _surface.ToPixels(distY);
                _surface.PositionY = Math.Min(0, _surface.PositionY);
                _surface.PositionY = Math.Max(-_surface.OffsetY - _surface.Height, _surface.PositionY);
                mousePosition = pos;
            }

            if (args.Type == TouchActionType.Moved && _draggedButton != null)
            {
                _draggedButton.Move(
                    _surface,
                    args.Location.X - _draggedButton.Width / 2,
                    args.Location.Y - _draggedButton.Height / 2
                );
            }

            if (args.Type == TouchActionType.Pressed)
            {
                _mouseDown = true;
                mousePosition = new SKPoint(args.Location.X, args.Location.Y);
                foreach (var b in ((Grid)Content).Children)
                {
                    if (_editButton.Text == EditText)
                        break;

                    if (!(b is ActivityButton button))
                        continue;

                    Rectangle rect = button.Bounds.Offset(button.TranslationX, button.TranslationY);
                    if (!rect.Contains(args.Location.X, args.Location.Y))
                        continue;

                    _draggedButton = button;
                    break;
                }
            }

            canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            _surface.CanvasScale = canvasView.CanvasSize.Width / (float)canvasView.Width;
            _surface.Height = canvasView.CanvasSize.Height - _surface.OffsetY*2;
            _surface.Canvas = args.Surface.Canvas;
            _button2.Update(_surface);
            _button1.Update(_surface);
            _timeLabel.TranslationX = _surface.XamOffsetX;
            
            SKCanvas canvas = args.Surface.Canvas;
            canvas.Clear(SKColors.Black);
            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Color.Blue.ToSKColor(),
                StrokeWidth = 3
            };

            // Paint axis
            {
                canvas.DrawLine(
                    _surface.OffsetX,
                    _surface.OffsetY,
                    _surface.OffsetX + _surface.Width,
                    _surface.OffsetY,
                    paint
                );
            }

            // Paint columns
            {
                paint.PathEffect = SKPathEffect.CreateDash(new float[] { 20f, 8f }, 1);
                paint.StrokeWidth = 1f;
                paint.Color = paint.Color.WithAlpha(125);

                int columnCount = (int)Math.Round(_surface.Width / _surface.ColumnWidth / _surface.Zoom);
                float columnOffset = _surface.PositionX % (_surface.ColumnWidth * _surface.Zoom);
                if (columnOffset < 0) columnOffset += _surface.ColumnWidth * _surface.Zoom;

                foreach (var label in _segmentLabels)
                    label.IsVisible = false;

                for (int i = 0; i < columnCount; i++)
                {
                    canvas.DrawLine(
                        _surface.ColumnWidth * _surface.Zoom * i + _surface.OffsetX + columnOffset,
                        _surface.OffsetY - 5,
                        _surface.ColumnWidth * _surface.Zoom * i + _surface.OffsetX + columnOffset,
                        _surface.OffsetY + 5,
                        paint);

                    canvas.DrawLine(
                        _surface.ColumnWidth * _surface.Zoom * i + _surface.OffsetX + columnOffset,
                        _surface.OffsetY + 5,
                        _surface.ColumnWidth * _surface.Zoom * i + _surface.OffsetX + columnOffset,
                        _surface.Height * _surface.Zoom + _surface.OffsetY + _surface.PositionY,
                        paint);
                }
                
                for (int j = 0; j <= 23; j++)
                {
                    _segmentLabels[j].IsVisible = false;
                    float offset = _surface.FromPixels(_surface.PositionX) + _surface.XamOffsetX;
                    float col = j * _surface.XamColumnWidth * _surface.Zoom + offset;
                    col %= _surface.XamWidth * _surface.Zoom;
                    if (col < 0) col += _surface.XamWidth * _surface.Zoom;
                    if (col > _surface.XamWidth) continue;
                    _segmentLabels[j].IsVisible = true;
                    _segmentLabels[j].TranslationX = col;
                    _segmentLabels[j].TranslationY = _surface.XamOffsetY - 40;

                    if (j == 0)
                    {
                        paint.Color = SKColors.Red;
                        canvas.DrawLine(_surface.ToPixels(col),
                            _surface.OffsetY,
                            _surface.ToPixels(col),
                            _surface.OffsetY + _surface.PositionY + _surface.Height * _surface.Zoom,
                            paint);

                        _timeLabel.TranslationX = col;
                    }
                }

                _timeLabel.TranslationY = _surface.XamOffsetY - 80;
            }

            // Paint scroll bar
            {
                paint.Color = SKColors.Gray;
                paint.PathEffect = null;
                
                canvas.DrawLine(5, _surface.OffsetY, 5, _surface.OffsetY + _surface.Height, paint);

                var rect = new SKRect(
                    0,
                    -_surface.PositionY + _surface.OffsetY,
                    10,
                    -_surface.PositionY + _surface.OffsetY + 30
                    );
                canvas.DrawRoundRect(rect, 2, 2, paint);
            }

            _buttonTimeLabel.IsVisible = false;
            if (_draggedButton != null)
            {
                paint.Color = SKColors.Orange.WithAlpha(125);
                canvas.DrawLine(
                    _surface.ToPixels((float)_draggedButton.TranslationX),
                    _surface.ToPixels((float)_draggedButton.TranslationY),
                    _surface.ToPixels((float)_draggedButton.TranslationX),
                    _surface.OffsetY + _surface.PositionY,
                    paint);

                canvas.DrawLine(
                    _surface.ToPixels((float)_draggedButton.TranslationX + (float)_draggedButton.Width),
                    _surface.ToPixels((float)_draggedButton.TranslationY),
                    _surface.ToPixels((float)_draggedButton.TranslationX + (float)_draggedButton.Width),
                    _surface.OffsetY + _surface.PositionY,
                    paint);



                paint.Color = SKColors.White;
                paint.TextSize = 20;
                
                string startTime = $"{_draggedButton.Activity.start.hours}:{_draggedButton.Activity.start.minutes:00}";
                _buttonTimeLabel.Text = startTime;
                _buttonTimeLabel.TranslationX = _draggedButton.TranslationX;
                _buttonTimeLabel.TranslationY = _surface.XamOffsetY - 20;
                _buttonTimeLabel.IsVisible = true;
            }

            _editButton.TranslationX = Application.Current.MainPage.Width - 100;

            ActivityButton.DrawConnection(_surface, _button1, _button2);
        }
    }
}
//*/