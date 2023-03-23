using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace LAMA.Models
{
    internal static class PermissionsManager
    {
        public static void GiveAllPermissions(CP cp)
        {
            foreach (CP.PermissionType permission in Enum.GetValues(typeof(CP.PermissionType)))
            {
                if (!cp.permissions.Contains(permission))
                    cp.permissions.Add(permission);
            }
        }

        public static void GiveAllPermissions()
        {
            CP cp = DatabaseHolder<CP, CPStorage>.Instance.rememberedList.getByID(LocalStorage.cpID);
            if (cp != null)
            {
                GiveAllPermissions(cp);
            }
        }

        public static void GivePermission(CP cp, CP.PermissionType permission)
        {
            if (!cp.permissions.Contains(permission))
                cp.permissions.Add(permission);
        }

        public static void GivePermission(CP.PermissionType permission)
        {
            CP cp = DatabaseHolder<CP, CPStorage>.Instance.rememberedList.getByID(LocalStorage.cpID);
            if (cp != null)
            {
                GivePermission(cp, permission);
            }
        }
    }
}
