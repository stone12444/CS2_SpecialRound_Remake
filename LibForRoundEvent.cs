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
using Microsoft.Extensions.Logging;
using System.Numerics;

namespace SpecialRounds
{
    public partial class SpecialRounds//存储事件监听事件所需要调用的专用方法
    {
        public void Round16_HurtEvent(EventPlayerHurt @event,CCSPlayerController player,CCSPlayerController attacker)//16回合专用Hurt事件方法-打人不扣钱
        {
            if (!(@event.Weapon == "weapon_knife" || @event.Weapon == "weapon_knife_t" || @event.Weapon == "knife" || @event.Weapon == "hegrenade" || @event.Weapon == "decoy" || @event.Weapon == "smokegrenade" || @event.Weapon == "flashbang" || @event.Weapon == "inferno"))//检测是否为刀，以及各种投掷物
            {
                if (player.Team != attacker.Team && attacker != null)
                {

                    if (attacker.InGameMoneyServices.Account >= 100)
                    {

                        attacker.InGameMoneyServices.Account += 100;
                        Console.WriteLine(attacker.InGameMoneyServices.Account);
                        Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");//刷新实体状态（某个属性）
                        
                    }
                    else
                    {
                        player.PlayerPawn.Value.Health = player.PlayerPawn.Value.Health + @event.DmgHealth;//血量加回去防止掉血
                        player.PlayerPawn.Value.ArmorValue = player.PlayerPawn.Value.ArmorValue + @event.DmgArmor;//加回去防止掉护甲
                        @event.Userid.PlayerPawn.Value.VelocityModifier = 1;
                    }

                }
            }
            

        }
        public void Round16_FireEvent(EventWeaponFire @event, CCSPlayerController player)//16回合专用开火事件方法-开火扣钱
        {
            
            if (!(@event.Weapon == "weapon_knife" || @event.Weapon == "weapon_knife_t" || @event.Weapon == "knife"||@event.Weapon== "hegrenade" || @event.Weapon == "decoy" || @event.Weapon == "smokegrenade" || @event.Weapon == "flashbang" || @event.Weapon == "inferno"))//检测是否为刀，以及各种投掷物
            {

                if (player != null)
                {


                    if (player.InGameMoneyServices!.Account >= 100)
                    {
                        player.InGameMoneyServices!.Account -= 100;//扣钱
                    }

                    Console.WriteLine(player.InGameMoneyServices!.Account);
                    Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");//刷新实体状态（某个属性）！！！，实体类方法基本都在https://docs.cssharp.dev/api/CounterStrikeSharp.API.Utilities.html

                }

            }
        }
        public void Round12_WeaponReloadEvent(EventWeaponReload @event, CCSPlayerController player_css)
        {
            if (CheckExcOneTIme)
            {
                spawnPoints = GetAllDeathmatchSpawnPoints();//获取一次所有出生点,全局静态
                CheckExcOneTIme = false;//确保执行一次
            }

            CBasePlayerPawn player = player_css.Pawn.Value;
            if (spawnPoints != null && spawnPoints.Count > 0)
            {
                // 随机选择一个出生点

                Random rnd = new Random();
                var spawnPoint = spawnPoints[rnd.Next(spawnPoints.Count)];

                // 获取出生点的位置和旋转


                // 将玩家传送到随机出生点

                Console.WriteLine($"{spawnPoint.AbsOrigin.X} is X,{spawnPoint.AbsOrigin.Y} is Y,{spawnPoint.AbsOrigin.Z} is Z,");
                player.Teleport(spawnPoint.AbsOrigin);
            }
        }//12回合专用装弹事件方法-装弹传送
        public void Round15_HurtTelportEvent(EventPlayerHurt @event, CCSPlayerController player, CCSPlayerController attacker)//15回合专用Hurt事件方法-互相传送
        {
            if (player.Team != attacker.Team && attacker != null)//不同阵营
            {

                CBasePlayerPawn tel_save_player = player.Pawn.Value;//引用相同类有相同内存地址，一个变其他跟着变,基类Vector也算
                CBasePlayerPawn atttel_save_player = attacker.Pawn.Value;
                float tel_save_temp_x, tel_save_temp_y, tel_save_temp_z, attel_save_temp_x, attel_save_temp_y, attel_save_temp_z;
                tel_save_temp_x = tel_save_player.AbsOrigin.X;
                tel_save_temp_y = tel_save_player.AbsOrigin.Y;
                tel_save_temp_z = tel_save_player.AbsOrigin.Z;
                attel_save_temp_x = atttel_save_player.AbsOrigin.X;
                attel_save_temp_y = atttel_save_player.AbsOrigin.Y;
                attel_save_temp_z = atttel_save_player.AbsOrigin.Z;
                //Console.WriteLine($"tel_save_temp-------{tel_save_temp_x} is X,{tel_save_temp_y} is Y,{tel_save_temp_z} is Z,");
                //Console.WriteLine($"attel_save_temp-------{attel_save_temp_x} is X,{attel_save_temp_y} is Y,{attel_save_temp_z} is Z,");
                tel_save_player.Teleport(new CounterStrikeSharp.API.Modules.Utils.Vector(attel_save_temp_x, attel_save_temp_y, attel_save_temp_z));
                atttel_save_player.Teleport(new CounterStrikeSharp.API.Modules.Utils.Vector(tel_save_temp_x, tel_save_temp_y, tel_save_temp_z));

            }
        }
        public void Round17_TeamHurtHpUpEvent(EventPlayerHurt @event, CCSPlayerController player, CCSPlayerController attacker)
        {
            if(player.Team == attacker.Team && attacker != null)//相同阵营
            {
                int health = player.PlayerPawn.Value.Health;
                if (player.PlayerPawn.Value.Health < 100&&(health+15)<=100)
                {
                    player.PlayerPawn.Value.Health = player.PlayerPawn.Value.Health + 15;
                    @event.Userid.PlayerPawn.Value.VelocityModifier = 1;
                }

            }
        }
    }
}
