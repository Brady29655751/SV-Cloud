using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainManager : Manager<MainManager>
{
    [SerializeField] private List<GameObject> normalObject, debugObject;
    protected override void Start() {
        normalObject.ForEach(x => x.SetActive(!GameManager.instance.debugMode));
        debugObject.ForEach(x => x.SetActive(GameManager.instance.debugMode));
        AudioSystem.instance.PlayMusic(AudioResources.Main);
    }

    public void BackToTitleScene() {
        SceneLoader.instance.ChangeScene(SceneId.Title);
    }

    public void GoToPortalScene() {
        SceneLoader.instance.ChangeScene(SceneId.Portal);
    }
}
