﻿using PointBlank.Battle.Data.Configs;
using PointBlank.Battle.Data.Sync;
using PointBlank.Battle.Data.Xml;
using PointBlank.Battle.Network;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace PointBlank.Battle
{
    internal class Program
    {
        protected static void Main(string[] args)
        {
            string Date = GetLinkerTime(Assembly.GetExecutingAssembly(), null).ToString("dd/MM/yyyy HH:mm");
            BattleConfig.Load();
            Logger.checkDirectory();
            Console.Clear();
            Console.Title = "Point Blank - Battle";
            Logger.title("________________________________________________________________________________");
            Logger.title("                                                                               ");
            Logger.title("                                                                               ");
            Logger.title("                               POINT BLANK BATTLE                              ");
            Logger.title("                                                                               ");
            Logger.title("                                                                               ");
            Logger.title("_______________________________ " + Date + " _______________________________");
            Logger.info("Server active at " + BattleConfig.hosIp + ":" + BattleConfig.hosPort);
            Logger.info("Synchronize infos to server: " + BattleConfig.sendInfoToServ);
            Logger.info("Drops Limit: " + BattleConfig.maxDrop);
            Logger.info("Ammo Limit: " + BattleConfig.useMaxAmmoInDrop);
            Logger.info("Duration C4: (" + BattleConfig.plantDuration + "s/" + BattleConfig.defuseDuration + "s)");
            MapXml.Load();
            CharaXml.Load();
            MeleeExceptionsXml.Load();
            ServersXml.Load();
            BattleSync.Start();
            BattleManager.Connect();
            while (true)
            {
                string Read = Console.ReadLine();
                if (Read.StartsWith("reload_object"))
                {
                    MapXml.Reset();
                    MapXml.Load();
                    Logger.debug("Reload Object Success.");
                }
            }
        }

        public static DateTime GetLinkerTime(Assembly assembly, TimeZoneInfo target = null)
        {
            var filePath = assembly.Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;

            var buffer = new byte[2048];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                stream.Read(buffer, 0, 2048);
            }

            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(epoch.AddSeconds(secondsSince1970), target ?? TimeZoneInfo.Local);
        }
    }
}