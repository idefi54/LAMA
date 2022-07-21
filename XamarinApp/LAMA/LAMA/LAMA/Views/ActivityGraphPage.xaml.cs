using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using TouchTracking;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using LAMA.Models;

namespace LAMA.Views
{
    class DrawSurface
    {
        public float CanvasScale;
        public float OffsetX;
        public float OffsetY;
        public float Width;
        public float ColumnWidth => Width / 24;
        public float RowHeight;
        public float TotalMinutes;
        public SKCanvas Canvas;

        public float XamOffsetX => FromPixels(OffsetX);
        public float XamOffsetY => FromPixels(OffsetY);
        public float XamWidth => FromPixels(Width);
        public float XamRowHeight => FromPixels(RowHeight);

        public float ToPixels(float x) => x * CanvasScale;
        public float FromPixels(float x) => x / CanvasScale;
    };

    class ActivityButton : Button
    {
        public DrawSurface DrawSurface { get; set; }
        public LarpActivity Activity { get; private set; }
        public ActivityButton(LarpActivity activity, DrawSurface surface, INavigation navigation) : base()
        {
            DrawSurface = surface;
            Activity = activity;
            IsEnabled = true;
            Clicked += (object sender, EventArgs args) => navigation.PushAsync(new DisplayActivityPage(activity));
        }

        public void UpdateVisual()
        {
            Text = Activity.name;
            WidthRequest = DrawSurface.FromPixels(Activity.duration.getRawMinutes() / 60.0f * DrawSurface.ColumnWidth);
            HeightRequest = DrawSurface.XamRowHeight;
            TranslationX = DrawSurface.FromPixels(Activity.start.getRawMinutes() / DrawSurface.TotalMinutes + DrawSurface.OffsetX);
            TranslationY = DrawSurface.XamOffsetY;
            HorizontalOptions = LayoutOptions.Start;
            VerticalOptions = LayoutOptions.Start;
            TextColor = Color.Black;
            BackgroundColor = GetColor(Activity.status);
            CornerRadius = GetCornerRadius(Activity.eventType);
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

        public static void DrawConnection(DrawSurface surface, ActivityButton a, ActivityButton b)
        {
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

            float xDist = bx - ax;
            float yDist = by - ay;

            var paint = new SKPaint();
            paint.Color = SKColors.WhiteSmoke;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 2;

            if (Math.Abs(xDist) > Math.Abs(yDist))
            {
                surface.Canvas.DrawLine(aRect.MidX, aRect.MidY, aRect.MidX + xDist / 2, aRect.MidY, paint);
                surface.Canvas.DrawLine(aRect.MidX + xDist / 2, aRect.MidY, aRect.MidX + xDist / 2, bRect.MidY, paint);
                surface.Canvas.DrawLine(aRect.MidX + xDist / 2, bRect.MidY, bRect.MidX, bRect.MidY, paint);
            }
        }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ActivityGraphPage : ContentPage
    {
        private Button _editButton;
        private Button _draggedButton;
        private const string EditText = "Edit";
        private const string FinishEditText = "Finish Edit";
        private DrawSurface _surface;
        public const int Minutes = 24 * 60;
        public const float Length = 1800;
        public const int RowSize = 50;
        public const int OffsetX = 20;
        public const int OffsetY = 20;
        private ActivityButton _button1;
        private ActivityButton _button2;
        private SKPoint mousePosition;
        private bool _firstCall = true;

        public ActivityGraphPage()
        {
            InitializeComponent();
            _draggedButton = null;
            _surface = new DrawSurface
            {
                CanvasScale = canvasView.CanvasSize.Width / (float)canvasView.Width,
                OffsetX = 20,
                OffsetY = 20,
                RowHeight = 50,
                Width = 1500,
                TotalMinutes = 24 * 60,
            };

            Grid g = Content as Grid;

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

            _button1 = new ActivityButton(activity, _surface, Navigation);
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

            _button2 = new ActivityButton(activity2, _surface, Navigation);
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
            mousePosition = new SKPoint(args.Location.X, args.Location.Y);
            if (args.Type == TouchActionType.Released)
            {
                _draggedButton = null;
                return;
            }

            if (args.Type == TouchActionType.Moved && _draggedButton != null)
            {
                _draggedButton.TranslationX = args.Location.X - _draggedButton.Width / 2;
                _draggedButton.TranslationY = args.Location.Y - _draggedButton.Height / 2;
            }

            if (args.Type == TouchActionType.Pressed)
            {
                foreach (var b in ((Grid)Content).Children)
                {
                    if (!(b is Button button))
                        continue;

                    Rectangle rect = button.Bounds.Offset(button.TranslationX, button.TranslationY);
                    if (!rect.Contains(args.Location.X, args.Location.Y))
                        continue;

                    _draggedButton = button;
                    break;
                }
            }

            this.canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {

            _surface.CanvasScale = canvasView.CanvasSize.Width / (float)canvasView.Width;
            _surface.Canvas = args.Surface.Canvas;
            if (_firstCall)
            {
                _button1.UpdateVisual();
                _button2.UpdateVisual();
            }

            _firstCall = false;

            SKCanvas canvas = args.Surface.Canvas;

            canvas.Clear(SKColors.Black);

            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Color.Blue.ToSKColor(),
                StrokeWidth = 3
            };
            canvas.DrawLine(
                OffsetX,
                10,
                OffsetX + Length,
                10,
                paint
            );

            for (int i = 0; i <= 23; i++)
            {
                canvas.DrawLine(
                    Length / 23 * i + OffsetX,
                    5,
                    Length / 23 * i + OffsetX,
                    15,
                    paint);
            }

            _editButton.TranslationX = Application.Current.MainPage.Width - 100;

            ActivityButton.DrawConnection(_surface, _button1, _button2);
            canvas.DrawCircle(mousePosition, 5, paint);
        }
    }
}