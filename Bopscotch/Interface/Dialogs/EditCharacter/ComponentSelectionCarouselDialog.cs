using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Asset_Management;
using Leda.Core.Gamestate_Management;

using Bopscotch.Data;
using Bopscotch.Data.Avatar;
using Bopscotch.Input;
using Bopscotch.Interface.Objects;
using Bopscotch.Interface.Dialogs.Carousel;

namespace Bopscotch.Interface.Dialogs.EditCharacter
{
    public class ComponentSelectionCarouselDialog : CarouselDialog
    {
        public AvatarComponentSet ComponentSet { private get; set; }

        public ComponentSelectionCarouselDialog(Scene.ObjectRegistrationHandler registrationHandler, Scene.ObjectUnregistrationHandler unregistrationHandler)
            : base(registrationHandler, unregistrationHandler)
        {
            Height = Dialog_Height;
            TopYWhenActive = Definitions.Back_Buffer_Height - Dialog_Height;
            CarouselCenter = new Vector2(Definitions.Back_Buffer_Center.X, Carousel_Center_Y);
            CarouselRadii = new Vector2(Carousel_Horizontal_Radius, Carousel_Vertical_Radius);
            _itemRenderDepths = new Range(Minimum_Item_Render_Depth, Maximum_Item_Render_Depth);
            _itemScales = new Range(Minimum_Item_Scale, Maximum_Item_Scale);

            AddIconButton("previous", new Vector2(Definitions.Back_Buffer_Center.X - 450, 175), Button.ButtonIcon.Previous, Color.DodgerBlue);
            AddIconButton("next", new Vector2(Definitions.Back_Buffer_Center.X + 450, 175), Button.ButtonIcon.Next, Color.DodgerBlue);

            AddButton("Select", new Vector2(Definitions.Right_Button_Column_X, 325), Button.ButtonIcon.Tick, Color.LawnGreen, 0.7f);
            AddButton("Back", new Vector2(Definitions.Left_Button_Column_X, 325), Button.ButtonIcon.Back, Color.Red, 0.7f);

            ActionButtonPressHandler = HandleActionButtonPress;
            TopYWhenInactive = Definitions.Back_Buffer_Height;

            SetupButtonLinkagesAndDefaultValues();

            registrationHandler(this);
        }

        private void HandleActionButtonPress(string action)
        {
            if (action == "Select") 
            { 
                Profile.Settings.SetAvatarCustomComponent(ComponentSet.Name, Selection);
                Profile.Save();
            }

            DismissWithReturnValue(action);
        }

        public override void Activate()
        {
            FlushItems();

            base.Activate();

            AddAvailableComponents();

            ActivateButton("Select");

            InitializeForSpin();
            SetInitialSelection(Data.Profile.Settings.GetAvatarCustomComponent(Profile.Settings.SelectedAvatarSlot, ComponentSet.Name));
        }

        private void AddAvailableComponents()
        {
            if (ComponentSet.SelectionNotMandatory)
            {
                AddSkeletonForComponent(ComponentSet.DisplaySkeleton, null, true);
            }

            foreach (AvatarComponent item in ComponentSet.Components)
            {
                if (item.Unlocked) { AddSkeletonForComponent(ComponentSet.DisplaySkeleton, item, (ComponentSet.Name != Body_Component_Set_Name)); }
            }
        }

        private void AddSkeletonForComponent(string skeletonName, AvatarComponent component, bool addBody)
        {
            ComponentSetDisplayAvatar avatar = new ComponentSetDisplayAvatar((component == null)  ? "none" : component.Name, 0.0f);
            avatar.CreateBonesFromDataManager(skeletonName);
            avatar.Name = skeletonName;

            if (addBody) { avatar.Components.Add(AvatarComponentManager.Component("body", "Blue")); }
            if (component != null) { avatar.Components.Add(component); }
            avatar.SkinFromComponents();
            avatar.RenderLayer = RenderLayer;

            AddItem(avatar);

        }

        private const float Carousel_Center_Y = 160.0f;
        private const float Carousel_Horizontal_Radius = 300.0f;
        private const float Carousel_Vertical_Radius = 50.0f;
        private const float Maximum_Item_Render_Depth = 0.1f;
        private const float Minimum_Item_Render_Depth = 0.05f;
        private const float Maximum_Item_Scale = 1.0f;
        private const float Minimum_Item_Scale = 0.75f;

        private const string Body_Component_Set_Name = "body";

        protected const int Dialog_Height = 450;
        protected const float Dialog_Margin = 30.0f;
    }
}
