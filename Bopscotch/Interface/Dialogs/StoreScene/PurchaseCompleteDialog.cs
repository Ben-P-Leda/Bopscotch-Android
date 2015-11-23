using Microsoft.Xna.Framework;

using System.Linq;
using System.Collections.Generic;

using Xamarin.InAppBilling;

using Bopscotch.Interface;

namespace Bopscotch.Interface.Dialogs.StoreScene
{
    public class PurchaseCompleteDialog : ButtonDialog
    {
        public string ItemCode { private get; set; }
        public string PurchaseOutcomeMessage { private get; set; }
        public bool PurchaseSuccessful { private get; set; }
        public IList<Product> Products { private get; set; }

        public PurchaseCompleteDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Definitions.Back_Buffer_Center.Y - (Dialog_Height / 2.0f);

            AddButton("OK", new Vector2(Definitions.Back_Buffer_Center.X, 200), Button.ButtonIcon.Tick, Color.LawnGreen);

            _cancelButtonCaption = "OK";
            _boxCaption = "";
        }

        public override void Activate()
        {
            if (PurchaseSuccessful)
            {
                Product selected = Products.FirstOrDefault(x => x.ProductId == ItemCode);
                string productName = selected != null ? selected.Title : ItemCode;

                _boxCaption = Translator.Translation("purchase-complete").Replace("[ITEM]", productName);
            }
            else
            {
                _boxCaption = PurchaseOutcomeMessage;
            }
            base.Activate();
        }

        public override void Reset()
        {
            base.Reset();
            WorldPosition = new Vector2(0.0f, -Height);
        }

        private const int Dialog_Height = 300;
        private const float Top_Line_Y = 80.0f;
        private const float Line_Height = 50.0f;
    }
}
