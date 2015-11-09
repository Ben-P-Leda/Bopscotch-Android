using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;

using Bopscotch.Data.Avatar;
using Bopscotch.Input;

namespace Bopscotch.Interface.Dialogs.Carousel
{
    public class AvatarSelectionCarouselDialog : CarouselDialog
    {
        public int SelectedAvatarSkinSlot { get; private set; }

        public AvatarSelectionCarouselDialog(Scene.ObjectRegistrationHandler registrationHandler, Scene.ObjectUnregistrationHandler unregistrationHandler)
            : base(registrationHandler, unregistrationHandler)
        {
            ActionButtonPressHandler = SelectionHandler;

            Height = Dialog_Height;
            CarouselCenter = new Vector2(Definitions.Back_Buffer_Center.X, Carousel_Center_Y);
            CarouselRadii = new Vector2(Carousel_Horizontal_Radius, Carousel_Vertical_Radius);
            _itemRenderDepths = new Range(Minimum_Item_Render_Depth, Maximum_Item_Render_Depth);
            _itemScales = new Range(Minimum_Item_Scale, Maximum_Item_Scale);

            _captionsForButtonsNotActivatedByGamepadStartButton.Add("Back");

            AddIconButton("previous", new Vector2(Definitions.Back_Buffer_Center.X - 450, 175), Button.ButtonIcon.Previous, Color.DodgerBlue);
            AddIconButton("next", new Vector2(Definitions.Back_Buffer_Center.X + 450, 175), Button.ButtonIcon.Next, Color.DodgerBlue);

            registrationHandler(this);
        }

        protected virtual void SelectionHandler(string buttonCaption)
        {
            SelectedAvatarSkinSlot = SelectedItem;
        }

        public override void Activate()
        {
            FlushItems();

            base.Activate();

            for (int i = 0; i < Definitions.Avatar_Customisation_Slot_Count; i++) { AddAvatar(i); }

            SelectedAvatarSkinSlot = -1;
            ActivateButton("Select");

            InitializeForSpin();
        }

        private void AddAvatar(int slotIndex)
        {
            CarouselAvatar avatar = new CarouselAvatar(slotIndex.ToString());
            avatar.CreateBonesFromDataManager(Definitions.Avatar_Skeleton_Front);
            avatar.SkinBones(AvatarComponentManager.FrontFacingAvatarSkin(slotIndex));
            avatar.RenderLayer = RenderLayer;
            avatar.StartRestingAnimationSequence();

            AddItem(avatar);
        }

        private const float Carousel_Center_Y = 160.0f;
        private const float Carousel_Horizontal_Radius = 300.0f;
        private const float Carousel_Vertical_Radius = 50.0f;
        private const float Maximum_Item_Render_Depth = 0.1f;
        private const float Minimum_Item_Render_Depth = 0.05f;
        private const float Maximum_Item_Scale = 1.0f;
        private const float Minimum_Item_Scale = 0.75f;

        protected const int Dialog_Height = 400;
        protected const float Dialog_Margin = 30.0f;
    }
}
