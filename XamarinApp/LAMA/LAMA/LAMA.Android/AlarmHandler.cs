using Android.Content;
using LAMA.Services;
using System.Diagnostics;

namespace LAMA.Droid
{
    [BroadcastReceiver(Enabled = true, Label = "Local Notifications Broadcast Receiver", Exported = true)]
    public class AlarmHandler : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {

            Debug.WriteLine("RECIEVED");

            if (intent.Action == "KILL")
            {
                Debug.WriteLine("KILL BUTTON PRESSED");

                GetLocationService.Stop();
                context.StopService(new Intent(context, typeof(AndroidLocationService)));
            }
        }
    }
}