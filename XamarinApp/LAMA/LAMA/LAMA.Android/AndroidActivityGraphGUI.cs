using System;
using LAMA.ActivityGraphLib;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

[assembly: Dependency(typeof(LAMA.Droid.AndroidActivityGraphGUI))]
namespace LAMA.Droid
{
    internal class AndroidActivityGraphGUI : IActivityGraphGUI
    {
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

            Layout<View> dateRow = CreateDateRow(graph, canvasView);
            (Layout<View> timeRow, Label[] timeLabels) = CreateTimeRow();
            graph.TimeLabels = timeLabels;
            graph.DateView = dateRow;

            var layout = new StackLayout
            {
                Children =
                {
                    dateRow,
                    timeRow,
                    canvasLayout
                }
            };

            return (layout, graph);
        }

        private Layout<View> CreateDateRow(ActivityGraph graph, SKCanvasView canvasView)
        {
            var dateStack = new StackLayout();
            dateStack.HorizontalOptions = LayoutOptions.Start;
            dateStack.VerticalOptions = LayoutOptions.Center;
            dateStack.Orientation = StackOrientation.Horizontal;
            {
                var leftButton = new Label();
                leftButton.Text = "   <   ";
                leftButton.FontAttributes = FontAttributes.Bold;
                leftButton.VerticalOptions = LayoutOptions.Center;
                leftButton.HorizontalOptions = LayoutOptions.Center;
                leftButton.TextColor = Color.Blue;
                leftButton.BackgroundColor = Color.LightBlue;

                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (object sender, EventArgs e) =>
                {
                    graph.TimeOffset = graph.TimeOffset.AddDays(-1);
                    canvasView.InvalidateSurface();
                };
                leftButton.GestureRecognizers.Add(tapGesture);
                dateStack.Children.Add(leftButton);
            }

            {
                var now = DateTime.Now;
                var dateLabel = new Label();
                dateLabel.VerticalOptions = LayoutOptions.Center;
                dateLabel.HorizontalOptions = LayoutOptions.Center;
                dateLabel.FontAttributes = FontAttributes.Bold;
                dateLabel.TextColor = Color.Red;
                dateLabel.Text = $"{now.Day:00}.{now.Month:00}.{now.Year:0000}";
                dateStack.Children.Add(dateLabel);
            }

            {
                var rightButton = new Label();
                rightButton.Text = "   >   ";
                rightButton.FontAttributes = FontAttributes.Bold;
                rightButton.VerticalOptions = LayoutOptions.CenterAndExpand;
                rightButton.HorizontalOptions = LayoutOptions.CenterAndExpand;
                rightButton.TextColor = Color.Blue;
                rightButton.BackgroundColor = Color.LightBlue;

                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (object sender, EventArgs e) =>
                {
                    graph.TimeOffset = graph.TimeOffset.AddDays(1);
                    canvasView.InvalidateSurface();
                };
                rightButton.GestureRecognizers.Add(tapGesture);
                dateStack.Children.Add(rightButton);
            }

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