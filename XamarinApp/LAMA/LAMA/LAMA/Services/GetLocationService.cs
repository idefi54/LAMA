using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace LAMA.Services
{
    public class GetLocationService
    {
        /// <summary>
        /// Stops all GetLocationServices from running.
        /// </summary>
        public static void Stop() => _running = false;
        public static bool IsRunning => _running;
        public const string SERVICE_RUNNING = "LocationServiceRunning";
        private static bool _running = false;

        public async Task Run(CancellationToken token)
        {
            _running = true;
            await Task.Run(async () =>
            {
                while (_running)
                {
                    //token.ThrowIfCancellationRequested();
                    try
                    {
                        await Task.Delay(3_000);

                        var request = new GeolocationRequest(GeolocationAccuracy.High);
                        var location = await Geolocation.GetLocationAsync(request);
                        if (location != null)
                        {
                            var message = new LocationMessage
                            {
                                Latitude = location.Latitude,
                                Longitude = location.Longitude
                            };

                            Device.BeginInvokeOnMainThread(() => MessagingCenter.Send(message, "Location"));
                        }
                    } catch (Exception ex)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            var errorMessage = new LocationErrorMessage();
                            MessagingCenter.Send(errorMessage, $"Error while updating location: {ex.Message}");
                        });
                    }
                }

                return;

            }, token);
        }
    }
}
