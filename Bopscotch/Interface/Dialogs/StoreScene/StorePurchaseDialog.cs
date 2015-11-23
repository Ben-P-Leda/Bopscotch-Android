using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using Xamarin.InAppBilling;

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

        public ButtonSelectionHandler ActionCallback { private get; set; }

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
            ActionCallback(action);
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

        public void InitializeProducts(IList<Product> products)
        {
            FlushItems();

            Dictionary<string, Point> iapImageMappings = new Dictionary<string, Point>()
            {
                { "bopscotch_10_lives", new Point(1,0) },
                { "bopscotch_20_lives", new Point(1,1) },
                { "bopscotch_50_lives", new Point(1,2) },
                { "bopscotch_2_tickets", new Point(0,0) },
                { "bopscotch_5_tickets", new Point(0,1) },
                { "bopscotch_10_tickets", new Point(0,2) }
            };

            _productInfo = new Dictionary<string, string>();

            foreach (KeyValuePair<string, Point> kvp in iapImageMappings)
            {
                Product p = products.FirstOrDefault(x => x.ProductId == kvp.Key);
                if (p != null)
                {
                    AddItem(kvp.Key, kvp.Value);
                    _productInfo.Add(kvp.Key, string.Format("{0} - {1}", p.Title.Replace("(Bopscotch)", ""), p.Price));
                }
            }
        }

        private void AddItem(string itemCode, Point matrixTopLeft)
        {
            CarouselFlatImage item = new CarouselFlatImage(itemCode, Items_Texture);
            item.RenderLayer = RenderLayer;
            item.Frame = new Rectangle(Item_Image_Width * matrixTopLeft.X, Item_Image_Height * matrixTopLeft.Y, Item_Image_Width, Item_Image_Height);
            item.Origin = new Vector2(Item_Image_Width, Item_Image_Height) / 2.0f;

            AddItem(item);
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

            TextWriter.Write(_productInfo[Selection], spriteBatch, _font, new Vector2(Definitions.Back_Buffer_Center.X, WorldPosition.Y + 275.0f),
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

        private const float Carousel_Center_Y = 140.0f;
        private const float Carousel_Horizontal_Radius = 270.0f;
        private const float Carousel_Vertical_Radius = 35.0f;
        private const int Purchase_Dialog_Height = 480;

        private const string Items_Texture = "iap-items";
        private const int Item_Image_Width = 200;
        private const int Item_Image_Height = 110;
    }
}
