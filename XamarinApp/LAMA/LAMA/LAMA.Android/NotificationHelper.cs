using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LAMA.Droid
{
    internal class NotificationHelper
    {
        private static string foregroundChannelId = "9001";
        private static Context context = global::Android.App.Application.Context;

        public Notification GetServiceStartedNotification()
        {
            var intent = new Intent(context, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.SingleTop);
            intent.PutExtra("Title", "Message");

            var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.UpdateCurrent);

            var actionIntent = new Intent(context, typeof(AlarmHandler));
            actionIntent.SetAction("KILL");
            var actionPendingIntent = PendingIntent.GetBroadcast(context, 1, actionIntent, PendingIntentFlags.UpdateCurrent);

            var notificationBuilder = new NotificationCompat.Builder(context, foregroundChannelId)
                .SetContentTitle("Location tracking")
                .SetContentText("Location is tracked in the background.")
                .SetSmallIcon(Resource.Drawable.xamarin_logo)
                .SetOngoing(true)
                .AddAction(Resource.Drawable.mtrl_ic_cancel, "KILL", actionPendingIntent)
                .SetContentIntent(pendingIntent);

            if (global::Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var notificationChannel = new NotificationChannel(
                    foregroundChannelId,
                    "Title",
                    NotificationImportance.High);

                notificationChannel.Importance = NotificationImportance.High;
                notificationChannel.EnableLights(true);
                notificationChannel.EnableVibration(true);
                notificationChannel.SetShowBadge(true);
                notificationChannel.SetVibrationPattern(new long[] { 100, 200, 300 });

                var notificationManager =
                    context.GetSystemService(Context.NotificationService)
                    as NotificationManager;

                if (notificationManager != null)
                {
                    notificationBuilder.SetChannelId(foregroundChannelId);
                    notificationManager.CreateNotificationChannel(notificationChannel);
                }
            }

            return notificationBuilder.Build();
        }
    }
}