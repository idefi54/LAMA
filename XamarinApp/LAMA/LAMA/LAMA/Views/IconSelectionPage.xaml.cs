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
        private Dictionary<Button, string> buttonIcons;
        private TaskCompletionSource<string> _taskCompletionSource;

        public IconSelectionPage(IEnumerable<string> icons = null)
        {
            InitializeComponent();
            buttonIcons = new Dictionary<Button, string>();

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

            foreach(string icon in icons)
            {
                var button = new Button();
                button.HorizontalOptions = LayoutOptions.Fill;
                button.VerticalOptions = LayoutOptions.Fill;
                button.BackgroundColor = Color.Transparent;
                button.BorderColor = Color.Transparent;
                buttonIcons[button] = icon;

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

        private Task<string> GetIcon()
        {
            _taskCompletionSource = new TaskCompletionSource<string>();
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
        public static async Task<string> ShowIconSelectionPage(INavigation navigation, IEnumerable<string> icons = null)
        {
            var page = new IconSelectionPage(icons);
            await navigation.PushModalAsync(page);
            string icon = await page.GetIcon();
            await navigation.PopModalAsync();
            return icon;
        }
    }
}