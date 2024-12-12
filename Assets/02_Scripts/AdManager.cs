//using GoogleMobileAds.Api;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//using Cysharp.Threading.Tasks;


//public class AdManager : Singleton<AdManager>
//{

//    private string testUnitId = "ca-app-pub-3940256099942544/5224354917";

//    private List<string> UnitIDList = new List<string>()
//    {
//        "ca-app-pub-9673687584530511/1232081459",
//        "ca-app-pub-9673687584530511/7605918112",
//        "ca-app-pub-9673687584530511/8317861411",
//        "ca-app-pub-9673687584530511/7004779742",
//        "ca-app-pub-9673687584530511/8619616887",
//    };

//    private Queue<RewardedAd> adQueue = new Queue<RewardedAd>();
//    private bool isInit = false;
//    public void InitAD()
//    {
//        if (!isInit)
//        {
//            MobileAds.Initialize(
//                initStatus => {
//                    isInit = true;
//                    Debug.Log("MobileAds.Initialize Complete");
//                    InitAdQueue();
//                });
//        }
//    }

//    private void InitAdQueue()
//    {
//        UniTask.Create(async () =>
//        {
//#if DEV
//            for (int i = 0; i < 3; i++)
//            {
//                await LoadRewardedAd(testUnitId);
//            }
//#else
//            for (int i = 0; i < UnitIDList.Count; i++)
//            {
//                await LoadRewardedAd(UnitIDList[i]);
//            }
//#endif
//        });
//    }
//    public async UniTask LoadRewardedAd(string adID)
//    {
//        UniTaskCompletionSource<RewardedAd> ucs = new UniTaskCompletionSource<RewardedAd>();
//        var adRequest = new AdRequest();
//        RewardedAd.Load(adID, adRequest, (RewardedAd ad, LoadAdError error) =>
//        {
//            if (error != null || ad == null)
//            {
//                Debug.LogError("Rewarded ad failed to load an ad " + "with error : " + error);
//                ucs.TrySetResult(null);
//                return;
//            }
            
//            Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());

//            ucs.TrySetResult(ad);
//        });

//        var result = await ucs.Task;
//        if (result != null)
//        {
//            if (result.CanShowAd())
//            {
//                adQueue.Enqueue(result);
//                RegisterReloadHandler(result);
                
//                Debug.Log($"Enqueue {adQueue.Count}");
//            }
//            else
//            {
//                Debug.LogError("LoadRewardedAd Fail Can not Show");
//                result.Destroy();
//                await UniTask.Yield();
//                await LoadRewardedAd(adID);
//            }
//        }
//        else
//        {
//            Debug.LogError("LoadRewardedAd Fail ad null");
//            await UniTask.Yield();
//            await LoadRewardedAd(adID);
//        }
//    }

//    public async UniTask<bool> ShowRewardAD()
//    {
//        UniTaskCompletionSource<bool> ucs = new UniTaskCompletionSource<bool>();
//        try
//        {
//            if (adQueue.Count > 0)
//            {
//                var rewardedAD = adQueue.Dequeue();
//                rewardedAD.Show(reward =>
//                {
//                    ucs.TrySetResult(true);
//                });
//            }
//            else
//            {
//                Debug.Log("adQueue.Count == 0");
//                ucs.TrySetResult(false);
//            }
//        }
//        catch
//        {
//            ucs.TrySetResult(false);
//        }
        
//        return await ucs.Task;
//    }

//    private void RegisterReloadHandler(RewardedAd ad)
//    {
//        ad.OnAdFullScreenContentClosed += () =>
//        {
//            Debug.Log("Rewarded Ad full screen content closed.");

//            // Reload the ad so that we can show another as soon as possible.
//            LoadRewardedAd(ad.GetAdUnitID());
//            ad.Destroy();
//        };
//        ad.OnAdFullScreenContentFailed += (AdError error) =>
//        {
//            Debug.LogError("Rewarded ad failed to open full screen content " +
//                           "with error : " + error);

//            // Reload the ad so that we can show another as soon as possible.
//            LoadRewardedAd(ad.GetAdUnitID());
//            ad.Destroy();
//        };
//    }


//}
