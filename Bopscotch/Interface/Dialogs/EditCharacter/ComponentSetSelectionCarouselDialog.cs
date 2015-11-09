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
    public class ComponentSetSelectionCarouselDialog : CarouselDialog
    {
        private List<AvatarComponentSet> _selectableComponentSets;

        public AvatarComponentSet SelectedComponentSet { get { return _selectableComponentSets[SelectedItem]; } }

        public ComponentSetSelectionCarouselDialog(Scene.ObjectRegistrationHandler registrationHandler, Scene.ObjectUnregistrationHandler unregistrationHandler)
            : base(registrationHandler, unregistrationHandler)
        {
            _selectableComponentSets = new List<AvatarComponentSet>();

            Height = Dialog_Height;
            TopYWhenActive = Definitions.Back_Buffer_Height - Dialog_Height;
            CarouselCenter = new Vector2(Definitions.Back_Buffer_Center.X, Carousel_Center_Y);
            CarouselRadii = new Vector2(Carousel_Horizontal_Radius, Carousel_Vertical_Radius);
            _itemRenderDepths = new Range(Minimum_Item_Render_Depth, Maximum_Item_Render_Depth);
            _itemScales = new Range(Minimum_Item_Scale, Maximum_Item_Scale);

            AddIconButton("previous", new Vector2(Definitions.Back_Buffer_Center.X - 450, 175), Button.ButtonIcon.Previous, Color.DodgerBlue);
            AddIconButton("next", new Vector2(Definitions.Back_Buffer_Center.X + 450, 175), Button.ButtonIcon.Next, Color.DodgerBlue);

            AddButton("Back", new Vector2(Definitions.Left_Button_Column_X, 325), Button.ButtonIcon.Back, Color.Red, 0.7f);
            AddButton("Change", new Vector2(Definitions.Right_Button_Column_X, 325), Button.ButtonIcon.Options, Color.LawnGreen, 0.7f);

            ActionButtonPressHandler = HandleActionButtonPress;
            TopYWhenInactive = Definitions.Back_Buffer_Height;

            SetupButtonLinkagesAndDefaultValues();

            registrationHandler(this);
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
        }

        public override void Activate()
        {
            FlushItems();
            _selectableComponentSets.Clear();

            base.Activate();

            AddAvailableComponentTypes();

            ActivateButton("Change");

            InitializeForSpin();
        }

        private void AddAvailableComponentTypes()
        {
            foreach (KeyValuePair<string, AvatarComponentSet> kvp in AvatarComponentManager.ComponentSets)
            {
                if (kvp.Value.HasUnlockedComponents) { AddComponentSetDisplaySkeleton(kvp.Value); }
            }
        }

        private void AddComponentSetDisplaySkeleton(AvatarComponentSet componentSet)
        {
            ComponentSetDisplayAvatar avatar = new ComponentSetDisplayAvatar(componentSet.Name, 0.0f);
            avatar.CreateBonesFromDataManager(componentSet.DisplaySkeleton);
            avatar.Name = componentSet.DisplaySkeleton;

            if (componentSet.Name != "body") { avatar.Components.Add(AvatarComponentManager.Component("body", "Blue")); }
            avatar.Components.Add((from c in componentSet.Components where c.Unlocked == true select c).First());
            avatar.SkinFromComponents();
            avatar.RenderLayer = RenderLayer;
            avatar.Annotation = componentSet.Name;

            AddItem(avatar);

            _selectableComponentSets.Add(componentSet);
        }

        private const float Carousel_Center_Y = 160.0f;
        private const float Carousel_Horizontal_Radius = 300.0f;
        private const float Carousel_Vertical_Radius = 50.0f;
        private const float Maximum_Item_Render_Depth = 0.1f;
        private const float Minimum_Item_Render_Depth = 0.05f;
        private const float Maximum_Item_Scale = 1.0f;
        private const float Minimum_Item_Scale = 0.75f;

        protected const int Dialog_Height = 450;
        protected const float Dialog_Margin = 30.0f;
    }
}
