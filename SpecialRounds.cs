using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Timers;
using System.ComponentModel;
using CounterStrikeSharp.API.Modules.Entities;
using System;
using System.Numerics;
using System.Collections.Generic;

namespace SpecialRounds;
[MinimumApiVersion(120)]
public static class GetUnixTime
{
    public static int GetUnixEpoch(this DateTime dateTime)
    {
        var unixTime = dateTime.ToUniversalTime() -
                       new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        return (int)unixTime.TotalSeconds;
    }
}
public partial class SpecialRounds : BasePlugin, IPluginConfig<ConfigSpecials>
{
    public override string ModuleName => "SpecialRounds";
    public override string ModuleAuthor => "DeadSwim";
    public override string ModuleDescription => "Simple Special rounds.";
    public override string ModuleVersion => "V. 1.0.6";
    //private static readonly int?[] IsVIP = new int?[65];
    public CounterStrikeSharp.API.Modules.Timers.Timer? timer_up;//计时器类：弹跳玩家回合检查
    public CounterStrikeSharp.API.Modules.Timers.Timer? timer_decoy;//计时器类：诱饵弹回合检查
    public CounterStrikeSharp.API.Modules.Timers.Timer? timer_grenade;//计时器类：手雷回合检查

    public ConfigSpecials Config { get; set; }
    public int Round;
    public bool EndRound;
    public bool IsRound;
    public int IsRoundNumber;
    public string NameOfRound = "";
    public bool isset = false;
    public int ONtickspeedRoundcheck = 0;//确保速度回合速度修改起作用
    public int IsRoundNumberCheck = 0;//去除重复回合
    public bool CheckExcOneTIme=true;//确保只只执行一次的参考变量
    public static List<CBaseEntity> spawnPoints = new List<CBaseEntity>();//存储死亡竞赛重生点
    public void OnConfigParsed(ConfigSpecials config)
    {
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        WriteColor("Special round is [*Loaded*]", ConsoleColor.Green);

        // 注册地图开始事件监听器
        RegisterListener<Listeners.OnMapStart>(name =>
        {
            EndRound = false;
            IsRound = false;
            NameOfRound = "";
            IsRoundNumber = 0;
            IsRoundNumberCheck = 0;
            Round = 0;
        });

        // 注册每个游戏循环的Tick事件监听器
        RegisterListener<Listeners.OnTick>(() =>
        {
            for (int i = 1; i < Server.MaxPlayers; i++)
            {
                var ent = NativeAPI.GetEntityFromIndex(i);
                if (ent == 0)
                    continue;

                var client = new CCSPlayerController(ent);
                if (client == null || !client.IsValid)
                    continue;
                if (IsRound)
                {
                    client.PrintToCenterHtml(
                    $"<font color='gray'>----</font> <font class='fontSize-l' color='green'>Special Rounds</font><font color='gray'>----</font><br>" +
                    $"<font color='gray'>Now playing</font> <font class='fontSize-m' color='green'>[{NameOfRound}]</font>"
                    );
                }
                if (IsRoundNumber == 9 || ONtickspeedRoundcheck == 1)
                {
                    foreach (var player in Utilities.GetPlayers().Where(player => player is { IsValid: true }))//获取每个玩家实体
                    {
                        player.PlayerPawn.Value!.VelocityModifier = 2.0f;
                    }
                }//监视速度特殊回合防止掉速
                OnTick(client);
            }
        });
        
    }

    public static SpecialRounds It;
    public SpecialRounds()
    {
        It = this;
    }

    public static void OnTick(CCSPlayerController controller)
    {
        if (!controller.PawnIsAlive)
            return;
        var pawn = controller.Pawn.Value;
        var flags = (PlayerFlags)pawn.Flags;
        var client = controller.Index;
        var buttons = controller.Buttons;

        if (It.IsRoundNumber != 6)
            return;
        if (buttons == PlayerButtons.Attack2)
            return;
        if (buttons == PlayerButtons.Zoom)
            return;
    }

    [ConsoleCommand("css_startround", "Start specific round")]//指令
    public void startround(CCSPlayerController? player, CommandInfo info)
    {
        if (AdminManager.PlayerHasPermissions(player, "@css/root"))
        {
            int round_id;
            bool parseSuccess = int.TryParse(info.ArgByIndex(1), out round_id);
            if (!parseSuccess)
            {
                player.PrintToChat("Invalid round ID.");
                return;
            }

            EndRound = false;
            IsRound = true;
            IsRoundNumber = round_id;
            player.PrintToChat("YOU START A SPECIAL ROUND!");
            Server.ExecuteCommand("mp_warmup_end");
        }
    }

    [GameEventHandler]
    public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)//回合结束
    {
        if (EndRound)
        {
            WriteColor($"SpecialRound - [*SUCCESS*] Turning off the special round.", ConsoleColor.Green);


            IsRound = false;
            EndRound = false;
            isset = false;
            IsRoundNumber = 0;
            NameOfRound = "";
        }
        Server.ExecuteCommand("mp_freezetime 0");
        Server.ExecuteCommand("mp_randomspawn 0");//回合结束后指令
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)//回合开始
    {
        ONtickspeedRoundcheck = 0;//速度特殊回合的tick监听重置为0，防止持续变速
        ResetRoundEffects();//重置效果
        if (GameRules().WarmupPeriod)
        {
            IsRound = false;
            EndRound = false;
            isset = false;
            IsRoundNumber = 0;
            NameOfRound = "";
        }
        else
        {
            StartRandomSpecialRound();
            NameOfRound = GetRoundName(IsRoundNumber);
        }

        foreach (var l_player in Utilities.GetPlayers())
        {
            // 针对每个玩家开始特定的特殊回合，默认为正常回合！
            CCSPlayerController player = l_player;
            var client = player.Index;
            switch (IsRoundNumber)
            {
                case 1:
                    StartKnifeRound(player);
                    
                    break;
                case 2:
                    StartBHOPRound(player);
                    break;
                case 3:
                    StartGravityRound(player);
                    break;
                case 4:
                    StartAWPRound(player);
                    
                    break;
                case 5:
                    StartP90Round(player);
                    break;
                case 6:
                    StartANORound(player);
                    
                    break;
                case 7:
                    StartSlapRound(player);
                    break;
                case 8:
                    StartDecoyRound(player);
                    
                    break;
                case 9:
                    StartSpeedRound(player);
                    break;
                case 10:
                    StartC4PlantAwnywhere(player);
                    break;
                case 11:
                    StartInfiniteTaserSpeed(player);
                    
                    break;
                case 12:
                    StartRandomTelepotWithReload(player);
                    break;
                case 13:
                    StartHeadOnlyRound(player);
                    break;
                case 14:
                    StartGrenadeOnlyRound(player);                   
                    break;
                case 15:
                    StartSwappositionsRound(player);
                    break;
                case 16:
                    StartAmmoWithPay(player);
                    break;
                case 17:
                    StartTeamHurtHpUpRound(player);
                    break;
                default:
                    StartDefaultRound(player);
                    break;
            }
        }
        te_team_giveC4();

        isset = false;
        return HookResult.Continue;
    }

    //////////////////////////////////////////////特殊回合所需监听事件//记得加装饰器   
    /////注意Post值，Pre：表示钩子在原始方法执行前被调用。Post：表示钩子在原始方法执行后被调用。
    [GameEventHandler(HookMode.Pre)]
    public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)//受伤命中事件监听
    {
        CCSPlayerController player = @event.Userid;//受害者
        CCSPlayerController attacker = @event.Attacker;//攻击者
        


        if (player.Connected != PlayerConnectedState.PlayerConnected || !player.PlayerPawn.IsValid || !@event.Userid.IsValid)//检测是否存活以及有效
            return HookResult.Continue;
        if (IsRoundNumber == 4 || IsRoundNumber == 6)//仅awp回合禁用刀伤害
        {
            BanWeaponDamage(player, @event,"knife");
        }
        if (IsRoundNumber == 8)//诱饵弹回合
        {
            BanWithOutDamage(player, @event,"decoy"); 
        }
        if (IsRoundNumber == 14) //雷战,等避免其他武器造成伤害的方法
        {
            BanWithOutDamage(player, @event, "hegrenade");
        }
        if (IsRoundNumber == 15)//击打互相传送
        {
            Round15_HurtTelportEvent(@event, player, attacker);
        }
        if (IsRoundNumber == 16 )//一枪100
        {
            Round16_HurtEvent(@event, player, attacker);
        }
        if (IsRoundNumber == 17) 
        {
            Round17_TeamHurtHpUpEvent(@event, player, attacker);
        }
        
        return HookResult.Continue;
    }

    [GameEventHandler(HookMode.Pre)]
    public HookResult OnWeaponFire(EventWeaponFire @event ,GameEventInfo info)//开火事件:info用于是否显示
    {
        
        CCSPlayerController player = @event.Userid!;//开枪者
        
        if (player!.Connected != PlayerConnectedState.PlayerConnected || !player.PlayerPawn.IsValid || !player.IsValid)
            return HookResult.Continue;
        if (IsRoundNumber == 16)
        {
            Round16_FireEvent(@event,player);
        }
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnWeaponZoom(EventWeaponZoom @event, GameEventInfo info)//缩放事件
    {
        var player = @event.Userid;
        if (IsRoundNumber == 6)
        {
            RegiveZoomWeapon(player);     
        }

        return HookResult.Continue;
    }
    [GameEventHandler(HookMode.Post)]
    public HookResult OnWeaponReload(EventWeaponReload @event, GameEventInfo info)//以event事件为基础所作更改（On xxx），此为装弹事件,具体到某位玩家
    {
        var player_css = @event.Userid;//具体到某位玩家
        if (IsRoundNumber == 12)//装弹传送回合用
        {
            Round12_WeaponReloadEvent(@event,player_css);
        }
        

        return HookResult.Continue;
    }

    /////////////////////////////////////////////////////////////////////////////////////
    // 获取特殊回合名称
    private string GetRoundName(int round_id)
    {
        return round_id switch
        {
            1 => "Knife only(拼刀)",
            2 => "Auto BHopping(按住空格键可连跳)",
            3 => "Gravity round(失重)",//失重
            4 => "Only AWP + infinite ammo + Speed(AWP+无限子弹+加速)",
            5 => "Only P90 + infinite ammo(仅P90+无限子弹)",
            6 => "Only NO SCOPE AWP + infinite ammo(不能开镜AWP+无限子弹)",
            7 => "Slapping round(间期性拍打玩家)",//间期性拍打玩家
            8 => "Decoy round(仅限诱饵弹打人伤害)",
            9 => "Speed round(加速回合)",
            10 => "Plant C4 Anywhere(到处下包)",
            11 => "infinite ammo taser + speed(无限电击枪+加速)",
            12 => "Random Telepot When Weapon Reload(装弹随机传送)",
            13 => "Head Only(仅头部有伤害)",
            14=> "Grenade Only(仅手雷伤害)",
            15=> "Swap positions with the player you shooted(与被命中的玩家互换位置)",
            16=>"Ammo Need Pay(一枪100块，扣完没伤害，并且当前回合获得的金钱也能用于支付)",
            17=>"teammate hurt and HP up(打队友回15血)",
            _ => "Default round(正常回合)"//默认回合
        };
    }

    // 启动随机特殊回合
    private void StartRandomSpecialRound()
    {
        Random rnd = new Random();
        IsRoundNumber = rnd.Next(1,18); // 生成1到14之间的随机数字,可先固定测试！！！！！！！！！！！！！
        if (IsRoundNumberCheck == IsRoundNumber)
        {
            StartRandomSpecialRound();
        }
        IsRoundNumberCheck = IsRoundNumber;//赋值，记住该回合数字
        IsRound = true;
        EndRound = false;
        // 更新回合名称
        NameOfRound = GetRoundName(IsRoundNumber);

        WriteColor($"SpecialRound - [*INFO*] Starting new special round: {NameOfRound}", ConsoleColor.Cyan);
    }
    private void ResetRoundEffects()//重置所有特殊回合所做的更改
    {
        change_cvar("mp_buytime", "30");
        Server.ExecuteCommand("mp_plant_c4_anywhere 0");
        Server.ExecuteCommand("sv_infinite_ammo 0");
        Server.ExecuteCommand("mp_freezetime 0");
        Server.ExecuteCommand("sv_autobunnyhopping false");
        Server.ExecuteCommand("sv_enablebunnyhopping false");
        Server.ExecuteCommand("sv_gravity 800");
        Server.ExecuteCommand("mp_randomspawn 0");
        Server.ExecuteCommand("mp_damage_headshot_only false");
        CheckExcOneTIme = true;
        if (IsRoundNumber == 7)//杀死创建的计时器
        {
            timer_up?.Kill();
        }
        if (IsRoundNumber == 8)//杀死创建的计时器
        {
            timer_decoy?.Kill();
        }
        if (IsRoundNumber == 14)
        {
            timer_grenade?.Kill();
        }
        foreach (var player in Utilities.GetPlayers().Where(player => player is { IsValid: true }))//遍历每位玩家
        {
            player.PlayerPawn.Value!.VelocityModifier = 1.0f;//修正速度
        }

    }

    
}
//制作新回合过程，1.GetRoundName方法写名字与序号，2.OnRoundStart中switch添加新回合（先制作新方法），3.制作新回合专用方法，写在LIBFORROUND.cs，4.在ResetRoundEffects()方法中恢复原状（如果需要的话）5.增加随机数范围
//其他：tick事件监听器（特殊），计时器（特殊）,伤害侦听器（特殊），缩放检测（特殊），以及各种装饰器的Post值,是发生后触发还是反之,
//特殊方法写在Lib.cs文件中,事件专用方法写在LIBFORROUNDEVENT.cs