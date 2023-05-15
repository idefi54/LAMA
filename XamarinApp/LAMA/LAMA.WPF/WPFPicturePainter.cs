using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using LAMA.Themes;
using Xamarin.Forms;

using Color = System.Drawing.Color;

[assembly: Dependency(typeof(LAMA.WPF.WPFPicturePainter))]
namespace LAMA.WPF
{
    class WPFPicturePainter : IPicturePainter
    {
        public Stream ColorImage(Stream stream, int r, int g, int b)
        {
            Bitmap bitmap = new Bitmap(stream);

            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                {
                    int a = bitmap.GetPixel(x, y).A;
                    bitmap.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                }

            stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            stream.Position = 0;
            return stream;

        }
    }
}
