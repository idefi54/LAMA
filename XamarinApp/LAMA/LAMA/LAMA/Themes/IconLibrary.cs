using System;
using System.Collections.Generic;
using System.Reflection;
using LAMA.Models;
using System.Text;
using Xamarin.Forms;
using System.Drawing;

using XColor = Xamarin.Forms.Color;
using DColor = System.Drawing.Color;
using System.IO;

namespace LAMA.Themes
{
    public static class IconLibrary
    {
        private const string ICON_PREFIX = "LAMA.Resources.Icons.";
        private const string ICON_BINDING_PREFIX = "LAMA.Resources.Icons.";

        public static ImageSource SaveIconSource => ImageSource.FromResource(ICON_PREFIX + "save_1.svg", Assembly.GetExecutingAssembly());

        public static readonly string[] _pointOfInterestIcons =
        {
            ICON_PREFIX + "electro_2.png",
            ICON_PREFIX + "flag_2.png",
            ICON_PREFIX + "home_2_1.png",
            ICON_PREFIX + "map.png",
            ICON_PREFIX + "music_1.png",
            ICON_PREFIX + "phone.png",
            ICON_PREFIX + "prize_1.png",
            ICON_PREFIX + "shirt_1.png",
            ICON_PREFIX + "bag_3_open.png",

        };

        private static readonly string[] _larpActivityStatusIcons =
        {
                ICON_PREFIX + "time_1.png",
                ICON_PREFIX + "mini_next.png",
                ICON_PREFIX + "profile_close_add.png",
                ICON_PREFIX + "sword.png",
                ICON_PREFIX + "accept_cr.png",
                ICON_PREFIX + "X_simple_1.png"
        };

        private static readonly string[] _larpActivityIcons =
        {
                ICON_PREFIX + "stop_cr.png",
                ICON_PREFIX + "tes_2.png",
                ICON_PREFIX + "pen_1.png",
                ICON_PREFIX + "profiles_2.png",
                ICON_PREFIX + "message_1_dots.png",
                ICON_PREFIX + "fix_2.png",
                ICON_PREFIX + "eye_1.png",
                ICON_PREFIX + "sword.png",
                ICON_PREFIX + "shield.png",
                ICON_PREFIX + "aim_1.png",
        };

        private static readonly string[] _cpIcons = { ICON_PREFIX + "location_3_profile.png" };

        private static Dictionary<Type, string[]> _iconsByClass = new Dictionary<Type, string[]>
        {
            { typeof(PointOfInterest), _pointOfInterestIcons },
            { typeof(LarpActivity), _larpActivityIcons },
            { typeof(CP), _cpIcons },
        };

        public static string[] GetAllIcons()
        {
            var iconList = new List<string>();
            var assembly = Assembly.GetExecutingAssembly();

            foreach (var resourceName in assembly.GetManifestResourceNames())
                if (resourceName.StartsWith(ICON_PREFIX))
                    iconList.Add(resourceName);

            return iconList.ToArray();
        }

        public static string[] GetIconsByClass<T>()
        {
            Type type = typeof(T);
            if (!_iconsByClass.ContainsKey(type))
                return null;

            return _iconsByClass[type];
        }

        public static string GetIconsByLarpActivityStatus(LarpActivity.Status status)
        {
            switch (status)
            {
                case LarpActivity.Status.awaitingPrerequisites: return _larpActivityStatusIcons[0];
                case LarpActivity.Status.readyToLaunch: return _larpActivityStatusIcons[1];
                case LarpActivity.Status.launched: return _larpActivityStatusIcons[2];
                case LarpActivity.Status.inProgress: return _larpActivityStatusIcons[3];
                case LarpActivity.Status.completed: return _larpActivityStatusIcons[4];
                case LarpActivity.Status.cancelled: return _larpActivityIcons[5];
                default: return _larpActivityIcons[0];
            }
        }

        public static ImageSource GetImageSourceFromResourcePath(string resourcePath, XColor? color = null)
        {
            var assembly = Assembly.GetExecutingAssembly();
            Bitmap bitmap = Bitmap.FromStream(assembly.GetManifestResourceStream(resourcePath)) as Bitmap;

            color = XColor.Red;
            if (color != null)
            {
                for (int y = 0; y < bitmap.Height; y++)
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        int r = (byte)(color.Value.R * 255);
                        int g = (byte)(color.Value.G * 255);
                        int b = (byte)(color.Value.B * 255);
                        int a = bitmap.GetPixel(x, y).A;
                        var dColor = DColor.FromArgb(a, r, g, b);
                        bitmap.SetPixel(x, y, dColor);
                    }
            }


            return ImageSource.FromStream(() =>
            {
                Stream stream = new MemoryStream();
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream;
            });
        }

        public static byte[] GetByteArrayFromResourcePath(string resourcePath, XColor? color = null)
        {
            var assembly = Assembly.GetExecutingAssembly();
            Bitmap bitmap = Bitmap.FromStream(assembly.GetManifestResourceStream(resourcePath)) as Bitmap;

            if (color != null)
            {
                for (int y = 0; y < bitmap.Height; y++)
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        int r = (byte)(color.Value.R * 255);
                        int g = (byte)(color.Value.G * 255);
                        int b = (byte)(color.Value.B * 255);
                        int a = bitmap.GetPixel(x, y).A;
                        var dColor = DColor.FromArgb(a, r, g, b);
                        bitmap.SetPixel(x, y, dColor);
                    }
            }


            Stream stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Position = 0;
            return stream.ToBytes();
        }
    }
}
