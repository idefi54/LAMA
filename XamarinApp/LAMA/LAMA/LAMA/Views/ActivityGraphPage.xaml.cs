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

                if (_graph.ActivityCreationMode)
                {
                    var aButton = new ActivityButton();
                    _graph.AddActivity(ActivityButton.CreateActivity(60, ));
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
            _graph.MouseX = args.Location.X;
            _graph.MouseY = args.Location.Y;
            _graph.InvalidateSurface();
        }
    }
}