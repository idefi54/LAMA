﻿using SkiaSharp;
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
        private SKCanvasView _canvasView;
        private Switch _editSwitch;
        private Button _plusButton;
        private Button _minusButton;
        private Button _leftButton;
        private Button _rightButton;
        private Button _calendarButton;
        private Label _editLabel;
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

            _canvasView = CreateCanvasView();
            Layout<View> controlRow = CreateControlRow();
            Layout<View> dateRow = CreateDateRow();
            Layout<View> timeRow = CreateTimeRow();

            var canvasGrid = new Grid();
            canvasGrid.Children.Add(_canvasView);
            canvasGrid.HorizontalOptions = LayoutOptions.FillAndExpand;
            canvasGrid.VerticalOptions = LayoutOptions.FillAndExpand;

            var layout = new StackLayout
            {
                Effects = { touchEffect },
                Children =
                {
                    controlRow,
                    dateRow,
                    timeRow,
                    canvasGrid
                }
            };
            Content = layout;

            _graph = new ActivityGraph(canvasGrid, _timeLabels, dateRow);
            ActivityGraph.Instance = _graph;
            _canvasView.InvalidateSurface();
        }

        private Layout<View> CreateControlRow()
        {
            var grid = new Grid();

            // plus and minus buttons
            {
                var plusMinusStack = new StackLayout();
                plusMinusStack.HorizontalOptions = LayoutOptions.FillAndExpand;
                plusMinusStack.Orientation = StackOrientation.Horizontal;

                _plusButton = new Button();
                _plusButton.Text = "+";
                _plusButton.VerticalOptions = LayoutOptions.FillAndExpand;
                _plusButton.HorizontalOptions = LayoutOptions.FillAndExpand;
                _plusButton.Clicked += (object sender, EventArgs args) => { _graph.Zoom += 0.25f; _canvasView.InvalidateSurface(); };
                grid.Children.Add(_plusButton, 0, 0);

                _minusButton = new Button();
                _minusButton.Text = "-";
                _minusButton.VerticalOptions = LayoutOptions.FillAndExpand;
                _minusButton.HorizontalOptions = LayoutOptions.FillAndExpand;
                _minusButton.Clicked += (object sender, EventArgs args) => { _graph.Zoom -= 0.25f; _canvasView.InvalidateSurface(); };
                grid.Children.Add(_minusButton, 1, 0);

                //grid.Children.Add(plusMinusStack, 0, 0);
            }

            // Calendar Button
            {
                _calendarButton = new Button();
                _calendarButton.Text = "Calendar";
                _calendarButton.VerticalOptions = LayoutOptions.Center;
                _calendarButton.HorizontalOptions = LayoutOptions.Center;
                _calendarButton.Clicked += (object sender, EventArgs args) => { Navigation.PushModalAsync(new CalendarPage()); };
                grid.Children.Add(_calendarButton, 2, 0);
            }

            // Edit control
            {
                var editStack = new StackLayout();
                editStack.Orientation = StackOrientation.Horizontal;

                _editLabel = new Label();
                _editLabel.Text = "Edit:";
                _editLabel.VerticalOptions = LayoutOptions.Center;
                _editLabel.HorizontalOptions = LayoutOptions.Center;
                editStack.Children.Add(_editLabel);

                _editSwitch = new Switch();
                _editSwitch.IsToggled = false;
                _editSwitch.VerticalOptions = LayoutOptions.Center;
                _editSwitch.HorizontalOptions = LayoutOptions.End;
                _editSwitch.Toggled += (object sender, ToggledEventArgs e) => { _graph.SwitchEditMode(e.Value); };
                editStack.Children.Add(_editSwitch);
                grid.Children.Add(editStack, 3, 0);
            }

            return grid;
        }

        private Layout<View> CreateDateRow()
        {
            var dateStack = new StackLayout();
            dateStack.HorizontalOptions = LayoutOptions.Start;
            dateStack.VerticalOptions = LayoutOptions.Center;
            dateStack.Orientation = StackOrientation.Horizontal;

            _leftButton = new Button();
            _leftButton.Text = "<";
            _leftButton.FontAttributes = FontAttributes.Bold;
            _leftButton.VerticalOptions = LayoutOptions.CenterAndExpand;
            _leftButton.HorizontalOptions = LayoutOptions.CenterAndExpand;
            _leftButton.TextColor = Color.Blue;
            _leftButton.BackgroundColor = Color.FromRgba(0, 0, 0, 0);
            _leftButton.Clicked += (object sender, EventArgs args) =>
            {
                _graph.TimeOffset = _graph.TimeOffset.AddDays(-1);
                _canvasView.InvalidateSurface();
            };
            dateStack.Children.Add(_leftButton);

            var now = DateTime.Now;
            _dateLabel = new Label();
            _dateLabel.VerticalOptions = LayoutOptions.CenterAndExpand;
            _dateLabel.HorizontalOptions = LayoutOptions.CenterAndExpand;
            _dateLabel.FontAttributes = FontAttributes.Bold;
            _dateLabel.TextColor = Color.Red;
            _dateLabel.Text = $"{now.Day:00}.{now.Month:00}.{now.Year:0000}";
            dateStack.Children.Add(_dateLabel);

            _rightButton = new Button();
            _rightButton.Text = ">";
            _rightButton.FontAttributes = FontAttributes.Bold;
            _rightButton.VerticalOptions = LayoutOptions.CenterAndExpand;
            _rightButton.HorizontalOptions = LayoutOptions.CenterAndExpand;
            _rightButton.TextColor = Color.Blue;
            _rightButton.BackgroundColor = Color.FromRgba(0, 0, 0, 0);
            _rightButton.Clicked += (object sender, EventArgs args) =>
            {
                _graph.TimeOffset = _graph.TimeOffset.AddDays(1);
                _canvasView.InvalidateSurface();
            };
            dateStack.Children.Add(_rightButton);

            return dateStack;
        }

        private Layout<View> CreateTimeRow()
        {
            var grid = new Grid();

            _timeLabels = new Label[24];
            for (int i = 0; i < 24; i++)
            {
                _timeLabels[i] = new Label();
                _timeLabels[i].Text = $"00:00";
                _timeLabels[i].VerticalOptions = LayoutOptions.Start;
                _timeLabels[i].HorizontalOptions = LayoutOptions.Start;
                //_timeLabels[i].FontSize = 14;

                grid.Children.Add(_timeLabels[i]);
            }

            return grid;
        }

        private SKCanvasView CreateCanvasView()
        {
            var canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            canvasView.HorizontalOptions = LayoutOptions.FillAndExpand;
            canvasView.VerticalOptions = LayoutOptions.FillAndExpand;

            return canvasView;
        }

        private void ReloadActivities()
        {
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
            if (_graph.DirtyActivities)
            {
                ReloadActivities();
                _graph.DirtyActivities = false;
            }
        }
    }
}