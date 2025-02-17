using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleRecordInfoView : IMonoBehaviour
{
    [SerializeField] private Text opNameText;
    [SerializeField] private Image formatImage;
    [SerializeField] private Text formatText;
    [SerializeField] private Image myCraftEmblemImage, opCraftEmblemImage;
    [SerializeField] private Text myCraftText, opCraftText;
    [SerializeField] private Text dateText, resultText;
    private BattleRecord currentRecord;

    public void SetBattleRecord(BattleRecord record) {
        currentRecord = record;
        if (record == null)
            return;

        var opName = record.isMaster ? record.settings.clientName : record.settings.masterName;
        var myCraft = record.isMaster ? record.masterDeck.craft : record.clientDeck.craft;
        var opCraft = record.isMaster ? record.clientDeck.craft : record.masterDeck.craft;
        var myEmblem = myCraft == 0 ? SpriteResources.GetCardGemSprite(0) : SpriteResources.GetCardEmblemSprite(myCraft);
        var opEmblem = opCraft == 0 ? SpriteResources.GetCardGemSprite(0) : SpriteResources.GetCardEmblemSprite(opCraft);

        opNameText?.SetText(opName);
        opNameText?.SetFontSize(26 - Mathf.Max(opName.Length, 10));
        formatImage?.SetSprite(record.settings.format.GetFormatSprite());
        formatText?.SetText("<color=#ffbb00>【" + record.settings.zone.GetZoneName() + "】</color>" + record.settings.format.GetFormatName());
        myCraftEmblemImage?.SetSprite(myEmblem);
        opCraftEmblemImage?.SetSprite(opEmblem);
        myCraftText?.SetText(((CardCraft)myCraft).GetCraftName());
        opCraftText?.SetText(((CardCraft)opCraft).GetCraftName());
        opCraftText?.SetColor(((CardCraft)opCraft).GetCraftColor());
        dateText?.SetText(record.date.ToString("yyyy/MM/dd HH:mm"));
        resultText?.SetText(record.GetRecordResultState().ToString().ToUpper());
        resultText?.SetColor(record.GetRecordResultState().GetResultColor());
    }

    public void PlayRecord() { 
        Player.currentBattleRecord = currentRecord;
        
        var settings = new BattleSettings(currentRecord.settings) { isLocal = true };
        var masterDeck = currentRecord.masterDeck;
        var clientDeck = currentRecord.clientDeck;
        
        Battle battle = new Battle(masterDeck, clientDeck, settings);
        SceneLoader.instance.ChangeScene(SceneId.Battle);
    }

}
