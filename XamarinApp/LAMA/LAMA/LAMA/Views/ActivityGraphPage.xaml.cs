using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using TouchTracking;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using LAMA.Models;
using System.Collections.Generic;

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