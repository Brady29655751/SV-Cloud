using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine {

public static class Utility {
    public static int lastScreenWidth, lastScreenHeight;
    public static Vector2Int GetScreenSize() {
        return new Vector2Int(Screen.width, Screen.height);
    }
    
    public static void SetScreenSize(int width, int height) {
        Screen.SetResolution(width, height, FullScreenMode.Windowed);
    }

    public static void InitScreenSizeWithRatio(float widthRatio, float heightRatio) {
        var screen = GetScreenSize();
        var width = screen.x;
        var height = screen.y;
        var screenRatio = screen.x / screen.y;
        var currentRatio = widthRatio / heightRatio;
        if (screenRatio > currentRatio) {
            width = (int)(screen.y / heightRatio * widthRatio);
        } else if (screenRatio < currentRatio) {
            height = (int)(screen.x / widthRatio * heightRatio);
        }
        SetScreenSize(width, height);
    }
}

}
