using System.Collections;
using System.Collections.Generic;
using UnityEngine.Purchasing;
using UnityEngine;
using UnityEngine.Purchasing.Extension;
using Cysharp.Threading.Tasks;

public class IapManager : Singleton<IapManager>, IDetailedStoreListener
{
    private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;

    public static readonly string testProductID = "gem_01_60";
    private UniTaskCompletionSource<bool> purchaseUcs;

    public void InitializePurchasing()
    {
        if (IsInitialized())
        {
            return;
        }
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(testProductID, ProductType.Consumable); // Consumable, NonConsumable, Subscription ¡ﬂ º±≈√
        UnityPurchasing.Initialize(this, builder);

    }
    private bool IsInitialized()
    {
        return storeController != null && storeExtensionProvider != null;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.LogError($"OnPurchaseFailed: {failureDescription.reason} {failureDescription.message}");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"OnInitializeFailed: {error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"OnInitializeFailed: {error} {message}");
    }

    public async UniTask<bool> BuyProduct(string productID)
    {
        if (!IsInitialized())
        {
            Debug.LogError("Purchasing is not initialized.");
            return false;
        }
        Product product = storeController.products.WithID(productID);
        if (product == null)
        {
            Debug.LogError("BuyProduct: Product is not available for purchase.");
            return false;
        }

        if (!product.availableToPurchase)
        {
            Debug.LogError("BuyProduct: Product is not available for purchase.");
            return false;
        }

        Debug.Log($"Buying product: {product.definition.id}");

        //TouchBlockManager.Instance.AddLock();
        purchaseUcs = new UniTaskCompletionSource<bool>();
        storeController.InitiatePurchase(product);
        var result = await purchaseUcs.Task;
        purchaseUcs = null;
        //TouchBlockManager.Instance.RemoveLock();
        return result;
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        Debug.Log($"ProcessPurchase");
        purchaseUcs.TrySetResult(true);
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        purchaseUcs.TrySetResult(false);
        Debug.LogError($"OnPurchaseFailed: {failureReason}");
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("OnInitialized: Successful.");
        storeController = controller;
        storeExtensionProvider = extensions;
    }
}
