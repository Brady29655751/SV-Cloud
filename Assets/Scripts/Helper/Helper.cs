using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace System {
    
public static class StringHelper {
    public static string TrimEmpty(this string str, bool trimNewline = true) {
        string result = str.Replace(" ", string.Empty).Replace("\t", string.Empty);
        if (trimNewline)
            return result.Replace("\r", string.Empty).Replace("\n", string.Empty);

        var index = result.FindAllIndex(x => x != '\n');
        return (index.Length == 0) ? string.Empty : result.Substring(index.Min(), index.Max() - index.Min() + 1);
    } 
    public static string TrimEnd(this string str, string suffix) {
        if (str == suffix)
            return string.Empty;
        return str.EndsWith(suffix) ? str.Substring(0, str.Length - suffix.Length) : str;
    }
    public static bool TryTrimEnd(this string str, string suffix, out string trim) {
        trim = TrimEnd(str, suffix);
        return str.EndsWith(suffix);
    }
    public static string TrimStart(this string str, string prefix) {
        if (str == prefix)
            return string.Empty;
        return str.StartsWith(prefix) ? str.Substring(prefix.Length) : str;
    }
    public static bool TryTrimStart(this string str, string prefix, out string trim) {
        trim = TrimStart(str, prefix);
        return str.StartsWith(prefix);
    }
    public static string TrimParentheses(this string str, string parentheses = "[]") {
        int startIndex = str.IndexOf(parentheses[0]);
        int endIndex = str.IndexOf(parentheses[1]);
        if ((startIndex == -1) || (endIndex == -1) || (endIndex < startIndex))
            return str;
        return str.Substring(startIndex + 1, endIndex - startIndex - 1);
    }
    public static bool TryTrimParentheses(this string str, out string trim, string parentheses = "[]") {
        trim = str.TrimParentheses(parentheses);
        return trim != str;
    }

    public static string GetDescription(this string data, string defaultReturn = null) {
        var desc = data.TrimEnd('\n', '\t', '\r', ' ').Replace("[ENDL]", "\n");

        if (desc == "none")
            return defaultReturn;

        return desc.Replace("[-]", "</color>").Replace("[", "<color=#").Replace("]", ">");
    }

    public static string ConcatToString(this IEnumerable<string> data, string lineEnd = "\n", bool trimEnd = true) {
        string result = string.Empty;
        foreach (var str in data)
            result += str + lineEnd;

        if (trimEnd)
            result = result.TrimEnd(lineEnd);
        
        return result;
    }

    public static string ToStringWithSign(this int num) {
        return ((num < 0) ? string.Empty : "+") + num.ToString();
    }

    /// <summary>
    /// Count length without rich-text tags.
    /// </summary>
    public static int GetPlainLength(this string str) {
        string trimStr = str.Trim();
        return trimStr.Length - ((trimStr.Length - trimStr.Replace("color", string.Empty).Length) / 10) * 23;
    }
    public static Vector2 GetPreferredSize(this string str, int maxCharX, int fontSize, int padX = 21, int padY = 21) {
        string[] split = str.Trim().Split('\n');
        int lineSizeX = -1;
        int line = split.Length;

        for (int i = 0; i < split.Length; i++) {
            int length = split[i].GetPlainLength();
            line += (length - 1) / maxCharX;
            lineSizeX = Mathf.Min(Mathf.Max(lineSizeX, length), maxCharX);
        }
        
        int sizeX = padX + (fontSize) * lineSizeX;
        int sizeY = padY + (fontSize + 5) * line;

        return new Vector2(sizeX, sizeY);
    }
    /// <summary>
    /// Parse the given string to list of float. <br/>
    /// We expect input to be "xxx,yyy,zzz....."
    /// </summary>
    public static List<float> ToFloatList(this string str, char delimeter = ',') {
        str = str.TrimEnd(delimeter).TrimEnd();

        if (string.IsNullOrEmpty(str) || (str == "none"))
            return new List<float>();

        var split = str.Split(delimeter);

        List<float> list = new List<float>();

        for (int i = 0; i < split.Length; i++) {
            if (float.TryParse(split[i], out float tmp)) {
                list.Add(tmp);
                continue;
            }
            return null;
        }
        return list;
    }
    public static List<int> ToIntList(this string str, char delimeter = ',') {
        str = str.TrimEnd(delimeter).TrimEnd();

        if (string.IsNullOrEmpty(str) || (str == "none"))
            return new List<int>();

        var split = str.Split(delimeter);

        List<int> list = new List<int>();

        for (int i = 0; i < split.Length; i++) {
            if (int.TryParse(split[i], out int tmp)) {
                list.Add(tmp);
                continue;
            }
            return null;
        }
        return list;
    }
    /// <summary>
    /// Parse the given string to Vector2. <br/>
    /// We expect input to be "xxx,yyy"
    /// </summary>
    public static Vector2 ToVector2(this string pos, Vector2 defaultValue = default(Vector2)) {
        var list = pos?.ToFloatList();
        return ((list == null) || (list.Count != 2)) ? defaultValue : new Vector2(list[0], list[1]);
    }
    /// <summary>
    /// Parse the given string to Vector4. <br/>
    /// We expect input to be "xxx,yyy,zzz,www"
    /// </summary>
    public static Vector4 ToVector4(this string pos, Vector4 defaultValue = default(Vector4)) {
        var list = pos?.ToFloatList();
        return ((list == null) || (list.Count != 4)) ? defaultValue : new Vector4(list[0], list[1], list[2], list[3]);
    }
    /// <summary>
    /// Parse the given string to Color. <br/>
    /// We expect input to be "rrr,ggg,bbb,aaa"
    /// </summary>
    public static Color ToColor(this string color, Color defaultValue = default(Color)) {
        var list = color?.ToFloatList();
        return ((list == null) || (list.Count != 4)) ? defaultValue : new Color(list[0], list[1], list[2], list[3]);
    }
    /// <summary>
    /// Parse the given string to Quaternion. <br/>
    /// We expect input to be "xxx,yyy,zzz"
    /// </summary>
    public static Quaternion ToQuaternion(this string rotation, Quaternion defaultValue = default(Quaternion)) {
        var q = Quaternion.identity;
        var list = rotation?.ToFloatList();
        q.eulerAngles = ((list == null) || (list.Count != 3)) ? defaultValue.eulerAngles : new Vector3(list[0], list[1], list[2]);
        return q;
    }
}

}
