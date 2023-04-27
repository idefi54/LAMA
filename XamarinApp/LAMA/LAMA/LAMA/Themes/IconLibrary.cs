using System;
using System.Collections.Generic;
using System.Reflection;
using LAMA.Models;
using System.Text;

namespace LAMA.Themes
{
    public static class IconLibrary
    {
        private const string ICON_PREFIX = "LAMA.Resources.Icons.";

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

        };

        private static readonly string[] _larpActivityIcons =
        {
                ICON_PREFIX + "time_1.png",
                ICON_PREFIX + "mini_next.png",
                ICON_PREFIX + "profile_close_add.png",
                ICON_PREFIX + "sword.png",
                ICON_PREFIX + "accept_cr.png",
                ICON_PREFIX + "sword.png"
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
                case LarpActivity.Status.awaitingPrerequisites: return _larpActivityIcons[0];
                case LarpActivity.Status.readyToLaunch: return _larpActivityIcons[1];
                case LarpActivity.Status.launched: return _larpActivityIcons[2];
                case LarpActivity.Status.inProgress: return _larpActivityIcons[3];
                case LarpActivity.Status.completed: return _larpActivityIcons[4];
                default: return _larpActivityIcons[0];
            }

        }
    }
}
