using Cysharp.Threading.Tasks;
using Firebase.Auth;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using System.Text;
using System.Threading;
using UnityEngine;

public class GPGSTest : Singleton<GPGSTest>
{
    public FirebaseAuth Auth { get; set; }
    
    public string Session { get; set; }

    private ISavedGameMetadata savedGameMetaData;
    public void InitializeFirebase()
    {
        Debug.Log("[Firebase] Setting up Firebase Auth");
        
        Auth = FirebaseAuth.DefaultInstance;
    }

    public void InitializeGPGS()
    {
        var config = new PlayGamesClientConfiguration.Builder()
         .RequestIdToken()
         .Build();
        PlayGamesPlatform.InitializeInstance(config);

        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
     
    }

    private async UniTask<Credential> GetGoogleCredential()
    {
        if (PlayGamesPlatform.Instance.IsAuthenticated())
            PlayGamesPlatform.Instance.SignOut();

        UniTaskCompletionSource ucs = new UniTaskCompletionSource();
        Social.localUser.Authenticate(ret =>
        {
            if (!ret)
            {
                ucs.TrySetCanceled();
                return;
            }

            ucs.TrySetResult();
        });

        await ucs.Task;
        return GoogleAuthProvider.GetCredential(((PlayGamesLocalUser)Social.localUser).GetIdToken(), null);
    }

    public async UniTask SignInWithPlatform(EPlatform _platform, CancellationTokenSource _cts)
    {
        Debug.Log("SignInWithPlatform 01");

        if (PlayGamesPlatform.Instance.localUser.authenticated)
        {
            Debug.Log("authenticated");
        }

        try
        {
            switch (_platform)
            {
                case EPlatform.Google:
                    {
                        Credential credential = await GetGoogleCredential();
                        await Auth.SignInWithCredentialAsync(credential).AsUniTask().AttachExternalCancellation(_cts.Token);
                    }
                    break;
                case EPlatform.Guest:
                    await Auth.SignInAnonymouslyAsync().AsUniTask().AttachExternalCancellation(_cts.Token);
                    break;
                default:
                    await Auth.SignInAnonymouslyAsync().AsUniTask().AttachExternalCancellation(_cts.Token);
                    break;
            }
        }
        catch(System.Exception e)
        {
            Debug.LogError($"Error {e.Message}");
        }
       
        string token = await Auth.CurrentUser.TokenAsync(true).AsUniTask().AttachExternalCancellation(_cts.Token);
        Debug.Log($"token {token}");
    }
  

    public void SignOut()
    {
        if (PlayGamesPlatform.Instance.IsAuthenticated())
            PlayGamesPlatform.Instance.SignOut();
        Auth.SignOut();
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

                        // 데이터 로드
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
                                        Utill.QuitApp();
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
        if (savedGameMetaData.IsOpen)
        {
            Debug.Log("Save Success");
            var update = new SavedGameMetadataUpdate.Builder().Build();

            //json
            var json = JsonUtility.ToJson(UserDataManager.Instance.baseData);
            byte[] data = Encoding.UTF8.GetBytes(json);

            // 저장 함수 실행    
            saveGameClient.CommitUpdate(savedGameMetaData, update, data, (status2, gameData2) =>
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
                    savedGameMetaData = gameData;
                    if (status == SavedGameRequestStatus.Success)
                    {
                        Debug.Log("Save Success");
                        var update = new SavedGameMetadataUpdate.Builder().Build();

                    //json
                    var json = JsonUtility.ToJson(UserDataManager.Instance.baseData);
                        byte[] data = Encoding.UTF8.GetBytes(json);

                    // 저장 함수 실행    
                    saveGameClient.CommitUpdate(savedGameMetaData, update, data, (status2, gameData2) =>
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
