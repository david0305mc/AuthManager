using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using TMPro;
using UniRx;
public class AuthTest : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI goldText;
    private void Awake()
    {
        UserDataManager.Instance.baseData = new BaseData();
        UserDataManager.Instance.baseData.gold.Subscribe(_value =>
        {
            goldText.SetText(_value.ToString());
        });
    }
    private void Start()
    {
        GPGSTest.Instance.InitializeFirebase();
        GPGSTest.Instance.InitializeGPGS();
    }
    public void OnClickLogin()
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        GPGSTest.Instance.SignInWithPlatform(EPlatform.Google, cts);
    }

    public void OnClickAdd()
    {
        UserDataManager.Instance.baseData.gold.Value++;
    }

    public void OnClickLoad()
    {
        GPGSTest.Instance.LoadGame();
    }

    public void OnClickSave()
    {
        GPGSTest.Instance.SaveGame();
    }

    public void OnClickSignOut()
    {
        GPGSTest.Instance.SignOut();
    }
}
