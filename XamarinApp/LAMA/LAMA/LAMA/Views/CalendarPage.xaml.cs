using System;
using LAMA.ActivityGraphLib;
using LAMA.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalendarPage : ContentPage
    {
        Button[] _daysInMonth;
        Label[] _daysInWeek;
        Button _rightButton;
        Button _leftButton;
        Color _color;
        Label _dateLabel;
        static DateTime _date;
        DateTime _first => _date.AddDays(-_date.Day + 1);
        string[] _days = { "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN" };

        public CalendarPage()
        {
            InitializeComponent();
            _daysInMonth = new Button[31];
            _daysInWeek = new Label[7];
            var grid = new Grid();
            _rightButton = new Button();
            _leftButton = new Button();
            _dateLabel = new Label();
            _date = DateTime.Now;

            _dateLabel.Text = $"{_date.Day:00}.{_date.Month:00}.{_date.Year:0000}";
            _dateLabel.VerticalOptions = LayoutOptions.Center;
            _dateLabel.HorizontalOptions = LayoutOptions.Center;
            grid.Children.Add(_dateLabel, 3, 0);

            _leftButton.Text = "<";
            _leftButton.HorizontalOptions = LayoutOptions.Center;
            _leftButton.VerticalOptions = LayoutOptions.Center;
            _leftButton.BackgroundColor = Color.FromUint(0u);
            _leftButton.FontSize = 20;
            _leftButton.TextColor = Color.Blue;
            _leftButton.Clicked += (object sender, EventArgs e) => { _date = _date.AddMonths(-1); Refresh(); };

            _rightButton.Text = ">";
            _rightButton.HorizontalOptions = LayoutOptions.Center;
            _rightButton.VerticalOptions = LayoutOptions.Center;
            _rightButton.BackgroundColor = Color.FromUint(0u);
            _rightButton.FontSize = 20;
            _rightButton.TextColor = Color.Blue;
            _rightButton.Clicked += (object sender, EventArgs e) => { _date = _date.AddMonths(1); Refresh(); };

            grid.Children.Add(_leftButton, 2, 0);
            grid.Children.Add(_rightButton, 4, 0);

            for (int i = 0; i < 7; i++)
            {
                _daysInWeek[i] = new Label {
                    Text = _days[i],
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                };
                grid.Children.Add(_daysInWeek[i], i, 1);
            }


            int dayCount = DateTime.DaysInMonth(_date.Year, _date.Month);
            for (int i = 0; i < 31; i++)
            {
                _daysInMonth[i] = new Button
                {
                    Text = $"{i + 1}",
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                };
                _color = _daysInMonth[i].BackgroundColor;

                _daysInMonth[i].Clicked += (object sender, EventArgs e) =>
                {
                    var time = ActivityGraph.Instance.TimeOffset;
                    TimeSpan small = new TimeSpan(0, time.Hour, time.Minute, time.Second, time.Millisecond);
                    ActivityGraph.Instance.TimeOffset = _first.AddDays(int.Parse((sender as Button).Text) - 1).Add(-small); Navigation.PopModalAsync();
                };
                grid.Children.Add(_daysInMonth[i], (i + (int)_first.DayOfWeek) % 7, (i + (int)_first.DayOfWeek) / 7 + 2);
            }

            for (int i = dayCount; i < 31; i++)
                _daysInMonth[i].IsVisible = false;

            if (_date.Year == DateTime.Now.Year && _date.Month == DateTime.Now.Month)
                _daysInMonth[DateTime.Now.Day - 1].BackgroundColor = Color.Red;

            Content = grid;
        }

        private void Refresh()
        {
            DateTime first = _date.AddDays(-_date.Day + 1);
            int dayCount = DateTime.DaysInMonth(_date.Year, _date.Month);
            Grid grid = Content as Grid;
            grid.Children.Clear();

            _dateLabel.Text = $"{_date.Day:00}.{_date.Month:00}.{_date.Year:0000}";

            grid.Children.Add(_dateLabel, 3, 0);
            grid.Children.Add(_leftButton, 2, 0);
            grid.Children.Add(_rightButton, 4, 0);

            for (int i = 0; i < 7; i++)
            {
                grid.Children.Add(_daysInWeek[i], i, 1);
            }

            for (int i = 0; i < dayCount; i++)
            {
                _daysInMonth[i].IsVisible = true;
                _daysInMonth[i].BackgroundColor = _color;
                grid.Children.Add(_daysInMonth[i], (i + (int)first.DayOfWeek) % 7, (i + (int)first.DayOfWeek) / 7 + 2);
            }

            if (_date.Year == DateTime.Now.Year && _date.Month == DateTime.Now.Month)
                _daysInMonth[DateTime.Now.Day - 1].BackgroundColor = Color.Red;
        }
    }
}