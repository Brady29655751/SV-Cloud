using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using Unity.IO.LowLevel.Unsafe;

public class TitleManager : Manager<TitleManager>
{
    [SerializeField] private InputHintbox inputHintbox;

    private bool IsLoading = false;

    protected override void Start() {
        AudioSystem.instance.PlayMusic(AudioResources.Title);
    }

    public void GameStart() {
        if (IsLoading)
            return;

        if (GameManager.versionData == null) {
            RequestManager.OnRequestFail("正在獲取版本資料，請稍後再進入遊戲");
            return;
        }

        if (GameManager.versionData.IsEmpty()) {
            RequestManager.OnRequestFail("獲取版本資料失敗，請重新啟動遊戲");
            return;
        }

        if (GameManager.versionData.buildVersion != Application.version) {
            IsLoading = true;
            RequestManager.OnRequestFail("檢測到新版本，正在獲取更新檔案大小\n請稍候");
            RequestManager.instance.GetDownloadSize(GameManager.gameDownloadUrl, OpenUpdateBuildHintbox);
            return;
        }

        if (!DatabaseManager.instance.VerifyData(out string error)) {
            RequestManager.OnRequestFail(error);
            return;
        }

        if (Player.gameData.IsEmpty()) {
            OpenNicknamePanel();
            return;
        }

        SceneLoader.instance.ChangeScene(SceneId.Main);
    }

    private void OpenNicknamePanel() {
        void SetNickname(string name) {
            Player.gameData.SetNickname(name);
            GameStart();
        }

        inputHintbox.SetHintboxActive(true);
        inputHintbox.SetOptionCallback(SetNickname);
    }

    private void OpenUpdateBuildHintbox(long size) {
        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");

        if (size == -1) {                
            hintbox.SetContent("檢測到新版本，但獲取檔案大小失敗\n請稍後再試");
            hintbox.SetOptionNum(1);
            return;
        }

        hintbox.SetContent("檢測到新版本。點擊確認選擇下載位置。\n建議預留 " + (size / 1_000_000) + " MB 以上空間。\n下載後舊版本可自行刪除。");
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(OpenSaveFileBrowser, true);
        hintbox.SetOptionCallback(OnCancel, false);
    }

    private void OpenSaveFileBrowser() {
        var downloadUrl = GameManager.gameDownloadUrl;
        var filter = new FileBrowser.Filter("Game", ".apk");
        FileBrowser.SetFilters(false, filter);
        FileBrowser.ShowSaveDialog(OnSuccess, OnCancel, FileBrowser.PickMode.Files, initialFilename: "SVCloud");
    }

    private void OnSuccess(string[] paths) {
        SceneLoader.instance.loadingScreen.SetActive(true);

        var loadingText = SceneLoader.instance.loadingText;
        var loadingSlider = SceneLoader.instance.loadingSlider;
        
        loadingText?.SetText("正在下載新版本");

        RequestManager.instance.Download(GameManager.gameDownloadUrl, paths[0], 
            () => { 
                Hintbox.OpenHintbox("下載完成，請關閉遊戲並解壓縮新檔案\n舊版本可自行刪除");
                SceneLoader.instance.loadingScreen.SetActive(false);
            }, 
            (error) => {
                RequestManager.OnRequestFail("下載失敗，請重新啟動\n錯誤：" + error);
                SceneLoader.instance.loadingScreen.SetActive(false);
            },
            (progress) => {
                loadingText?.SetText(Mathf.Clamp(Mathf.CeilToInt(progress * 100), 0, 100) + " %");
                loadingSlider.value = progress;
            }
        );
    }

    private void OnCancel() {
        IsLoading = false;
    }
}
