using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using TouchTracking;
using TouchTracking.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using LAMA.ActivityGraphLib;
using LAMA.Models;
using System.Collections.Generic;

namespace LAMA.Views
{
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
        private Label _dateLabel;
        private Label[] _timeLabels;
        private ActivityButton _draggedButton;
        private TouchTrackingPoint _mousePos;
        private bool _mouseDown = false;

        public ActivityGraphPage()
        {
            InitializeComponent();

            var touchEffect = new TouchEffect();
            touchEffect.Capture = true;
            touchEffect.TouchAction += OnTouchEffectAction;

            CreateCanvasView();

            var grid = new Grid
            {
                Effects = { touchEffect },
                Children = { _canvasView }
            };

            CreateLabels(grid);
            CreateButtons(grid);

            _graph = new ActivityGraph(_canvasView, _timeLabels, _dateLabel, _leftButton, _rightButton);
            Content = grid;
            _canvasView.InvalidateSurface();
        }

        private void CreateLabels(Grid grid)
        {
            _timeLabels = new Label[24];
            for (int i = 0; i < 24; i++)
            {
                _timeLabels[i] = new Label();
                _timeLabels[i].Text = $"00:00";
                _timeLabels[i].VerticalOptions = LayoutOptions.Start;
                _timeLabels[i].HorizontalOptions = LayoutOptions.Start;

                grid.Children.Add(_timeLabels[i]);
            }

            var now = DateTime.Now;
            _dateLabel = new Label
            {
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.Red,
                Text = $"{now.Day:00}.{now.Month:00}.{now.Year:0000}"
            };
            grid.Children.Add(_dateLabel);
        }

        private void CreateButtons(Grid grid)
        {
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
                ActivityGraph.TimeOffset = ActivityGraph.TimeOffset.AddDays(1); _canvasView.InvalidateSurface();
            };
            grid.Children.Add(_rightButton);

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
            _calendarButton.HorizontalOptions = LayoutOptions.Start;
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
        }

        private void CreateCanvasView()
        {
            float offsetY = 100;
            _canvasView = new SKCanvasView();
            _canvasView.PaintSurface += OnCanvasViewPaintSurface;
            _canvasView.TranslationX = 0;
            _canvasView.TranslationY = offsetY;
            _canvasView.HeightRequest = Application.Current.MainPage.Height - offsetY;
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