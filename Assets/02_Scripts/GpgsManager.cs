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
    public static readonly string SessionString = "Session";
    public static readonly string UserDataString = "UserData";
    public string Session { get; set; }

    private ISavedGameMetadata gameMetaData;

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
        {
            //PlayGamesPlatform.Instance.SignOut();
            return true;
        }
        
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

    public async UniTask<(SavedGameRequestStatus, string)> LoadData(string _fileName)
    {
        UniTaskCompletionSource<(SavedGameRequestStatus, string)> ucs = new UniTaskCompletionSource<(SavedGameRequestStatus, string)>();
        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;
        Debug.Log($"Load _fileName {_fileName}");
        saveGameClient.OpenWithAutomaticConflictResolution(_fileName,
            DataSource.ReadNetworkOnly,
            ConflictResolutionStrategy.UseLastKnownGood,
            (status, data) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    Debug.Log("!! Load Success");
                    // 데이터 로드
                    saveGameClient.ReadBinaryData(data, (readStatus, loadedData) =>
                    {
                        if (readStatus == SavedGameRequestStatus.Success)
                        {
                            string utfString = System.Text.Encoding.UTF8.GetString(loadedData);
                            Debug.Log($"Read Success data {utfString}");
                            ucs.TrySetResult((readStatus, utfString));
                        }
                        else
                        {
                            ucs.TrySetResult((readStatus, string.Empty));
                            Debug.Log($"Read Failed Status {readStatus}");
                        }
                    });
                }
                else
                {
                    Debug.Log("Load Failed");
                    ucs.TrySetResult((status, string.Empty));
                }
            });
        return await ucs.Task;
    }
    public async UniTask<bool> SaveData(string _fileName, string _json)
    {
        UniTaskCompletionSource<bool> ucs = new UniTaskCompletionSource<bool>();
        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;
        // 데이터 접근
        saveGameClient.OpenWithAutomaticConflictResolution(_fileName,
            DataSource.ReadNetworkOnly,
            ConflictResolutionStrategy.UseLastKnownGood,
            (status, _gameMetaData) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    var update = new SavedGameMetadataUpdate.Builder().Build();
                        //json    
                    //var json = JsonUtility.ToJson(UserDataManager.Instance.baseData);
                    byte[] data = Encoding.UTF8.GetBytes(_json);
                        // 저장 함수 실행        
                    saveGameClient.CommitUpdate(_gameMetaData, update, data, (status2, gameData2) =>
                    {
                        if (status2 == SavedGameRequestStatus.Success)
                        {    
                            // 저장완료부분        
                            Debug.Log($"Save Data Success {_json}");
                            ucs.TrySetResult(true);
                        }
                        else
                        {
                            ucs.TrySetResult(false);
                            Debug.Log($"Save Data Failed {_json}");
                        }
                    });
                }
                else
                {
                    ucs.TrySetResult(false);
                    Debug.Log("Save No.....");
                }
            });
        return await ucs.Task;
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
                gameMetaData = data;
                if (status == SavedGameRequestStatus.Success)
                {
                    Debug.Log("!! Load Success");

                    // 데이터 로드
                    saveGameClient.ReadBinaryData(gameMetaData, (status, loadedData) =>
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
                                            // 실제 빌드된 앱에서 앱 종료
                                            Application.Quit();
#endif
                                        return;
                                    }
                                }

                                // 로컬과 버전 비교

                                if (UserDataManager.Instance.baseData.dbVersion < serverData.dbVersion)
                                {
                                    UserDataManager.Instance.baseData = Utill.CopyAll(serverData);
                                }
                                AuthTest.Instance.UpdateUI();
                                //UserDataManager.Instance.baseData.gold.Value = baseData.gold.Value;
                                // 불러온 데이터를 따로 처리해주는 부분 필요!    
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
        if (gameMetaData.IsOpen)
        {
            Debug.Log("Save Success");
            var update = new SavedGameMetadataUpdate.Builder().Build();

            //json
            var json = JsonUtility.ToJson(UserDataManager.Instance.baseData);
            byte[] data = Encoding.UTF8.GetBytes(json);

            // 저장 함수 실행    
            saveGameClient.CommitUpdate(gameMetaData, update, data, (status2, gameData2) =>
            {
                if (status2 == SavedGameRequestStatus.Success)
                {
                    // 저장완료부분    
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
            // 데이터 접근
            saveGameClient.OpenWithAutomaticConflictResolution(fileName,
                DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLastKnownGood,
                (status, gameData) =>
                {
                    gameMetaData = gameData;
                    if (status == SavedGameRequestStatus.Success)
                    {
                        Debug.Log("Save Success");
                        var update = new SavedGameMetadataUpdate.Builder().Build();

                        //json
                        var json = JsonUtility.ToJson(UserDataManager.Instance.baseData);
                        byte[] data = Encoding.UTF8.GetBytes(json);

                        // 저장 함수 실행    
                        saveGameClient.CommitUpdate(gameMetaData, update, data, (status2, gameData2) =>
                        {
                            if (status2 == SavedGameRequestStatus.Success)
                            {
                                // 저장완료부분    
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
