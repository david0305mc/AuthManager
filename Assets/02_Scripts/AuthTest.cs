using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using TMPro;
using Cysharp.Threading.Tasks;
using GooglePlayGames;

public class AuthTest : SingletonMono<AuthTest>
{

    [SerializeField] private TextMeshProUGUI goldText;

    protected override void OnSingletonAwake()
    {
        base.OnSingletonAwake();
        UserDataManager.Instance.baseData = new BaseData();
        GpgsManager.Instance.Session = Utill.GenerateAesSessionKey();
    }

    public void UpdateUI()
    {
        goldText.SetText(UserDataManager.Instance.baseData.gold.ToString());
    }
    private void Start()
    {
        GpgsManager.Instance.InitializeGPGS();
        //AdManager.Instance.InitAD();
        //IapManager.Instance.InitializePurchasing();
    }
    public void OnClickLogin()
    {
        CancellationTokenSource cts = new CancellationTokenSource();

        UniTask.Create(async () =>
        {
            if (!await GameTime.InitGameTime())
            {
                Debug.LogError("DisConnect Network");
                return;
            }
            
            bool success = await GpgsManager.Instance.SignIn();
            if (success)
            {
                if (!await GpgsManager.Instance.SaveData(GpgsManager.SessionString, GpgsManager.Instance.Session))
                {
                    Debug.LogError("Save Filed");
                    return;
                }

                UserDataManager.Instance.LoadLocalData(PlayGamesPlatform.Instance.GetUserId());
                var result = await GpgsManager.Instance.LoadData(GpgsManager.UserDataString);
                if (result.Item1 != GooglePlayGames.BasicApi.SavedGame.SavedGameRequestStatus.Success)
                {
                    Debug.LogError("Load Filed");
                    return;
                }

                if (!string.IsNullOrEmpty(result.Item2))
                {
                    var serverData = JsonUtility.FromJson<BaseData>(result.Item2);
                    if (serverData.userUID != UserDataManager.Instance.baseData.userUID)
                    {
                        Debug.LogError($"serverData.userUID != UserDataManager.Instance.baseData.userUID {serverData.userUID}");
                        //serverData.userUID = UserDataManager.Instance.baseData.userUID;
                    }

                    if (UserDataManager.Instance.baseData.dbVersion < serverData.dbVersion)
                    {
                        UserDataManager.Instance.baseData = Utill.CopyAll(serverData);
                        Debug.Log("dbVersion < serverData.dbVersion");
                    }
                    else
                    {
                        Debug.Log("dbVersion > serverData.dbVersion");
                    }
                }
                else
                {
                    Debug.Log("new User");
                }

                UpdateUI();

                //await GpgsManager.Instance.LoadGame(true);
                //UserDataManager.Instance.baseData.session = GpgsManager.Instance.Session;
                //await GpgsManager.Instance.SaveGame();
            }
            else
            {
                Debug.LogError("GPGS Login Failed");
            }
        });
    }

    public void OnClickAdd()
    {
        //string data = "Hello, World!";
        //string session = Utill.GenerateAesSessionKey();
        //Debug.Log($"Original Data: {data} {session}");
        //return;

        UserDataManager.Instance.baseData.gold.Value = UserDataManager.Instance.baseData.gold.Value + 1;
        UpdateUI();
        UserDataManager.Instance.SaveLocalData();
        //UserDataManager.Instance.baseData.gold.Value++;
    }

    public void OnClickDelete()
    {
        UniTask.Create(async () =>
        {
            if (await GpgsManager.Instance.DeleteServerData(GpgsManager.UserDataString))
            {
                Utill.QuitApp();
            }
        });    
    }

    public void ShowAD()
    {
        //UniTask.Create(async () => {
        //    if (await AdManager.Instance.ShowRewardAD())
        //    {
        //        Debug.Log("RewardAD Completed");
        //    }
        //});    
    }

    public void OnClickSave()
    {
        UniTask.Create(async () =>
        {
            var result = await GpgsManager.Instance.LoadData(GpgsManager.SessionString);
            if (result.Item1 != GooglePlayGames.BasicApi.SavedGame.SavedGameRequestStatus.Success)
            {
                return;
            }

            if (result.Item2 != GpgsManager.Instance.Session)
            {
                Debug.LogError("Invalid Session");
                Utill.QuitApp();
                return;
            }

            UserDataManager.Instance.baseData.dbVersion = GameTime.Get();
            await GpgsManager.Instance.SaveData(GpgsManager.UserDataString, JsonUtility.ToJson(UserDataManager.Instance.baseData));
            //await GpgsManager.Instance.LoadGame();
            //GpgsManager.Instance.SaveGame();

            //await GpgsManager.Instance.LoadGame();
        });
    }

    public void OnClickPurchase()
    {
        UniTask.Create(async () =>
        {
            //await IapManager.Instance.BuyProduct(IapManager.testProductID);
            Debug.Log("OnClickPurchase Complete");
        });
        
    }

    public void OnClickSignOut()
    {
        GpgsManager.Instance.SignOut();
        Utill.QuitApp();
    }


}
