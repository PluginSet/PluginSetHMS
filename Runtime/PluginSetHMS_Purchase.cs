#if ENABLE_HMS && ENABLE_HMS_PURCHASE
using System;
using System.Collections.Generic;
using HmsPlugin;
using HuaweiMobileServices.IAP;
using HuaweiMobileServices.Utils;
using PluginSet.Core;
using UnityEngine;

namespace PluginSetHMS.Runtime
{
    /***
     *{
     * " autoRenewing":false,
     *  "orderId":"202311111333336613af5f1317.109480861",
     *  "packageName":"com.shoot.bingospace.huawei",
     *  "applicationId":"109480861",
     *  "kind":0,
     *  "productId":"com.shoot.bingospace.elite",
     *  "productName":"elite",
     *  "purchaseTime":1699709663000,
     *  "developerPayload":"",
     *  "purchaseToken":"0000018bbe9739fc9f0dae7d6a18036b09ce7f33c31ceb249ba6574e3c7f1e24d916d67fffb93946x4742.7.109480861",
     *  "consumptionState":0,
     *  "confirmed":0,
     *  "purchaseType":0,
     *  "currency":"GBP",
     *  "price":989,
     *  "country":"GB",
     *  "payOrderId":"sandboxAd9291acbe7d432c8e5819d843a85861",
     *  "payType":"34",
     *  "sdkChannel":"1"}
     */
    [Serializable]
    public class HuaweiPurchaseData
    {
        [SerializeField]
        public bool autoRenewing;
        [SerializeField]
        public string orderId;
        [SerializeField]
        public string packageName;
        [SerializeField]
        public string applicationId;
        [SerializeField]
        public int kind;
        [SerializeField]
        public string productId;
        [SerializeField]
        public string productName;
        [SerializeField]
        public long purchaseTime;
        [SerializeField]
        public string developerPayload;
        [SerializeField]
        public string purchaseToken;
        [SerializeField]
        public int consumptionState;
        [SerializeField]
        public int purchaseType;
        [SerializeField]
        public string currency;
        [SerializeField]
        public long price;
        [SerializeField]
        public string country;
        [SerializeField]
        public string payOrderId;
        [SerializeField]
        public string payType;
    }
    
    public partial class PluginSetHMS: IIAPurchasePlugin
    {
        public bool IsEnablePayment => true;
        
        private event Action<string> onTransactionCompleted = null;
        
        private Action<Result> _paymentCallback;

        [HMSStartExecutable]
        private void StartPurchasePlugin()
        {
            HMSIAPManager.Instance.OnInitializeIAPSuccess = OnInitializeIAPSuccess;
            HMSIAPManager.Instance.OnInitializeIAPFailure = OnInitializeIAPFailure;
            HMSIAPManager.Instance.OnBuyProductSuccess = OnBuyProductSuccess;
            HMSIAPManager.Instance.OnBuyProductFailure = OnBuyProductFailure;
            HMSIAPManager.Instance.OnConsumePurchaseSuccess = OnConsumePurchaseSuccess;
            HMSIAPManager.Instance.OnConsumePurchaseFailure = OnConsumePurchaseFailure;
            HMSIAPManager.Instance.OnUnconsumeConsumePurchaseLoaded = OnUnconsumeConsumePurchaseLoaded;

            if (HMSIAPKitSettings.Instance.Settings.GetBool(HMSIAPKitSettings.InitializeOnStart))
            {
                InitWithProducts(null);
            }
        }

        [HMSDisposeExecutable]
        private void DisposePurchasePlugin()
        {
            HMSIAPManager.Instance.OnInitializeIAPSuccess = null;
            HMSIAPManager.Instance.OnInitializeIAPFailure = null;
            HMSIAPManager.Instance.OnBuyProductSuccess = null;
            HMSIAPManager.Instance.OnBuyProductFailure = null;
            HMSIAPManager.Instance.OnConsumePurchaseSuccess = null;
            HMSIAPManager.Instance.OnConsumePurchaseFailure = null;
            HMSIAPManager.Instance.OnUnconsumeConsumePurchaseLoaded = null;
        }
        
        private void OnInitializeIAPSuccess()
        {
            SendNotification(PluginConstants.IAP_ON_INIT_SUCCESS, new Result()
            {
                Success = true,
                PluginName = Name,
            });
        }

        private void OnInitializeIAPFailure(HMSException obj)
        {
            SendNotification(PluginConstants.IAP_ON_INIT_FAILED, new Result()
            {
                Success = false,
                PluginName = Name,
                Code = obj.ErrorCode,
                Error = obj.Message,
            });
        }

        private void OnUnconsumeConsumePurchaseLoaded(InAppPurchaseData obj)
        {
            OnBuyProductCallback(obj);
        }

        private void OnBuyProductSuccess(PurchaseResultInfo obj)
        {
            OnBuyProductCallback(obj.InAppPurchaseData);
        }

        private void OnBuyProductCallback(InAppPurchaseData obj)
        {
            var data = new HuaweiPurchaseData()
            {
                autoRenewing = obj.AutoRenewing,
                orderId = obj.OrderID,
                packageName = obj.PackageName,
                applicationId = obj.ApplicationId,
                kind = obj.Kind,
                productId = obj.ProductId,
                productName = obj.ProductName,
                purchaseTime = obj.PurchaseTime,
                developerPayload = obj.DeveloperPayload,
                purchaseToken = obj.PurchaseToken,
                consumptionState = obj.ConsumptionState,
                purchaseType = obj.PurchaseType,
                currency = obj.Currency,
                price = obj.Price,
                country = obj.Country,
                payOrderId = obj.PayOrderId,
                payType = obj.PayType,
            };
            
            var json = JsonUtility.ToJson(data);

            if (_paymentCallback == null)
            {
                SendNotification(PluginConstants.IAP_CALLBACK_LOST_PAYMENTS, new Result()
                {
                    Success = true,
                    PluginName = Name,
                    Data = json,
                    DataObject = data,
                });
                return;
            }
            
            _paymentCallback.Invoke(new Result()
            {
                Success = true,
                PluginName = Name,
                Data = json,
                DataObject = data,
            });
            _paymentCallback = null;
        }

        private void OnBuyProductFailure(int obj)
        {
            if (_paymentCallback == null)
                return;

            if (obj == OrderStatusCode.ORDER_STATE_CANCEL)
                obj = PluginConstants.CancelCode;
            
            _paymentCallback.Invoke(new Result()
            {
                Success = false,
                Code = obj,
                PluginName = Name,
                Error = "Purchase failed"
            });
            _paymentCallback = null;
        }

        private void OnConsumePurchaseSuccess(ConsumeOwnedPurchaseResult obj)
        {
            onTransactionCompleted?.Invoke(obj.ConsumePurchaseData.PurchaseToken);
        }

        private void OnConsumePurchaseFailure(HMSException obj)
        {
            Debug.LogWarning($"OnConsumePurchaseFailure: {obj}");
        }

        public void InitWithProducts(Dictionary<string, int> products)
        {
            HMSIAPManager.Instance.InitializeIAP(products);
        }
        
        public void Pay(string productId, Action<Result> callback = null, string jsonData = null)
        {
            _paymentCallback = callback;
            HMSIAPManager.Instance.PurchaseProduct(productId, false, jsonData);
        }

        public void PaymentComplete(string token)
        {
            HMSIAPManager.Instance.ConsumePurchaseWithToken(token);
        }

        public void AddOnPaymentCompleted(Action<string> completed)
        {
            onTransactionCompleted += completed;
        }

        public void RemoveOnPaymentCompleted(Action<string> completed)
        {
            onTransactionCompleted -= completed;
        }
    }
}
#endif