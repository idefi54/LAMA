using LAMA.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IconSelectionPage : ContentPage
    {
        private const float IMAGE_SIZE = 20f;
        private Dictionary<Button, int> buttonIcons;
        private TaskCompletionSource<int> _taskCompletionSource;

        public IconSelectionPage(IList<string> icons = null)
        {
            InitializeComponent();
            buttonIcons = new Dictionary<Button, int>();

            if (icons == null)
            {
                var iconList = new List<string>();
                var assembly = Assembly.GetExecutingAssembly();

                foreach (var resourceName in assembly.GetManifestResourceNames())
                    if (resourceName.StartsWith("LAMA.Resources.Icons."))
                        iconList.Add(resourceName);

                icons = iconList.ToArray();
            }

            int columns = 10;
            int count = 0;

            for (int i = 0; i < icons.Count; i++)
            {
                var icon = icons[i];
                var button = new Button();
                button.HorizontalOptions = LayoutOptions.Fill;
                button.VerticalOptions = LayoutOptions.Fill;
                button.BackgroundColor = Color.Transparent;
                button.BorderColor = Color.Transparent;
                button.Clicked += ButtonClicked;
                buttonIcons[button] = i;

                var image = new Image();
                image.HorizontalOptions = LayoutOptions.Center;
                image.VerticalOptions = LayoutOptions.Center;
                image.HeightRequest = IMAGE_SIZE;
                image.WidthRequest = IMAGE_SIZE;
                image.InputTransparent = true;
                image.Parent = button;
                image.Source = ImageSource.FromResource(icon, Assembly.GetExecutingAssembly());

                int column = count % columns;
                int row = count / columns;
                Grid.Children.Add(button, column, row);
                Grid.Children.Add(image, column, row);
                count++;
            }
        }

        private void ButtonClicked(object sender, EventArgs e)
        {
            Button button = sender as Button;

            if (_taskCompletionSource != null)
            {
                _taskCompletionSource.SetResult(buttonIcons[button]);
                _taskCompletionSource = null;
            }
        }

        private Task<int> GetIcon()
        {
            _taskCompletionSource = new TaskCompletionSource<int>();
            return _taskCompletionSource.Task;
        }

        /// <summary>
        /// <br>Use like this:</br>
        ///     <br><c>
        ///     string icon = await IconSelectionPage.ShowIconSelectionPage(navigation); // Shows all available icons
        ///     </c></br>
        ///     <br><c>
        ///     string icon = await IconSelectionPage.ShowIconSelectionPage(navigation, new string[] {"LAMA.Resources.Icons.Icon1"});
        ///     </c></br>
        /// </summary>
        /// <param name="navigation"></param>
        /// <returns></returns>
        public static async Task<int> ShowIconSelectionPage(INavigation navigation, IList<string> icons = null)
        {
            var page = new IconSelectionPage(icons);
            await navigation.PushModalAsync(page);
            int icon = await page.GetIcon();
            await navigation.PopModalAsync();
            return icon;
        }
    }
}