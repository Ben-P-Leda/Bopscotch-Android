using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;

using Xamarin.InAppBilling;

using Microsoft.Xna.Framework;

using Leda.Core.Asset_Management;

using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Scenes.Gameplay.Survival;
using Bopscotch.Interface.Dialogs;
using Bopscotch.Interface.Dialogs.StoreScene;
using Bopscotch.Interface.Content;

namespace Bopscotch.Scenes.NonGame
{
    public class StoreScene : MenuDialogScene
    {
        private bool _returnToGame;

        private PurchaseCompleteDialog _purchaseCompleteDialog;
        private ConsumablesDialog _consumablesDialog;

        private InAppBillingServiceConnection _connection;
        private bool _connected = false;
        private Dictionary<string, Product> _products;

        public StoreScene()
            : base()
        {
            _purchaseCompleteDialog = new PurchaseCompleteDialog("");
            _purchaseCompleteDialog.SelectionCallback = PurchaseDialogButtonCallback;
            _consumablesDialog = new ConsumablesDialog();

            _dialogs.Add("loading-store", new LoadingDialog(ConnectToStore));
            _dialogs.Add("store-closed", new StoreClosedDialog());
            _dialogs.Add("store-items", new StorePurchaseDialog(RegisterGameObject, UnregisterGameObject));
            _dialogs.Add("purchase-complete", _purchaseCompleteDialog);
            _dialogs.Add("consumables", _consumablesDialog);

            BackgroundTextureName = Background_Texture_Name;

            _connection = new InAppBillingServiceConnection(Bopscotch.Game1.Activity, Store_Key);
            _connection.OnConnected += LoadProducts;

            MainActivity.BillingServiceConnection = _connection;
        }

        private void PurchaseDialogButtonCallback(string buttonCaption)
        {
            if ((buttonCaption == "Back") && (_consumablesDialog.Active)) { _consumablesDialog.DismissWithReturnValue(""); }
        }

        private void HandlePurchaseFailure()
        {
            ActivateDialog("store-items");
        }

        protected override void CompletePostStartupLoadInitialization()
        {
            base.CompletePostStartupLoadInitialization();

            foreach (KeyValuePair<string, ButtonDialog> kvp in _dialogs) { kvp.Value.ExitCallback = HandleActiveDialogExit; }
        }

        public override void Activate()
        {
            _returnToGame = NextSceneParameters.Get<bool>("return-to-game");

            base.Activate();

            MusicManager.StopMusic();

            ActivateDialog("loading-store");
        }

        private void ConnectToStore()
        {
            MainActivity.BillingServiceConnection.Connect();
        }

        private async void LoadProducts()
        {
            _connected = true;

            Android.Util.Log.Debug("Leda", "Connected to Play Store...");

            IList<Product> products = await MainActivity.BillingServiceConnection.BillingHandler.QueryInventoryAsync(new List<string> {
                "bopscotch_10_lives",
                "bopscotch_20_lives",
                "bopscotch_50_lives",
                "bopscotch_2_tickets",
                "bopscotch_5_tickets",
                "bopscotch_10_tickets",
                ReservedTestProductIDs.Purchased,
                ReservedTestProductIDs.Canceled,
                ReservedTestProductIDs.Refunded,
                ReservedTestProductIDs.Unavailable
            }, ItemType.Product);

            _dialogs["loading-store"].DismissWithReturnValue("");

            if (products != null)
            {
                _connection.BillingHandler.OnProductPurchased += BillingHandler_OnProductPurchased;
                _connection.BillingHandler.InAppBillingProcesingError += BillingHandler_InAppBillingProcesingError;
                _connection.BillingHandler.OnProductPurchasedError += BillingHandler_OnProductPurchasedError;
                _connection.BillingHandler.OnPurchaseFailedValidation += BillingHandler_OnPurchaseFailedValidation;
                _connection.BillingHandler.OnUserCanceled += BillingHandler_OnUserCanceled;

                _products = new Dictionary<string, Product>();
                foreach (Product p in products)
                {
                    _products.Add(p.ProductId, p);
                }

                ((StorePurchaseDialog)_dialogs["store-items"]).InitializeProducts(products);
                _purchaseCompleteDialog.Products = products;
                ActivateDialog("store-items");
                _consumablesDialog.Activate();
            }
            else
            {
                ActivateDialog("store-closed");
            }

            var purchases = _connection.BillingHandler.GetPurchases(ItemType.Product);
        }

        void BillingHandler_OnUserCanceled()
        {
            Android.Util.Log.Debug("Leda", "Purchase CANCELLED");
            ActivateDialog("store-items");
        }

        void BillingHandler_OnPurchaseFailedValidation(Purchase purchase, string purchaseData, string purchaseSignature)
        {
            Android.Util.Log.Debug("Leda", "Purchase VALIDATION FAILED");
            ActivateDialog("store-items");
        }

        void BillingHandler_OnProductPurchasedError(int responseCode, string sku)
        {
            Android.Util.Log.Debug("Leda", "Purchase ERROR");
            ActivateDialog("store-items");
        }

        void BillingHandler_InAppBillingProcesingError(string message)
        {
            Android.Util.Log.Debug("Leda", "General ERROR");
            ActivateDialog("store-items");
        }

        void BillingHandler_OnProductPurchased(int response, Purchase purchase, string purchaseData, string purchaseSignature)
        {
            Android.Util.Log.Debug("Leda", "SUCCESS!");

            string productCode = purchase.ProductId;
            _connection.BillingHandler.ConsumePurchase(purchase);

            FulfillPurchase(productCode);
            _purchaseCompleteDialog.ItemCode = productCode;
            ActivateDialog("purchase-complete");
        }

        private void HandleActiveDialogExit(string selectedOption)
        {
            if (selectedOption == "Buy")
            {
                InitiatePurchase(((StorePurchaseDialog)_dialogs["store-items"]).Selection);
            }
            else if (_lastActiveDialogName == "purchase-complete")
            {
                ActivateDialog("store-items");
            }
            else if (!string.IsNullOrWhiteSpace(selectedOption))
            {
                if ((_returnToGame) && (Data.Profile.Lives > 0))
                {
                    NextSceneType = typeof(SurvivalGameplayScene);
                }
                else
                {
                    NextSceneType = typeof(TitleScene);
                }
                Deactivate();
            }
        }

        private void InitiatePurchase(string selection)
        {
            _connection.BillingHandler.BuyProduct(_products[selection]);
        }

        private void FulfillPurchase(string productCode)
        {
            switch (productCode)
            {
                case "bopscotch_10_lives": Data.Profile.Lives += 10; break;
                case "bopscotch_20_lives": Data.Profile.Lives += 20; break;
                case "bopscotch_50_lives": Data.Profile.Lives += 50; break;
                case "bopscotch_2_tickets": Data.Profile.GoldenTickets += 2; break;
                case "bopscotch_5_tickets": Data.Profile.GoldenTickets += 5; break;
                case "bopscotch_10_tickets": Data.Profile.GoldenTickets += 10; break;
            }

            Data.Profile.Save();
        }

        protected override void CompleteDeactivation()
        {
            if (_connected)
            {
                _connection.Disconnect();
                MainActivity.BillingServiceConnection = null;
            }

            base.CompleteDeactivation();
        }

        private const string Background_Texture_Name = "background-1";
        
        public const float Dialog_Margin = 40.0f;

        // TODO: Better way of storing this...
        private const string Store_Key = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAtNa/cYz42G1sGlhgQN1kAcUAWue9MaU/Ru8kYIj0DXbj7e0oGb+vwVDE7qlnz6/Hd4UrP0eicW3UCPre1+bx+boicOye/1DgC56Db9xnyXT7ouDrH6KXpuhPuMWUO9IiI32qn2dTeEV6EzqGd1Z4EB+mGdUkjjI4NfFoSf3CdqLC0TZhbfw29FccJNS9Bca4bgMv36/oEpsirx4kYQuPa6d0m2W7VvTGlWslekExk9RdoUxw/y1/lAPiMyRWWgzRb5zYxXuUSn1sMlDnGLz1JCZoPRGK3s4G9LoGIBUajQ7vKNWkX2AwUavkH+B0MzA+08N/jJrrwCIYTHLel6rkIQIDAQAB";
    }
}
