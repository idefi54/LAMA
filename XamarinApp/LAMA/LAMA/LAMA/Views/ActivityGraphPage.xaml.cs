using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using TouchTracking;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using LAMA.Models;

namespace LAMA.Views
{
    class ActivityButton
    {
        public Button Button { get; private set; }
        public LarpActivity Activity { get; private set; }

        public ActivityButton(LarpActivity activity, INavigation navigation, float spaceWidth, float spaceOffsetX, float spaceOffsetY, float rowSize, int totalMinutes)
        {
            Activity = activity;
            int durationMinutes = activity.duration.getRawMinutes();
            Button = new Button()
            {
                IsEnabled = true,
                Text = activity.name,
                WidthRequest = durationMinutes,
                HeightRequest = rowSize,
                TranslationX = (float)activity.start.getRawMinutes() / totalMinutes * spaceWidth + spaceOffsetX,
                TranslationY = spaceOffsetY,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                TextColor = Color.Black,
                BackgroundColor = GetColor(activity.status),
                CornerRadius = GetCornerRadius(activity.eventType)
            };
            Button.Clicked += (object sender, EventArgs args) => navigation.PushAsync(new DisplayActivityPage(activity));
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

        private static float ConvertToPixels(SKCanvasView view, float x)
        {
            return (float)(view.CanvasSize.Width * x / view.Width);
        }

        public static void DrawConnection(SKCanvasView view, SKCanvas canvas, ActivityButton a, ActivityButton b)
        {
            SKRect aBox = a.Button.Bounds.Offset(a.Button.TranslationX, a.Button.TranslationY).ToSKRect();
            SKRect bBox = b.Button.Bounds.Offset(b.Button.TranslationX, b.Button.TranslationY).ToSKRect();
            aBox.Top = ConvertToPixels(view, aBox.Top);
            aBox.Left = ConvertToPixels(view, aBox.Left);
            aBox.Bottom = ConvertToPixels(view, aBox.Bottom);
            aBox.Right = ConvertToPixels(view, aBox.Right);

            bBox.Top = ConvertToPixels(view, bBox.Top);
            bBox.Left = ConvertToPixels(view, bBox.Left);
            bBox.Bottom = ConvertToPixels(view, bBox.Bottom);
            bBox.Right = ConvertToPixels(view, bBox.Right);


            float xDist = aBox.MidX - bBox.MidX;
            float yDist = aBox.MidY - bBox.MidY;

            var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Red,
                StrokeWidth = 3
            };

            if (Math.Abs(xDist) > Math.Abs(yDist))
            {
                canvas.DrawLine(aBox.MidX, aBox.MidY, bBox.MidX, bBox.MidY, paint);
                //canvas.DrawLine(aBox.MidX, aBox.MidY, aBox.MidX + xDist / 2, aBox.MidY, paint);
                //canvas.DrawLine(aBox.MidX + xDist / 2, aBox.MidY, aBox.MidX + xDist / 2, aBox.MidY + yDist / 2, paint);
                //canvas.DrawLine(aBox.MidX + xDist / 2, aBox.MidY + yDist / 2, bBox.MidX, bBox.MidY, paint);
            } else
            {
                //canvas.DrawLine(aBox.MidX, aBox.MidY, aBox.Right + xDist / 2, aBox.MidY, paint);
                //canvas.DrawLine(aBox.MidX + xDist / 2, aBox.MidY, aBox.MidX + xDist / 2, aBox.MidY + yDist / 2, paint);
                //canvas.DrawLine(aBox.MidX + xDist / 2, aBox.MidY + yDist / 2, bBox.Left, bBox.MidY, paint);
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
        public const int Minutes = 24 * 60;
        public const float Length = 1800;
        public const int RowSize = 50;
        public const int OffsetX = 20;
        public const int OffsetY = 20;
        private ActivityButton _button1;
        private ActivityButton _button2;
        private SKPoint mousePosition;

        public ActivityGraphPage()
        {
            InitializeComponent();
            _draggedButton = null;

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

            _button1 = new ActivityButton(activity, Navigation, Length, OffsetX, OffsetY, RowSize, Minutes);
            g.Children.Add(_button1.Button);

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

            _button2 = new ActivityButton(activity2, Navigation, Length, OffsetX, OffsetY, RowSize, Minutes);
            g.Children.Add(_button2.Button);

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

            ActivityButton.DrawConnection(canvasView, canvas, _button1, _button2);
            canvas.DrawCircle(mousePosition, 5, paint);
        }
    }
}