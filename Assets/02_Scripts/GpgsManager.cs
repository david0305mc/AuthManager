using Cysharp.Threading.Tasks;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GpgsManager : Singleton<GpgsManager>
{
    public string Session { get; set; }

    private ISavedGameMetadata savedGameMetaData;

    public void InitializeGPGS()
    {
        var config = new PlayGamesClientConfiguration.Builder()
         .RequestIdToken()
         .Build();
        PlayGamesPlatform.InitializeInstance(config);

        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
    }

    public async UniTask<bool> SignIn()
    {
        if (PlayGamesPlatform.Instance.IsAuthenticated())
            PlayGamesPlatform.Instance.SignOut();

        UniTaskCompletionSource<bool> ucs = new UniTaskCompletionSource<bool>();
        Social.localUser.Authenticate(ret =>
        {
            ucs.TrySetResult(ret);
        });

        return await ucs.Task;
    }

    public void SignOut()
    {
        if (PlayGamesPlatform.Instance.IsAuthenticated())
            PlayGamesPlatform.Instance.SignOut();
    }
    public async UniTask LoadGame(bool isLogin = false)
    {
        Debug.Log("LoadGame0");
        UniTaskCompletionSource ucs = new UniTaskCompletionSource();
        if (PlayGamesPlatform.Instance.SavedGame == null)
        {
            Debug.LogError("PlayGamesPlatform.Instance.SavedGame == null");
        }
        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;
        Debug.Log("LoadGame1");
        string fileName = "testFile";
        saveGameClient.OpenWithAutomaticConflictResolution(fileName,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLastKnownGood,
            (status, data) =>
            {
                savedGameMetaData = data;
                if (status == SavedGameRequestStatus.Success)
                {
                    Debug.Log("!! Load Success");

                    // ������ �ε�
                    saveGameClient.ReadBinaryData(savedGameMetaData, (status, loadedData) =>
                    {
                        if (status == SavedGameRequestStatus.Success)
                        {

                            string utfString = System.Text.Encoding.UTF8.GetString(loadedData);
                            Debug.Log($"Read Success data {utfString}");
                            if (utfString != string.Empty)
                            {
                                var serverData = JsonUtility.FromJson<BaseData>(utfString);
                                if (!isLogin)
                                {
                                    if (Session != serverData.session)
                                    {

#if UNITY_EDITOR
                                        UnityEditor.EditorApplication.isPlaying = false;
#else
                                            // ���� ����� �ۿ��� �� ����
                                            Application.Quit();
#endif
                                        return;
                                    }
                                }

                                // ���ð� ���� ��

                                if (UserDataManager.Instance.baseData.dbVersion < serverData.dbVersion)
                                {
                                    UserDataManager.Instance.baseData = Utill.CopyAll(serverData);
                                }
                                AuthTest.Instance.UpdateUI();
                                //UserDataManager.Instance.baseData.gold.Value = baseData.gold.Value;
                                // �ҷ��� �����͸� ���� ó�����ִ� �κ� �ʿ�!    
                            }
                        }
                        else
                        {
                            Debug.Log($"Read Failed Status {status} {status.ToString()}");
                        }
                        ucs.TrySetResult();
                    });
                }
                else
                {
                    Debug.Log("?? no");
                }
            });
        await ucs.Task;
    }

    public async UniTask<bool> SaveGame()
    {
        UniTaskCompletionSource<bool> ucs = new UniTaskCompletionSource<bool>();
        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;

        string fileName = "testFile";
        UserDataManager.Instance.baseData.dbVersion = GameTime.Get();
        if (savedGameMetaData.IsOpen)
        {
            Debug.Log("Save Success");
            var update = new SavedGameMetadataUpdate.Builder().Build();

            //json
            var json = JsonUtility.ToJson(UserDataManager.Instance.baseData);
            byte[] data = Encoding.UTF8.GetBytes(json);

            // ���� �Լ� ����    
            saveGameClient.CommitUpdate(savedGameMetaData, update, data, (status2, gameData2) =>
            {
                if (status2 == SavedGameRequestStatus.Success)
                {
                    // ����Ϸ�κ�    
                    Debug.Log($"Save Data Success {json}");
                    ucs.TrySetResult(true);
                }
                else
                {
                    ucs.TrySetResult(false);
                    Debug.Log($"Save Data Failed {json}");
                }
            });
        }
        else
        {
            // ������ ����
            saveGameClient.OpenWithAutomaticConflictResolution(fileName,
                DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLastKnownGood,
                (status, gameData) =>
                {
                    savedGameMetaData = gameData;
                    if (status == SavedGameRequestStatus.Success)
                    {
                        Debug.Log("Save Success");
                        var update = new SavedGameMetadataUpdate.Builder().Build();

                        //json
                        var json = JsonUtility.ToJson(UserDataManager.Instance.baseData);
                        byte[] data = Encoding.UTF8.GetBytes(json);

                        // ���� �Լ� ����    
                        saveGameClient.CommitUpdate(savedGameMetaData, update, data, (status2, gameData2) =>
                        {
                            if (status2 == SavedGameRequestStatus.Success)
                            {
                                // ����Ϸ�κ�    
                                Debug.Log($"Save Data Success {json}");
                                ucs.TrySetResult(true);
                            }
                            else
                            {
                                ucs.TrySetResult(false);
                                Debug.Log($"Save Data Failed {json}");
                            }
                        });
                    }
                    else
                    {
                        Debug.Log("Save No.....");
                    }
                });
        }

        return await ucs.Task;

    }
}