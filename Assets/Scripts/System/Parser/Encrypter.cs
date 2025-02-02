using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

public static class Encrypter
{
    public static string CryptoKey => "ShadowverseCloud";
    public static string Base64Dict => "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

    public static string IntToBase64String(int SourceInt) {
        string result = "";
        if (SourceInt == 0)
            return "A";
            
        while (SourceInt > 0) {
            var q = SourceInt / 64;
            var r = SourceInt % 64;
            result = Base64Dict[r] + result;
            SourceInt = q;
        }
        return result;
    } 

    public static int Base64StringToInt(string SourceStr) {
        int result = 0;
        for (int i = SourceStr.Length - 1; i >= 0; i--) {
            var c = SourceStr[i];
            var index = Base64Dict.IndexOf(c);
            if (index < 0)
                return -1;

            result += index * (int)Math.Pow(64, SourceStr.Length - 1 - i);
        }
        return result;
    }

    /// <summary>
    /// 字串加密(非對稱式)
    /// </summary>
    /// <param name="Source">加密前字串</param>
    /// <param name="CryptoKey">加密金鑰</param>
    /// <returns>加密後字串</returns>
    public static string AESEncryptBase64(string SourceStr)
    {
        string encrypt = "";
        try
        {
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
            byte[] key = sha256.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));
            byte[] iv = md5.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));
            aes.Key = key;
            aes.IV = iv;
            byte[] dataByteArray = Encoding.UTF8.GetBytes(SourceStr);
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(dataByteArray, 0, dataByteArray.Length);
                cs.FlushFinalBlock();
                encrypt = Convert.ToBase64String(ms.ToArray());
            }
        }
        catch (Exception)
        {
            RequestManager.OnRequestFail("加密失敗");
        }
        return encrypt;
    }

    /// <summary>
    /// 字串解密(非對稱式)
    /// </summary>
    /// <param name="Source">解密前字串</param>
    /// <param name="CryptoKey">解密金鑰</param>
    /// <returns>解密後字串</returns>
    public static string AESDecryptBase64(string SourceStr)
    {
        string decrypt = "";
        try
        {
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
            byte[] key = sha256.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));
            byte[] iv = md5.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));
            aes.Key = key;
            aes.IV = iv;
            byte[] dataByteArray = Convert.FromBase64String(SourceStr);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(dataByteArray, 0, dataByteArray.Length);
                    cs.FlushFinalBlock();
                    decrypt = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }
        catch (Exception)
        { 
            RequestManager.OnRequestFail("解密失敗");
        }
        return decrypt;
    }

}