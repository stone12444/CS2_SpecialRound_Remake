中文
第一个重做的CS2特殊回合插件，基于CounterStrikeSharp和Metamod开发的CS2 Linux服务端插件
原项目地址为：https://github.com/DeadSwimek/cs2-specialrounds
支持的回合有
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
    
