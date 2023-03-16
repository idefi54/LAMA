using LAMA.Extensions;
using LAMA.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
	public class ExampleImageViewModel : BaseViewModel
	{
		public Command ButtonExampleCommand { get; }

		string[] imagePaths = { "LAMA.Resources.Icons.message_1_dots.png", "LAMA.Resources.Icons.message_2_exp-1.png" };

		private int _currentImage;
		int currentImage
		{
			get 
			{ 
				return _currentImage; 
			}
			set 
			{
				_currentImage = value;
				_currentImage = _currentImage % imagePaths.Length;
				CurrentImagePath = ImageSource.FromResource(imagePaths[_currentImage], typeof(ExampleImageViewModel).GetTypeInfo().Assembly);
			}
		}

		private ImageSource _currentImagePath;
		public ImageSource CurrentImagePath 
		{
			get 
			{ 
				return _currentImagePath; 
			} 
			set 
			{ 
				SetProperty(ref _currentImagePath, value); 
			}
		}

		public ExampleImageViewModel()
		{
			ButtonExampleCommand = new Command(ButtonExample);

			CurrentImagePath = ImageSource.FromResource(imagePaths[currentImage], typeof(ExampleImageViewModel).GetTypeInfo().Assembly);
		}

		public void ButtonExample()
		{
			currentImage++;

			//DependencyService.Get<IMessageService>().ShowAlertAsync("This does something... the something is just this message, nothing more.\nMove along please, I want to be alone.");
		}
	}
}
