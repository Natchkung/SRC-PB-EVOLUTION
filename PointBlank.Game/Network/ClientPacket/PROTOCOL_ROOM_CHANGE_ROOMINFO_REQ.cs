using PointBlank.Core;
using PointBlank.Core.Models.Enums;
using PointBlank.Core.Models.Room;
using PointBlank.Game.Data.Model;
using PointBlank.Game.Network.ServerPacket;
using System;

namespace PointBlank.Game.Network.ClientPacket
{
    public class PROTOCOL_ROOM_CHANGE_ROOMINFO_REQ : ReceivePacket
    {
        public PROTOCOL_ROOM_CHANGE_ROOMINFO_REQ(GameClient client, byte[] data)
        {
            makeme(client, data);
        }

        public override void read()
        {
            try
            {
                int count = 0;
                Account player = _client._player;
                Room room = player == null ? null : player._room;
                if (room == null || room.RoomState != RoomState.Ready || room._leader != player._slotId)
                {
                    return;
                }
                readD();
                room.name = readUnicode(46);
                room.mapId = (MapIdEnum)readC();
                room.rule = readC();
                room.stage = readC();
                room.RoomType = (RoomType)readC();
                readC();
                readC();
                room.initSlotCount(readC(), true);
                room._ping = readC();
                RoomWeaponsFlag weaponsFlag = (RoomWeaponsFlag)readH();
                room.Flag = (RoomStageFlag)readD();
                readC();
                readC();
                readC();
                readB(66);
                room.killtime = readC();
                readC();
                readC();
                readC();
                room.Limit = readC();
                room.WatchRuleFlag = readC();
                if (room.RoomType == RoomType.Ace)
                    room.WatchRuleFlag = 142;
                room.BalanceType = readH();
                if (room.RoomType == RoomType.Ace)
                    room.BalanceType = 0;
                readB(16);
                readB(4);
                room.aiCount = readC();
                room.aiLevel = readC();
                if (weaponsFlag != room.weaponsFlag)
                {
                    if (room.SniperMode)
                    {
                        room.weaponsFlag = weaponsFlag + 32;
                    }
                    if (room.ShotgunMode)
                    {
                        room.weaponsFlag = weaponsFlag + 64;
                    }
                    if (!room.ShotgunMode && !room.SniperMode)
                    {
                        room.weaponsFlag = weaponsFlag;
                    }
                }
                for (int i = 0; i < 16; i++)
                {
                    Slot slot = room._slots[i];
                    if (slot.state == SlotState.READY)
                    {
                        slot.state = SlotState.NORMAL;
                        count++;    
                    }
                }
                room.StopCountDown(CountDownEnum.StopByHost);
                if (count > 0)
                    room.updateSlotsInfo();
                room.SetSeed();
                room.updateRoomInfo();
            }
            catch (Exception ex)
            {
                Logger.info("PROTOCOL_BATTLE_CHANGE_ROOMINFO_REQ: " + ex.ToString());
            }
        }

        public override void run()
        {

        }
    }
}