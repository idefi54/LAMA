using LAMA.ActivityGraphLib;
using LAMA.Views;
using SkiaSharp.Views.Forms;
using System;
using Xamarin.Forms;
using System.Windows;

[assembly: Dependency(typeof(LAMA.UWP.UWPActivityGraphGUI))]
namespace LAMA.UWP
{

    internal class UWPActivityGraphGUI : IActivityGraphGUI
    {
        private bool previous = false;

        public (Layout<View>, ActivityGraph) CreateGUI(INavigation navigation)
        {
            var canvasView = CreateCanvasView();
            Layout<View> canvasLayout = new Grid();
            canvasLayout.Children.Add(canvasView);
            canvasLayout.HorizontalOptions = LayoutOptions.FillAndExpand;
            canvasLayout.VerticalOptions = LayoutOptions.FillAndExpand;
            var graph = new ActivityGraph(canvasLayout);
            canvasView.PaintSurface += (object sender, SKPaintSurfaceEventArgs args) =>
            {
                graph.Update(args);
                graph.Draw(args.Surface.Canvas);
            };

            WPF.App.Current.MainWindow.KeyDown += (object sender, System.Windows.Input.KeyEventArgs e) =>
            {
                if (e.Key == System.Windows.Input.Key.LeftCtrl)
                    graph.SwitchActivityCreationMode(true);

                canvasView.InvalidateSurface();
            };

            WPF.App.Current.MainWindow.KeyUp += (object sender, System.Windows.Input.KeyEventArgs e) =>
            {
                if (e.Key == System.Windows.Input.Key.LeftCtrl)
                    graph.SwitchActivityCreationMode(false);

                canvasView.InvalidateSurface();
            };

            Layout<View> controlRow = CreateControlRow(graph, canvasView, navigation);
            Layout<View> dateRow = CreateDateRow(graph, canvasView);
            (Layout<View> timeRow, Label[] timeLabels) = CreateTimeRow();
            graph.TimeLabels = timeLabels;
            graph.DateView = dateRow;

            var layout = new StackLayout
            {
                Children =
                {
                    controlRow,
                    dateRow,
                    timeRow,
                    canvasLayout
                }
            };

            return (layout, graph);
        }

        private Layout<View> CreateControlRow(ActivityGraph graph, SKCanvasView canvasView, INavigation navigation)
        {
            var grid = new Grid();

            // plus and minus buttons
            {
                var plusMinusStack = new StackLayout();
                plusMinusStack.HorizontalOptions = LayoutOptions.FillAndExpand;
                plusMinusStack.Orientation = StackOrientation.Horizontal;

                var plusButton = new Button();
                plusButton.Text = "+";
                plusButton.VerticalOptions = LayoutOptions.Center;
                plusButton.HorizontalOptions = LayoutOptions.Center;
                plusButton.Clicked += (object sender, EventArgs args) => { graph.Zoom += 0.25f; canvasView.InvalidateSurface(); };
                grid.Children.Add(plusButton, 0, 0);

                var minusButton = new Button();
                minusButton.Text = "-";
                minusButton.VerticalOptions = LayoutOptions.Center;
                minusButton.HorizontalOptions = LayoutOptions.Center;
                minusButton.Clicked += (object sender, EventArgs args) => { graph.Zoom -= 0.25f; canvasView.InvalidateSurface(); };
                grid.Children.Add(minusButton, 1, 0);
            }

            // Calendar Button
            {
                var calendarButton = new Button();
                calendarButton.Text = "Calendar";
                calendarButton.VerticalOptions = LayoutOptions.Center;
                calendarButton.HorizontalOptions = LayoutOptions.Center;
                calendarButton.Clicked += async (object sender, EventArgs args) => {
                    graph.TimeOffset = await CalendarPage.ShowCalendarPage(navigation);
                };
                grid.Children.Add(calendarButton, 2, 0);
            }

            // Edit control
            {
                var editStack = new StackLayout();
                editStack.Orientation = StackOrientation.Horizontal;

                var editLabel = new Label();
                editLabel.Text = "Edit:";
                editLabel.VerticalOptions = LayoutOptions.Center;
                editLabel.HorizontalOptions = LayoutOptions.Center;
                editStack.Children.Add(editLabel);

                var editSwitch = new Switch();
                editSwitch.IsToggled = false;
                editSwitch.VerticalOptions = LayoutOptions.Center;
                editSwitch.HorizontalOptions = LayoutOptions.End;
                editSwitch.Toggled += (object sender, ToggledEventArgs e) => { graph.SwitchEditMode(e.Value); };
                editStack.Children.Add(editSwitch);
                grid.Children.Add(editStack, 3, 0);
            }

            return grid;
        }

        private Layout<View> CreateDateRow(ActivityGraph graph, SKCanvasView canvasView)
        {
            var dateStack = new StackLayout();
            dateStack.HorizontalOptions = LayoutOptions.Start;
            dateStack.VerticalOptions = LayoutOptions.Center;
            dateStack.Orientation = StackOrientation.Horizontal;

            var leftButton = new Button();
            leftButton.Text = "<";
            leftButton.FontAttributes = FontAttributes.Bold;
            leftButton.VerticalOptions = LayoutOptions.CenterAndExpand;
            leftButton.HorizontalOptions = LayoutOptions.CenterAndExpand;
            leftButton.TextColor = Color.Blue;
            leftButton.BackgroundColor = Color.FromRgba(0, 0, 0, 0);
            leftButton.Clicked += (object sender, EventArgs args) =>
            {
                graph.TimeOffset = graph.TimeOffset.AddDays(-1);
                canvasView.InvalidateSurface();
            };
            dateStack.Children.Add(leftButton);

            var now = DateTime.Now;
            var dateLabel = new Label();
            dateLabel.VerticalOptions = LayoutOptions.CenterAndExpand;
            dateLabel.HorizontalOptions = LayoutOptions.CenterAndExpand;
            dateLabel.FontAttributes = FontAttributes.Bold;
            dateLabel.TextColor = Color.Red;
            dateLabel.Text = $"{now.Day:00}.{now.Month:00}.{now.Year:0000}";
            dateStack.Children.Add(dateLabel);

            var rightButton = new Button();
            rightButton.Text = ">";
            rightButton.FontAttributes = FontAttributes.Bold;
            rightButton.VerticalOptions = LayoutOptions.CenterAndExpand;
            rightButton.HorizontalOptions = LayoutOptions.CenterAndExpand;
            rightButton.TextColor = Color.Blue;
            rightButton.BackgroundColor = Color.FromRgba(0, 0, 0, 0);
            rightButton.Clicked += (object sender, EventArgs args) =>
            {
                graph.TimeOffset = graph.TimeOffset.AddDays(1);
                canvasView.InvalidateSurface();
            };
            dateStack.Children.Add(rightButton);

            return dateStack;
        }

        private (Layout<View>, Label[]) CreateTimeRow()
        {
            var grid = new Grid();

            var timeLabels = new Label[24];
            for (int i = 0; i < 24; i++)
            {
                timeLabels[i] = new Label();
                timeLabels[i].Text = $"00:00";
                timeLabels[i].VerticalOptions = LayoutOptions.Start;
                timeLabels[i].HorizontalOptions = LayoutOptions.Start;

                grid.Children.Add(timeLabels[i]);
            }

            return (grid, timeLabels);
        }

        private SKCanvasView CreateCanvasView()
        {
            var canvasView = new SKCanvasView();
            canvasView.HorizontalOptions = LayoutOptions.FillAndExpand;
            canvasView.VerticalOptions = LayoutOptions.FillAndExpand;

            return canvasView;
        }
    }
}
