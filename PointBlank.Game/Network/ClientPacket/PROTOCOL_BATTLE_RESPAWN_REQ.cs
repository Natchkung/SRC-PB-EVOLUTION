﻿using PointBlank.Core;
using PointBlank.Core.Managers;
using PointBlank.Core.Models.Account.Players;
using PointBlank.Core.Models.Enums;
using PointBlank.Core.Models.Room;
using PointBlank.Core.Network;
using PointBlank.Game.Data.Configs;
using PointBlank.Game.Data.Managers;
using PointBlank.Game.Data.Model;
using PointBlank.Game.Data.Sync;
using PointBlank.Game.Network.ServerPacket;
using System;

namespace PointBlank.Game.Network.ClientPacket
{
    public class PROTOCOL_BATTLE_RESPAWN_REQ : ReceivePacket
    {
        private PlayerEquipedItems Equip;
        private int WeaponsFlag;


        public PROTOCOL_BATTLE_RESPAWN_REQ(GameClient client, byte[] data)
        {
            makeme(client, data);
        }

        public override void read()
        {
            Room Room = _client._player._room;
            bool Team = _client._player._slotId % 2 == 0;
            Equip = new PlayerEquipedItems();
            Equip.Primary = readD();
            readD();
            Equip._secondary = readD();
            readD();
            Equip._melee = readD();
            readD();
            Equip._grenade = readD();
            readD();
            Equip._special = readD();
            readD();
            if (Room.RoomType == RoomType.Boss || Room.RoomType == RoomType.CrossCounter)
            {
                if (!Room.swapRound)
                {
                    if (Team)
                    {
                        Equip._red = _client._player._equip._red;
                        Equip._blue = _client._player._equip._blue;
                        Equip._dino = readD();
                        readD();
                    }
                    else
                    {
                        Equip._red = _client._player._equip._red;
                        Equip._blue = readD();
                        Equip._dino = _client._player._equip._dino;
                        readD();
                    }
                }
                else
                {
                    if (Team)
                    {
                        Equip._red = _client._player._equip._red;
                        Equip._blue = readD();
                        Equip._dino = _client._player._equip._dino;
                        readD();
                    }
                    else
                    {
                        Equip._red = _client._player._equip._red;
                        Equip._blue = _client._player._equip._blue;
                        Equip._dino = readD();
                        readD();
                    }
                }
            }
            else
            {
                if (Team)
                {
                    Equip._red = readD();
                    Equip._blue = _client._player._equip._blue;
                    readD();
                }
                else
                {
                    Equip._red = _client._player._equip._red;
                    Equip._blue = readD();
                    readD();
                }
                Equip._dino = _client._player._equip._dino;
            }
            Equip.face = readD();
            readD();
            Equip._helmet = readD();
            readD();
            Equip.jacket = readD();
            readD();
            Equip.poket = readD();
            readD();
            Equip.glove = readD();
            readD();
            Equip.belt = readD();
            readD();
            Equip.holster = readD();
            readD();
            Equip.skin = readD();
            readD();
            Equip._beret = readD();
            readD();
            WeaponsFlag = readH();
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
                Room r = p._room;
                if (r != null && r.RoomState == RoomState.Battle)
                {
                    Slot slot = r.getSlot(p._slotId);
                    if (slot != null && slot.state == SlotState.BATTLE)
                    {
                        if (slot._deathState.HasFlag(DeadEnum.Dead) || slot._deathState.HasFlag(DeadEnum.UseChat))
                        {
                            slot._deathState = DeadEnum.Alive;
                        }
                        PlayerManager.CheckEquipedItems(Equip, p._inventory._items, true);
                        CheckEquipment(p, r, Equip);
                        slot._equip = Equip;
                        if ((WeaponsFlag & 8) > 0)
                        {
                            InsertItem(Equip.Primary, slot);
                        }
                        if ((WeaponsFlag & 4) > 0)
                        {
                            InsertItem(Equip._secondary, slot);
                        }
                        if ((WeaponsFlag & 2) > 0)
                        {
                            InsertItem(Equip._melee, slot);
                        }
                        if ((WeaponsFlag & 1) > 0)
                        {
                            InsertItem(Equip._grenade, slot);
                        }
                        InsertItem(Equip._special, slot);
                        if (slot._team == 0)
                        {
                            InsertItem(Equip._red, slot);
                        }
                        else
                        {
                            InsertItem(Equip._blue, slot);
                        }
                        if (r.MaskActive)
                        {
                            Equip._helmet = 1000800000;
                            InsertItem(Equip._helmet, slot);
                        }
                        else
                        {
                            InsertItem(Equip._helmet, slot);
                        }
                        InsertItem(Equip._beret, slot);
                        InsertItem(Equip._dino, slot);
                        using (PROTOCOL_BATTLE_RESPAWN_ACK packet = new PROTOCOL_BATTLE_RESPAWN_ACK(r, slot))
                        {
                            r.SendPacketToPlayers(packet, SlotState.BATTLE, 0);
                        }
                        if (slot.firstRespawn)
                        {
                            slot.firstRespawn = false;
                            GameSync.SendUDPPlayerSync(r, slot, p.effects, 0);
                        }
                        else
                        {
                            GameSync.SendUDPPlayerSync(r, slot, p.effects, 2);
                        }
                        /*if (r.weaponsFlag != WeaponsFlag)
                        {
                            Logger.warning("WeaponFlag [Room: " + r.weaponsFlag + " Player: " + WeaponsFlag + "]");
                        }*/
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.warning("PROTOCOL_BATTLE_RESPAWN_REQ: " + ex.ToString());
            }
        }

        public void CheckEquipment(Account Player, Room Room, PlayerEquipedItems Equip)
        {
            if (Room.BarrettActive)
            {
                if (Equip.Primary == 105032 || Equip.Primary == 105082 || Equip.Primary == 105232 || Equip.Primary == 105292)
                {
                    if (Room.SniperMode)
                    {
                        Equip.Primary = 105003;
                    }
                    else
                    {
                        Equip.Primary = 103004;
                    }
                }
            }
            if (Room.ShotgunActive)
            {
                if ((Equip.Primary < 107000) && (Equip.Primary > 106000))
                {
                    Equip.Primary = 103004;
                }
            }
            if (Room.GameRuleActive)
            {
                for (int i = 0; i < GameRuleManager.GameRules.Count; i++)
                {
                    GameRule Rule = GameRuleManager.GameRules[i];
                    int ItemClass = ComDiv.getIdStatics(Rule.WeaponId, 1);
                    int ClassType = ComDiv.getIdStatics(Rule.WeaponId, 2);
                    if (ItemClass == 1)
                    {
                        if (Equip.Primary == Rule.WeaponId)
                        {
                            if (Room.SniperMode)
                            {
                                Equip.Primary = 105003;
                            }
                            else
                            {
                                Equip.Primary = 103004;
                            }
                            if (Room.ShotgunMode)
                            {
                                Equip.Primary = 106001;
                            }
                            else
                            {
                                Equip.Primary = 103004;
                            }
                        }
                    }
                    if (ItemClass == 2)
                    {
                        if (Equip._secondary == Rule.WeaponId)
                        {
                            Equip._secondary = 202003;
                        }
                    }
                    if (ItemClass == 3)
                    {
                        if (Equip._melee == Rule.WeaponId)
                        {
                            Equip._melee = 301001;
                        }
                    }
                    if (ItemClass == 4)
                    {
                        if (Equip._grenade == Rule.WeaponId)
                        {
                            Equip._grenade = 407001;
                        }
                    }
                    if (ItemClass == 5)
                    {
                        if (Equip._special == Rule.WeaponId)
                        {
                            Equip._special = 508001;
                        }
                    }
                    if (ItemClass == 6)
                    {
                        if (ClassType == 1)
                        {
                            if (Equip._red == Rule.WeaponId)
                            {
                                Equip._red = 601001;
                            }
                        }
                        else if (ClassType == 2)
                        {
                            if (Equip._blue == Rule.WeaponId)
                            {
                                Equip._blue = 602002;
                            }
                        }
                    }
                    if (ItemClass == 27)
                    {
                        if (Equip._beret == Rule.WeaponId)
                        {
                            Equip._beret = 0;
                        }
                    }
                    if (ItemClass == 8)
                    {
                        if (Equip._helmet == Rule.WeaponId)
                        {
                            Equip._helmet = 1000800000;
                        }
                    }
                }
            }
            if (Room.GameRuleWCActive)
            {
                for (int i = 0; i < GameRuleManager.GameRules.Count; i++)
                {
                    GameRule Rule = GameRuleManager.GameRules[i];
                    int ItemClass = ComDiv.getIdStatics(Rule.WeaponId, 1);
                    int ClassType = ComDiv.getIdStatics(Rule.WeaponId, 2);
                    if (ItemClass == 1)
                    {
                        if (Equip.Primary == Rule.WeaponId)
                        {
                            if (Room.SniperMode)
                            {
                                Equip.Primary = 105003;
                            }
                            else
                            {
                                Equip.Primary = 103004;
                            }
                            if (Room.ShotgunMode)
                            {
                                Equip.Primary = 106001;
                            }
                            else
                            {
                                Equip.Primary = 103004;
                            }
                        }
                    }
                    if (ItemClass == 2)
                    {
                        if (Equip._secondary == Rule.WeaponId)
                        {
                            Equip._secondary = 202003;
                        }
                    }
                    if (ItemClass == 3)
                    {
                        if (Equip._melee == Rule.WeaponId)
                        {
                            Equip._melee = 301001;
                        }
                    }
                    if (ItemClass == 4)
                    {
                        if (Equip._grenade == Rule.WeaponId)
                        {
                            Equip._grenade = 407001;
                        }
                    }
                    if (ItemClass == 5)
                    {
                        if (Equip._special == Rule.WeaponId)
                        {
                            Equip._special = 508001;
                        }
                    }
                    if (ItemClass == 6)
                    {
                        if (ClassType == 1)
                        {
                            if (Equip._red == Rule.WeaponId)
                            {
                                Equip._red = 601001;
                            }
                        }
                        else if (ClassType == 2)
                        {
                            if (Equip._blue == Rule.WeaponId)
                            {
                                Equip._blue = 602002;
                            }
                        }
                    }
                    if (ItemClass == 27)
                    {
                        if (Equip._beret == Rule.WeaponId)
                        {
                            Equip._beret = 0;
                        }
                    }
                    if (ItemClass == 8)
                    {
                        if (Equip._helmet == Rule.WeaponId)
                        {
                            Equip._helmet = 1000800000;
                        }
                    }
                }
            }
            if (Room.RPG7Active)
            {
                Equip.Primary = 116005;
            }
        }

        private void InsertItem(int id, Slot slot)
        {
            lock (slot.armas_usadas)
            {
                if (!slot.armas_usadas.Contains(id))
                {
                    slot.armas_usadas.Add(id);
                }
            }
        }
    }
}