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

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ActivityGraphPage : ContentPage
    {
        private ActivityGraph _graph;
        private ActivityButton _draggedButton;
        private TouchTrackingPoint _mousePos;
        private bool _mouseDown = false;

        public ActivityGraphPage()
        {
            InitializeComponent();

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

            _graph.InvalidateSurface();
        }
    }
}