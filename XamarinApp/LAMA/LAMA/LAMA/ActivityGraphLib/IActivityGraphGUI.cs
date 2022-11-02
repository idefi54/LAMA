using Xamarin.Forms;

namespace LAMA.ActivityGraphLib
{
    public interface IActivityGraphGUI
    {
        (Layout<View>, ActivityGraph) CreateGUI(INavigation navigation);
    }
}
