﻿using PointBlank.Battle.Data.Configs;
using PointBlank.Battle.Data.Enums;
using SharpDX;
using System;
using System.Net;

namespace PointBlank.Battle.Data.Models
{
    public class Player
    {
        public int Slot = -1, Team, Life = 100, MaxLife = 100, PlayerIdByUser = -2, PlayerIdByServer = -1, WeaponId, RespawnByUser = -2, RespawnByLogic, RespawnByServer = -1, AceCheck = 0;
        public float PlantDuration, DefuseDuration, C4Time;
        public byte Extensions;
        public Half3 Position;
        public IPEndPoint Client;
        public DateTime Date, LastPing, C4First;
        public PlayerEquipedItems Equip;
        public CLASS_TYPE WeaponClass;
        public CHARACTER_RES_ID _character;
        public bool Dead = true, NeverRespawn = true, Integrity = true, Immortal;

        public Player(int Slot)
        {
            this.Slot = Slot;
            this.Team = (Slot % 2);
        }

        public void LogPlayerPos(Half3 EndBullet)
        {
            Logger.warning("Player Position X: " + Position.X + " Y: " + Position.Y + " Z: " + Position.Z);
            Logger.warning("End Bullet Position X: " + EndBullet.X + " Y: " + EndBullet.Y + " Z: " + EndBullet.Z);
        }

        public bool CompareIp(IPEndPoint Ip)
        {
            return Client != null && Ip != null && Client.Address.Equals(Ip.Address) && Client.Port == Ip.Port;
        }

        public bool RespawnIsValid()
        {
            return RespawnByServer == RespawnByUser;
        }

        public bool RespawnLogicIsValid()
        {
            return RespawnByServer == RespawnByLogic;
        }

        public bool AccountIdIsValid()
        {
            return (PlayerIdByServer == PlayerIdByUser);
        }

        public bool AccountIdIsValid(int Number)
        {
            return (PlayerIdByServer == Number);
        }

        public void CheckLifeValue()
        {
            if (Life > MaxLife)
            {
                Life = MaxLife;
            }
        }

        public void ResetAllInfos()
        {
            Client = null;
            Date = new DateTime();
            PlayerIdByUser = -2;
            PlayerIdByServer = -1;
            Integrity = true;
            ResetBattleInfos();
        }

        public void ResetBattleInfos()
        {
            RespawnByServer = -1;
            RespawnByUser = -2;
            RespawnByLogic = 0;
            Immortal = false;
            Dead = true;
            NeverRespawn = true;
            WeaponId = 0;
            LastPing = new DateTime();
            C4First = new DateTime();
            C4Time = 0;
            Position = new Half3();
            Life = 100;
            AceCheck = 0;
            MaxLife = 100;
            PlantDuration = BattleConfig.plantDuration;
            DefuseDuration = BattleConfig.defuseDuration;
        }

        public void ResetLife()
        {
            Life = MaxLife;
        }
    }
}