using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;

//using Windows.ApplicationModel.Store;

using Microsoft.Xna.Framework;

using Leda.Core.Asset_Management;

using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Interface.Dialogs;
using Bopscotch.Interface.Dialogs.StoreScene;
using Bopscotch.Interface.Content;

namespace Bopscotch.Scenes.NonGame
{
    public class StoreScene : MenuDialogScene
    {
        //private ListingInformation _products;

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
            base.Activate();

            MusicManager.StopMusic();

            //if ((_products == null) || (_products.ProductListings.Count < 1)) 
            { 
                ActivateDialog("store-status"); 
            }
            //else 
            //{ 
            //    ActivateDialog("store-items");
            //    _consumablesDialog.Activate();
            //}
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
                NextSceneType = typeof(TitleScene);
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
        }

        private const string Background_Texture_Name = "background-1";
        
        public const float Dialog_Margin = 40.0f;
    }
}
