using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;

namespace SpecialRounds
{
    public partial class SpecialRounds//partial���Խ�����ڲ�ͬ�ط����洢��Ҫ���õ�����غϷ���
    {
        internal static CCSGameRules GameRules()//��ȡ��ǰ��Ϸ״̬������ȣ����޸Ĺ���
        {
            return Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
        }
        static void WriteColor(string message, ConsoleColor color)//д�ڿ���̨��
        {
            var pieces = Regex.Split(message, @"(\[[^\]]*\])");

            for (int i = 0; i < pieces.Length; i++)
            {
                string piece = pieces[i];

                if (piece.StartsWith("[") && piece.EndsWith("]"))
                {
                    Console.ForegroundColor = color;
                    piece = piece.Substring(1, piece.Length - 2);
                }

                Console.Write(piece);
                Console.ResetColor();
            }

            Console.WriteLine();
        }
        static public int get_hp(CCSPlayerController? player)
        {
            if (player == null || !player.PawnIsAlive)
            {
                return 100;
            }
            return player.PlayerPawn.Value.Health;
        }//��ȡѪ��
        private static bool CheckIsHaveWeapon(string weapon_name, CCSPlayerController? pc)//�����������Ƿ���ĳ������
        {
            if (pc == null || !pc.IsValid)
                return false;

            var pawn = pc.PlayerPawn.Value.WeaponServices!;
            foreach (var weapon in pawn.MyWeapons)
            {
                if (weapon is { IsValid: true, Value.IsValid: true })
                {
                    if (weapon.Value.DesignerName.Contains($"{weapon_name}"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public void DecoyCheck(CCSPlayerController? player)//�ն����غ�:��ʱ����
        {
            if (!player!.PawnIsAlive || !player.IsValid)
                return;
            var client = player.Index;
            var pawn = player.PlayerPawn;
            if (IsRoundNumber == 8)
            {
                if (IsRound == true)
                {
                    if (CheckIsHaveWeapon("weapon_decoy", player) == false)
                    {
                        player.GiveNamedItem("weapon_decoy");
                    }
                }
                else
                {
                    timer_decoy?.Kill();
                }
            }
            else
            {
                timer_decoy?.Kill();
            }
        }
        public void GrenadeCheck(CCSPlayerController? player)//���񵯻غ�:��ʱ����
        {
            if (!player.PawnIsAlive || !player.IsValid || player == null)
                return;
            var client = player.Index;
            var pawn = player.PlayerPawn;
            if (IsRoundNumber == 14)
            {
                if (IsRound == true)
                {
                    if (CheckIsHaveWeapon("weapon_hegrenade", player) == false)
                    {
                        player.GiveNamedItem("weapon_hegrenade");
                    }
                }
                else
                {
                    timer_grenade?.Kill();
                }
            }
            else
            {
                timer_grenade?.Kill();
            }
        }
        public void goup(CCSPlayerController? player)//�����Ե����
        {
            if(player == null || !player.IsValid)
            {
                //WriteColor($"Special Rounds - [*goup*] is not valid or is disconnected.", ConsoleColor.Red);
                return;
            }
            if(!player.PawnIsAlive)
            {
                WriteColor($"Special Rounds - [*{player.PlayerName}*] is death.", ConsoleColor.Red);
                return;  
            }
            var pawn = player.Pawn.Value;


            var random = new Random();
            var vel = new CounterStrikeSharp.API.Modules.Utils.Vector(pawn.AbsVelocity.X, pawn.AbsVelocity.Y, pawn.AbsVelocity.Z);

            vel.X += ((random.Next(180) + 50) * ((random.Next(2) == 1) ? -1 : 1));
            vel.Y += ((random.Next(180) + 50) * ((random.Next(2) == 1) ? -1 : 1));
            vel.Z += random.Next(200) + 100;
            if (IsRoundNumber == 7)
            {
                if (IsRound)
                {
                    pawn.Teleport(pawn.AbsOrigin!, pawn.AbsRotation!, vel);
                }
                else
                {
                    timer_up?.Kill();
                }
            }
            else { 
                timer_up?.Kill();
            }

            
        }
        static public bool is_alive(CCSPlayerController? player)//����Ƿ���
        {
            if (!player.PawnIsAlive)
            {
                return false;
            }
            return true;
        }

        static public bool change_cvar(string cvar, string value)//ִ�з��������
        {
                var find_cvar = ConVar.Find($"{cvar}");
                if (find_cvar == null)
                {
                    WriteColor($"SpecialRound - [*ERROR*] Canno't set {cvar} to {value}.", ConsoleColor.Red);
                    return false;
                }
                Server.ExecuteCommand($"{cvar} {value}");
                return true;
        }
        static List<CBaseEntity> GetAllDeathmatchSpawnPoints()//��ȡ����ʵ���е���������������
        {
         var spawnPoints = new List<CBaseEntity>();

          // ��������ʵ�壬��һ���������Ի�ȡ����ʵ��
            foreach (var CEntityInstance_entity in Utilities.GetAllEntities())
            {
            var entity = CEntityInstance_entity.As<CBaseEntity>();
            // ���ʵ���Ƿ���CInfoDeathmatchSpawn��������
            string deathmatchSpawn = CEntityInstance_entity.DesignerName;
            if (deathmatchSpawn == "info_deathmatch_spawn")//ͨ������ɸѡ
                {

                    spawnPoints.Add(entity);
                }

            }

            return spawnPoints;
        }
        public static void te_team_giveC4()//�����������Ƿ���c4��������û�е�����¸���һ��
        {
            int c4 = 0;
            foreach (var tplayer in Utilities.GetPlayers())//��ȡÿ�����ʵ��,�Ҷ���Ϊt
            {
                if (CheckIsHaveWeapon("weapon_c4", tplayer) && tplayer.Team == CsTeam.Terrorist)
                {
                    c4 = 1;
                    break;
                }
            }
            if (c4 == 0)
            {
                foreach (var tplayer in Utilities.GetPlayers())//��ȡÿ�����ʵ��,�Ҷ���Ϊt
                {
                    if (tplayer.Team == CsTeam.Terrorist)
                    {
                        tplayer.GiveNamedItem("weapon_c4");
                        break;
                    }

                }
            }
        }
        /// /////////////////////////////////////////////
        public void BanWithOutDamage(CCSPlayerController player, EventPlayerHurt @event, string weapon)//ban����xxx������˺���-���������¼�
        {
            if (@event.Weapon != weapon)
            {
                player.PlayerPawn.Value.Health = player.PlayerPawn.Value.Health + @event.DmgHealth;//Ѫ���ӻ�ȥ��ֹ��Ѫ
                player.PlayerPawn.Value.ArmorValue = player.PlayerPawn.Value.ArmorValue + @event.DmgArmor;//�ӻ�ȥ��ֹ������
                @event.Userid.PlayerPawn.Value.VelocityModifier = 1;
                player.PrintToChat($" {ChatColors.Default}[{ChatColors.Green}Server{ChatColors.Default}] Can not Hit By other Weapon");
            }
        }
        public void BanWeaponDamage(CCSPlayerController player, EventPlayerHurt @event, string weapon)//banxxx���˺�-���������¼�
        {
            if (@event.Weapon == weapon)
            {
                player.PlayerPawn.Value.Health = player.PlayerPawn.Value.Health + @event.DmgHealth;//Ѫ���ӻ�ȥ��ֹ��Ѫ
                player.PlayerPawn.Value.ArmorValue = player.PlayerPawn.Value.ArmorValue + @event.DmgArmor;//�ӻ�ȥ��ֹ������
                //player.PlayerPawn.Value.Health = player.Health;�ɵ�����ɱ
                @event.Userid.PlayerPawn.Value.VelocityModifier = 1;
                player.PrintToChat($" {ChatColors.Default}[{ChatColors.Green}Server{ChatColors.Default}] Can not Hit By other Weapon");
            }
        }

        public void RemoveAllWeapon(CCSPlayerController player)//�Ƴ�������������
        {
            foreach (var weapon in player.PlayerPawn.Value!.WeaponServices!.MyWeapons)//�Ƴ���������
            {
                if (weapon is { IsValid: true, Value.IsValid: true })
                {
                    weapon.Value.Remove();
                }
            }
        }
        public void RegiveZoomWeapon(CCSPlayerController player)//��ֹ��ǰ�������ŷ���
        {
            var weaponservices = player.PlayerPawn.Value.WeaponServices!;
            var currentWeapon = weaponservices.ActiveWeapon.Value.DesignerName;

            weaponservices.ActiveWeapon.Value.Remove();//��ȡ��������ʵ�岢ɾ������
            player.GiveNamedItem(currentWeapon);
        }


    }       
}

