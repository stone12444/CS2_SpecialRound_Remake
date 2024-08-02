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
    public partial class SpecialRounds//partial可以将类分在不同地方，存储需要调用的特殊回合方法
    {
        internal static CCSGameRules GameRules()//获取当前游戏状态，热身等，可修改规则
        {
            return Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
        }
        static void WriteColor(string message, ConsoleColor color)//写在控制台的
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
        }//获取血量
        private static bool CheckIsHaveWeapon(string weapon_name, CCSPlayerController? pc)//检查玩家手上是否有某种武器
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
        public void DecoyCheck(CCSPlayerController? player)//诱饵弹回合:定时给予
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
        public void GrenadeCheck(CCSPlayerController? player)//手榴弹回合:定时给予
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
        public void goup(CCSPlayerController? player)//周期性弹玩家
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
        static public bool is_alive(CCSPlayerController? player)//玩家是否存活
        {
            if (!player.PawnIsAlive)
            {
                return false;
            }
            return true;
        }

        static public bool change_cvar(string cvar, string value)//执行服务端命令
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
        static List<CBaseEntity> GetAllDeathmatchSpawnPoints()//获取所有实体中的死亡竞赛重生点
        {
         var spawnPoints = new List<CBaseEntity>();

          // 遍历所有实体，有一个方法可以获取所有实体
            foreach (var CEntityInstance_entity in Utilities.GetAllEntities())
            {
            var entity = CEntityInstance_entity.As<CBaseEntity>();
            // 检查实体是否是CInfoDeathmatchSpawn或其子类
            string deathmatchSpawn = CEntityInstance_entity.DesignerName;
            if (deathmatchSpawn == "info_deathmatch_spawn")//通过名称筛选
                {

                    spawnPoints.Add(entity);
                }

            }

            return spawnPoints;
        }
        public static void te_team_giveC4()//检查玩家手里是否有c4，并且在没有的情况下给予一个
        {
            int c4 = 0;
            foreach (var tplayer in Utilities.GetPlayers())//获取每个玩家实体,且队伍为t
            {
                if (CheckIsHaveWeapon("weapon_c4", tplayer) && tplayer.Team == CsTeam.Terrorist)
                {
                    c4 = 1;
                    break;
                }
            }
            if (c4 == 0)
            {
                foreach (var tplayer in Utilities.GetPlayers())//获取每个玩家实体,且队伍为t
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
        public void BanWithOutDamage(CCSPlayerController player, EventPlayerHurt @event, string weapon)//ban除了xxx以外的伤害！-仅限受伤事件
        {
            if (@event.Weapon != weapon)
            {
                player.PlayerPawn.Value.Health = player.PlayerPawn.Value.Health + @event.DmgHealth;//血量加回去防止掉血
                player.PlayerPawn.Value.ArmorValue = player.PlayerPawn.Value.ArmorValue + @event.DmgArmor;//加回去防止掉护甲
                @event.Userid.PlayerPawn.Value.VelocityModifier = 1;
                player.PrintToChat($" {ChatColors.Default}[{ChatColors.Green}Server{ChatColors.Default}] Can not Hit By other Weapon");
            }
        }
        public void BanWeaponDamage(CCSPlayerController player, EventPlayerHurt @event, string weapon)//banxxx的伤害-仅限受伤事件
        {
            if (@event.Weapon == weapon)
            {
                player.PlayerPawn.Value.Health = player.PlayerPawn.Value.Health + @event.DmgHealth;//血量加回去防止掉血
                player.PlayerPawn.Value.ArmorValue = player.PlayerPawn.Value.ArmorValue + @event.DmgArmor;//加回去防止掉护甲
                //player.PlayerPawn.Value.Health = player.Health;可导致秒杀
                @event.Userid.PlayerPawn.Value.VelocityModifier = 1;
                player.PrintToChat($" {ChatColors.Default}[{ChatColors.Green}Server{ChatColors.Default}] Can not Hit By other Weapon");
            }
        }

        public void RemoveAllWeapon(CCSPlayerController player)//移除所有武器方法
        {
            foreach (var weapon in player.PlayerPawn.Value!.WeaponServices!.MyWeapons)//移除所有武器
            {
                if (weapon is { IsValid: true, Value.IsValid: true })
                {
                    weapon.Value.Remove();
                }
            }
        }
        public void RegiveZoomWeapon(CCSPlayerController player)//禁止当前武器缩放方法
        {
            var weaponservices = player.PlayerPawn.Value.WeaponServices!;
            var currentWeapon = weaponservices.ActiveWeapon.Value.DesignerName;

            weaponservices.ActiveWeapon.Value.Remove();//获取武器本身实体并删除武器
            player.GiveNamedItem(currentWeapon);
        }


    }       
}

