using System.IO;
using System.Threading.Tasks;

namespace LAMA.Themes
{
    public interface IPicturePainter
    {
        Stream ColorImage(Stream stream, int r, int g, int b);
    }
}
