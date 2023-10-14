using System;

namespace MinecraftLaunch.Modules.Utilities;

public class TimeUtil {
    public static int LocalYear => DateTime.Now.Year;

    public static int WorldYear => DateTime.UtcNow.Year;

    public const string TimeFormat = @"yyyy\-MM\-dd hh\:mm";

    public static bool IsLeapYear => LocalYear % 4 == 0 && (LocalYear % 100 != 0 || LocalYear % 400 == 0);

    public static bool IsWorldLeapYear => WorldYear % 4 == 0 && (WorldYear % 100 != 0 || WorldYear % 400 == 0);

    public static string GetCurrentTimeSlot() {
        int currentHour = DateTime.Now.Hour;
        if (currentHour >= 3 && currentHour < 5) {
            return "凌晨好";
        }

        if (currentHour >= 5 && currentHour < 11) {
            return "清晨好";
        }

        if (currentHour >= 11 && currentHour < 13) {
            return "中午好";
        }

        if (currentHour >= 13 && currentHour < 17) {
            return "下午好";
        }

        if (currentHour >= 17 && currentHour < 19) {
            return "傍晚好";
        }

        if (currentHour >= 19 || currentHour < 3) {
            return "晚上好";
        }

        return "未知时间段";
    }

    public static long GetCurrentTimeStamp() {
        return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
    }
}
