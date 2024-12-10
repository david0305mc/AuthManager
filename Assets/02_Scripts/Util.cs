using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Security.Cryptography;

public partial class Utill
{
    public static T MakeGameObject<T>(string objectname, Transform parent) where T : MonoBehaviour
    {
        return MakeGameObject(objectname, parent).AddComponent<T>();
    }
    public static GameObject MakeGameObject(string objectname, Transform parent)
    {
        GameObject TempObject = new GameObject(objectname);
        if (parent != null)
        {
            TempObject.transform.SetParent(parent);
        }
        return TempObject;
    }

    public static T InstantiateGameObject<T>(GameObject prefab, Transform parent = null) where T : MonoBehaviour
    {
        return InstantiateGameObject(prefab, parent).GetComponent<T>();
    }
    public static GameObject InstantiateGameObject(GameObject prefab, Transform parent = null)
    {
        var go = GameObject.Instantiate(prefab, parent, false);
        go.name = prefab.name;
        return go;
    }

    public static void QuitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public static string LoadFromFile(string filePath)
    {
        string text = default;
        if (File.Exists(filePath))
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                }
            }
        }
        return text;
    }

    public static void SaveFile(string filePath, string data)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        {
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.Write(data);
            }
        }
    }

    public static async UniTask<string> LoadFromFileAsync(string filePath)
    {
        string text;
        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                text = await sr.ReadToEndAsync();
            }
        }
        return text;
    }

    public static async void SaveFileAsync(string filePath, string data)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        {
            using (StreamWriter writer = new StreamWriter(fs))
            {
               await  writer.WriteAsync(data);
            }
        }
    }

    private static readonly string EncryptionCode = "David";
    public static string EncryptXOR(string data)
    {
        string modifiedData= default;

        for (int i = 0; i < data.Length; i++)
        {
            modifiedData += (char)(data[i] ^ EncryptionCode[i % EncryptionCode.Length]);
        }
        return modifiedData;
    }

    public static T Load<T>(string path) where T : UnityEngine.Object
    {
        return Resources.Load<T>(path);
    }
    public static T CopyAll<T>(T source) where T : new()
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        T target = new T();
        Type type = typeof(T);

        // 필드 복사
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            object value = field.GetValue(source);
            field.SetValue(target, value);
        }

        // 속성 복사
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (PropertyInfo property in properties)
        {
            if (property.CanWrite)
            {
                object value = property.GetValue(source);
                property.SetValue(target, value);
            }
        }

        return target;
    }
    public static string ComputeMD5Hash(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // 바이트 배열을 16진수 문자열로 변환
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }

    public static string GenerateAesSessionKey()
    {
        using (var aes = Aes.Create())
        {
            aes.GenerateKey();
            return Convert.ToBase64String(aes.Key);
        }
    }
}

public static class RaycastUtilities
{
    public static bool PointerIsOverUI(Vector2 screenPos)
    {
        var hitObject = UIRaycast(ScreenPosToPointerData(screenPos));
        return hitObject != null && hitObject.layer == LayerMask.NameToLayer("UI");
    }

    public static GameObject UIRaycast(PointerEventData pointerData)
    {
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        return results.Count < 1 ? null : results[0].gameObject;
    }

    static PointerEventData ScreenPosToPointerData(Vector2 screenPos)
       => new(EventSystem.current) { position = screenPos };
    public static GameObject UIRaycast(PointerEventData pointerData, int layerMask)
    {
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var item in results)
        {
            if (item.gameObject.layer == layerMask)
            {
                return item.gameObject;
            }
        }
        return null;
    }

}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = UnityEngine.JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.data;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.data = array;
        return JsonUtility.ToJson(wrapper);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] data;
    }

}



public static class PathInfo
{
    private static string _dataPath = string.Empty;
    public static string DataPath
    {
        get
        {
            if (_dataPath == string.Empty)
            {
                StringBuilder sb = new StringBuilder();

                if (Application.isEditor == true)
                {
                    sb.Append(Application.dataPath);
                    sb.Append("/../Cache");
                }
                else
                {
                    sb.Append(Application.persistentDataPath);
                }

                _dataPath = sb.ToString();
            }

            return _dataPath;
        }
    }
}