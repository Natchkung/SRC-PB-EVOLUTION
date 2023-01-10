using PointBlank.Battle.Data.Models;
using PointBlank.Battle.Data.Configs;
using PointBlank.Battle.Data.Enums;
using PointBlank.Battle.Data.Xml;
using PointBlank.Battle.Network;
using PointBlank.Battle.Network.Packets;
using System;
using System.Collections.Generic;
using PointBlank.Battle.Network.Actions.Damage;
using PointBlank.Battle.Data.Models.Event;
using PointBlank.Battle.Network.Actions.Event;

namespace PointBlank.Battle.Data.Sync.Client
{
    public static class RespawnSync
    {
        public static List<AssistModel> Assists = new List<AssistModel>();
        public static void Load(ReceivePacket p)
        {
            uint UniqueRoomId = p.readUD();
            uint Seed = p.readUD();
            long roomTick = p.readQ();
            int syncType = p.readC();
            int round = p.readC();
            int slotId = p.readC();
            int spawnNumber = p.readC();
            byte accountId = p.readC();
            int primary = p.readD();
            int secondary = p.readD();
            int melee = p.readD();
            int grenade = p.readD();
            int special = p.readD();
            int Type = 0, Number = 0, HpBonus = 0;
            bool C4Speed = false;
            if (syncType == 0 || syncType == 2)
            {
                Type = p.readC();
                Number = p.readH();
                HpBonus = p.readC();
                C4Speed = p.readC() == 1;
                if (48 < p.getBuffer().Length)
                {
                    Logger.warning("[RespawnSync]: " + BitConverter.ToString(p.getBuffer()));
                }
            }
            else
            {
                if (43 < p.getBuffer().Length)
                {
                    Logger.warning("[RespawnSync]: " + BitConverter.ToString(p.getBuffer()));
                }
            }
            Room room = RoomsManager.getRoom(UniqueRoomId);
            if (room == null)
            {
                return;
            }

            lock (Assists)
            {
                AssistModel Assist = Assists.Find(x => x.RoomId == room.RoomId);
                Assists.Remove(Assist);
                foreach (AssistModel AssistFix in Assists.FindAll(x => x.RoomId == room.RoomId))
                {
                    Assists.Remove(AssistFix);
                }
            }

            room.ResyncTick(roomTick, Seed);
            Player Player = room.getPlayer(slotId, true);
            if (Player != null && Player.PlayerIdByUser != accountId)
            {
                Player.PlayerIdByUser = accountId; //
            }
            if (Player != null && Player.PlayerIdByUser == accountId)
            {
                Player.Equip = new PlayerEquipedItems
                {
                    _primary = primary,
                    _secondary = secondary,
                    _melee = melee,
                    _grenade = grenade,
                    _special = special
                };
                Player.AceCheck = 0;
                Player.PlayerIdByServer = accountId;
                Player.RespawnByServer = spawnNumber;
                Player.Integrity = false;
                if (round > room.ServerRound)
                {
                    room.ServerRound = round;
                }
                if (syncType == 0 || syncType == 2)
                {
                    Player.RespawnByLogic++;
                    Player.Dead = false;
                    Player.PlantDuration = BattleConfig.plantDuration;
                    Player.DefuseDuration = BattleConfig.defuseDuration;
                    if (C4Speed)
                    {
                        Player.PlantDuration -= AllUtils.Percentage(BattleConfig.plantDuration, 50);
                        Player.DefuseDuration -= AllUtils.Percentage(BattleConfig.defuseDuration, 25);
                    }
                    if (!room.BotMode)
                    {
                        if (room.SourceToMap == -1)
                        {
                            room.RoundResetRoomF1(round);
                        }
                        else
                        {
                            room.RoundResetRoomS1(round);
                        }
                    }
                    if (Type == 255)
                    {
                        Player.Immortal = true;
                    }
                    else
                    {
                        Player.Immortal = false;
                        int CharaHp = CharaXml.getLifeById(Number, Type);
                        CharaHp += AllUtils.Percentage(CharaHp, HpBonus);
                        Player.MaxLife = CharaHp;
                        Player.ResetLife();
                    }
                }
                if (room.BotMode || syncType == 2 || !room.ObjectsIsValid())
                {
                    return;
                }
                List<ObjectHitInfo> SyncList = new List<ObjectHitInfo>();
                for (int i = 0; i < room.Objects.Length; i++)
                {
                    ObjectInfo rObj = room.Objects[i];
                    ObjectModel mObj = rObj.Model;
                    if (mObj != null && (syncType != 2 && mObj.Destroyable && rObj.Life != mObj.Life || mObj.NeedSync))
                    {
                        SyncList.Add(new ObjectHitInfo(3)
                        {
                            ObjSyncId = mObj.NeedSync ? 1 : 0,
                            AnimId1 = mObj.Animation,
                            AnimId2 = rObj.Animation != null ? rObj.Animation.Id : 255,
                            DestroyState = rObj.DestroyState,
                            ObjId = mObj.Id,
                            ObjLife = rObj.Life,
                            SpecialUse = AllUtils.GetDuration(rObj.UseDate),
                        });
                    }
                }
                for (int i = 0; i < room.Players.Length; i++)
                {
                    Player pR = room.Players[i];
                    if (pR.Slot != slotId && pR.AccountIdIsValid() && !pR.Immortal && pR.Date != new DateTime() && (pR.MaxLife != pR.Life || pR.Dead))
                    {
                        SyncList.Add(new ObjectHitInfo(4)
                        {
                            ObjId = pR.Slot,
                            ObjLife = pR.Life
                        });
                    }
                }
                if (SyncList.Count > 0)
                {
                    byte[] Packet = PROTOCOL_EVENTS_ACTION.getCode(PROTOCOL_EVENTS_ACTION.getCodeSyncData(SyncList), room.StartTime, round, 255);
                    BattleManager.Send(Packet, Player.Client);
                }
                SyncList = null;
            }
        }
    }
}