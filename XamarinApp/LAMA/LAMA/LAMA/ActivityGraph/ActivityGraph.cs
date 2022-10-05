using LAMA.Models;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ActivityGraphLib
{
    public class ActivityGraph
    {
        // private
        private float _width;
        private float _height;
        public float OffsetY { get; private set; }
        public float XamOffset => (float)_canvasView.TranslationY;
        private float _columnWidth => _width / 24;
        private float _maxOffsetY => -_height * Zoom + _height - 200;

        private Label[] _timeLabels;
        private Label _dateLabel;
        private List<ActivityButton> _activityButtons;
        private SKCanvasView _canvasView;
        private Button _leftButton;
        private Button _rightButton;

        // Public
        private float _zoom;
        public float Zoom { get { return _zoom; } set { _zoom = Math.Max(1, value); } }
        public static DateTime TimeOffset = DateTime.Now;
        public float MinuteWidth => _columnWidth / 60;

        public ActivityGraph(SKCanvasView canvasView, Label[] timeLabels, Label dateLabel, Button leftButton, Button rightButton)
        {
            _canvasView = canvasView;
            Zoom = 2;
            OffsetY = 0;
            //TimeOffset = DateTime.Now;
            _timeLabels = timeLabels;
            _dateLabel = dateLabel;
            _activityButtons = new List<ActivityButton>();
            _leftButton = leftButton;
            _rightButton = rightButton;
        }

        public float FromPixels(float x) => x * (float)_canvasView.Width / _width;
        public float ToPixels(float x) => x * _width / (float)_canvasView.Width;

        public void Update(SKPaintSurfaceEventArgs args)
        {
            _width = args.Info.Width;
            _height = args.Info.Height;
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
            foreach (ActivityButton button in _activityButtons)
                button.Update();

            SKPaint paint = new SKPaint();
            paint.Color = SKColors.Blue;
            paint.StrokeWidth = 1;
            canvas.DrawLine(0, 5, _width, 5, paint);

            int columnCount = (int)Math.Round(_width / _columnWidth / Zoom);
            float minutePosition = _columnWidth * Zoom - TimeOffset.Minute * (_columnWidth * Zoom / 60);
            float columnOffset = minutePosition % (_columnWidth * Zoom);
            if (columnOffset < 0) columnOffset += _columnWidth * Zoom;

            foreach (Label label in _timeLabels) label.IsVisible = false;

            _dateLabel.TranslationX = _leftButton.Width * 2;
            _dateLabel.FontAttributes = FontAttributes.Bold;
            _dateLabel.Text = $"{TimeOffset.Day:00}.{TimeOffset.Month:00}.{TimeOffset.Year:0000}";
            _dateLabel.TranslationY = _canvasView.TranslationY - _dateLabel.Height - _timeLabels[0].Height;


            for (int i = 0; i < columnCount; i++)
            {
                int time = (TimeOffset.Hour + i) % 24;
                if (TimeOffset.Minute > 0) time = (time + 1) % 24;
                _timeLabels[i].IsVisible = true;
                _timeLabels[i].Text = $"{time:00}:00";
                _timeLabels[i].TranslationX = FromPixels(_columnWidth * Zoom * i + columnOffset);
                _timeLabels[i].TranslationY = _canvasView.TranslationY - _timeLabels[i].Height;

                if (time == 0)
                {
                    var tomorrow = TimeOffset.AddDays(1);
                    _dateLabel.Text = $"{tomorrow.Day:00}.{tomorrow.Month:00}.{tomorrow.Year:0000}";
                    _dateLabel.TranslationX = FromPixels(_columnWidth * Zoom * i + columnOffset);
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

            if (_dateLabel.TranslationX < _leftButton.Width * 2)
                _dateLabel.TranslationX = _leftButton.Width * 2;
            if (_dateLabel.TranslationX > Application.Current.MainPage.Width - _rightButton.Width * 2 - _dateLabel.Width)
                _dateLabel.TranslationX = Application.Current.MainPage.Width - _rightButton.Width * 2 - _dateLabel.Width;

            _leftButton.TranslationY = _dateLabel.TranslationY + _dateLabel.Height / 2 - _rightButton.Height / 2;
            _rightButton.TranslationY = _dateLabel.TranslationY + _dateLabel.Height / 2 - _rightButton.Height / 2;
            _leftButton.TranslationX = _dateLabel.TranslationX - _leftButton.Width;
            _rightButton.TranslationX = _dateLabel.TranslationX + _dateLabel.Width;

            paint.PathEffect = null;
            paint.Color = SKColors.Blue;
            paint.StrokeWidth = 1;
            canvas.DrawLine(0, _height * Zoom + OffsetY, _width, _height * Zoom + OffsetY, paint);

            paint.Color = SKColors.Gray;
            float offset = 20;
            canvas.DrawLine(5, offset, 5, _height - 200 + offset, paint);
            canvas.DrawRoundRect(0, OffsetY / _maxOffsetY * (_height - 200 - 20) + offset, 10, 20, 5, 5, paint);
            paint.StrokeWidth = 3;
            canvas.DrawLine(0, offset, 10, offset, paint);
            canvas.DrawLine(0, offset + _height - 200, 10, offset + _height - 200, paint);
        }

        public void SwitchEditMode(bool edit)
        {
            foreach (ActivityButton button in _activityButtons)
            {
                button.IsEnabled = !edit;
            }
        }

        public ActivityButton GetButtonAt(float x, float y)
        {
            foreach (ActivityButton button in _activityButtons)
            {
                if (button.Bounds.Offset(button.TranslationX, button.TranslationY).Contains(x, y))
                    return button;
            }

            return null;
        }

        public ActivityButton AddActivity(LarpActivity activity)
        {
            var button = new ActivityButton(activity, this);
            _activityButtons.Add(button);
            return button;
        }
    }
}
