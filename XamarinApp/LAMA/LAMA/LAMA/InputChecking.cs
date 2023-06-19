using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA
{
    public static class InputChecking
    {
        public static bool CheckInput(string input, string inputName, int maxLength, bool canBeEmpty = false)
        {
            if (!canBeEmpty && String.IsNullOrWhiteSpace(input))
            {
                App.Current.MainPage.DisplayAlert("Chybějící Údaj", $"Pole \"{inputName}\" nebylo vyplněno, vyplňte prosím toto pole.", "OK");
                return false;
            }
            if (input != null && input.Length > maxLength) 
            {
                App.Current.MainPage.DisplayAlert("Maximální Délka Vstupu", $"V poli \"{inputName}\" byla překročena maximální délka vstupu ({maxLength} znaků)", "OK");
                return false;
            }
            return true;
        }
    }
}
