using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Events;
using System.IO.Compression;


public static class DataManager
{
    public static string saveDirectory = "DataCache";
    public static string ToPlayfabJson<T>(T data)
    {
        return JsonConvert.SerializeObject(data, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
    }

    // generics
    #region New

    // T = any
    public static void SaveData<T>(T data, string prefKey)
    {
        //SaveDataToDisk(data, baseDir, fileName);
        SaveDataToPrefs(data, prefKey);
    }

    // T = any
    public static T LoadData<T>(string prefKey)
    {
        //return LoadDataFromDisk<T>(baseDir, fileName);
        return LoadDataFromPrefs<T>(prefKey);
    }

    private static T LoadDataFromPrefs<T>(string prefKey)
    {
        string prefContents = PlayerPrefs.GetString(prefKey);
        return FromJson<T>(prefContents);
    }

    private static void SaveDataToPrefs<T>(T data, string prefKey)
    {
        PlayerPrefs.SetString(prefKey, ToJson(data));
    }

    public static void SaveDataToDisk<T>(T data, string baseDir, string fileName = "Dummy_Data.json")
    {
#if UNITY_EDITOR
        Debug.LogFormat("Saving data to {0}", Path.Combine(GetFullPath(baseDir), fileName));
#endif
        if (!Directory.Exists(GetFullPath(baseDir)))
        {
            Directory.CreateDirectory(GetFullPath(baseDir));
        }

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(Path.Combine(GetFullPath(baseDir), fileName), json);
    }

    public static T LoadDataFromDisk<T>(string baseDir, string fileName = "Dummy_Data.json")
    {
#if UNITY_EDITOR
        Debug.LogFormat("Loading data from {0}", Path.Combine(GetFullPath(baseDir), fileName));
#endif
        string path = Path.Combine(GetFullPath(baseDir), fileName);
        if (File.Exists(path))
        {
            string data = File.ReadAllText(path);
            return FromJson<T>(data);
        }
        return default;
    }

    public static bool DataExists(string baseDir, string fileName = "Dummy_Data.json")
    {
        string path = Path.Combine(GetFullPath(baseDir), fileName);
        if (File.Exists(path))
        {
            return true;
        }
        return false;
    }

    #endregion

    #region Converters
    public static List<T> JsonToList<T>(string json)
    {
        return JsonConvert.DeserializeObject<List<T>>(json);
    }

    public static string ListToJson<T>(List<T> list, Formatting f = Formatting.None)
    {
        return JsonConvert.SerializeObject(list, f, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
    }

    public static T FromJson<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }

    public static string ToJson<T>(T data, Formatting f = Formatting.None)
    {
        return JsonConvert.SerializeObject(data, f, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
    }

    #endregion

    #region Compression Helpers

    public static string Compressed(string s)
    {
        byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(s);
        using (var outputStream = new MemoryStream())
        {
            using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
                gZipStream.Write(inputBytes, 0, inputBytes.Length);

            var outputBytes = outputStream.ToArray();
            return Convert.ToBase64String(outputBytes);
        }
    }

    public static string Decompressed(string s)
    {
        byte[] inputBytes = Convert.FromBase64String(s);

        using (var inputStream = new MemoryStream(inputBytes))
        using (var gZipStream = new GZipStream(inputStream, CompressionMode.Decompress))
        using (var outputStream = new MemoryStream())
        {
            gZipStream.CopyTo(outputStream);
            var outputBytes = outputStream.ToArray();

            return System.Text.Encoding.UTF8.GetString(outputBytes);
        }
    }

    #endregion

    #region Disk Operations

    public static string GetFullPath(string dir)
    {
        string path = "";
        path = Path.Combine(Application.persistentDataPath, Path.Combine(saveDirectory, dir));
        return path;
    }

    public static string ReadJsonToString(string directory, string fileName)
    {
        string fullPath = Path.Combine(GetFullPath(directory), fileName);
        if (File.Exists(fullPath))
        {
            StreamReader sr = new StreamReader(fullPath);
            string contents = sr.ReadToEnd();
            sr.Close();
            return contents;
        }
        else
        {
            return null;
        }
    }

    public static void DeleteFile(string directory, string fileName)
    {
        string fullPath = Path.Combine(GetFullPath(directory), fileName);
        if (fullPath != null)
        {
            File.Delete(fullPath);
        }
    }

    public static void DeleteAllFilesInDir(string directory)
    {
        DirectoryInfo info = new DirectoryInfo(GetFullPath(directory));
        foreach (FileInfo fi in info.GetFiles())
        {
            File.Delete(fi.FullName);
        }
    }

    static FileInfo[] GetAllFilesContent(string directory)
    {
        if (!Directory.Exists(GetFullPath(saveDirectory)))
        {
            Directory.CreateDirectory(GetFullPath(saveDirectory));
        }
        DirectoryInfo info = new DirectoryInfo(GetFullPath(directory));
        FileInfo[] fileInfo = info.GetFiles().OrderBy(prop => prop.CreationTime).ToArray();
        return fileInfo;

    }

    public static List<string> GetJsonStringsFromAllFiles(string directory)
    {
        List<string> jsons = new List<string>();
        foreach (FileInfo fi in GetAllFilesContent(directory))
        {
            if (fi.Extension.Contains("json"))
            {
                jsons.Add(ReadJsonToString(directory, fi.Name));
            }
        }
        return jsons;
    }

    /* HEX/Byte converter methods in case of need, not used now */
    public static string ByteArrayToHex(byte[] ba)
    {
        string hex = BitConverter.ToString(ba);
        return hex.Replace("-", "");
    }

    public static byte[] StringToByteArray(String hex)
    {
        int NumberChars = hex.Length;
        byte[] bytes = new byte[NumberChars / 2];
        for (int i = 0; i < NumberChars; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        return bytes;
    }

    public static string RemoveWhitespace(string input)
    {
        return new string(input.ToCharArray()
            .Where(c => !Char.IsWhiteSpace(c))
            .ToArray());
    }

    #endregion



}


