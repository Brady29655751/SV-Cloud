using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BattleState
{
    public BattleSettings settings;
    public BattleResult result;
    public Effect currentEffect;

    public BattleUnit masterUnit, clientUnit;
    protected bool isMasterTurn = false;
    public bool IsMasterTurn { 
        get => isMasterTurn;
        set => SetMasterTurn(value);
    }

    public BattleUnit currentUnit => isMasterTurn ? masterUnit : clientUnit;
    public BattleUnit myUnit => settings.isLocal ? masterUnit : (PhotonNetwork.IsMasterClient ? masterUnit : clientUnit);
    public BattleUnit opUnit => settings.isLocal ? clientUnit : (PhotonNetwork.IsMasterClient ? clientUnit : masterUnit);

    public BattleState(BattleDeck masterDeck, BattleDeck clientDeck, BattleSettings settings) {
        this.settings = settings;
        this.result = new BattleResult();

        isMasterTurn = Random.Range(0, 2) == 0;

        masterUnit = new BattleUnit(0, settings.masterName , masterDeck, isMasterTurn);
        clientUnit = new BattleUnit(1, settings.clientName , clientDeck, !isMasterTurn);
    }

    public BattleState(BattleState rhs) {
        isMasterTurn = rhs.isMasterTurn;
        settings = new BattleSettings(rhs.settings);
        result = new BattleResult(rhs.result);
        currentEffect = new Effect(rhs.currentEffect);
        masterUnit = new BattleUnit(rhs.masterUnit);
        clientUnit = new BattleUnit(rhs.clientUnit);
    }

    public virtual BattleUnit GetUnitById(int id) {
        if (masterUnit.id == id)
            return masterUnit;
        if (clientUnit.id == id)
            return clientUnit;
        return null;
    }
    public virtual BattleUnit GetRhsUnitById(int lhsId) {
        return GetUnitById(1 - lhsId);
    }

    public virtual void SetMasterTurn(bool newTurn) {
        isMasterTurn = newTurn;
        masterUnit.isMyTurn = isMasterTurn;
        clientUnit.isMyTurn = !isMasterTurn;
    }

    public virtual BattleCardPlaceInfo GetCardPlaceInfo(BattleCard card) {
        var unit = GetBelongUnit(card);

        if (unit == null)
            return new BattleCardPlaceInfo();

        var place = unit.GetBelongPlace(card);
        var index = place?.cards.IndexOf(card) ?? 0;  

        return new BattleCardPlaceInfo(){
            unitId = (unit.id == myUnit.id) ? 0 : 1,
            place = place?.PlaceId ?? BattlePlaceId.None,
            index = index,
        };
    }

    public virtual BattleUnit GetBelongUnit(BattleCard card) {
        for (var placeId = BattlePlaceId.Deck; placeId <= BattlePlaceId.Grave; placeId++) {
            var myPlace = myUnit.GetPlace(placeId);
            var opPlace = opUnit.GetPlace(placeId);
            if ((myPlace != null) && myPlace.Contains(card))
                return myUnit;
            
            if ((opPlace != null) && opPlace.Contains(card))
                return opUnit;
        }
        return null;
    }

    public virtual void RemoveUntilEffect() {
        for (var placeId = BattlePlaceId.Deck; placeId <= BattlePlaceId.Grave; placeId++) {
            var lhsPlace = currentUnit.GetPlace(placeId);
            var rhsPlace = GetRhsUnitById(currentUnit.id).GetPlace(placeId);

            lhsPlace?.cards.ForEach(x => x.RemoveUntilEffect());
            rhsPlace?.cards.ForEach(x => x.RemoveUntilEffect());
        }
    }

}
