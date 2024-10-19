using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class Card : IIdentifyHandler
{
    public const int DATA_COL = 7;
    public const int DESC_COL = 2;
    public static string[] StatusNames => new string[] { "cost", "atk", "hp" };
    public static Card Get(int id) => DatabaseManager.instance.GetCardInfo(id);
    public static GameObject Prefab => ResourceManager.instance.GetPrefab("Card");
    public static int GetBaseId(int uid) => ((CardType)(uid % 1000 / 100) != CardType.Evolved) ? uid : (uid - uid % 1000 + 100 + uid % 100);
    public static int GetEvolveId(int uid) => ((CardType)(uid % 1000 / 100) != CardType.Follower) ? 0 : (uid - uid % 1000 + 900 + uid % 100);

    public int id;
    public int Id => id;
    public int GroupId => id / (int)1e8;
    public int ZoneId => id % (int)1e8 / (int)1e7;
    public int PackId => id % (int)1e8 / (int)1e5;
    public int CraftId => id % (int)1e5 / 10000;
    public int RarityId => id % 10000 / 1000;
    public int TypeId => id % 1000 / 100;
    public int EvolveTypeId => (int)EvolveType;
    public int SerialNum => id % 100;

    public int NameId { get; protected set; }
    public int ArtworkId { get; protected set; }
    public int CountLimit { get; protected set; }

    public CardGroup Group => (CardGroup)GroupId;
    public CardZone Zone => (CardZone)ZoneId;
    public CardPack Pack => (CardPack)PackId;
    public CardCraft Craft => (CardCraft)CraftId;
    public CardType Type => (CardType)TypeId;
    public CardType EvolveType => Type == CardType.Evolved ? CardType.Follower : CardType.Evolved;
    public CardRarity Rarity => (CardRarity)RarityId;

    public string name;
    public int cost, atk, hp, hpMax, countdown;
    public List<CardTrait> traits = new List<CardTrait>();
    public List<CardKeyword> keywords = new List<CardKeyword>();
    public List<int> tokenIds = new List<int>();
    public List<int> effectIds = new List<int>();
    public List<Effect> effects = new List<Effect>();
    public Dictionary<string, string> options = new Dictionary<string, string>();
    public string description;

    public Task<Texture2D> Artwork => Addressables.LoadAssetAsync<Texture2D>(ArtworkId.ToString()).Task;
    public Card BaseCard => Card.Get(Card.GetBaseId(id));
    public Card EvolveCard => Card.Get(Card.GetEvolveId(id));


    public static Card GetLeaderCard(int craft) => Card.Get(100000000 + craft * 10000);

    public Card(string[] _data, int startIndex) {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);

        id = int.Parse(_slicedData[0]);
        var status = _slicedData[1].ToIntList('/');
        cost = status[0];
        atk = status[1];
        hp = status[2];
        hpMax = hp;
        traits = _slicedData[2].ToIntList('/').Select(x => (CardTrait)x).ToList();
        keywords = _slicedData[3].ToIntList('/').Select(x => (CardKeyword)x).ToList();

        options.ParseOptions(_slicedData[4]);
        InitExtraPorperty();

        effectIds = _slicedData[5].ToIntList('/');
        tokenIds = _slicedData[6].ToIntList('/');
    }

    private void InitExtraPorperty() {
        int ArtId = int.Parse(options.Get("artId", id.ToString()));
        ArtworkId = (Type == CardType.Evolved) ? Card.GetBaseId(ArtId) : ArtId;

        NameId = int.Parse(options.Get("nameId", id.ToString()));
        CountLimit = int.Parse(options.Get("limit", "3"));
        countdown = int.Parse(options.Get("countdown", "-1"));
    }

    public Card(Card rhs) {
        id = rhs.id;
        name = rhs.name;
        cost = rhs.cost;
        atk = rhs.atk;
        hp = rhs.hp;
        hpMax = rhs.hpMax;
        traits = rhs.traits.ToList();
        keywords = rhs.keywords.ToList();
        effectIds = new List<int>(rhs.effectIds);
        tokenIds = new List<int>(rhs.tokenIds);
        options = new Dictionary<string, string>(rhs.options);

        description = rhs.description;

        ArtworkId = rhs.ArtworkId;
        NameId = rhs.NameId;
        CountLimit = rhs.CountLimit;
        countdown = rhs.countdown;
        
        SetEffects(rhs.effectIds);
    }

    public void SetEffects(List<int> effectIds, Func<int, Effect> effectFunc = null) {
        effectFunc ??= (x => Effect.Get(x));
        effects = effectIds.Select(effectFunc).Where(x => x != null).ToList();
    }

    public void ClearEffect(int clearId) {
        effectIds.RemoveAll(x => x == clearId);
        effects.RemoveAll(x => x.id == clearId);
    }

    public void ClearEffects(string timing = "all") {
        if (timing == "all") {
            effectIds.Clear();
            effects.Clear();

            SetIdentifier("evolveCost", 1);
            foreach (var property in CardDatabase.PropertyEffects)
                SetIdentifier(property, 0);
        } else {
            effectIds.RemoveAll(x => Effect.Get(x)?.timing == timing);
            effects.RemoveAll(x => x.timing == timing);
        }
    }

    public void SetDescription(string[] _data) {
        name = _data[0];
        description = _data[1].GetDescription((Type == CardType.Leader) ? string.Empty : "（沒有卡片能力記敘）");
    }

    public int GetIdentifier(string id) 
    {
        string trimId;

        if (id.TryTrimStart("trait", out trimId)) {
            if (trimId == string.Empty)
                return traits.Select(x => (int)x).Aggregate((total, next) => total * GameManager.versionData.traitCount + next);

            while (trimId.TryTrimParentheses(out var traitId)) {
                var checkTrait = traitId.Split('|').Select(x => (CardTrait)int.Parse(x));
                if (!checkTrait.Any(traits.Contains))
                    return 0;

                trimId = trimId.TrimStart("[" + traitId + "]");
            }
            return 1;
        }

        if (id.TryTrimStart("keyword", out trimId)) {
            if (trimId == string.Empty)
                return keywords.Select(x => (int)x).Aggregate((total, next) => total * GameManager.versionData.keywordCount + next);

            while (trimId.TryTrimParentheses(out var keywordId)) {
                var checkKeyword = keywordId.Split('|').Select(x => (CardKeyword)int.Parse(x));
                if (!checkKeyword.Any(keywords.Contains))
                    return 0;

                trimId = trimId.TrimStart("[" + keywordId + "]");
            }
            return 1;
        }

        if (id.TryTrimStart("type", out trimId)) {
            if (trimId.TryTrimParentheses(out var typeId)) {
                var checkType = typeId.Split('|').Select(x => (CardType)int.Parse(x));
                return checkType.Contains(Type) ? 1 : 0;
            }
        }

        return id switch {
            "uid" => this.id,
            "id" => NameId,
            "group" => GroupId,
            "zone" => ZoneId,
            "pack" => PackId,
            "craft" => CraftId,
            "type" => TypeId,
            "rarity" => RarityId,
            "serial" => SerialNum,

            "initCost" => Card.Get(this.id).cost,
            "initAtk" => Card.Get(this.id).atk,
            "initHp" => Card.Get(this.id).hp,

            "cost" => cost,
            "atk" => atk,
            "hp" => hp,
            "hpMax" => hpMax,
            "countdown" => countdown,
            "evolveCost" => int.Parse(options.Get("evolveCost", "1")),
            "maxAttackChance" => int.Parse(options.Get("maxAttackChance", "1")),
            _ => int.Parse(options.Get(id, "0")),
        };
    }

    public bool TryGetIdenfier(string id, out int value)
    {
        value = GetIdentifier(id);
        return true;
    }

    public void SetIdentifier(string id, int value)
    {
        options.Set(id, value.ToString());
    }

    public bool IsFollower() => (Type == CardType.Follower) || (Type == CardType.Evolved);
    public bool IsFormat(GameFormat format) {
        int newPackId = GameManager.versionData.NewPackIds[ZoneId];
        int zonePackId = PackId % 100;
        return format switch {
            GameFormat.Rotation => (zonePackId == 0) ||
                zonePackId.IsWithin(newPackId - 4, newPackId),
            GameFormat.TwoPick => (zonePackId == 0) ||
                zonePackId.IsWithin(newPackId - 3, newPackId),
            _ => true,
        };
    }
}

public struct CardStatus 
{
    public int cost, atk, hp;

    public CardStatus(int cost, int atk, int hp) {
        this.cost = cost;
        this.atk = atk;
        this.hp = hp;
    }

    public bool IsEmpty() => (cost == 0) && (atk == 0) && (hp == 0);
}
