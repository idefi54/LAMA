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
        private DateTime _firstDayInMonth => _date.AddDays(-_date.Day + 1);
        private TaskCompletionSource<DateTime> _taskCompletionSource;

        /// <summary>
        /// Starts at null.
        /// Selecting day on graph page updates this value.
        /// </summary>

        private CalendarPage()
        {
            // Standard initialization
            InitializeComponent();
            var now = DateTime.Now;
            _date = new DateTime(now.Year, now.Month, now.Day);

            // Create basic layout
            var grid = new Grid();

            // Create GUI
            (_dateLabel, _leftButton, _rightButton) = CreateDateControl(grid);
            _daysInMonth = CreateDaysInMonth(grid);
            _daysInWeek = CreateDaysInWeek(grid);

            // Assign root layout
            Content = grid;
        }

        private Button[] CreateDaysInMonth(Grid grid)
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

                // Add only corrent number of buttons
                if (i < daysInCurrentMonth)
                    grid.Children.Add(
                        daysInMonth[i],
                        (i + (int)_firstDayInMonth.DayOfWeek) % 7,
                        (i + (int)_firstDayInMonth.DayOfWeek) / 7 + 2);
            }

            // Set current day as red
            if (_date.Year == DateTime.Now.Year && _date.Month == DateTime.Now.Month)
                daysInMonth[DateTime.Now.Day - 1].BackgroundColor = Color.Red;

            return daysInMonth;
        }

        private Label[] CreateDaysInWeek(Grid grid)
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
                grid.Children.Add(daysInWeek[i], i, 1);
            }

            return daysInWeek;
        }

        private (Label, Button, Button) CreateDateControl(Grid grid)
        {
            var dateLabel = new Label();
            dateLabel.Text = $"{_date.Day:00}.{_date.Month:00}.{_date.Year:0000}";
            dateLabel.VerticalOptions = LayoutOptions.Center;
            dateLabel.HorizontalOptions = LayoutOptions.Center;
            grid.Children.Add(dateLabel, 3, 0);

            var leftButton = new Button();
            leftButton.Text = "<";
            leftButton.HorizontalOptions = LayoutOptions.Center;
            leftButton.VerticalOptions = LayoutOptions.Center;
            leftButton.BackgroundColor = Color.FromUint(0u);
            leftButton.FontSize = 20;
            leftButton.TextColor = Color.Blue;
            leftButton.Clicked += (object sender, EventArgs e) => { _date = _date.AddMonths(-1); Refresh(); };
            grid.Children.Add(leftButton, 2, 0);

            var rightButton = new Button();
            rightButton.Text = ">";
            rightButton.HorizontalOptions = LayoutOptions.Center;
            rightButton.VerticalOptions = LayoutOptions.Center;
            rightButton.BackgroundColor = Color.FromUint(0u);
            rightButton.FontSize = 20;
            rightButton.TextColor = Color.Blue;
            rightButton.Clicked += (object sender, EventArgs e) => { _date = _date.AddMonths(1); Refresh(); };
            grid.Children.Add(rightButton, 4, 0);

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

            // Get back to now offseted graph page
            //Navigation.PopModalAsync();
        }

        /// <summary>
        /// Refreshes calendar visuals after clicking buttons. (updates to _date)
        /// </summary>
        private void Refresh()
        {
            Grid grid = Content as Grid;
            grid.Children.Clear();

            _dateLabel.Text = $"{_date.Day:00}.{_date.Month:00}.{_date.Year:0000}";

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
                grid.Children.Add(
                    _daysInMonth[i],
                    (i + (int)_firstDayInMonth.DayOfWeek) % 7,
                    (i + (int)_firstDayInMonth.DayOfWeek) / 7 + 2);
            }

            if (_date.Year == DateTime.Now.Year && _date.Month == DateTime.Now.Month)
                _daysInMonth[DateTime.Now.Day - 1].BackgroundColor = Color.Red;
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
        public static async Task<DateTime> ShowCalendarPage(INavigation navigation)
        {
            var page = new CalendarPage();
            await navigation.PushModalAsync(page);
            DateTime date = await page.GetDate();
            await navigation.PopModalAsync();
            return date;
        }

    }
}