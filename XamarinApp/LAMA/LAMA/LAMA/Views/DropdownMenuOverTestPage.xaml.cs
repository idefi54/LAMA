using LAMA.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LAMA.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DropdownMenuOverTestPage : ContentPage
	{
		public DropdownMenuOverTestPage()
		{
			InitializeComponent();

			BindingContext = new DropdownMenuTestViewModel(DropdownAnimation);
		}


		// the entire code bellow was needed to perform the animation...
		// ...I understand if noone want's to use it, I wouldn't as well, but I wanted to try how it's done in xamarin
		double desiredHeight = 200;
		double desiredOpacity = 0.2;

		// animation taken from: https://stackoverflow.com/questions/36498306/xamarin-forms-fade-to-hidden/36501796#36501796
		public void DropdownAnimation(bool show, Action<double,bool> OnCompleteCallback)
		{
			// get reference to the layout to animate
			var dropdownMenu = this.FindByName<Frame>("DropdownMenu");
			var fadeout = this.FindByName<BoxView>("Fadeout");


			// setup information for animation
			double startingHeight = show ? 0 : desiredHeight; // the layout's height when we begin animation
			double endingHeight = show ? desiredHeight : 0; // final desired height of the layout
			Rectangle rectangle = dropdownMenu.Bounds;
			Action<double> callback = input => { rectangle.Height = input; dropdownMenu.Layout(rectangle); }; // update the height of the layout with this callback
			double startingFade = show ? 0 : desiredOpacity;
			double endingFade = show ? desiredOpacity : 0;
			Action<double> fadecallback = input => { fadeout.Opacity = input; };

			uint rate = 16; // pace at which aniation proceeds
			uint length = 1000; // one second animation
			Easing easing = Easing.CubicOut; // There are a couple easing types, just tried this one for effect

			// now start animation with all the setup information
			dropdownMenu.Animate("invis", callback, startingHeight, endingHeight, rate, length, easing, OnCompleteCallback);
			fadeout.Animate("fade", fadecallback, startingFade, endingFade, rate, length, easing);
		}
	}
}