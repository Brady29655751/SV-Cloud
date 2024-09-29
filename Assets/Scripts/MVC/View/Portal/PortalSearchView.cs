using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortalSearchView : IMonoBehaviour
{
    private string[] inputFieldType => new string[] { "name", "trait", "keyword", "description" };

    [SerializeField] private GameObject detailSearchPanel;
    [SerializeField] private IInputField nameInputField, traitInputField, keywordInputField, descriptionInputField;
    [SerializeField] private List<Outline> formatOutlines, zoneOutlines;
    [SerializeField] private List<Image> craftImages, packImages, typeImages, rarityImages;
    [SerializeField] private List<Image> costImages, atkImages, hpImages;

    public void SetDetailSearchPanelActive(bool active) {
        detailSearchPanel?.SetActive(active);
    }

    public void ClearInputField() {
        SetInputField("all", string.Empty);
    }

    public void SetFormat(int format) {
        if (!format.IsInRange(0, formatOutlines.Count))
            return;

        foreach (var outline in formatOutlines)
            outline.effectColor = Color.clear;

        formatOutlines[format].effectColor = Color.cyan;
    }

    public void SetZone(int zone) {
        for (int i = 0; i < packImages.Count; i++) {
            var pack = (CardPack)(zone * 100 + i);
            packImages[i].gameObject.SetActive(i <= GameManager.versionData.NewPackIds[zone]);
            packImages[i].GetComponentInChildren<Text>()?.SetText(pack.GetPackName());
        }
        
        if (List.IsNullOrEmpty(zoneOutlines))
            return;

        if (!(zone - 1).IsInRange(0, zoneOutlines.Count))
            return;

        foreach (var outline in zoneOutlines)
            outline.effectColor = Color.clear;

        zoneOutlines[zone - 1].effectColor = Color.cyan;
    }

    public void SetInputField(string which, string input) {
        if (which == "all") {
            for(int i = 0; i < inputFieldType.Length; i++)
                SetInputField(inputFieldType[i], input);

            return;
        }

        var ipf = which switch {
            "name" => nameInputField,
            "trait" => traitInputField,
            "keyword" => keywordInputField,
            "description" => descriptionInputField,
            _ => null,
        };

        ipf?.SetInputString(input);
    }

    public void SetImageList(string which, List<int> items, Color chosen, Color notChosen) {
        var list = which switch {
            "craft" => craftImages,
            "pack" => packImages,
            "type" => typeImages,
            "rarity" => rarityImages,
            "cost" => costImages,
            "atk" => atkImages,
            "hp" => hpImages,
            _ => null,
        };
        Func<int, int> indent = which switch {
            "pack" => (x) => x % 100,
            "type" => (x) => x - 1,
            "rarity" => (x) => x - 1,
            _ => (x) => x,
        };

        for (int i = 0; i < list.Count; i++) {
            list[i].color = notChosen;
        }

        for (int i = 0; i < items.Count; i++) {
            int index = indent.Invoke(items[i]);
            list[index].color = chosen;
        }
    }
}
