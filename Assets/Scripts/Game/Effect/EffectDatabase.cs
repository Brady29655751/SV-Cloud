using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EffectDatabase {
    private static Dictionary<string, EffectAbility> abilityConvDict = new Dictionary<string, EffectAbility>() {
        {"none",            EffectAbility.None          },
        {"result",          EffectAbility.SetResult     },
        {"keep",            EffectAbility.KeepCard      },
        {"turn_start",      EffectAbility.TurnStart     },
        {"turn_end",        EffectAbility.TurnEnd       },
        {"use",             EffectAbility.Use           },
        {"cover",           EffectAbility.Cover         },
        {"attack",          EffectAbility.Attack        },
        {"evolve",          EffectAbility.Evolve        },
        {"fusion",          EffectAbility.Fusion        },
        {"act",             EffectAbility.Act           },

        {"accelerate",      EffectAbility.Accelerate    },
        {"crystalize",      EffectAbility.Crystalize    },   

        {"random",          EffectAbility.Random        },
        {"index",           EffectAbility.Index         },

        {"set_keyword",     EffectAbility.SetKeyword    },
        {"draw",            EffectAbility.Draw          },
        {"summon",          EffectAbility.Summon        },
        {"damage",          EffectAbility.Damage        },
        {"heal",            EffectAbility.Heal          },
        {"destroy",         EffectAbility.Destroy       },
        {"vanish",          EffectAbility.Vanish        },
        {"return",          EffectAbility.Return        },
        {"transform",       EffectAbility.Transform     },
        {"buff",            EffectAbility.Buff          },
        {"debuff",          EffectAbility.Debuff        },

        {"get_token",       EffectAbility.GetToken      },
        {"boost",           EffectAbility.SpellBoost    },
        {"set_cost",        EffectAbility.SetCost       },
        {"ramp",            EffectAbility.Ramp          },
        {"add_effect",      EffectAbility.AddEffect     },
        {"remove_effect",   EffectAbility.RemoveEffect  },
        {"set_grave",       EffectAbility.SetGrave      },
        {"set_countdown",   EffectAbility.SetCountdown  },
        {"add_deck",        EffectAbility.AddDeck       },
        {"hybrid",          EffectAbility.Hybrid        },

        {"bury",            EffectAbility.Bury          },
        {"reanimate",       EffectAbility.Reanimate     },
        {"discard",         EffectAbility.Discard       },
        {"travel",          EffectAbility.Travel        },
        {"reveal",          EffectAbility.Reveal        },
        {"set_damage",      EffectAbility.SetDamage     },
        {"set_heal",        EffectAbility.SetHeal       },
        {"set_pp",          EffectAbility.SetPP         },
        {"choose",          EffectAbility.Choose        },
        {"set_value",       EffectAbility.SetValue      },
    };

    private static Dictionary<string, string> leaderInfoDict = new Dictionary<string, string>() {
        {"combo", "連擊數" },
        {"rally", "協作數" },
        {"destroyedFollowerCount", "已被破壞的從者數" },
        {"destroyedAmuletCount",   "已被破壞的護符數" },
        {"turn_draw_cards", "本回合自己抽取的卡片數" },
        {"turn_give_op_leader_damage", "追擊" },
        {"abuse", "凌虐" },
    };

    private static Dictionary<string, string> sourceInfoDict = new Dictionary<string, string>() {
        {"boost", "魔力增幅" },
    };

    public static EffectAbility ToEffectAbility(this string ability) {
        return abilityConvDict.Get(ability, EffectAbility.None);
    }

    public static bool IsTargetSelectableAbility(this EffectAbility ability) {
        return (ability == EffectAbility.Use) || (ability == EffectAbility.Evolve);
    }

    public static string[] GetLeaderInfoKeys() => leaderInfoDict.Keys.ToArray();
    public static string[] GetLeaderInfoValues() => leaderInfoDict.Values.ToArray();
    public static string ToLeaderInfoValue(this string key) => leaderInfoDict.Get(key, string.Empty);
    public static string[] GetSourceInfoKeys() => sourceInfoDict.Keys.ToArray();
    public static string[] GetSourceInfoValues() => sourceInfoDict.Values.ToArray();
    public static string ToSourceInfoValue(this string key) => sourceInfoDict.Get(key, string.Empty);
}

public enum EffectAbility {
    None = 0,   SetResult = 1,  KeepCard = 2,   TurnStart = 3,  TurnEnd = 4,
    Use = 5,    Cover = 6,  Attack = 7, Evolve = 8, Fusion = 9, Act = 10,
    Accelerate = 11,    Crystalize = 12,

    Random = 91,
    Index = 92,

    SetKeyword  = 100,   
    Draw        = 101, 
    Summon      = 102,   
    Damage      = 103,   
    Heal        = 104,
    Destroy     = 105,
    Vanish      = 106,
    Return      = 107,
    Transform   = 108,
    Buff        = 109,
    Debuff      = 110,

    GetToken    = 111,
    SpellBoost  = 112,
    SetCost     = 113,
    Ramp        = 114,
    AddEffect   = 115,
    RemoveEffect= 116,
    SetGrave    = 117,
    SetCountdown= 118,
    AddDeck     = 119,
    Hybrid      = 120,

    Bury        = 121,
    Reanimate   = 122,
    Discard     = 123,
    Travel      = 124,
    Reveal      = 125,
    SetDamage   = 126,
    SetHeal     = 127,
    SetPP       = 128,
    Choose      = 129,
    SetValue    = 130,
}