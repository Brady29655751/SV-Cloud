using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortalSearchController : IMonoBehaviour
{
    [SerializeField] private PortalSearchModel searchModel;
    [SerializeField] private PortalSearchView searchView;

    public event Action<CardFilter> onSearchEvent;

    public void Search() {
        onSearchEvent?.Invoke(searchModel.filter);
    }

    public void Clear() {
        searchModel.Clear();
        searchView.ClearInputField();
        SetFormat(1);
        SelectCraft(-1);
        SelectPack(-1);
        SelectType(-1);
        SelectRarity(-1);
        SelectCost(-1);
        SelectAtk(-1);
        SelectHp(-1);
        SetToken(false);
    }

    public void ClearName() {
        searchView.SetInputField("name", string.Empty);
    }

    public void SetDetailSearchPanelActive(bool active) {
        searchView.SetDetailSearchPanelActive(active);
    }

    public void SetFormat(int formatId) {
        searchModel.SetInt("format", formatId);
        searchView.SetFormat(formatId);
    }

    public void SelectCraft(int craft) {
        searchModel.SelectInt("craft", craft);
        searchView.SetImageList("craft", searchModel.filter.craftList, Color.white, Color.gray);
    }

    public void SelectPack(int pack) {
        searchModel.SelectInt("pack", pack);
        searchView.SetImageList("pack", searchModel.filter.packList, ColorHelper.chosen, Color.black);
    }

    public void SelectType(int type) {
        searchModel.SelectInt("type", type);
        searchView.SetImageList("type", searchModel.filter.typeList, ColorHelper.chosen, Color.black);
    }

    public void SelectRarity(int rarity) {
        searchModel.SelectInt("rarity", rarity);
        searchView.SetImageList("rarity", searchModel.filter.rarityList, ColorHelper.chosen, Color.black);
    }

    public void SelectCost(int cost) {
        searchModel.SelectInt("cost", cost);
        searchView.SetImageList("cost", searchModel.filter.costList, Color.white, Color.gray);
    }

    public void SelectAtk(int atk) {
        searchModel.SelectInt("atk", atk);
        searchView.SetImageList("atk", searchModel.filter.atkList, Color.white, Color.gray);
    }

    public void SelectHp(int hp) {
        searchModel.SelectInt("hp", hp);
        searchView.SetImageList("hp", searchModel.filter.hpList, Color.white, Color.gray);
    }

    public void SetName(string name) {
        searchModel.SetString("name", name);
    }

    public void SetTrait(string trait) {
        searchModel.SelectInt("trait", (int)trait.ToCardTrait());
    }

    public void SetKeyword(string keyword) {
        searchModel.SelectInt("keyword", (int)keyword.ToCardKeyword());
    }

    public void SetDescription(string description) {
        searchModel.SetString("description", description);
    }

    public void SetToken(bool token) {
        searchModel.SetBool("token", token);
    }
}
