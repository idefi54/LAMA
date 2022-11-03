using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Extentions
{
	public static class DateTimeExtention
	{
		public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
		{
			DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
			return dateTime;
		}

		public static long ToUnixTimeSeconds(this DateTime dateTime)
		{
			DateTime foo = DateTime.Now;
			long unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();
			return unixTime;
		}
	}
}
