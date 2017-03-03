using PoeHUD.Hud.Settings;
using PoeHUD.Plugins;
using SharpDX;

namespace MapAndXPLogger
{
    public class MapAndXPLoggerSettings : SettingsBase
    {
        public MapAndXPLoggerSettings()
        {
            enableTracking = false;
            enableLogging = false;
            xpLoggertextHeight = 16;
            xpLoggerTextColor = new ColorBGRA(100, 240, 80, 255);
            xpLoggerTextColor2 = new ColorBGRA(240, 75, 200, 255);
            xpLoggerTextColor3 = new ColorBGRA(72, 255, 255, 255);
            minZoneLevel = new RangeNode<int>(70, 60, 90);
            minZoneXpGain = new RangeNode<int>(75000, 1, 250000);
            guiXPos = new RangeNode<int>(1600, 200, 2400);
        }

        [Menu("Enable Tracking", 1)]
        public ToggleNode enableTracking { get; set; }
        [Menu("GUI X Position", 2,1)]
        public RangeNode<int> guiXPos { get; set; }
        [Menu("Enable Logging", 3)]
        public ToggleNode enableLogging { get; set; }
        [Menu("Minimum zone level to track", 4)]
        public RangeNode<int> minZoneLevel { get; set; }
        [Menu("Minimum xp gain to track", 5)]
        public RangeNode<int> minZoneXpGain { get; set; }

        public ColorNode xpLoggerTextColor { get; set; }
        public ColorNode xpLoggerTextColor2 { get; set; }
        public ColorNode xpLoggerTextColor3 { get; set; }
        public int xpLoggertextHeight { get; set; }
    }
}
