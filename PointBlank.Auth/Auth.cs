using Microsoft.VisualBasic.Devices;
using Npgsql;
using PointBlank.Auth.Data.Managers;
using PointBlank.Core.Network;
using PointBlank.Core.Sql;
using PointBlank.Core.Xml;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace PointBlank.Auth
{
    public class Auth
    {
        public static int Count = 0;
        public static async void Update()
        {
            while (true)
            {
                Console.Title = "Point Blank - Auth [Users: " + AuthManager._socketList.Count + " Online: " + ServersXml.getServer(0)._LastCount + " Used RAM: " + (GC.GetTotalMemory(true) / 1024) + " KB]";

                if (Count == 5)
                {
                    ComDiv.updateDB("onlines", "auth", ServersXml.getServer(0)._LastCount);
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
                    ComDiv.updateDB("accounts", "face", 1000700000, "face", 0);
                    ComDiv.updateDB("accounts", "jacket", 1000900000, "jacket", 0);
                    ComDiv.updateDB("accounts", "poket", 1001000000, "poket", 0);
                    ComDiv.updateDB("accounts", "glove", 1001100000, "glove", 0);
                    ComDiv.updateDB("accounts", "belt", 1001200000, "belt", 0);
                    ComDiv.updateDB("accounts", "holster", 1001300000, "holster", 0);
                    ComDiv.updateDB ("accounts", "skin", 1001400000, "skin", 0);
                    Count = 0;
                }

                Count++;
                await Task.Delay(1000);
            }
        }
    }
}