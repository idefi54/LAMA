using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Extensions
{
	public static class EnumExtension
	{
		public static string ToShortString(this LarpActivity.EventType eventType)
		{
			switch (eventType)
			{
				case LarpActivity.EventType.normal:
					return "(N)";
				case LarpActivity.EventType.preparation:
					return "(P)";
			}
			return "(?)";
		}

		public static string ToFriendlyString(this LarpActivity.EventType eventType)
		{
			switch (eventType)
			{
				case LarpActivity.EventType.normal:
					return "Normální";
				case LarpActivity.EventType.preparation:
					return "Příprava";
			}
			return $"Unknown {nameof(LarpActivity.EventType)}";
		}

		public static string ToFriendlyString(this LarpActivity.Status status)
		{
            switch (status)
            {
                case LarpActivity.Status.awaitingPrerequisites:
                    return "Čeká na předpoklady";
                case LarpActivity.Status.readyToLaunch:
                    return "Připravena na spuštění";
                case LarpActivity.Status.launched:
                    return "Spuštěna";
                case LarpActivity.Status.inProgress:
                    return "V průběhu";
                case LarpActivity.Status.completed:
                    return "Dokončená";
                case LarpActivity.Status.cancelled:
					return "Zrušená";
            }
            return $"Unknown {nameof(LarpActivity.Status)}";
        }
    }
}