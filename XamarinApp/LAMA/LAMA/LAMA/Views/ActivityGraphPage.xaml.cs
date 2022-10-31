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
        private TouchActionEventArgs _firstTouch;
        private TouchActionEventArgs _secondTouch;
        private float _baseDistance;
        private float _baseZoom;
        private TouchTrackingPoint _lastPosition;
        private bool _mouseDown = false;

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
        }

        void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            // adds if not present
            // updates if present
            _touchActions[args.Id] = args;

            // New Press
            if (args.Type == TouchActionType.Pressed)
            {
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
                _draggedButton.Move(args.Location.X, args.Location.Y);
            }

            // Control graph view
            if (args.Type == TouchActionType.Moved && _draggedButton == null)
            {
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
}