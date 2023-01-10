using Npgsql;
using PointBlank.Core;
using PointBlank.Core.Managers;
using PointBlank.Core.Models.Account.Clan;
using PointBlank.Core.Models.Account.Players;
using PointBlank.Core.Network;
using PointBlank.Core.Sql;
using PointBlank.Game.Data.Managers;
using PointBlank.Game.Data.Model;
using PointBlank.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Data;
//using SqlConnection = PointBlank.Core.Sql.SqlConnection;

namespace PointBlank.Game.Network.ClientPacket
{
    public class PROTOCOL_LOBBY_GET_ROOMLIST_REQ : ReceivePacket
    {
        public PROTOCOL_LOBBY_GET_ROOMLIST_REQ(GameClient client, byte[] data)
        {
            makeme(client, data);
        }

        public override void read()
        {

        }

        public override void run()
        {
            try
            {
                Account p = _client._player;
                if (p == null)
                {
                    return;
                }
                Channel channel = p.getChannel();
                if (channel != null)
                {
                    channel.RemoveEmptyRooms();
                    List<Room> Rooms = channel._rooms;
                    List<Account> Waiting = channel.getWaitPlayers();
                    int RoomPages = (int)Math.Ceiling(Rooms.Count / 15d), PlayerPages = (int)Math.Ceiling(Waiting.Count / 10d);
                    if (p.LastRoomPage >= RoomPages)
                    {
                        p.LastRoomPage = 0;
                    }
                    if (p.LastPlayerPage >= PlayerPages)
                    {
                        p.LastPlayerPage = 0;
                    }
                    int RoomsCount = 0, PlayersCount = 0;
                    byte[] RoomsArray = GetRoomListData(p.LastRoomPage, ref RoomsCount, Rooms);
                    byte[] WaitingArray = GetPlayerListData(p.LastPlayerPage, ref PlayersCount, Waiting);
                    _client.SendPacket(new PROTOCOL_LOBBY_GET_ROOMLIST_ACK(Rooms.Count, Waiting.Count, p.LastRoomPage++, p.LastPlayerPage++, RoomsCount, PlayersCount, RoomsArray, WaitingArray));

                    if(p.Information == false)
                    {
                        p.Information = true;
                        using (NpgsqlConnection connection = SqlConnection.getInstance().conn())
                        {
                            NpgsqlCommand command = connection.CreateCommand();
                            connection.Open();
                            command.CommandText = "SELECT * FROM server_messages ORDER BY id ASC;";
                            command.CommandType = CommandType.Text;
                            NpgsqlDataReader data = command.ExecuteReader();
                            while (data.Read())
                            {
                                _client.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK(data.GetString(1), 0, data.GetInt32(3), false, data.GetString(2)));
                            }
                            command.Dispose();
                            data.Close();
                            connection.Dispose();
                            connection.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.warning("PROTOCOL_LOBBY_GET_ROOMLIST_REQ: " + ex.ToString());
            }
        }

        private byte[] GetRoomListData(int page, ref int count, List<Room> list)
        {
            int startid = 0;
            if (page == 0)
                startid = 15;
            else
                startid = 16;
            using (SendGPacket p = new SendGPacket())
            {
                for (int i = (page * startid); i < list.Count; i++)
                {
                    WriteRoomData(list[i], p);
                    if (++count == 15)
                    {
                        break;
                    }
                }
                return p.mstream.ToArray();
            }
        }

        private void WriteRoomData(Room room, SendGPacket p)
        {
            p.writeD(room._roomId);
            p.writeUnicode(room.name, 46);
            p.writeC((byte)room.mapId);
            p.writeC((byte)room.rule);
            p.writeC(room.stage);
            p.writeC((byte)room.RoomType);
            p.writeC((byte)room.RoomState);
            p.writeC((byte)room.getAllPlayers().Count);
            p.writeC((byte)room.getSlotCount());
            p.writeC(5); //(byte)room._ping
            p.writeH((ushort)room.weaponsFlag);
            p.writeD(room.getFlag());
            p.writeH(0);
        }

        private void WritePlayerData(Account pl, SendGPacket p)
        {
            Clan clan = ClanManager.getClan(pl.clanId);
            p.writeD(pl.getSessionId());
            p.writeD(clan._logo);
            p.writeC((byte)clan.effect);
            p.writeUnicode(clan._name, 34);
            p.writeH((short)pl.getRank());
            p.writeUnicode(pl.player_name, 66);
            p.writeC((byte)pl.name_color); // 0
            p.writeC(210);
        }

        private byte[] GetPlayerListData(int page, ref int count, List<Account> list)
        {
            int startid = 0;
            if (page == 0)
                startid = 10;
            else
                startid = 11;
            using (SendGPacket p = new SendGPacket())
            {
                for (int i = (page * startid); i < list.Count; i++)
                {
                    WritePlayerData(list[i], p);
                    if (++count == 10)
                    {
                        break;
                    }
                }
                return p.mstream.ToArray();
            }
        }
    }
}