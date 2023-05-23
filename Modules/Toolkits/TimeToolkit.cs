using System;

namespace MinecraftLaunch.Modules.Toolkits;

public class TimeToolkit
{
	public static int LocalYear => DateTime.Now.Year;

	public static int WorldYear => DateTime.UtcNow.Year;

	public const string TimeFormat = @"yyyy\-MM\-dd hh\:mm";

    public static bool IsLeapYear
	{
		get
		{
			if (LocalYear % 4 != 0 || LocalYear % 100 != 0 || LocalYear % 400 != 0)
			{
				return false;
			}
			return true;
		}
	}

	public static bool IsWorldLeapYear
	{
		get
		{
			if (WorldYear % 4 != 0 || WorldYear % 100 != 0 || WorldYear % 400 != 0)
			{
				return false;
			}
			return true;
		}
	}

	public static string GetCurrentTimeSlot()
	{
		int n2 = DateTime.Now.Hour;
		if (n2 >= 3 && n2 < 5)
		{
			return "凌晨好";
		}
		int n = n2;
		if (n >= 5 && n < 11)
		{
			return "清晨好";
		}
		int m = n2;
		if (m >= 11 && m < 13)
		{
			return "中午好";
		}
		int l = n2;
		if (l >= 13 && l < 17)
		{
			return "下午好";
		}
		int k = n2;
		if (k >= 17 && k < 19)
		{
			return "傍晚好";
		}
		int j = n2;
		if ((j <= 23 && j >= 19) || j > 23)
		{
			return "晚上好";
		}
		int i = n2;
		if (i >= 0 && i < 3)
		{
			return "午夜好";
		}
		return "未知时间段";
	}

	public static long GetCurrentTimeStamp()
	{
		return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
	}
}
