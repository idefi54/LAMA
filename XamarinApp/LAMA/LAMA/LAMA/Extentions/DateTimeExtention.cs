using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Extentions
{
	public static class DateTimeExtention
	{
		public static DateTime UnixTimeStampMilisecondsToDateTime(long unixTimeStamp)
		{
			DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			dateTime = dateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
			return dateTime;
		}

		public static long ToUnixTimeMiliseconds(this DateTime dateTime)
		{
			DateTime foo = DateTime.Now;
			long unixTime = ((DateTimeOffset)foo).ToUnixTimeMilliseconds();
			return unixTime;
		}
	}
}
