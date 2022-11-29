using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Extentions
{
	public static class EnumExtentions
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
	}
}