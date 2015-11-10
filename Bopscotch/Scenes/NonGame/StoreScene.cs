using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;

//using Windows.ApplicationModel.Store;

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
        private bool _loadedProducts;
        private bool _returnToGame;

        private PurchaseCompleteDialog _purchaseCompleteDialog;
        private ConsumablesDialog _consumablesDialog;

        public StoreScene()
            : base()
        {
            _purchaseCompleteDialog = new PurchaseCompleteDialog("");
            _purchaseCompleteDialog.SelectionCallback = PurchaseDialogButtonCallback;
            _consumablesDialog = new ConsumablesDialog();
            
            _dialogs.Add("store-status", new StoreStatusDialog());
            _dialogs.Add("store-items", new StorePurchaseDialog(RegisterGameObject, UnregisterGameObject));
            _dialogs.Add("purchase-complete", _purchaseCompleteDialog);
            _dialogs.Add("consumables", _consumablesDialog);

            BackgroundTextureName = Background_Texture_Name;

            _loadedProducts = false;
            LoadProducts();
        }

        private void PurchaseDialogButtonCallback(string buttonCaption)
        {
            if ((buttonCaption == "Back") && (_consumablesDialog.Active)) { _consumablesDialog.DismissWithReturnValue(""); }
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

            if (!_loadedProducts) 
            { 
                ActivateDialog("store-status"); 
            }
            else 
            { 
                ActivateDialog("store-items");
                _consumablesDialog.Activate();
            }
        }

        private async void LoadProducts()
        {
            //try
            //{
            //    _products = await CurrentApp.LoadListingInformationAsync();
            //}
            //catch (Exception)
            //{
            //    _products = null;
            //}

            //if (_products != null)
            //{
            //    ((StorePurchaseDialog)_dialogs["store-items"]).InitializeProducts(_products);
            //}
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
            else
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
            //selection = "Bopscotch_Test_Product";

            //Deployment.Current.Dispatcher.BeginInvoke(async () =>
            //{
            //    try
            //    {
            //        string receipt = await CurrentApp.RequestProductPurchaseAsync(selection, true);

            //        if (CurrentApp.LicenseInformation.ProductLicenses[selection].IsActive)
            //        {
            //            CurrentApp.ReportProductFulfillment(selection);

            //            FulfillPurchase(selection);

            //            _purchaseCompleteDialog.ItemCode = selection;
            //            ActivateDialog("purchase-complete");
            //        }
            //        else
            //        {
            //            ActivateDialog("store-items");
            //        }
            //    }
            //    catch (Exception)
            //    {
            //        ActivateDialog("store-items");
            //    }
            //});
        }

        private void FulfillPurchase(string productCode)
        {
            switch (productCode)
            {
                case "Bopscotch_Test_Product": Data.Profile.Lives += 1; Data.Profile.GoldenTickets += 1; break;
                case "Bopscotch_10_Lives": Data.Profile.Lives += 10; break;
                case "Bopscotch_20_Lives": Data.Profile.Lives += 20; break;
                case "Bopscotch_50_Lives": Data.Profile.Lives += 50; break;
                case "Bopscotch_2_Tickets": Data.Profile.GoldenTickets += 2; break;
                case "Bopscotch_5_tickets": Data.Profile.GoldenTickets += 5; break;
                case "Bopscotch_10_Tickets": Data.Profile.GoldenTickets += 10; break;
            }

            Data.Profile.Save();
        }

        private const string Background_Texture_Name = "background-1";
        
        public const float Dialog_Margin = 40.0f;
    }
}
