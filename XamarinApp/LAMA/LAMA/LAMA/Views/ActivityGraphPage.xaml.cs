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
using LAMA.Models;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ActivityGraphPage : ContentPage
    {
        private ActivityGraph _graph;
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

        public ActivityGraphPage(LarpActivity activity)
        {
            // Regular setup
            InitializeComponent();
            _touchActions = new Dictionary<long, TouchActionEventArgs>();

            // Create platform specific GUI
            var gui = DependencyService.Get<IActivityGraphGUI>();
            (Content, _graph) = gui.CreateGUI(Navigation);

            if (activity != null)
                _graph.FocusOnActivity(activity);

            // Setup Touch effect - this is a nuget package
            var touchEffect = new TouchEffect();
            touchEffect.Capture = true;
            touchEffect.TouchAction += OnTouchEffectAction;
            Content.Effects.Add(touchEffect);
        }

        protected override void OnAppearing()
        {
            _graph.ReloadActivities();
        }

        public void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            float px = _graph.ToPixels(args.Location.X);
            float py = _graph.ToPixels(args.Location.Y - _graph.XamOffset);

            // New Press
            if (args.Type == TouchActionType.Pressed)
            {
                // Add press to dictionary
                _touchActions[args.Id] = args;

                // Clicking a button
                if (_touchActions.Count == 1)
                {
                    var button = _graph.GetButtonAt(px, py);

                    if (!_graph.EditMode && button != null)
                    {
                        _touchActions.Clear();
                        button.Click();
                    }
                    else if (button != null)
                    {
                        button.ClickEdit(px, py);
                        _graph.DraggedButton = button;
                    }
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

                // Create new activity -> redirect to NewActivtyPage
                if (_graph.Mode == ActivityGraph.EditingMode.Create && _graph.EditMode)
                {
                    var time = _graph.ToLocalTime(px);
                    _touchActions.Remove(args.Id);
                    _graph.DraggedButton = null;

                    long hourMilliseconds = 60 * 60 * 1000;
                    var rememberedList = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList;
                    var activity = new LarpActivity();
                    activity.GraphY = _graph.CalculateGraphY(py);
                    activity.start = time.ToUnixTimeMilliseconds() - hourMilliseconds / 2;
                    activity.duration = hourMilliseconds;
                    activity.day = time.Day;

                    Navigation.PushAsync(new NewActivityPage((Models.DTO.LarpActivityDTO activityDTO) =>
                    {
                        //Error - must be long, otherwise two activities might have the same id
                        activityDTO.ID = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.nextID();
                        LarpActivity newActivity = activityDTO.CreateLarpActivity();
                        DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.add(newActivity);
                    }, activity
                    ));
                }
            }

            // Moving button
            if (args.Type == TouchActionType.Moved && _graph.DraggedButton != null)
            {
                if (_touchActions.ContainsKey(args.Id))
                    _touchActions[args.Id] = args;
            }

            // Control graph view
            if (args.Type == TouchActionType.Moved && _graph.DraggedButton == null)
            {
                if (_touchActions.ContainsKey(args.Id))
                    _touchActions[args.Id] = args;

                // Scroll graph
                if (_touchActions.Count == 1)
                {
                    float diffX = _graph.ToPixels(args.Location.X - _lastLocation.X);
                    float diffY = _graph.ToPixels(args.Location.Y - _lastLocation.Y);
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
                if (_graph.DraggedButton != null)
                {
                    _graph.DraggedButton.ReleaseEdit(px, py);
                    _graph.DraggedButton = null;
                }

                _touchActions.Remove(args.Id);
            }

            // Redraw graph every touch
            _graph.MouseX = args.Location.X;
            _graph.MouseY = args.Location.Y;
            _graph.InvalidateSurface();
        }
    }
}