using PointBlank.Core;
using PointBlank.Core.Managers;
using PointBlank.Core.Managers.Events;
using PointBlank.Core.Managers.Server;
using PointBlank.Core.Network;
using PointBlank.Core.Xml;
using PointBlank.Game.Data.Xml;
using PointBlank.Game.Data.Managers;
using PointBlank.Game.Data.Sync;
using System;
using System.Diagnostics;
using System.Reflection;
using PointBlank.Core.Filters;
using PointBlank.Game.Data.Configs;
using System.Text;
using PointBlank.Game.Network.ServerPacket;
using PointBlank.Game.Data.Chat;
using PointBlank.Game.Data.Model;
using PointBlank.Core.Models.Enums;
using PointBlank.Core.Models.Room;
using PointBlank.Core.Models.Account.Players;
using System.Windows.Forms;
using System.Net;
using PointBlank.Game.Data.Utils;

namespace PointBlank.Game
{
    public class Programm
    {
        public static void Main(string[] args)
        {
            string Date = ComDiv.GetLinkerTime(Assembly.GetExecutingAssembly(), null).ToString("dd/MM/yyyy HH:mm");
            Console.Title = "Point Blank - Game";
            Logger.StartedFor = "Game";
            Logger.checkDirectorys();
            Console.Clear();
            Logger.title("________________________________________________________________________________");
            Logger.title("                                                                               ");
            Logger.title("                                                                               ");
            Logger.title("                                POINT BLANK GAME                               ");
            Logger.title("                                                                               ");
            Logger.title("                                                                               ");
            Logger.title("_______________________________ " + Date + " _______________________________");
            GameConfig.Load();
            BasicInventoryXml.Load();
            CafeInventoryXml.Load();
            ServerConfigSyncer.GenerateConfig(GameConfig.configId);
            ServersXml.Load();
            ChannelsXml.Load(GameConfig.serverId);
            EventLoader.LoadAll();
            TitlesXml.Load();
            TitleAwardsXml.Load();
            ClanManager.Load();
            NickFilter.Load();
            MissionCardXml.LoadBasicCards(1);
            RankXml.Load();
            BattleServerXml.Load();
            RankXml.LoadAwards();
            ClanRankXml.Load();
            MissionAwardsXml.Load();
            MissionsXml.Load();
            Translation.Load();
            ShopManager.Load(1);
            MapsXml.Load();
            RandomBoxXml.LoadBoxes();
            TicketManager.GetTickets();
            CouponEffectManager.LoadCouponFlags();
            ICafeManager.GetList();
            GameRuleManager.getGameRules(GameConfig.ruleId);
            AllUtils.UpdateUserOffline();
            GameSync.Start();
            bool Started = GameManager.Start();
            Logger.info("Text Encode: " + Config.EncodeText.EncodingName);
            Logger.info("Mode: " + (GameConfig.isTestMode ? "Test" : "Public"));
            Logger.debug(StartSuccess());
            ComDiv.updateDB("info_channels", "online", 0);
            if (Started)
            {
                Game.Update();
            }

            while (true)
            {
                string Read = Console.ReadLine();
                if (Read.StartsWith("msg "))
                {
                    string Result = "";
                    try
                    {
                        string msg = Read.Substring(4);
                        int count = 0;
                        using (PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK packet = new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(msg))
                        {
                            count = GameManager.SendPacketToAllClients(packet);
                            Result = "Send message to: " + count + " players.";
                        }
                    }
                    catch
                    {
                        Result = "Command Error.";
                    }
                    Logger.console(Result);
                    Logger.LogConsole(Read, Result);
                }
                else if (Read.StartsWith("ban "))
                {
                    string Result = "";
                    try
                    {
                        Account Account = AccountManager.getAccount(long.Parse(Read.Substring(4)), 0);
                        if (Account == null)
                        {
                            Result = "Invalid Player.";
                        }
                        else if (Account.access != AccessLevel.Banned)
                        {
                            if (ComDiv.updateDB("accounts", "access_level", -1, "player_id", Account.player_id))
                            {
                                BanManager.SaveAutoBan(Account.player_id, Account.login, Account.player_name, "ใช้โปรแกรมช่วยเล่น", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), Account.PublicIP.ToString(), "Ban from Console");
                                using (PROTOCOL_LOBBY_CHATTING_ACK packet = new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 0, true, "แบนผู้เล่น [" + Account.player_name + "] ถาวร - ใช้โปรแกรมช่วยเล่น"))
                                {
                                    GameManager.SendPacketToAllClients(packet);
                                }
                                Account.access = AccessLevel.Banned;
                                Account.SendPacket(new PROTOCOL_AUTH_ACCOUNT_KICK_ACK(2), false);
                                Account.Close(1000, true);
                                Result = "Ban Success.";
                            }
                            else
                            {
                                Result = "Ban Player Failed.";
                            }
                        }
                        else
                        {
                            Result = "Players are already banned.";
                        }
                    }
                    catch
                    {
                        Result = "Command Error.";
                    }
                    Logger.console(Result);
                    Logger.LogConsole(Read, Result);
                }
                else if (Read.StartsWith("unban "))
                {
                    string Result = "";
                    try
                    {
                        Account victim = AccountManager.getAccount(long.Parse(Read.Substring(6)), 0);
                        if (victim == null)
                        {
                            Result = "Invalid Player.";
                        }
                        else if (victim.access == AccessLevel.Banned || victim.ban_obj_id > 0)
                        {
                            if (ComDiv.updateDB("accounts", "access_level", 0, "player_id", victim.player_id))
                            {
                                if (ComDiv.updateDB("accounts", "ban_obj_id", 0, "player_id", victim.player_id))
                                {
                                    ComDiv.deleteDB("auto_ban", "player_id", victim.player_id);
                                }
                                victim.access = AccessLevel.Normal;
                                Result = "Unban Success.";
                            }
                            else
                            {
                                Result = "Unban Player Failed.";
                            }
                        }
                        else
                        {
                            Result = "Player not being banned.";
                        }
                    }
                    catch
                    {
                        Result = "Command Error.";
                    }
                    Logger.console(Result);
                    Logger.LogConsole(Read, Result);
                }
                else if (Read.StartsWith("kickall"))
                {
                    string Result = "";
                    try
                    {
                        int Total = 0;
                        using (PROTOCOL_AUTH_ACCOUNT_KICK_ACK packet = new PROTOCOL_AUTH_ACCOUNT_KICK_ACK(0))
                        {
                            if (GameManager._socketList.Count > 0)
                            {
                                byte[] data = packet.GetCompleteBytes("Console.KickAll");
                                foreach (GameClient client in GameManager._socketList.Values)
                                {
                                    Account p = client._player;
                                    if (p != null && p._isOnline && (int)p.access <= 2)
                                    {
                                        p.SendCompletePacket(data);
                                        p.Close(1000, true);
                                        Total++;
                                    }
                                }
                            }
                        }
                        Result = "Kick " + Total + " players.";
                    }
                    catch
                    {
                        Result = "Command Error.";
                    }
                    Logger.console(Result);
                    Logger.LogConsole(Read, Result);
                }
                else if (Read.StartsWith("reload_shop"))
                {
                    string Result = "";
                    try
                    {
                        ShopManager.Reset();
                        ShopManager.Load(1);
                        foreach (GameClient client in GameManager._socketList.Values)
                        {
                            Account Player = client._player;
                            if (Player != null && Player._isOnline)
                            {
                                for (int i = 0; i < ShopManager.ShopDataItems.Count; i++)
                                {
                                    ShopData data = ShopManager.ShopDataItems[i];
                                    Player.SendPacket(new PROTOCOL_AUTH_SHOP_ITEMLIST_ACK(data, ShopManager.TotalItems));
                                }
                                for (int i = 0; i < ShopManager.ShopDataGoods.Count; i++)
                                {
                                    ShopData data = ShopManager.ShopDataGoods[i];
                                    Player.SendPacket(new PROTOCOL_AUTH_SHOP_GOODSLIST_ACK(data, ShopManager.TotalGoods));
                                }
                                for (int i = 0; i < ShopManager.ShopDataItemRepairs.Count; i++)
                                {
                                    ShopData data = ShopManager.ShopDataItemRepairs[i];
                                    Player.SendPacket(new PROTOCOL_AUTH_SHOP_REPAIRLIST_ACK(data, ShopManager.TotalRepairs));
                                }
                                //int cafe = Player.pc_cafe;
                                if (Player.pc_cafe == 0)
                                {
                                    for (int i = 0; i < ShopManager.ShopDataMt1.Count; i++)
                                    {
                                        ShopData data = ShopManager.ShopDataMt1[i];
                                        Player.SendPacket(new PROTOCOL_AUTH_SHOP_MATCHINGLIST_ACK(data, ShopManager.TotalMatching1));
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < ShopManager.ShopDataMt2.Count; i++)
                                    {
                                        ShopData data = ShopManager.ShopDataMt2[i];
                                        Player.SendPacket(new PROTOCOL_AUTH_SHOP_MATCHINGLIST_ACK(data, ShopManager.TotalMatching2));
                                    }
                                }
                                Player.SendPacket(new PROTOCOL_SHOP_GET_SAILLIST_ACK(true));
                            }
                        }
                        Result = "Reload Shop Success.";
                    }
                    catch
                    {
                        Result = "Command Error.";
                    }
                    Logger.console(Result);
                    Logger.LogConsole(Read, Result);
                }
                else if (Read.StartsWith("reload_event"))
                {
                    string Result = "";
                    try
                    {
                        EventLoader.ReloadAll();
                        Result = "Reloaded Event Success.";
                    }
                    catch
                    {
                        Result = "Command Error.";
                    }
                    Logger.console(Result);
                    Logger.LogConsole(Read, Result);
                }
                else if (Read.StartsWith("reload_rule"))
                {
                    string Result = "";
                    try
                    {
                        GameRuleManager.Reload();
                        Result = "Reloaded GameRule Success.";
                    }
                    catch
                    {
                        Result = "Command Error.";
                    }
                    Logger.console(Result);
                    Logger.LogConsole(Read, Result);
                }
                else if (Read.StartsWith("reload_map"))
                {
                    string Result = "";
                    try
                    {
                        MapsXml.Load();
                        Result = "Reloaded Maps Success.";
                    }
                    catch
                    {
                        Result = "Command Error.";
                    }
                    Logger.console(Result);
                    Logger.LogConsole(Read, Result);
                }
                else if (Read.StartsWith("reload_code"))
                {
                    string Result = "";
                    try
                    {
                        TicketManager.GetTickets();
                        Result = "Reloaded Item Code Success.";
                    }
                    catch
                    {
                        Result = "Command Error.";
                    }
                    Logger.console(Result);
                    Logger.LogConsole(Read, Result);
                }
            }
        }

        private static string StartSuccess()
        {
            if (Logger.erro)
            {
                return "Start failed.";
            }
            return "Active Server. (" + DateTime.Now.ToString("dd/MM/yy HH:mm:ss") + ")";
        }
    }
}