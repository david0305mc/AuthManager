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
   

    public void SaveLocalData(bool _updateVersion = true)
    {
        if (_updateVersion)
        {
            baseData.dbVersion = GameTime.Get();
        }
        var saveData = JsonUtility.ToJson(baseData);
        //saveData = Utill.EncryptXOR(saveData);
        Utill.SaveFile(LocalFilePath, saveData);
    }
    public void LoadLocalData(string _uid)
    {
        if (File.Exists(LocalFilePath))
        {
            var localData = Utill.LoadFromFile(LocalFilePath);
            //localData = Utill.EncryptXOR(localData);
            baseData = JsonUtility.FromJson<BaseData>(localData);
            if (baseData.userUID != _uid)
            {
                Debug.Log("CreateNewUser 0");
                CreateNewUser(_uid);
            }
            //LocalData.UpdateRefData();
        }
        else
        {
            Debug.Log("CreateNewUser 1");
            CreateNewUser(_uid);
        }
        Debug.Log($"load Local {baseData.userUID}");
    }

    public void CreateNewUser(string _uid)
    {
        // NewGame
        baseData = new BaseData();
        //LocalData.UpdateRefData();
        //LoadDefaultData();
        baseData.level = 1;
        baseData.userUID = _uid;
        baseData.dbVersion = 0;

        SaveLocalData(false);
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

public class BaseData : SData
{
    public long dbVersion;
    public string userUID;
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
