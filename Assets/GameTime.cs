using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

public class GameTime 
{
    private static long initGameTime = 0;
    private static float initClientStartupTime = 0;

    private static void InitLocalBase()
    {
        Init((DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds);
    }
    public static async UniTask InitGameTime()
    {
        string dateTimeString = await GetNetworkTime();

        if (string.IsNullOrEmpty(dateTimeString))
        {
            InitLocalBase();
        }
        else
        {
            Debug.Log($"date {dateTimeString}");
            Init((DateTime.Parse(dateTimeString) - DateTime.UnixEpoch).TotalSeconds);
        }
    }
    public static async UniTask<string> GetNetworkTime()
    {
        using (var request = UnityWebRequest.Get("www.google.com"))
        {
            var webRequest = await request.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"result.isNetworkError");
                return string.Empty;
            }
            else
            {
                return request.GetResponseHeader("date");

            }

        }
    }

    private static void Init(double _gameTime)
    {
        initGameTime = Convert.ToInt64(_gameTime);
        initClientStartupTime = Time.realtimeSinceStartup;
        //Debug.LogFormat("[GameTime/Set] {0}", Get(DateTimeKind.Utc));
    }

    public static long Get()
    {
        if (initGameTime == 0)
        {
            Init((DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds);
        }

        return initGameTime + Convert.ToInt64(Time.realtimeSinceStartup - initClientStartupTime);
    }

    //public static long GetLocalMidnight()
    //{
    //    DateTime localTime = Utill.ConvertFromUnixTimestamp(Get()).ToLocalTime();
    //    Debug.Log($"Korea {localTime}");
    //    DateTime midNightTime = new DateTime(localTime.Year, localTime.Month, localTime.Day + 1);
    //    Debug.Log($"Korea midnight {midNightTime}");
    //    return Convert.ToInt64(Utill.ConvertToUnitxTimeStamp(midNightTime));
    //}

}
