using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using TMPro;
using Cysharp.Threading.Tasks;

public class AuthTest : SingletonMono<AuthTest>
{

    [SerializeField] private TextMeshProUGUI goldText;

    protected override void OnSingletonAwake()
    {
        base.OnSingletonAwake();
        UserDataManager.Instance.baseData = new BaseData();
        GPGSTest.Instance.Session = Utill.GenerateAesSessionKey();
    }

    public void UpdateUI()
    {
        goldText.SetText(UserDataManager.Instance.baseData.gold.ToString());
    }
    private void Start()
    {
        GPGSTest.Instance.InitializeFirebase();
        GPGSTest.Instance.InitializeGPGS();
    }
    public void OnClickLogin()
    {
        CancellationTokenSource cts = new CancellationTokenSource();

        UniTask.Create(async () =>
        {
            await GameTime.InitGameTime();
            LoadLocalData();
            await GPGSTest.Instance.SignInWithPlatform(EPlatform.Google, cts);
            await GPGSTest.Instance.LoadGame(true);
            UserDataManager.Instance.baseData.session = GPGSTest.Instance.Session;
            await GPGSTest.Instance.SaveGame();
        });
    }

    public void OnClickAdd()
    {
        //string data = "Hello, World!";
        //string session = Utill.GenerateAesSessionKey();
        //Debug.Log($"Original Data: {data} {session}");
        //return;

        BaseData baseData = new BaseData();
        baseData.gold.Value = UserDataManager.Instance.baseData.gold.Value + 1;
        UserDataManager.Instance.baseData = Utill.CopyAll(baseData);
        Debug.Log($"UserDataManager.Instance.baseData {UserDataManager.Instance.baseData.gold}");
        UpdateUI();
        SaveLocalData();
        //UserDataManager.Instance.baseData.gold.Value++;
    }

    public void OnClickLoad()
    {
        GPGSTest.Instance.LoadGame();
    }

    public void OnClickSave()
    {
        UniTask.Create(async () =>
        {
            await GPGSTest.Instance.LoadGame();
            GPGSTest.Instance.SaveGame();
        });
    }

    public void OnClickSignOut()
    {
        GPGSTest.Instance.SignOut();
    }

    private void SaveLocalData()
    {
        UserDataManager.Instance.SaveLocalData();
    }

    private void LoadLocalData()
    {
        UserDataManager.Instance.LoadLocalData(0);
        UpdateUI();
    }
}
