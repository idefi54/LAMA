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
        private ActivityButton _draggedButton;
        private Dictionary<long, TouchActionEventArgs> _touchActions;
        private TouchTrackingPoint _lastLocation;
        private float _baseDistance;
        private float _baseZoom;

        public ActivityGraphPage(LarpActivity activity = null)
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
            // New Press
            if (args.Type == TouchActionType.Pressed)
            {
                // Add press to dictionary
                _touchActions[args.Id] = args;

                // Clicking a button
                if (_touchActions.Count == 1)
                {
                    _draggedButton = _graph.GetButtonAt(args.Location.X, args.Location.Y);
                    _draggedButton?.ClickEdit(args.Location.X, args.Location.Y);
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
                if (_graph.ActivityCreationMode)
                {
                    var time = _graph.ToLocalTime(_lastLocation.X);
                    _touchActions.Remove(args.Id);
                    _draggedButton = null;
                    Navigation.PushAsync(new NewActivityPage((Models.DTO.LarpActivityDTO activityDTO) =>
                    {
                        //Error - must be long, otherwise two activities might have the same id
                        activityDTO.ID = (int)DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.nextID();
                        activityDTO.start = time.ToUnixTimeMilliseconds();
                        activityDTO.day = time.Day;
                        LarpActivity newActivity = activityDTO.CreateLarpActivity();
                        DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.add(newActivity);
                    }
                    ));
                }
            }

            // Moving button
            if (args.Type == TouchActionType.Moved && _draggedButton != null)
            {
                if (_touchActions.ContainsKey(args.Id))
                    _touchActions[args.Id] = args;
                _draggedButton.MoveEdit(args.Location.X, args.Location.Y);
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
                _draggedButton?.ReleaseEdit();
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