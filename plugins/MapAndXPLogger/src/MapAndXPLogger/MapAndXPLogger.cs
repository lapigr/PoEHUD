using PoeHUD.Plugins;
using PoeHUD.Controllers;
using PoeHUD.Poe.Components;
using SharpDX;
using PoeHUD.Framework.Helpers;
using SharpDX.Direct3D9;
using System.IO;
using PoeHUD.Models;
using PoeHUD.Hud.Menu;
using System;

namespace MapAndXPLogger
{
    public class MapAndXPLogger : BaseSettingsPlugin<MapAndXPLoggerSettings>
    {
        private long sessionXpStart;
        private long sessionXpGain;
        private long mapXpGain;
        private long mapCount;
        private string lastKnownHash;
        private string lastKnownAreaName;
        private System.Collections.Generic.List<string> xpLog;

        public MapAndXPLogger()
        {
            //PluginName = "MapAndXPLogger";
            //Initialise();
        }

        public override void Initialise()
        {
            PluginName = "MapAndXPLogger";
            sessionXpStart = GameController.Player.GetComponent<Player>().XP;
            lastKnownHash = "";
            lastKnownAreaName = "";
            mapCount = 0;
            sessionXpGain = 0;
            mapXpGain = 0;
            xpLog = new System.Collections.Generic.List<string>();
            GameController.Area.OnAreaChange += area => OnAreaChange(area);
        }

        public override void OnClose()
        {
            base.OnClose();
            if (Settings.enableLogging)
            {
                if(xpLog.Count>0 && !xpLog[xpLog.Count - 1].Contains(mapXpGain.ToString()))
                {
                    xpLog.Add(lastKnownAreaName + ",(??)," + mapXpGain + "," + Convert.ToInt64(mapXpGain / LevelXpPenalty()));
                }
                foreach (string entry in xpLog)
                {
                    File.AppendAllText("plugins/xpLog.log", entry + "\n");
                }
            }
        }

        public override void Render()
        {
            try
            {
                base.Render();
                if (Settings.enableTracking)
                {
                    Vector2 position = new Vector2(Settings.guiXPos, 15);

                    float boxWidth = 164;
                    float boxHeight = Settings.xpLoggertextHeight * 2;
                    var bounds = new RectangleF(position.X - boxWidth - 104, position.Y - 7, boxWidth + 110, boxHeight + 18);
                    Graphics.DrawImage("preload-end.png", bounds, new ColorBGRA(0, 0, 0, 255));
                    Graphics.DrawImage("preload-start.png", bounds, new ColorBGRA(0, 0, 0, 255));

                    string debug1 =  "Count: ("+mapCount+")" +"    Previous Map" ;
                    string sessionXpText = $"Session: {ConvertHelper.ToShorten(sessionXpGain, "0.00")}";
                    string prevMapXp= $"{ConvertHelper.ToShorten(mapXpGain, "0.00")}";
                    Size2 areaNameSize = Graphics.DrawText(debug1, Settings.xpLoggertextHeight, position - 1, Settings.xpLoggerTextColor, FontDrawFlags.Right);
                    Vector2 secondLine = position.Translate(-10-Graphics.MeasureText(sessionXpText, Settings.xpLoggertextHeight).Width, areaNameSize.Height + 2);
                    Size2 sessionXpSize = Graphics.DrawText(sessionXpText, Settings.xpLoggertextHeight, secondLine, Settings.xpLoggerTextColor3, FontDrawFlags.Right);
                    Vector2 secondLine2 = position.Translate(-1, areaNameSize.Height + 2);
                    Graphics.DrawText(prevMapXp, Settings.xpLoggertextHeight, secondLine2, Settings.xpLoggerTextColor2, FontDrawFlags.Right);
                }
            }
            catch
            {

            }
        }

        private void OnAreaChange(AreaController area)
        {
            if(lastKnownAreaName.Equals("") || lastKnownHash.Equals(""))
            {
                lastKnownHash = $"{GameController.Area.CurrentArea.Hash}";
                lastKnownAreaName = $"{GameController.Area.CurrentArea.Name}";
            }
            int areaLevel = GameController.Area.CurrentArea.RealLevel;
            if (!GameController.Area.CurrentArea.IsHideout && !GameController.Area.CurrentArea.IsTown && !$"{GameController.Area.CurrentArea.Name}".Contains("Crumbling Lab") 
            && areaLevel > Settings.minZoneLevel)
            {
                int currentMapHash = GameController.Area.CurrentArea.Hash;

                if (!lastKnownHash.Equals(currentMapHash.ToString()))
                {
                    long playerCurrentXp = GameController.Player.GetComponent<Player>().XP;
                    if (playerCurrentXp < 0)
                    {
                        System.Threading.Thread.Sleep(2000);
                        playerCurrentXp = GameController.Player.GetComponent<Player>().XP; 
                    }
                    mapXpGain = playerCurrentXp - (sessionXpStart + sessionXpGain);
                    if (mapXpGain < Settings.minZoneXpGain)
                        return;
                    sessionXpGain += mapXpGain;
                    mapCount++;
                    lastKnownHash = $"{GameController.Area.CurrentArea.Hash}";
                    xpLog.Add(lastKnownAreaName+","+areaLevel+","+ mapXpGain + "," +Convert.ToInt64(mapXpGain / LevelXpPenalty()));
                    lastKnownAreaName = $"{GameController.Area.CurrentArea.Name}";
                }
            }
        }

        //stolen valor
        private double LevelXpPenalty()
        {
            int arenaLevel = GameController.Area.CurrentArea.RealLevel;
            int characterLevel = GameController.Player.GetComponent<Player>().Level;
            double safeZone = Math.Floor(Convert.ToDouble(characterLevel) / 16) + 3;
            double effectiveDifference = Math.Max(Math.Abs(characterLevel - arenaLevel) - safeZone, 0);
            double xpMultiplier = Math.Max(Math.Pow((characterLevel + 5) / (characterLevel + 5 + Math.Pow(effectiveDifference, 2.5)), 1.5), 0.01);
            return xpMultiplier;
        }
    }
}
