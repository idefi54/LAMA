using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchTracking;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ActivityGraphPage : ContentPage
    {
        private Button _editButton;
        private Button _draggedButton;
        private const string EditText = "Edit";
        private const string FinishEditText = "Finish Edit";

        public ActivityGraphPage()
        {
            InitializeComponent();
            _draggedButton = null;

            Grid g = Content as Grid;

            g.Children.Add(new Button
            {
                IsEnabled = true,
                Text = $"Button1",
                TranslationX = 20,
                TranslationY = 20,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                TextColor = Color.White
            });


            g.Children.Add(new Button
            {
                IsEnabled = true,
                Text = $"Button2",
                TranslationX = 200,
                TranslationY = 200,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                TextColor = Color.White
            });


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
            canvas.DrawLine(0, 10, (float)args.Info.Width, 10, paint);
            for (int i = 0; i < 20; i++)
            {
                canvas.DrawLine(
                    args.Info.Width / 20 * i,
                    5,
                    args.Info.Width / 20 * i,
                    15,
                    paint);
            }

            _editButton.TranslationX = Application.Current.MainPage.Width - 100;
        }
    }
}