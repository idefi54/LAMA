using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace LAMA.Themes
{
    public static class ColorPalette
    {
        public static Color BackgroundColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        public static Color PrimaryColor = new Color(0.3f, 0.55f, 1.0f, 1.0f);
        public static Color LighterPrimaryColor = new Color(0.5f, 0.75f, 1.0f, 1.0f);
        public static Color TextColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
        public static Color MicroTextColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
        public static Color ButtonTextColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        public static Color DisabledButtonColor = new Color(0.6f, 0.6f, 0.6f, 1.0f);
        public static Color DeleteButtonColor = new Color(0.9f, 0.3f, 0.2f, 1.0f);

        public static Color MyMessageColor = Color.LightSkyBlue;
        public static Color OtherMessageColor = Color.LightGray;

        public static Color InteractableColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        
        public static SKColor ActivityGraphAwaitingPrerequisites = SKColors.White;
        public static SKColor ActivityGraphReadyToLaunch = SKColors.LightBlue;
        public static SKColor ActivityGraphLaunched = SKColors.LightGreen;
        public static SKColor ActivityGraphInProgress = SKColors.PeachPuff;
        public static SKColor ActivityGraphCompleted = SKColors.Gray;
        public static SKColor ActivityGraphCancelled = SKColors.Red;
        public static SKColor ActivityGraphDefault = SKColors.White;


        public static Color ActivityAwaitingPrerequisites = Color.FromRgb(0.3, 0.3, 0.3);
        public static Color ActivityReadyToLaunch = Color.DarkBlue;
        public static Color ActivityLaunched = Color.DarkGreen;
        public static Color ActivityInProgress = Color.Orange;
        public static Color ActivityCompleted = Color.Purple;//Color.FromRgb(0.1, 0.1, 0.1);
        public static Color ActivityCancelled = Color.Red;
        public static Color ActivityDefault = Color.DarkGray;

        public static Color BorrowColor = Color.SteelBlue;
        public static Color ReturnColor = Color.SpringGreen;

    }
}
