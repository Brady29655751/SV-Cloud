using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RM = ResourceManager;

namespace UnityEngine.UI {

public static class TransformHelper {
    public static void DestoryChildren(this Transform transform) {
        foreach (Transform t in transform) {
            GameObject.Destroy(t.gameObject);
        }
    }
}

public static class SpriteSize {
    public static Vector2 GetTextureSize(this Texture2D texture) {
        return new Vector2(texture.width, texture.height);
    }

    public static Vector2 GetSpriteSize(this Sprite sprite) {
        return sprite.texture.GetTextureSize();
    }

    public static float GetResizedWidth(this Texture2D texture, float newHeight) {
        return texture.width * newHeight / texture.height;
    }
    public static float GetResizedWidth(this Sprite sprite, float newHeight) {
        return GetResizedWidth(sprite.texture, newHeight);
    }
    public static float GetResizedHeight(this Texture2D texture, float newWidth) {
        return texture.height * newWidth / texture.width;
    }
    public static float GetResizedHeight(this Sprite sprite, float newWidth) {
        return GetResizedHeight(sprite.texture, newWidth);
    }
    public static KeyValuePair<RectTransform.Axis, float>GetResizedSize(this Texture2D texture, Vector2 size, bool shrink = true) {
        if (size == Vector2.zero)
            return new KeyValuePair<RectTransform.Axis, float>(RectTransform.Axis.Horizontal, 0);
        if (size.x == 0) {
            return new KeyValuePair<RectTransform.Axis, float>(RectTransform.Axis.Vertical, texture.GetResizedWidth(size.y));
        }
        if (size.y == 0) {
            return new KeyValuePair<RectTransform.Axis, float>(RectTransform.Axis.Horizontal, texture.GetResizedWidth(size.x));
        }

        float width = texture.GetResizedWidth(size.y);
        float height = texture.GetResizedHeight(size.x);
        var resizeX = new KeyValuePair<RectTransform.Axis, float>(RectTransform.Axis.Horizontal, width);
        var resizeY = new KeyValuePair<RectTransform.Axis, float>(RectTransform.Axis.Horizontal, height);
        if (shrink) {
            return (width <= size.x) ? resizeX : resizeY;
        }
        return (width >= size.x) ? resizeX : resizeY;
        
    }
    public static KeyValuePair<RectTransform.Axis, float>GetResizedSize(this Sprite sprite, Vector2 size, bool shrink = true) {
        return GetResizedSize(sprite.texture, size, shrink);
    }

    public static void ResetAllTriggers(this Animator anim) {
        foreach (var param in anim.parameters) {
            if (param.type == AnimatorControllerParameterType.Trigger) {
                anim.ResetTrigger(param.name);
            }
        }
    }
}

public static class SpriteResources {
    public static Texture2D Cursor => RM.instance.Get<Texture2D>("cursor");
    public static GameObject SplitLine => RM.instance.GetPrefab("Split Line");
    public static GameObject Log => RM.instance.GetPrefab("Log");
    public static GameObject BattleRecord => RM.instance.GetPrefab("Battle Record");
    public static Sprite Empty => GetCardEmblemSprite(0);
    public static Sprite DefaultSleeve => RM.instance.GetSprite("Game/sleeve");
    public static Sprite PP => RM.instance.GetSprite("Game/pp/pp");
    public static Sprite PPUsed => RM.instance.GetSprite("Game/pp/pp_used");
    public static Sprite EP => RM.instance.GetSprite("Game/ep/ep");
    public static Sprite EPUsed => RM.instance.GetSprite("Game/ep/ep_used");
    public static Sprite EPContainer => RM.instance.GetSprite("Game/ep/container/1");
    public static Sprite EPContainerUsed => RM.instance.GetSprite("Game/ep/container/0");
    public static Sprite DeathCard => RM.instance.GetSprite("Game/icon/death");

    public static void SetSprite(this Image image, Sprite sprite) {
        if (image == null)
            return;

        image.sprite = sprite;
    }

    public static void SetTexture(this RawImage image, Texture2D texture) {
        if (image == null)
            return;

        image.texture = texture;
    }

    public static Color GetLeaderBattleColor(CardCraft craft) {
        return craft switch {
            CardCraft.Neutral => Color.black,
            CardCraft.Elf => new Color32(95, 164, 64, 255),
            CardCraft.Royal => Color.yellow,
            CardCraft.Witch => ColorHelper.chosen,
            CardCraft.Dragon => new Color32(255, 204, 32, 255),
            CardCraft.Necro => new Color(180, 73, 255, 255),
            CardCraft.Vampire => Color.red,
            CardCraft.Bishop => Color.white,            
            CardCraft.Nemesis => new Color32(0, 168, 255, 255),
            _ => Color.black,
        };
    }

    public static Sprite GetCardIcon(string icon) => RM.instance.GetSprite("Card Style/icon/" + icon);

    public static Sprite GetCardFrameSprite(int type, int rarity) {
        type = (type == (int)CardType.Evolved) ? (int)CardType.Follower : type;
        return RM.instance.GetSprite("Card Style/frame/cardType_" + type + rarity);
    }

    public static Sprite GetBattleCardFrameSprite(int type, int rarity) {
        var category = (CardType)type switch {
            CardType.Follower   =>  "follower",
            CardType.Evolved    =>  "follower",
            CardType.Amulet     =>  "amulet",
            _ => string.Empty
        };
        
        rarity += ((type == (int)CardType.Evolved) ? 4 : 0) - 1;

        return RM.instance.GetSprite("Game/frame/" + category + "/" + rarity);
    }

    public static Sprite GetDetailBackgroundSprite(int craft) {
        return RM.instance.GetSprite("Card Style/bg/background_" + craft);
    }

    public static Sprite GetThemeBackgroundSprite(int craft) {
        return RM.instance.GetSprite("Background/theme/theme_bg_" + craft);
    }

    public static Sprite GetLeaderBackgroundSprite(int craft) {
        return RM.instance.GetSprite("Class/bg/class_bg_" + craft);
    }

    public static Sprite GetLeaderProfileSprite(int craft) {
        return RM.instance.GetSprite("Class/profile/class_profile_" + craft);
    }

    public static Sprite GetCraftCheckboxSprite(int craft) {
        return RM.instance.GetSprite("Class/checkbox/class_checkbox(" + craft + ")");
    }

    public static Sprite GetCardGemSprite(int craft) {
        return RM.instance.GetSprite("Card Style/gem/gem_" + craft);
    }

    public static Sprite GetCardEmblemSprite(int craft) {
        return RM.instance.GetSprite("Card Style/emblem/Emblem_" + craft);
    }

    public static Sprite GetCardCostSprite(int cost) {
        if (!cost.IsWithin(0, 9))
            return null;

        return RM.instance.GetSprite("Card Style/cost/" + cost);
    }

    public static Sprite GetCardAtkHpSprite(int num) {
        if (!num.IsWithin(0, 9))
            return null;

        return RM.instance.GetSprite("Card Style/atk/atk_" + num);
    }

    public static Sprite GetFormatSprite(this GameFormat format) {
        var path = format switch {
            GameFormat.Unlimited        => "unlimited",
            GameFormat.Rotation         => "rotation",
            GameFormat.GemOfFortune     => "gem",
            GameFormat.TwoPick          => "2pick",
            GameFormat.AllStarTwoPick   => "2pick",
            _ => string.Empty,
        };
        return RM.instance.GetSprite("Format/icon_" + path) ?? SpriteResources.Empty;
    }

}

public static class TextHelper {
    public static void SetText(this Text text, string content) {
        if (text == null)
            return;

        text.text = content;
    }

    public static void SetFontSize(this Text text, int fontsize) {
        if (text == null)
            return;
        
        text.fontSize = fontsize;
    }

    public static void SetColor(this Text text, Color color) {
        if (text == null)
            return;
        
        text.color = color;
    }

    public static void SetColor(this Outline outline, Color color) {
        if (outline == null)
            return;

        outline.effectColor = color;
    }
}

public static class AudioResources {
    public static AudioClip Title => RM.instance.GetAudio("Title");
    public static AudioClip Main => RM.instance.GetAudio("Main");
    public static AudioClip Win => RM.instance.GetAudio("Win");
    public static AudioClip Lose => RM.instance.GetAudio("Lose");
    public static AudioClip Bell => RM.instance.GetAudio("Bell");
    public static AudioClip GetThemeBattleClip(int craft) {
        return RM.instance.GetAudio("Theme/" + craft);
    }    
}

public static class ColorHelper {
    public static Color black192 => new Color32(0, 0, 0, 192);
    public static Color gray192 => new Color32(192, 192, 192, 255);
    public static Color chosen => new Color32(3, 109, 159, 255);
    public static Color gold => new Color32(255, 187, 0, 255);
    public static Color green => new Color32(119, 226, 12, 255);
    public static Color green192 => new Color32(0, 192, 0, 255);
    public static Color blue => new Color32(82, 229, 249, 255);
    public static Color purple => new Color32(192, 0, 255, 255);

    // public static Color secretSkill => new Color32(252, 237, 105, 255); 
    // public static Color normalSkill => new Color32(82, 229, 249, 255);
    // public static Color storm => new Color32(119, 226, 12, 192);

    public static Color storm => new Color32(63, 255, 127, 192);
    public static Color rush => new Color32(255, 235, 4, 192);
    public static Color target => new Color32(255, 127, 0, 192);

    public static void SetColor(this Image image, Color color) {
        if (image == null)
            return;

        image.color = color;
    }

    public static Color GetAtkHpTextColor(int Hp, int HpMax, int HpInit) {
        if (Hp < HpMax)
            return Color.red;

        if (Hp > HpInit)
            return ColorHelper.green;

        if (HpMax == HpInit)
            return Color.white;

        return ColorHelper.gold;
    }

    public static Color GetAtkHpOutlineColor(int Hp, int HpMax, int HpInit) {
        return (GetAtkHpTextColor(Hp, HpMax, HpInit) == Color.red) ? ColorHelper.gray192 : Color.black;
    }

    public static Color GetResultColor(this BattleResultState resultState) {
        return resultState switch {
            BattleResultState.Win   =>  ColorHelper.gold,
            BattleResultState.Lose  =>  ColorHelper.gray192,
            BattleResultState.Draw  =>  Color.cyan,
            _ => Color.white,
        };
    }

    public static Color GetCraftColor(this CardCraft craft) {
        return craft switch {
            CardCraft.Elf       =>  ColorHelper.storm,
            CardCraft.Royal     =>  Color.yellow,
            CardCraft.Witch     =>  ColorHelper.chosen,
            CardCraft.Dragon    =>  ColorHelper.gold,
            CardCraft.Necro     =>  ColorHelper.purple,
            CardCraft.Vampire   =>  Color.red,
            CardCraft.Bishop    =>  Color.white, 
            CardCraft.Nemesis   =>  Color.cyan,
            _                   =>  ColorHelper.gray192,
        };
    }
}

public static class EventHelper {
    public static void SetListener(this Events.UnityEvent unityEvent, Action unityCall) {
        unityEvent.RemoveAllListeners();
        if (unityCall == null)
            return;

        unityEvent.AddListener(unityCall.Invoke);
    }

    public static void SetListener<T>(this Events.UnityEvent<T> unityEvent, Action<T> unityCall) {
        unityEvent.RemoveAllListeners();
        if (unityCall == null)
            return;
            
        unityEvent.AddListener(unityCall.Invoke);
    }
}

}

public enum FontOption {
    Weibei = 0,
    MSJH = 1,
}

