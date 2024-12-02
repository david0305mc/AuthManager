using System.Collections;
using System.Collections.Generic;
using System.IO;
using UniRx;
using UnityEngine;

public class UserDataManager : Singleton<UserDataManager>
{
    private static readonly string LocalFilePath = Path.Combine(Application.persistentDataPath, "SaveData");
    private static readonly string LocalVersionPath = Path.Combine(Application.persistentDataPath, "VersionData");
    public BaseData baseData { get; set; } 
    public InventoryData inventoryData { get; set; } = new InventoryData();
    public DBVersion dbVersion { get; set; }


    public void SaveLocalData()
    {
        {
            var saveData = JsonUtility.ToJson(baseData);
            //saveData = Utill.EncryptXOR(saveData);
            Utill.SaveFile(LocalFilePath, saveData);
        }

        {
            var saveData = JsonUtility.ToJson(dbVersion);
            //saveData = Utill.EncryptXOR(saveData);
            Utill.SaveFile(LocalVersionPath, saveData);
        }
    }
    public void LoadLocalData(ulong _uno)
    {
        if (File.Exists(LocalFilePath))
        {
            var localData = Utill.LoadFromFile(LocalFilePath);
            //localData = Utill.EncryptXOR(localData);
            baseData = JsonUtility.FromJson<BaseData>(localData);
            if (baseData.UNO != _uno)
            {
                CreateNewUser(_uno);
            }
            //LocalData.UpdateRefData();
        }
        else
        {
            CreateNewUser(_uno);
        }

        if (File.Exists(LocalVersionPath))
        {
            var localData = Utill.LoadFromFile(LocalVersionPath);
            dbVersion = JsonUtility.FromJson<DBVersion>(localData);
        }
        else
        {
            dbVersion = new DBVersion();
            dbVersion.dbVersion = 0;
        }
    }

    public void CreateNewUser(ulong _uno)
    {
        // NewGame
        baseData = new BaseData();
        //LocalData.UpdateRefData();
        //LoadDefaultData();
        baseData.level = 1;
        baseData.UNO = _uno;

        dbVersion = new DBVersion();
        dbVersion.dbVersion = 0;

        SaveLocalData();
    }

}


public class SData
{ 
    public void GetClassName()
    {
        Debug.Log(GetType().Name);
    }
    public T ConvertToObject<T>(string _saveData)
    {
        return JsonUtility.FromJson<T>(_saveData);
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}

public class DBVersion : SData
{
    public long dbVersion;
}
public class BaseData : SData
{
    public ulong UNO;
    public IntReactiveProperty gold = new IntReactiveProperty();
    public int level;
    
    public SerializableDictionary<int, int> dicTest = new SerializableDictionary<int, int>();

    public void AddGold()
    {
        gold.Value++;
    }
    public void AddDicTest(int add)
    {
        GetClassName();
        if (dicTest.ContainsKey(1))
        {
            dicTest[1] = dicTest[1] + add;
        }
        else
        {
            dicTest[1] = add;
        }
        
    }
}

public class InventoryData : SData
{
    public List<int> itemList = new List<int>();
    public void AddItem()
    {
        GetClassName();
        itemList.Add(Random.Range(0, 1000));
    }
}
