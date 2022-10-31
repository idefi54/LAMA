﻿using LAMA.Models;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Xamarin.Forms;

namespace LAMA.ActivityGraphLib
{
    public class ActivityGraph
    {
        public static ActivityGraph Instance
        {
            get;
            set;
        }

        // private
        private float _width;
        private float _height;
        public float OffsetY { get; private set; }
        public float XamOffset => (float)_canvasLayout.Y;
        private float _columnWidth => _width / 24;
        private float _maxOffsetY => -_height * Zoom + _height - 200;

        public Label[] TimeLabels { get; set; }
        public Layout<View> DateView { get; set; }
        private Layout<View> _canvasLayout;
        private Label _dateLabel => DateView?.Children[1] as Label;
        private SKCanvasView _canvasView => _canvasLayout.Children[0] as SKCanvasView;

        // Public
        private float _zoom;
        public float Zoom { get { return _zoom; } set { _zoom = Math.Max(1, value); } }

        private DateTime _timeOffset;
        public DateTime TimeOffset
        {
            get { return _timeOffset; }
            set
            {
                _timeOffset = value;
                if (Math.Abs(_timeOffset.Subtract(loadPoint).Days) > 3)
                {
                    ReloadActivities();
                    loadPoint = _timeOffset;
                }

            }
        }

        private static DateTime loadPoint;
        public float MinuteWidth => _columnWidth / 60;
        public bool DirtyActivities = false;

        public ActivityGraph(Layout<View> canvasGrid)
        {
            Zoom = 2;
            OffsetY = 0;
            _canvasLayout = canvasGrid;
            TimeOffset = DateTime.Now;
            ReloadActivities();
            _canvasView.InvalidateSurface();
        }

        public float FromPixels(float x) => x * (float)_canvasView.Width / _width;
        public float ToPixels(float x) => x * _width / (float)_canvasView.Width;

        public void Update(SKPaintSurfaceEventArgs args)
        {
            _width = args.Info.Width;
            _height = args.Info.Height;

            if (TimeLabels == null)
                return;

            if (_width < 1000)
            {
                //_dateLabel.FontSize = 8;
                foreach(var label in TimeLabels)
                    label.FontSize = 8;
            
            } else
            {
                //_dateLabel.FontSize = 14;
                foreach (var label in TimeLabels)
                    label.FontSize = 14;
            }
        }

        public void Move(float dx, float dy)
        {
            dx = ToPixels(dx);
            dx /= _columnWidth * Zoom / 60;
            TimeOffset = TimeOffset.AddMinutes(-dx);
            OffsetY += ToPixels(dy);
            OffsetY = Math.Min(OffsetY, 0);
            OffsetY = Math.Max(_maxOffsetY, OffsetY);
        }

        public void Draw(SKCanvas canvas)
        {
            canvas.Clear(SKColors.Black);
            foreach (ActivityButton button in ActivityButtons())
                button.Update();

            SKPaint paint = new SKPaint();
            paint.Color = SKColors.Blue;
            paint.StrokeWidth = 1;
            canvas.DrawLine(0, 5, _width, 5, paint);

            int columnCount = (int)Math.Round(_width / _columnWidth / Zoom);
            float minutePosition = _columnWidth * Zoom - TimeOffset.Minute * (_columnWidth * Zoom / 60);
            float columnOffset = minutePosition % (_columnWidth * Zoom);
            if (columnOffset < 0) columnOffset += _columnWidth * Zoom;

            if (TimeLabels != null)
                foreach (Label label in TimeLabels)
                    label.IsVisible = false;

            if (_dateLabel != null)
                _dateLabel.Text = $"{TimeOffset.Day:00}.{TimeOffset.Month:00}.{TimeOffset.Year:0000}";

            for (int i = 0; i < columnCount; i++)
            {
                int time = (TimeOffset.Hour + i) % 24;
                if (TimeOffset.Minute > 0) time = (time + 1) % 24;

                if (TimeLabels != null)
                {
                    TimeLabels[i].IsVisible = true;
                    TimeLabels[i].Text = $"{time:00}:00";
                    TimeLabels[i].TranslationX = FromPixels(_columnWidth * Zoom * i + columnOffset);
                }

                if (time == 0 && _dateLabel != null)
                {
                    var t = TimeOffset;

                    // this if happens if we draw the line exactly at position 0 so it is not tommorrow yet
                    if (TimeOffset.Minute != 0 || TimeOffset.Hour != 0)
                        t = TimeOffset.AddDays(1);

                    _dateLabel.Text = $"{t.Day:00}.{t.Month:00}.{t.Year:0000}";
                    DateView.TranslationX = FromPixels(_columnWidth * Zoom * i + columnOffset);
                }

                paint.PathEffect = SKPathEffect.CreateDash(new float[] { 15f, 10f }, -OffsetY);
                paint.Color = (time == 0) ? SKColors.Red : SKColors.Blue;
                paint.Color = paint.Color.WithAlpha(125);
                paint.StrokeWidth = (time == 0) ? 3 : 1;

                canvas.DrawLine(
                    _columnWidth * Zoom * i + columnOffset,
                    0,
                    _columnWidth * Zoom * i + columnOffset,
                    _height * Zoom + OffsetY,
                    paint
                    );
            }

            if (DateView?.TranslationX < 0)
                DateView.TranslationX = 0;

            if (DateView?.TranslationX > Application.Current.MainPage.Width - DateView?.Width)
                DateView.TranslationX = Application.Current.MainPage.Width - DateView.Width;

            paint.PathEffect = null;
            paint.Color = SKColors.Blue;
            paint.StrokeWidth = 1;
            canvas.DrawLine(0, _height * Zoom + OffsetY, _width, _height * Zoom + OffsetY, paint);

            paint.Color = SKColors.Gray;
            float offset = 0;
            canvas.DrawLine(5, offset, 5, _height - 200 + offset, paint);
            canvas.DrawRoundRect(0, OffsetY / _maxOffsetY * (_height - 200 - 20) + offset, 10, 20, 5, 5, paint);
            paint.StrokeWidth = 3;
            canvas.DrawLine(0, offset, 10, offset, paint);
            canvas.DrawLine(0, offset + _height - 200, 10, offset + _height - 200, paint);

            DrawConnections(canvas);
        }

        public void SwitchEditMode(bool edit)
        {
            foreach (ActivityButton button in ActivityButtons())
                button.IsEnabled = !edit;
        }

        public void ReloadActivities()
        {

            // Remove all instances of ActivityButton
            for (int i = 0; i < _canvasLayout.Children.Count - 1; i++)
                _canvasLayout.Children.RemoveAt(1);

            var rememberedList = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList;
            var activities = rememberedList.sqlConnection.ReadData();
            var currentActivities =
                            from activity in activities
                            where Math.Abs(activity.day - TimeOffset.Day) <= 9 ||
                            DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) - Math.Abs(activity.day - DateTime.Now.Day) <= 9
                            select activity;

            foreach (LarpActivity activity in currentActivities)
                _canvasLayout.Children.Add(new ActivityButton(activity, this));
        }

        public void DrawConnections(SKCanvas canvas)
        {
            foreach (ActivityButton button1 in ActivityButtons())
                foreach (ActivityButton button2 in ActivityButtons())
                    if (button1.Activity.prerequisiteIDs.Contains(button2.Activity.ID))
                        ActivityButton.DrawConnection(canvas, this, button1, button2);
        }

        public ActivityButton GetButtonAt(float x, float y)
        {
            foreach (ActivityButton button in ActivityButtons())
            {
                if (button.Bounds.Offset(button.TranslationX, button.TranslationY + _canvasLayout.Y).Contains(x, y))
                    return button;
            }

            return null;
        }

        public ActivityButton AddActivity(LarpActivity activity)
        {
            var button = new ActivityButton(activity, this);
            _canvasLayout.Children.Add(button);
            return button;
        }

        public void InvalidateSurface()
        {
            _canvasView.InvalidateSurface();
        }

        private IEnumerable<ActivityButton> ActivityButtons()
        {
            for (int i = 1; i < _canvasLayout.Children.Count; i++)
                yield return _canvasLayout.Children[i] as ActivityButton;
        }
    }
}