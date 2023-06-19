using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Graphics;
using LAMA.Themes;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(LAMA.Droid.AndroidPicturePainter))]
namespace LAMA.Droid
{
    internal class AndroidPicturePainter : IPicturePainter
    {
        public Stream ColorImage(Stream stream, int r, int g, int b)
        {
            var options = new BitmapFactory.Options();
            options.InMutable = true;
            Bitmap bitmap = BitmapFactory.DecodeStream(stream, null, options);

            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                {
                    int a = (byte)(bitmap.GetColor(x, y).Alpha() * 255);
                    bitmap.SetPixel(x, y, Android.Graphics.Color.Argb(a, r, g, b));
                }

            stream = new MemoryStream();
            bitmap.Compress(Bitmap.CompressFormat.Png, 1, stream); // PNG ignores 'int quality'
            stream.Position = 0;
            return stream;
        }
    }
}