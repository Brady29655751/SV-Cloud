using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortalSearchView : IMonoBehaviour
{
    private string[] inputFieldType => new string[] { "name", "trait", "keyword", "description" };

    [SerializeField] private GameObject detailSearchPanel;
    [SerializeField] private IInputField nameInputField, traitInputField, keywordInputField, descriptionInputField;
    [SerializeField] private List<Outline> formatOutlines;
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
        int indent = which switch {
            "pack" => 100,
            "type" => 1,
            "rarity" => 1,
            _ => 0,
        };

        for (int i = 0; i < list.Count; i++) {
            list[i].color = notChosen;
        }

        for (int i = 0; i < items.Count; i++) {
            int index = items[i] - indent;
            list[index].color = chosen;
        }
    }
    
}
