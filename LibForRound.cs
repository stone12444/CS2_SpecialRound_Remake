using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CounterStrikeSharp.API.Modules.Timers;
using System.Xml.Serialization;

namespace SpecialRounds
{
    public partial class SpecialRounds//存储各种特殊回合方法,遍历每个玩家
    {
        private void StartKnifeRound(CCSPlayerController player)
        {
            Server.ExecuteCommand("mp_buytime 0");
            WriteColor($"SpecialRound - [*ROUND START*] Starting special round {NameOfRound}.", ConsoleColor.Green);
            if (IsRound)
            {

                if (!is_alive(player))
                    return;
                RemoveAllWeapon(player);
                player.GiveNamedItem("weapon_knife");
                if (!EndRound)
                {
                    EndRound = true;
                }
            }
        }

        private void StartBHOPRound(CCSPlayerController player)
        {
            WriteColor($"SpecialRound - [*ROUND START*] Starting special round {NameOfRound}.", ConsoleColor.Green);
            if (IsRound)
            {
                change_cvar("sv_autobunnyhopping", "true");
                change_cvar("sv_enablebunnyhopping", "true");
                if (!EndRound)
                {
                    EndRound = true;
                }
            }
        }

        private void StartGravityRound(CCSPlayerController player)
        {
            WriteColor($"SpecialRound - [*ROUND START*] Starting special round {NameOfRound}.", ConsoleColor.Green);
            if (IsRound)
            {
                change_cvar("sv_gravity", "400");
                if (!EndRound)
                {
                    EndRound = true;
                }
            }
        }

        private void StartAWPRound(CCSPlayerController player)
        {
            WriteColor($"SpecialRound - [*ROUND START*] Starting special round {NameOfRound}.", ConsoleColor.Green);
            if (IsRound)
            {
                if (!is_alive(player))
                    return;
                Server.ExecuteCommand("sv_infinite_ammo 1");
                Server.ExecuteCommand("mp_buytime 0");
                CCSPlayerPawn? pawn = player.PlayerPawn.Value;
                Server.PrintToConsole($"{player.PlayerPawn.Value!.Speed}");
                pawn.VelocityModifier = 2.0f;
                ONtickspeedRoundcheck = 1;//速度特殊回合的tick监听启动，防止掉速
                player.PlayerPawn.Value!.Health = 200;
                RemoveAllWeapon(player);
                player.GiveNamedItem("weapon_awp");
                player.GiveNamedItem("weapon_knife");
                if (!EndRound)
                {
                    EndRound = true;
                }
            }
        }

        private void StartP90Round(CCSPlayerController player)
        {
            Server.ExecuteCommand("sv_infinite_ammo 1");
            Server.ExecuteCommand("mp_buytime 0");
            WriteColor($"SpecialRound - [*ROUND START*] Starting special round {NameOfRound}.", ConsoleColor.Green);
            if (IsRound)
            {
                if (!is_alive(player))
                    return;
                RemoveAllWeapon(player);
                player.GiveNamedItem("weapon_p90");
                if (!EndRound)
                {
                    EndRound = true;
                }
            }
        }

        private void StartANORound(CCSPlayerController player)
        {
            Server.ExecuteCommand("sv_infinite_ammo 1");
            Server.ExecuteCommand("mp_buytime 0");
            WriteColor($"SpecialRound - [*ROUND START*] Starting special round {NameOfRound}.", ConsoleColor.Green);
            if (IsRound)
            {
                if (!is_alive(player))
                    return;
                RemoveAllWeapon(player);
                player.GiveNamedItem("weapon_knife");
                player.GiveNamedItem("weapon_awp");
                if (!EndRound)
                {
                    EndRound = true;
                }
            }
        }

        private void StartSlapRound(CCSPlayerController player)
        {
            WriteColor($"SpecialRound - [*ROUND START*] Starting special round {NameOfRound}.", ConsoleColor.Green);
            if (IsRound)
            {
                Random rnd = new Random();
                int random = rnd.Next(3, 10);
                float random_time = random;
                timer_up = AddTimer(random + 0.1f, () => { goup(player); }, TimerFlags.REPEAT);
            }
        }

        private void StartDecoyRound(CCSPlayerController player)
        {
            WriteColor($"SpecialRound - [*ROUND START*] Starting special round {NameOfRound}.", ConsoleColor.Green);
            if (IsRound)
            {
                RemoveAllWeapon(player);
                player.PlayerPawn.Value!.Health = 1;
                player.GiveNamedItem("weapon_knife");
                player.GiveNamedItem("weapon_decoy");
                Server.ExecuteCommand("mp_buytime 0");
                timer_decoy = AddTimer(2.0f, () => { DecoyCheck(player); }, TimerFlags.REPEAT);
                Server.PrintToConsole($"{player.PlayerName}");
            }
        }

        private void StartSpeedRound(CCSPlayerController player)
        {
            WriteColor($"SpecialRound - [*ROUND START*] Starting special round {NameOfRound}.", ConsoleColor.Green);
            if (IsRound)
            {
                CCSPlayerPawn? pawn = player.PlayerPawn.Value;
                Server.PrintToConsole($"{player.PlayerPawn.Value!.Speed}");
                pawn.VelocityModifier = 2.0f;
                ONtickspeedRoundcheck = 1;//速度特殊回合的tick监听启动，防止掉速
                player.PlayerPawn.Value!.Health = 200;//设置200血
            }
        }
        private void StartC4PlantAwnywhere(CCSPlayerController player)
        {
            WriteColor($"SpecialRound - [*ROUND START*] Starting special round {NameOfRound}.", ConsoleColor.Green);
            if (IsRound)
            {
                Server.ExecuteCommand("mp_plant_c4_anywhere 1");
                if (!EndRound)
                {
                    EndRound = true;
                }
            }
        }
        private void StartInfiniteTaserSpeed(CCSPlayerController player)
        {
            WriteColor($"SpecialRound - [*ROUND START*] Starting special round {NameOfRound}.", ConsoleColor.Green);
            if (IsRound)
            {

                if (!is_alive(player))
                    return;
                RemoveAllWeapon(player);
                player.GiveNamedItem("weapon_knife");
                player.GiveNamedItem("weapon_taser");
                Server.ExecuteCommand("sv_infinite_ammo 1");
                CCSPlayerPawn? pawn = player.PlayerPawn.Value;
                Server.PrintToConsole($"{player.PlayerPawn.Value!.Speed}");
                pawn.VelocityModifier = 2.0f;
                ONtickspeedRoundcheck = 1;//速度特殊回合的tick监听启动，防止掉速

                if (!EndRound)
                {
                    EndRound = true;
                }
            }

        }
        private void StartRandomTelepotWithReload(CCSPlayerController player)
        {
            WriteColor($"SpecialRound - [*ROUND START*] Starting special round {NameOfRound}.", ConsoleColor.Green);
            if (IsRound)
            {
                //实现方法
                Server.ExecuteCommand("mp_randomspawn 1");
                if (!EndRound)
                {
                    EndRound = true;
                }
            }

        }
        private void StartHeadOnlyRound(CCSPlayerController player)
        {
            WriteColor($"SpecialRound - [*ROUND START*] Starting special round {NameOfRound}.", ConsoleColor.Green);
            if (IsRound)
            {
                Server.ExecuteCommand("mp_damage_headshot_only true");
                if (!EndRound)
                {
                    EndRound = true;
                }
            }

        }
        private void StartGrenadeOnlyRound(CCSPlayerController player)
        {
            WriteColor($"SpecialRound - [*ROUND START*] Starting special round {NameOfRound}.", ConsoleColor.Green);
            if (IsRound)
            {
                RemoveAllWeapon(player);
                player.PlayerPawn.Value!.Health = 300;
                player.GiveNamedItem("weapon_knife");
                player.GiveNamedItem("weapon_hegrenade");
                Server.ExecuteCommand("mp_buytime 0");
                timer_grenade = AddTimer(2.0f, () => { GrenadeCheck(player); }, TimerFlags.REPEAT);
                Server.PrintToConsole($"{player.PlayerName}");
            }
        }
        private void StartSwappositionsRound(CCSPlayerController player)
        {
            WriteColor($"SpecialRound - [*ROUND START*] Starting special round {NameOfRound}.", ConsoleColor.Green);
            if (IsRound)
            {
                //空
                if (!EndRound)
                {
                    EndRound = true;
                }
            }
        }
        private void StartAmmoWithPay(CCSPlayerController player)
        {
            WriteColor($"SpecialRound - [*ROUND START*] Starting special round {NameOfRound}.", ConsoleColor.Green);
            if (IsRound)
            {
                //空
                if (!EndRound)
                {
                    EndRound = true;
                }
            }
        }
        private void StartTeamHurtHpUpRound(CCSPlayerController player)
        {
            WriteColor($"SpecialRound - [*ROUND START*] Starting special round {NameOfRound}.", ConsoleColor.Green);
            if (IsRound) 
            {
                //空
                if (!EndRound)
                {
                    EndRound = true;
                }
            }
        }
        private void StartDefaultRound(CCSPlayerController player)
        {
            WriteColor($"SpecialRound - [*ROUND START*] Starting special round {NameOfRound}.", ConsoleColor.Green);
            if (IsRound)
            {
                //空正常回合
                if (!EndRound)
                {
                    EndRound = true;
                }
            }

        }
    }
}
