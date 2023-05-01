using System;
using System.Collections.Generic;
using System.Reflection;
using LAMA.Models;
using System.Text;
using Xamarin.Forms;

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
                default: return _larpActivityStatusIcons[0];
            }

        }
    }
}
