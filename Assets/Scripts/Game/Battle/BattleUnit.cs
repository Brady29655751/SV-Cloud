using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleUnit : IIdentifyHandler
{
    public int id;
    public string name;
    public int turn = 0;
    public Leader leader;
    public BattleDeck deck;
    public BattleField field;
    public BattleHand hand;
    public BattleGrave grave;

    public Queue<BattleCard> targetQueue;

    public bool isDone = false;
    public bool isMyTurn = false;
    public bool isFirst;

    public bool IsMasterUnit => id == 0;
    public string IsFirstText => isFirst ? "先手" : "後手";
    public bool isEvolveEnabled => (turn - (isFirst ? 1 : 0)) >= Player.currentBattle.Settings.evolveStart;

    public int Id => id;

    public BattleUnit(int unitId, string nickname, BattleDeck initDeck, bool first) {
        id = unitId;
        name = nickname;
        turn = 0;

        leader = new Leader(first, initDeck.craft);
        deck = initDeck;
        field = new BattleField();
        hand = new BattleHand();
        grave = new BattleGrave();

        targetQueue = new Queue<BattleCard>();

        isFirst = first;
        isDone = false;
        isMyTurn = first;

        Draw(3, out _, out _);
    }

    public BattleUnit(BattleUnit rhs) {
        id = rhs.id;
        name = rhs.name;
        turn = rhs.turn;

        leader = new Leader(rhs.leader);
        deck = new BattleDeck(rhs.deck);
        field = new BattleField(rhs.field);
        hand = new BattleHand(rhs.hand);
        grave = new BattleGrave(rhs.grave);

        targetQueue = new Queue<BattleCard>(rhs.targetQueue);

        isFirst = rhs.isFirst;
        isDone = rhs.isDone;
        isMyTurn = rhs.isMyTurn;
    }

    /// <summary>
    /// Draw cards.
    /// </summary>
    /// <returns>Total cards drawn, including inHand and inGrave.</returns>    
    public List<BattleCard> Draw(int count, out List<BattleCard> inHand, out List<BattleCard> inGrave) {
        return Draw(count, new BattleCardFilter(-1), out inHand, out inGrave);
    }
    
    public List<BattleCard> Draw(int count, BattleCardFilter filter, out List<BattleCard> inHand, out List<BattleCard> inGrave) {
        var availableCount = hand.MaxCount - hand.Count;
        var result = deck.cards.Where(filter.FilterWithCurrentCard).Take(count).ToList();

        if (result.Count > availableCount) {
            inHand = result.GetRange(0, availableCount);
            inGrave = result.GetRange(availableCount, result.Count - availableCount);
        } else {
            inHand = (result.Count == 0) ? new List<BattleCard>() : result.GetRange(0, result.Count);
            inGrave = new List<BattleCard>();
        }

        inGrave.ForEach(x => x.SetIdentifier("graveReason", (int)BattleCardGraveReason.DrawTooMuch));
        inGrave.ForEach(x => x.SetIdentifier("graveIsMyTurn", isMyTurn ? 1 : 0));
        inGrave.ForEach(x => x.SetIdentifier("graveTurn", turn));

        hand.cards.AddRange(inHand);
        grave.cards.AddRange(inGrave);
        deck.cards.RemoveAll(result.Contains);

        leader.AddIdentifier("turn_draw_cards", inHand.Count);

        return result;
    }

    public bool TryGetIdenfier(string id, out int value)
    {
        value = GetIdentifier(id);
        return value == int.MinValue;
    }

    public int GetIdentifier(string id)
    {
        var prefix = id.Split('.', '[' );
        var place = GetPlace(prefix[0].ToBattlePlace());
        if (place != null)
            return place.GetIdentifier(id.TrimStart(prefix[0]).TrimStart('.'));

        return id switch {
            "id"        => Id,
            "isFirst"   => isFirst ? 1 : 0,
            "isMyTurn"  => isMyTurn ? 1 : 0,
            "isDone"    => isDone ? 1 : 0,
            "isAwake"   => leader.GetIdentifier("isAwake"),
            "isVenge"   => leader.GetIdentifier("isVenge"),
            "isEager"   => leader.GetIdentifier("isEager"),
            "isChase"   => leader.GetIdentifier("isChase"),
            "isRed"     => leader.GetIdentifier("isRed"),
            "isGreedy"  => (hand.Count >= 7) ? 1 : 0,
            "isReson"   => 1 - (deck.Count % 2),
            _ => int.MinValue,
        };
    }

    public void SetIdentifier(string id, int value)
    {
        var prefix = id.Split('.');
        var place = GetPlace(prefix[0].ToBattlePlace());
        if (place != null)
            place.SetIdentifier(id.TrimStart(prefix[0] + "."), value);
    }

    public BattlePlace GetPlace(BattlePlaceId id) {
        return id switch {
            BattlePlaceId.Deck      => deck,
            BattlePlaceId.Hand      => hand,
            BattlePlaceId.Leader    => leader,
            // BattlePlaceId.Territory => territory,
            BattlePlaceId.Field     => field,
            BattlePlaceId.Grave     => grave,
            _ => null,
        };
    }

    public BattlePlace GetBelongPlace(BattleCard card) {
        for (var placeId = BattlePlaceId.Deck; placeId <= BattlePlaceId.Grave; placeId++) {
            var place = GetPlace(placeId);
            if ((place != null) && (place.Contains(card)))
                return place;
        }
        return null;
    }
    
}