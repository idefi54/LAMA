using Xamarin.Forms;

namespace LAMA.ActivityGraphLib
{
    /// <summary>
    /// Specific to each platform.
    /// </summary>
    public interface IActivityGraphGUI
    {
        /// <summary>
        ///  Creates whole GUI for Activity Graph Page.
        /// </summary>
        /// <param name="navigation">Some buttons redirect user somewhere else.</param>
        /// <returns></returns>
        (Layout<View>, ActivityGraph) CreateGUI(INavigation navigation);
    }
}
