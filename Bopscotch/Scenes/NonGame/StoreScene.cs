using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Reflection;

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

        private StorePurchaseDialog _itemsCarouselDialog;
        private PurchaseCompleteDialog _purchaseCompleteDialog;
        private ConsumablesDialog _consumablesDialog;

        private InAppBillingServiceConnection _connection;
        private bool _connected = false;
        private Dictionary<string, Product> _products;

        public StoreScene()
            : base()
        {
            _itemsCarouselDialog = new StorePurchaseDialog(RegisterGameObject, UnregisterGameObject);
            _itemsCarouselDialog.ActionCallback = ItemDialogActionButtonCallback;
            _purchaseCompleteDialog = new PurchaseCompleteDialog();
            _purchaseCompleteDialog.SelectionCallback = PurchaseDialogButtonCallback;
            _consumablesDialog = new ConsumablesDialog();

            _dialogs.Add("loading-store", new LoadingDialog(ConnectToStore));
            _dialogs.Add("store-closed", new StoreClosedDialog());
            _dialogs.Add("store-items", _itemsCarouselDialog);
            _dialogs.Add("purchase-complete", _purchaseCompleteDialog);
            _dialogs.Add("consumables", _consumablesDialog);

            BackgroundTextureName = Background_Texture_Name;
        }

        private void PurchaseDialogButtonCallback(string buttonCaption)
        {
            if ((buttonCaption == "Back") && (_consumablesDialog.Active))
            {
                _consumablesDialog.DismissWithReturnValue("Back");
            }
        }

        private void ItemDialogActionButtonCallback(string buttonCaption)
        {
            if (buttonCaption == "Back")
            {
                _consumablesDialog.DismissWithReturnValue("Back");
                _dialogs["store-items"].DismissWithReturnValue("Back");

            }
            else if (buttonCaption == "Buy")
            {
                InitiatePurchase(((StorePurchaseDialog)_dialogs["store-items"]).Selection);
            }
        }

        protected override void CompletePostStartupLoadInitialization()
        {
            base.CompletePostStartupLoadInitialization();

            foreach (KeyValuePair<string, ButtonDialog> kvp in _dialogs) { kvp.Value.ExitCallback = HandleActiveDialogExit; }
        }

        public override void Activate()
        {
            _returnToGame = NextSceneParameters.Get<bool>("return-to-game");

            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            List<string> providers = new List<string>();
            Dictionary<string, string> lookups = new Dictionary<string, string>();
            foreach (Type t in types)
            {
                PropertyInfo pi = t.GetProperty(Bopscotch.Definitions.KeyChain, BindingFlags.NonPublic | BindingFlags.Static);
                if (pi != null)
                {
                    providers.Add(t.FullName);
                    lookups.Add(t.FullName, pi.GetValue(null).ToString());
                }
            }
            providers = providers.OrderBy(x => x.LastIndexOf('.')).ToList();

            string sk = "";
            foreach (string p in providers)
            {
                sk = sk + lookups[p];
            }

            _connection = new InAppBillingServiceConnection(Bopscotch.Game1.Activity, sk);
            _connection.OnConnected += LoadProducts;

            MainActivity.BillingServiceConnection = _connection;

            base.Activate();

            MusicManager.StopMusic();

            ActivateDialog("loading-store");
        }

        private void ConnectToStore()
        {
            _connection.Connect();
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

            _dialogs["loading-store"].DismissWithReturnValue("Loaded");

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
        }

        void BillingHandler_OnUserCanceled()
        {
            Android.Util.Log.Debug("Leda", "Purchase CANCELLED");
            FinishPurchaseProcess(false, "Purchase cancelled");
        }

        void BillingHandler_OnPurchaseFailedValidation(Purchase purchase, string purchaseData, string purchaseSignature)
        {
            Android.Util.Log.Debug("Leda", "Purchase VALIDATION FAILED");
            FinishPurchaseProcess(false, "Validation failed");
        }

        void BillingHandler_OnProductPurchasedError(int responseCode, string sku)
        {
            Android.Util.Log.Debug("Leda", "Purchase ERROR");
            FinishPurchaseProcess(false, "Error in purchase process");
        }

        void BillingHandler_InAppBillingProcesingError(string message)
        {
            Android.Util.Log.Debug("Leda", "General ERROR");
            FinishPurchaseProcess(false, "Could not complete purchase");
        }

        void BillingHandler_OnProductPurchased(int response, Purchase purchase, string purchaseData, string purchaseSignature)
        {
            Android.Util.Log.Debug("Leda", "SUCCESS!");

            string productCode = purchase.ProductId;
            _connection.BillingHandler.ConsumePurchase(purchase);

            FulfillPurchase(productCode);
            FinishPurchaseProcess(true, productCode);
        }

        private void FinishPurchaseProcess(bool completedSuccessfully, string outcomeInfo)
        {
            _purchaseCompleteDialog.PurchaseSuccessful = completedSuccessfully;

            if (completedSuccessfully)
            {
                _purchaseCompleteDialog.ItemCode = outcomeInfo;
            }
            else
            {
                _purchaseCompleteDialog.PurchaseOutcomeMessage = outcomeInfo;
            }

            _dialogs["store-items"].DismissWithReturnValue("");
        }

        private void HandleActiveDialogExit(string selectedOption)
        {
            if (_lastActiveDialogName == "purchase-complete")
            {
                ActivateDialog("store-items");
            }
            else if ((_lastActiveDialogName == "store-items") && (string.IsNullOrWhiteSpace(selectedOption)))
            {
                ActivateDialog("purchase-complete");
            }
            else if ((!string.IsNullOrWhiteSpace(selectedOption)) && (selectedOption != "Loaded"))
            {
                if ((_returnToGame) && (Data.Profile.Lives > 0))
                {
                    NextSceneType = typeof(SurvivalGameplayScene);
                    MusicManager.PlayLoopedMusic("survival-gameplay");
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

    }
}