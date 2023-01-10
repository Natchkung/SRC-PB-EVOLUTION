using Npgsql;
using PointBlank.Core;
using PointBlank.Core.Models.Account.Players;
using PointBlank.Core.Network;
using PointBlank.Core.Sql;
using PointBlank.Core.Xml;
using PointBlank.Game.Data.Configs;
using PointBlank.Game.Data.Managers;
using PointBlank.Game.Data.Model;
using System;
using System.Data;
using System.Threading.Tasks;

namespace PointBlank.Game
{
    public static class Game
    {

        public static int Count = 0;
        public static async void Update()
        {
            while (true)
            {
                Console.Title = "Point Blank - Game [Users: " + GameManager._socketList.Count + " Online: " + ServersXml.getServer(GameConfig.serverId)._LastCount + " Used RAM: " + (GC.GetTotalMemory(true) / 1024) + " KB]";

                if (Count == 5)
                {
                    ComDiv.updateDB("onlines", "game", ServersXml.getServer(GameConfig.serverId)._LastCount);
                    int auth = 0;
                    int game = 0;
                    int online = 0;
                    int oldonlines = 0;
                    using (NpgsqlConnection connection = SqlConnection.getInstance().conn())
                    {
                        NpgsqlCommand command = connection.CreateCommand();
                        connection.Open();
                        command.CommandText = "SELECT * FROM onlines";
                        command.CommandType = CommandType.Text;
                        NpgsqlDataReader data = command.ExecuteReader();
                        while (data.Read())
                        {
                            auth = data.GetInt32(0);
                            game = data.GetInt32(1);
                            oldonlines = data.GetInt32(2);
                        }
                        command.Dispose();
                        data.Close();
                        connection.Dispose();
                        connection.Close();
                    }
                    online = auth + game;
                    if (online > oldonlines)
                    {
                        ComDiv.updateDB("onlines", "maximum", online);
                    }

                    Count = 0;
                }

                Count++;

                if (DateTime.Now.ToString("HH:mm") == "00:00")
                {
                    foreach (Account Player in AccountManager._accounts.Values)
                    {
                        if (Player != null)
                        {
                            Player.Daily = new PlayerDailyRecord();
                        }
                    }
                    foreach (GameClient Client in GameManager._socketList.Values)
                    {
                        if (Client != null && Client._player != null && Client._player._isOnline)
                        {
                            Client._player.Daily = new PlayerDailyRecord();
                        }
                    }
                    ComDiv.updateDB("player_dailyrecord", "total", 0);
                    ComDiv.updateDB("player_dailyrecord", "wins", 0);
                    ComDiv.updateDB("player_dailyrecord", "loses", 0);
                    ComDiv.updateDB("player_dailyrecord", "draws", 0);
                    ComDiv.updateDB("player_dailyrecord", "kills", 0);
                    ComDiv.updateDB("player_dailyrecord", "deaths", 0);
                    ComDiv.updateDB("player_dailyrecord", "headshots", 0);
                    ComDiv.updateDB("player_dailyrecord", "point", 0);
                    ComDiv.updateDB("player_dailyrecord", "exp", 0);
                    ComDiv.updateDB("info_channels", "online", 0);
                }
                await Task.Delay(1000);
            }
        }
    }
}