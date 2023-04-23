using LAMA.Colors;
using LAMA.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace LAMA.Extensions
{
	public static class LarpActivityExtension
	{
        public static SKColor GetGraphColor(this LarpActivity activity)
        {
            switch (activity.status)
            {
                case LarpActivity.Status.awaitingPrerequisites:
                    return ColorPalette.ActivityGraphAwaitingPrerequisites;
                case LarpActivity.Status.readyToLaunch:
                    return ColorPalette.ActivityGraphReadyToLaunch;
                case LarpActivity.Status.launched:
                    return ColorPalette.ActivityGraphLaunched;
                case LarpActivity.Status.inProgress:
                    return ColorPalette.ActivityGraphInProgress;
                case LarpActivity.Status.completed:
                    return ColorPalette.ActivityGraphCompleted;
                case LarpActivity.Status.cancelled:
                    return ColorPalette.ActivityGraphCancelled;
                default:
                    return ColorPalette.ActivityGraphDefault;
            }
        }

        public static SKColor GetCalculatedGraphColor(this LarpActivity activity)
		{
            Color color = activity.GetColor();

            SKColor skcolor = new SKColor(
                (byte)(byte.MaxValue * color.R),
                (byte)(byte.MaxValue * color.G),
                (byte)(byte.MaxValue * color.B),
                (byte)(byte.MaxValue * color.A));

            return skcolor;
		}

        public static Color GetColor(this LarpActivity activity)
        {
            switch (activity.status)
            {
                case LarpActivity.Status.awaitingPrerequisites:
                    return ColorPalette.ActivityAwaitingPrerequisites;
                case LarpActivity.Status.readyToLaunch:
                    return ColorPalette.ActivityReadyToLaunch;
                case LarpActivity.Status.launched:
                    return ColorPalette.ActivityLaunched;
                case LarpActivity.Status.inProgress:
                    return ColorPalette.ActivityInProgress;
                case LarpActivity.Status.completed:
                    return ColorPalette.ActivityCompleted;
                case LarpActivity.Status.cancelled:
                    return ColorPalette.ActivityCancelled;
                default:
                    return ColorPalette.ActivityDefault;
            }
        }

        public static Color GetColor(this LarpActivity activity, double alpha)
        {
            return activity.GetColor().MultiplyAlpha(alpha);
        }
    }
}
