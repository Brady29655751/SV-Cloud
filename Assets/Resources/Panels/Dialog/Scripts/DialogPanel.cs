using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DialogPanel : Panel
{
    [SerializeField] private Image dialogBackgroundImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text contentText;


    private Dictionary<int, Dialog> dialogDict;
    private int currentDialogIndex = 1;
    private Dialog currentDialog => dialogDict.Get(currentDialogIndex);

    public void SetStory(Dictionary<int, Dialog> dialogDict) {
        this.dialogDict = dialogDict;
        currentDialogIndex = 1;
        SetDialog(currentDialog);
    }

    public void SetStory(string filePath) {
        ResourceManager.instance.LoadDialogInfo(filePath, SetStory);    
    }

    public void SetDialog(Dialog dialog) {
        if (dialog == null) {
            ClosePanel();
            return;
        }

        if (dialog.Background != null)
            dialogBackgroundImage?.SetSprite(dialog.Background);
            
        if (dialog.BackgroundColor != null)
            dialogBackgroundImage?.SetColor(dialog.BackgroundColor);

        if (dialog.BGM != null)
            AudioSystem.instance.PlayMusic(dialog.BGM);

        if (dialog.Name != null)
            nameText?.SetText(dialog.Name);
            
        contentText?.SetText(dialog.Content);
    }

    public void Next() {
        if (currentDialog?.next == null) {
            currentDialogIndex++;
            SetDialog(currentDialog);
            return;
        }
    }
}
