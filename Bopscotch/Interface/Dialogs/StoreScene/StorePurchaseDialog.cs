using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

//using Windows.ApplicationModel.Store;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Timing;
using Leda.Core.Asset_Management;
using Leda.Core.Gamestate_Management;

using Bopscotch.Data;
using Bopscotch.Data.Avatar;
using Bopscotch.Input;
using Bopscotch.Interface.Objects;
using Bopscotch.Interface.Dialogs.Carousel;

namespace Bopscotch.Interface.Dialogs.StoreScene
{
    public class StorePurchaseDialog : CarouselDialog
    {
        private SpriteFont _font;
        private Dictionary<string, string> _productInfo;
        private Timer _textTransitionTimer;
        private Color _textTint;

        public StorePurchaseDialog(Scene.ObjectRegistrationHandler registrationHandler, Scene.ObjectUnregistrationHandler unregistrationHandler)
            : base(registrationHandler, unregistrationHandler)
        {

            Height = Purchase_Dialog_Height;
            TopYWhenActive = Definitions.Back_Buffer_Height - (Purchase_Dialog_Height + Bopscotch.Scenes.NonGame.StoreScene.Dialog_Margin);
            CarouselCenter = new Vector2(Definitions.Back_Buffer_Center.X, Carousel_Center_Y);
            CarouselRadii = new Vector2(Carousel_Horizontal_Radius, Carousel_Vertical_Radius);
            _itemRenderDepths = new Range(Minimum_Item_Render_Depth, Maximum_Item_Render_Depth);
            _itemScales = new Range(Minimum_Item_Scale, Maximum_Item_Scale);

            AddIconButton("previous", new Vector2(Definitions.Back_Buffer_Center.X - 450, 175), Button.ButtonIcon.Previous, Color.DodgerBlue);
            AddIconButton("next", new Vector2(Definitions.Back_Buffer_Center.X + 450, 175), Button.ButtonIcon.Next, Color.DodgerBlue);

            AddButton("Back", new Vector2(Definitions.Left_Button_Column_X, 400), Button.ButtonIcon.Back, Color.DodgerBlue, 0.7f);
            AddButton("Buy", new Vector2(Definitions.Right_Button_Column_X, 400), Button.ButtonIcon.Tick, Color.Orange, 0.7f);

            _nonSpinButtonCaptions.Add("Buy");

            ActionButtonPressHandler = HandleActionButtonPress;
            TopYWhenInactive = Definitions.Back_Buffer_Height;

            SetupButtonLinkagesAndDefaultValues();

            registrationHandler(this);

            _textTransitionTimer = new Timer("");
            GlobalTimerController.GlobalTimer.RegisterUpdateCallback(_textTransitionTimer.Tick);
            _textTransitionTimer.NextActionDuration = 1;
            _textTint = Color.White;

            _font = Game1.Instance.Content.Load<SpriteFont>("Fonts\\arial");
        }

        private void HandleActionButtonPress(string action)
        {
            DismissWithReturnValue(action);
        }

        protected override void SetupButtonLinkagesAndDefaultValues()
        {
            base.SetupButtonLinkagesAndDefaultValues();

            SetMovementLinksForButton("Back", "previous", "", "", "Change");
            SetMovementLinksForButton("Change", "next", "", "Back", "");
            SetMovementLinksForButton("next", "", "Change", "previous", "");

            _defaultButtonCaption = "Change";
            _activeButtonCaption = "Change";
            _nonSpinButtonCaptions.Add("Change");
            _cancelButtonCaption = "Back";
        }

        //public void InitializeProducts(ListingInformation products)
        //{
        //    Dictionary<string, Point> iapImageMappings = new Dictionary<string, Point>()
        //    {
        //        { "Bopscotch_10_Lives", new Point(0,0) },
        //        { "Bopscotch_20_Lives", new Point(0,1) },
        //        { "Bopscotch_50_Lives", new Point(0,2) },
        //        { "Bopscotch_2_Tickets", new Point(1,0) },
        //        { "Bopscotch_5_Tickets", new Point(1,1) },
        //        { "Bopscotch_10_Tickets", new Point(1,2) }
        //    };

        //    _productInfo = new Dictionary<string, string>();

        //    foreach (KeyValuePair<string, ProductListing> kvp in products.ProductListings)
        //    {
        //        if (iapImageMappings.ContainsKey(kvp.Key))
        //        {
        //            AddItem(kvp.Key, iapImageMappings[kvp.Key]);
        //            _productInfo.Add(kvp.Key, string.Format("{0} - {1}", kvp.Value.Name, kvp.Value.FormattedPrice));
        //        }
        //    }
        //}

        private void AddItem(string itemCode, Point matrixTopLeft)
        {
            CarouselFlatImage area = new CarouselFlatImage(itemCode, Items_Texture);
            area.RenderLayer = RenderLayer;
            area.Frame = new Rectangle(0, 0, Item_Image_Width, Item_Image_Height);

            AddItem(area);
        }

        public override void Activate()
        {
            base.Activate();

            ActivateButton("Change");

            InitializeForSpin();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            TextWriter.Write(Selection, spriteBatch, _font, new Vector2(Definitions.Back_Buffer_Center.X, WorldPosition.Y + 275.0f),
                TransitionTint(_textTint), TransitionTint(Color.Black), 3.0f, 0.75f, 0.1f, TextWriter.Alignment.Center);
        }

        protected Color TransitionTint(Color unfadedColour)
        {
            if (Rotating) { return Color.Lerp(unfadedColour, Color.Transparent, _textTransitionTimer.CurrentActionProgress); }
            return Color.Lerp(Color.Transparent, unfadedColour, _textTransitionTimer.CurrentActionProgress);
        }

        private const float Maximum_Item_Render_Depth = 0.1f;
        private const float Minimum_Item_Render_Depth = 0.05f;
        private const float Maximum_Item_Scale = 1.0f;
        private const float Minimum_Item_Scale = 0.75f;

        private const float Carousel_Center_Y = 160.0f;
        private const float Carousel_Horizontal_Radius = 270.0f;
        private const float Carousel_Vertical_Radius = 35.0f;
        private const int Purchase_Dialog_Height = 480;

        private const string Items_Texture = "iap-items";
        private const int Item_Image_Width = 200;
        private const int Item_Image_Height = 110;
    }
}
