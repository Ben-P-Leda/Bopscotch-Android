using System.Collections.Generic;
using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Gamestate_Management;

using Bopscotch.Data;
using Bopscotch.Data.Avatar;
using Bopscotch.Effects.Popups;

namespace Bopscotch.Interface.Content
{
    public class SurvivalAreaCompleteContentFactory
    {
        private Scene.ObjectRegistrationHandler _registerObject;
        private List<PopupRequiringDismissal> _newItemPopups;

        public SurvivalAreaCompleteContentFactory(Scene.ObjectRegistrationHandler registrationHandler)
        {
            _registerObject = registrationHandler;
            _newItemPopups = new List<PopupRequiringDismissal>();
        }

        public void CreateContentForHeaderMessage()
        {
            _registerObject(new TextContent(Translator.Translation("area-complete-header").Replace("[AREA]", Translator.Translation(Profile.CurrentAreaData.Name)),
                new Vector2(Definitions.Back_Buffer_Center.X, Message_Top_Y)) { FadeFraction = 0.0f, Scale = 0.8f });
        }

        public void CreateContentForUnlockableItems()
        {
            float yPosition = Content_Top_Y;

            _newItemPopups.Clear();

            CreateContentForItemList(Profile.FullVersionOnlyUnlocks, Profile.LockState.FullVersionOnly, yPosition);
            yPosition += (Unlock_Line_Height * Profile.FullVersionOnlyUnlocks.Count);

            CreateContentForItemList(Profile.NewlyUnlockedContent, Profile.LockState.NewlyUnlocked, yPosition);
            yPosition += (Unlock_Line_Height * Profile.NewlyUnlockedContent.Count);

            CreateContentForItemList(Profile.UnlockedContent, Profile.LockState.Unlocked, yPosition);
            yPosition += (Unlock_Line_Height * Profile.UnlockedContent.Count);

            CreateContentForItemList(Profile.LockedContent, Profile.LockState.Locked, yPosition);
        }

        public void CreateContentForItemList(List<XElement> itemList, Profile.LockState lockState, float yPosition)
        {
            foreach (XElement el in itemList)
            {
                switch (el.Attribute("type").Value)
                {
                    case "area": CreateAreaDisplayItems(el.Attribute("name").Value, lockState, yPosition); break;
                    case "golden-ticket": CreateGoldenTicketDisplayItems(lockState, yPosition); break;
                    case "avatar-component": CreateAvatarComponentDisplayItems(el.Attribute("set").Value, el.Attribute("name").Value, lockState, yPosition); break;
                    case "avatar-costume": CreateAvatarCostumeDisplayItems(el.Attribute("name").Value, lockState, yPosition); break;
                }

                CreateIconForUnlockableItem(lockState, yPosition);
                yPosition += Unlock_Line_Height;
            }
        }

        private void CreateAreaDisplayItems(string name, Profile.LockState lockState, float yPosition)
        {
            string messageText = Translator.Translation(string.Concat("area-", lockState).ToLower()).Replace("[AREA]", Translator.Translation(name));
            if (lockState == Profile.LockState.FullVersionOnly) { messageText = Translator.Translation("buy full game to unlock"); }

            _registerObject(
                new TextContent(messageText, new Vector2(Unlock_Text_X, yPosition + Unlock_Text_Y_Offset))
                {
                    FadeFraction = 0.0f,
                    FadeFractionModifier = LockStateFadeModifier(lockState),
                    Alignment = TextWriter.Alignment.Left,
                    Scale = Unlock_Text_Scale
                });

            _registerObject(
                new ImageContent(Profile.AreaSelectionTexture(name), new Vector2(Unlock_Image_X, yPosition))
                {
                    FadeFraction = 0.0f,
                    FadeFractionModifier = LockStateFadeModifier(lockState),
                    Scale = 0.15f
                });
        }

        private float LockStateFadeModifier(Profile.LockState lockState)
        {
            return (lockState == Profile.LockState.NewlyUnlocked ? 1.0f : 0.65f);
        }

        private void CreateGoldenTicketDisplayItems(Profile.LockState lockState, float yPosition)
        {
            string messageText = Translator.Translation(string.Concat("ticket-", lockState).ToLower());
            if (lockState == Profile.LockState.FullVersionOnly) { messageText = Translator.Translation("buy full game to unlock"); }

            _registerObject(
                new TextContent(messageText, new Vector2(Unlock_Text_X, yPosition + Unlock_Text_Y_Offset))
                {
                    FadeFraction = 0.0f,
                    FadeFractionModifier = LockStateFadeModifier(lockState),
                    Alignment = TextWriter.Alignment.Left,
                    Scale = Unlock_Text_Scale
                });

            _registerObject(
                new ImageContentWithGlow("golden-ticket", new Vector2(Unlock_Image_X, yPosition), (lockState != Profile.LockState.NewlyUnlocked), 0.75f, 0.5f)
                {
                    FadeFraction = 0.0f,
                    FadeFractionModifier = LockStateFadeModifier(lockState)
                });
        }

        private void CreateAvatarComponentDisplayItems(string setName, string componentName, Profile.LockState lockState, float yPosition)
        {
            string name = string.Concat(componentName, " ", setName);
            string messageText = Translator.Translation(string.Concat("item-", lockState).ToLower()).Replace("[ITEM]", Translator.Translation(name));
            if (lockState == Profile.LockState.FullVersionOnly) { messageText = Translator.Translation("buy full game to unlock"); }

            _registerObject(
                new TextContent(messageText, new Vector2(Unlock_Text_X, yPosition + Unlock_Text_Y_Offset))
                {
                    FadeFraction = 0.0f,
                    FadeFractionModifier = LockStateFadeModifier(lockState),
                    Alignment = TextWriter.Alignment.Left,
                    Scale = Unlock_Text_Scale
                });

            AvatarComponent component = AvatarComponentManager.Component(setName, componentName);
            AvatarContent avatar = new AvatarContent(new Vector2(Unlock_Image_X, yPosition), AvatarComponentManager.DisplaySkeletonForSet(setName));

            if (setName != "body") { avatar.AddComponent(AvatarComponentManager.Component("body", "Blue")); }
            avatar.AddComponent(AvatarComponentManager.Component(setName, componentName));
            avatar.SkinSkeleton();
            avatar.FadeFraction = 0.0f;
            avatar.FadeFractionModifier = LockStateFadeModifier(lockState);
            avatar.Scale = 0.65f;
            _registerObject(avatar);
        }

        private void CreateAvatarCostumeDisplayItems(string costumeName, Profile.LockState lockState, float yPosition)
        {
            string name = string.Concat(costumeName, " costume");
            string messageText = Translator.Translation(string.Concat("item-", lockState).ToLower()).Replace("[ITEM]", Translator.Translation(name));
            if (lockState == Profile.LockState.FullVersionOnly) { messageText = Translator.Translation("buy full game to unlock"); }

            _registerObject(
                new TextContent(messageText, new Vector2(Unlock_Text_X, yPosition + Unlock_Text_Y_Offset))
                {
                    FadeFraction = 0.0f,
                    FadeFractionModifier = LockStateFadeModifier(lockState),
                    Alignment = TextWriter.Alignment.Left,
                    Scale = Unlock_Text_Scale
                });

            AvatarContent avatar = new AvatarContent(new Vector2(Unlock_Image_X, yPosition), Avatar_Costume_Display_Skeleton);
            avatar.AddComponent(AvatarComponentManager.Component("body", "Blue"));

            foreach (XElement el in AvatarComponentManager.CostumeComponents[costumeName].Elements("component"))
            {
                avatar.AddComponent(AvatarComponentManager.Component(el.Attribute("set").Value, el.Attribute("name").Value));
            }
            avatar.SkinSkeleton();
            avatar.FadeFraction = 0.0f;
            avatar.FadeFractionModifier = LockStateFadeModifier(lockState);
            avatar.Scale = 0.65f;
            _registerObject(avatar);
        }

        private void CreateIconForUnlockableItem(Profile.LockState lockState, float yPosition)
        {
            switch (lockState)
            {
                case Profile.LockState.NewlyUnlocked: CreatePopupTickForNewlyUnlockedItem(yPosition); break;
                case Profile.LockState.Locked: CreateStaticIconForUnlockableItem("icon-lock", lockState, yPosition); break;
                case Profile.LockState.Unlocked: CreateStaticIconForUnlockableItem("icon-tick", lockState, yPosition); break;
            }
        }

        private void CreatePopupTickForNewlyUnlockedItem(float yPosition)
        {
            PopupRequiringDismissal itemPopup = new PopupRequiringDismissal();
            itemPopup.MappingName = "popup-tick-icon";
            itemPopup.DisplayPosition = new Vector2(Unlock_Icon_X, yPosition);

            _newItemPopups.Add(itemPopup);
            _registerObject(itemPopup);
        }

        private void CreateStaticIconForUnlockableItem(string textureName, Profile.LockState lockState, float yPosition)
        {
            _registerObject(
                new ImageContent(textureName, new Vector2(Unlock_Icon_X, yPosition))
                {
                    FadeFraction = 0.0f,
                    FadeFractionModifier = LockStateFadeModifier(lockState),
                    Scale = Unlock_Icon_Scale
                });
        }

        public void CreateMessageContentForAreaWithNoUnlockables()
        {
        }

        public void ActivatePopups()
        {
            for (int i = 0; i < _newItemPopups.Count; i++) { _newItemPopups[i].Activate(); }
        }

        private const float Message_Top_Y = 175.0f;
        private const float Message_Line_Height = 40.0f;
        private const float Text_Scale = 0.6f;
        private const float Outline_Thickness = 4.0f;

        private const float Content_Top_Y = 350.0f;

        private const float Unlock_Line_Height = 105.0f;
        private const float Unlock_Icon_X = 250.0f;
        private const float Unlock_Icon_Scale = 0.5f;
        private const float Unlock_Text_X = 275.0f;
        private const float Unlock_Text_Y_Offset = -25.0f;
        private const float Unlock_Text_Scale = 0.6f;
        private const float Unlock_Image_X = 1255.0f;

        private const string Avatar_Costume_Display_Skeleton = "player-side";

    }
}
