using System;
using TouchTracking;
using TouchTracking.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using LAMA.ActivityGraphLib;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using LAMA.Extensions;
using SkiaSharp.Views.Forms;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ActivityGraphPage : ContentPage
    {
        private ActivityGraph _graph;
        private ActivityButton _draggedButton;
        private Dictionary<long, TouchActionEventArgs> _touchActions;
        private TouchTrackingPoint _lastLocation;
        private float _baseDistance;
        private float _baseZoom;

        public ActivityGraphPage()
        {
            // Regular setup
            InitializeComponent();
            _touchActions = new Dictionary<long, TouchActionEventArgs>();

            // Create platform specific GUI
            var gui = DependencyService.Get<IActivityGraphGUI>();
            (Content, _graph) = gui.CreateGUI(Navigation);
            
            // Setup Touch effect - this is a nuget package
            var touchEffect = new TouchEffect();
            touchEffect.Capture = true;
            touchEffect.TouchAction += OnTouchEffectAction;
            Content.Effects.Add(touchEffect);
        }

        void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            // New Press
            if (args.Type == TouchActionType.Pressed)
            {
                // Add press to dictionary
                _touchActions[args.Id] = args;

                // Clicking a button
                if (_touchActions.Count == 1)
                {
                    _draggedButton = _graph.GetButtonAt(args.Location.X, args.Location.Y);
                }

                // Save for computing change in location
                _lastLocation = args.Location;

                // Prepare for zoom;
                if (_touchActions.Count > 1)
                {
                    // Take first to and save difference
                    KeyValuePair<long, TouchActionEventArgs>[] arr = _touchActions.ToArray();
                    var a = arr[0].Value.Location;
                    var b = arr[1].Value.Location;
                    float dx = a.X - b.X;
                    float dy = a.Y - b.Y;
                    _baseDistance = (float)Math.Abs(Math.Sqrt(dx * dx + dy * dy));
                    _baseZoom = _graph.Zoom;
                }
            }

            // Moving button
            if (args.Type == TouchActionType.Moved && _draggedButton != null)
            {
                if (_touchActions.ContainsKey(args.Id))
                    _touchActions[args.Id] = args;
                _draggedButton.Move(args.Location.X, args.Location.Y);
            }

            // Control graph view
            if (args.Type == TouchActionType.Moved && _draggedButton == null)
            {
                if (_touchActions.ContainsKey(args.Id))
                    _touchActions[args.Id] = args;
                // Scroll graph
                if (_touchActions.Count == 1)
                {
                    float diffX = args.Location.X - _lastLocation.X;
                    float diffY = args.Location.Y - _lastLocation.Y;
                    _lastLocation = args.Location;
                    _graph.Move(diffX, diffY);
                }

                // Zoom graph
                if (_touchActions.Count > 1)
                {
                    KeyValuePair<long, TouchActionEventArgs>[] arr = _touchActions.ToArray();
                    var a = arr[0].Value.Location;
                    var b = arr[1].Value.Location;
                    float dx = a.X - b.X;
                    float dy = a.Y - b.Y;
                    float distance = (float)Math.Abs(Math.Sqrt(dx * dx + dy * dy));
                    _graph.Zoom = _baseZoom + (distance - _baseDistance) / 100;
                }
            }

            // Release
            if (args.Type == TouchActionType.Released)
            {
                _draggedButton = null;
                _touchActions.Remove(args.Id);
            }

            // Redraw graph every touch
            _graph.InvalidateSurface();
        }
    }
    /*
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ActivityGraphPage : ContentPage
    {
        private ActivityGraph _graph;
        private ActivityButton _draggedButton;
        private Dictionary<long, TouchActionEventArgs> _touchActions;
        private TouchActionEventArgs _firstTouch;
        private TouchActionEventArgs _secondTouch;
        private float _baseDistance;
        private float _baseZoom;
        private TouchTrackingPoint _lastPosition;
        private bool _mouseDown = false;

        private SKCanvasView _canvasView;
        private Xamarin.Forms.Switch _editSwitch;
        private Button _plusButton;
        private Button _minusButton;
        private Button _leftButton;
        private Button _rightButton;
        private Button _calendarButton;

        public ActivityGraphPage()
        {
            InitializeComponent();
            _touchActions = new Dictionary<long, TouchActionEventArgs>();

            var gui = DependencyService.Get<IActivityGraphGUI>();
            (Content, _graph) = gui.CreateGUI(Navigation);
            ActivityGraph.Instance = _graph;

            var touchEffect = new TouchEffect();
            touchEffect.Capture = true;
            touchEffect.TouchAction += OnTouchEffectAction;
            Content.Effects.Add(touchEffect);
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

            _editSwitch = new Xamarin.Forms.Switch();
            _editSwitch.IsToggled = false;
            _editSwitch.VerticalOptions = LayoutOptions.Start;
            _editSwitch.HorizontalOptions = LayoutOptions.Start;
            _editSwitch.TranslationX = Application.Current.MainPage.Width - 100;
            _editSwitch.TranslationY = 20;
            _editSwitch.Toggled += (object sender, ToggledEventArgs e) => { _graph.SwitchEditMode(e.Value); };
            grid.Children.Add(_editSwitch);

            Content = grid;
            _canvasView.InvalidateSurface();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            // New Press
            if (args.Type == TouchActionType.Pressed)
            {
                _touchActions[args.Id] = args;

                if (_touchActions.Count == 1)
                {
                    _draggedButton = _graph.GetButtonAt(args.Location.X, args.Location.Y);
                }

                _lastPosition = args.Location;

                if (_touchActions.Count > 1)
                {
                    // Take first to and save difference
                    KeyValuePair<long, TouchActionEventArgs>[] arr = _touchActions.ToArray();
                    var a = arr[0].Value.Location;
                    var b = arr[1].Value.Location;
                    float dx = a.X - b.X;
                    float dy = a.Y - b.Y;
                    _baseDistance = (float)Math.Abs(Math.Sqrt(dx * dx + dy * dy));
                    _baseZoom = _graph.Zoom;
                }
            }

            // Moving button
            if (args.Type == TouchActionType.Moved && _draggedButton != null)
            {
                if (_touchActions.ContainsKey(args.Id))
                    _touchActions[args.Id] = args;
                _draggedButton.Move(args.Location.X, args.Location.Y);
            }

            // Control graph view
            if (args.Type == TouchActionType.Moved && _draggedButton == null)
            {
                if (_touchActions.ContainsKey(args.Id))
                    _touchActions[args.Id] = args;
                // Scroll graph
                if (_touchActions.Count == 1)
                {
                    float diffX = args.Location.X - _lastPosition.X;
                    float diffY = args.Location.Y - _lastPosition.Y;
                    _lastPosition = args.Location;
                    _graph.Move(diffX, diffY);
                }

                // Zoom graph
                if (_touchActions.Count > 1)
                {
                    KeyValuePair<long, TouchActionEventArgs>[] arr = _touchActions.ToArray();
                    var a = arr[0].Value.Location;
                    var b = arr[1].Value.Location;
                    float dx = a.X - b.X;
                    float dy = a.Y - b.Y;
                    float distance = (float)Math.Abs(Math.Sqrt(dx * dx + dy * dy));
                    _graph.Zoom = _baseZoom + (distance - _baseDistance) / 100;
                }
            }

            // Release
            if (args.Type == TouchActionType.Released)
            {
                _draggedButton = null;
                _touchActions.Remove(args.Id);
            }

            _graph.InvalidateSurface();
        }
    }
    */
}