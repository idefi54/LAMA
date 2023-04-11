using System;
using System.Threading.Tasks;
using LAMA.ActivityGraphLib;
using LAMA.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LAMA.Views
{
    /// <summary>
    /// <br>Don't create nor push this page.</br>
    /// <br>Use static method <see cref="ShowCalendarPage(INavigation)"/> instead.</br>
    /// </summary>

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalendarPage : ContentPage
    {
        private Button[] _daysInMonth;
        private Label[] _daysInWeek;
        private Button _rightButton;
        private Button _leftButton;
        private Color _color;
        private Label _dateLabel;
        private DateTime _date;
        private DateTime? _highlightDay;
        private DateTime _firstDayInMonth => _date.AddDays(-_date.Day + 1);
        private string _dateLabelText => $"{_date.Month:00}.{_date.Year:0000}";
        private TaskCompletionSource<DateTime> _taskCompletionSource;

        /// <summary>
        /// Starts at null.
        /// Selecting day on graph page updates this value.
        /// </summary>

        private CalendarPage(DateTime? highlightDay = null)
        {
            // Standard initialization
            InitializeComponent();

            _date = highlightDay ?? DateTime.Now;
            // Get rid of time (date only)
            _date = new DateTime(_date.Year, _date.Month, _date.Day);
     
            _highlightDay = highlightDay;


            // Create GUI
            (_dateLabel, _leftButton, _rightButton) = CreateDateControl();
            _daysInMonth = CreateDaysInMonth();
            _daysInWeek = CreateDaysInWeek();

            // Assign root layout
            Content = new Grid();
            Refresh(Content as Grid);
        }

        private Button[] CreateDaysInMonth()
        {
            // 31 days in a month max
            var daysInMonth = new Button[31];
            int daysInCurrentMonth = DateTime.DaysInMonth(_date.Year, _date.Month);

            // Create all 31 buttons
            for (int i = 0; i < 31; i++)
            {
                daysInMonth[i] = new Button
                {
                    Text = $"{i + 1}", // order in month
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                };

                // Save default theme color for refresh
                _color = daysInMonth[i].BackgroundColor;

                // Clicking the button will adjust the graph time offset to that day
                daysInMonth[i].Clicked += ButtonClicked;
            }

            return daysInMonth;
        }

        private Label[] CreateDaysInWeek()
        {
            // 7 days in a week
            var daysInWeek = new Label[7];
            string[] dayNames = { "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN" };

            for (int i = 0; i < 7; i++)
            {
                daysInWeek[i] = new Label
                {
                    Text = dayNames[i],
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                };
            }

            return daysInWeek;
        }

        private (Label, Button, Button) CreateDateControl()
        {
            var dateLabel = new Label();
            dateLabel.Text = _dateLabelText;
            dateLabel.VerticalOptions = LayoutOptions.Center;
            dateLabel.HorizontalOptions = LayoutOptions.Center;
            dateLabel.FontAttributes = FontAttributes.Bold;

            var leftButton = new Button();
            leftButton.Text = "<";
            leftButton.HorizontalOptions = LayoutOptions.Center;
            leftButton.VerticalOptions = LayoutOptions.Center;
            leftButton.BackgroundColor = Color.FromUint(0u);
            leftButton.FontSize = 20;
            leftButton.TextColor = Color.Blue;
            leftButton.Clicked += (object sender, EventArgs e) => { _date = _date.AddMonths(-1); Refresh(Content as Grid); };

            var rightButton = new Button();
            rightButton.Text = ">";
            rightButton.HorizontalOptions = LayoutOptions.Center;
            rightButton.VerticalOptions = LayoutOptions.Center;
            rightButton.BackgroundColor = Color.FromUint(0u);
            rightButton.FontSize = 20;
            rightButton.TextColor = Color.Blue;
            rightButton.Clicked += (object sender, EventArgs e) => { _date = _date.AddMonths(1); Refresh(Content as Grid); };

            return (dateLabel, leftButton, rightButton);
        }

        /// <summary>
        /// Change graph offset to day of the button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonClicked(object sender, EventArgs e)
        {
            DateTime date =
                _firstDayInMonth
                .AddDays(int.Parse((sender as Button).Text) - 1); // Text carries order of the day in the month

            if (_taskCompletionSource != null)
            {
                _taskCompletionSource.SetResult(date);
                _taskCompletionSource = null;
            }
        }

        /// <summary>
        /// Refreshes calendar visuals after clicking buttons. (updates to _date)
        /// </summary>
        private void Refresh(Grid grid)
        {
            grid.Children.Clear();

            _dateLabel.Text = _dateLabelText;

            // Add date control row
            grid.Children.Add(_dateLabel, 3, 0);
            grid.Children.Add(_leftButton, 2, 0);
            grid.Children.Add(_rightButton, 4, 0);

            // Add day labels
            for (int i = 0; i < 7; i++)
                grid.Children.Add(_daysInWeek[i], i, 1);

            // Update and add days in month buttons
            int daysInCurrentMonth = DateTime.DaysInMonth(_date.Year, _date.Month);
            for (int i = 0; i < daysInCurrentMonth; i++)
            {
                _daysInMonth[i].BackgroundColor = _color;
                _daysInMonth[i].BorderWidth = 0;
                _daysInMonth[i].BorderColor = Color.Transparent;

                grid.Children.Add(
                    _daysInMonth[i],
                    (i + (int)_firstDayInMonth.DayOfWeek) % 7,
                    (i + (int)_firstDayInMonth.DayOfWeek) / 7 + 2);
            }

            if (_date.Year == DateTime.Now.Year && _date.Month == DateTime.Now.Month)
                _daysInMonth[DateTime.Now.Day - 1].BackgroundColor = Color.Red;

            if (_highlightDay != null
                && _date.Year == _highlightDay?.Year
                && _date.Month == _highlightDay?.Month)
            {
                var button = _daysInMonth[_highlightDay.Value.Day - 1];
                button.BorderColor = Color.DarkGreen;
                button.BorderWidth = 7;
            }
        }

        private Task<DateTime> GetDate()
        {
            _taskCompletionSource = new TaskCompletionSource<DateTime>();
            return _taskCompletionSource.Task;
        }

        /// <summary>
        /// <br>Use like this:</br>
        ///     <br><c>
        ///     var date = await CalendarPage.ShowCalendarPage(navigation);
        ///     </c></br>
        /// </summary>
        /// <param name="navigation"></param>
        /// <returns></returns>
        public static async Task<DateTime> ShowCalendarPage(INavigation navigation, DateTime? highlightDay = null)
        {
            var page = new CalendarPage(highlightDay);
            await navigation.PushModalAsync(page);
            DateTime date = await page.GetDate();
            await navigation.PopModalAsync();
            return date;
        }

    }
}