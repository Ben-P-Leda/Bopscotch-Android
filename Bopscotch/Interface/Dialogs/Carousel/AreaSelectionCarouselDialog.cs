using System.Collections.Generic;
using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Gamestate_Management;
using Leda.Core.Timing;

using Bopscotch.Input;

namespace Bopscotch.Interface.Dialogs.Carousel
{
    public class AreaSelectionCarouselDialog : CarouselDialog
    {
        protected List<XElement> _dataSource;
        protected Color _textTint;
        protected Timer _textTransitionTimer;

        public AreaSelectionCarouselDialog(Scene.ObjectRegistrationHandler registrationHandler, Scene.ObjectUnregistrationHandler unregistrationHandler)
            : base(registrationHandler, unregistrationHandler)
        {
            RotationSpeedInDegrees = Rotation_Speed_In_Degrees;
            _itemRenderDepths = new Range(Minimum_Item_Render_Depth, Maximum_Item_Render_Depth);
            _itemScales = new Range(Minimum_Item_Scale, Maximum_Item_Scale);

            CreateButtons();

            ActionButtonPressHandler = HandleNonSpinButtonAction;

            _textTransitionTimer = new Timer("");
            GlobalTimerController.GlobalTimer.RegisterUpdateCallback(_textTransitionTimer.Tick);
            _textTransitionTimer.NextActionDuration = 1;

            _textTint = Color.White;
        }

        protected virtual void CreateButtons()
        {
            _cancelButtonCaption = "Back";
        }

        protected virtual void HandleNonSpinButtonAction(string buttonCaption)
        {
        }

        public override void Activate()
        {
            FlushItems();
            Visible = true;

            base.Activate();

            GetAreaData();
            CreateCarouselContent();

            InitializeForSpin();
        }

        protected virtual void GetAreaData()
        {
            _dataSource = Data.Profile.SimpleAreaData;
        }

        private void CreateCarouselContent()
        {
            foreach (XElement el in _dataSource) { AddArea(el.Attribute("name").Value, el.Attribute("texture").Value, (bool)el.Attribute("locked")); }
        }

        protected void AddArea(string areaName, string textureName, bool locked)
        {
            CarouselFlatImage area = new CarouselAreaImage(areaName, "thumb-" + textureName, locked);
            area.RenderLayer = RenderLayer;
            area.MasterScale = 1.0f;

            AddItem(area);
        }

        protected override void HandleDialogExitCompletion()
        {
            FlushItems();

            base.HandleDialogExitCompletion();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            DrawAreaDetails(spriteBatch);
        }

        protected virtual void DrawAreaDetails(SpriteBatch spriteBatch)
        {
        }

        protected Color TransitionTint(Color unfadedColour)
        {
            if (Rotating) { return Color.Lerp(unfadedColour, Color.Transparent, _textTransitionTimer.CurrentActionProgress); }
            return Color.Lerp(Color.Transparent, unfadedColour, _textTransitionTimer.CurrentActionProgress);
        }

        protected override void HandleRotationComplete()
        {
            base.HandleRotationComplete();

            for (int i = 0; i < _dataSource.Count; i++)
            {
                if (_dataSource[i].Attribute("name").Value == Selection)
                {
                    _textTint = ((bool)_dataSource[i].Attribute("locked") ? Color.Gray : Color.White);
                }
            }
        }

        private const float Maximum_Item_Render_Depth = 0.1f;
        private const float Minimum_Item_Render_Depth = 0.05f;
        private const float Maximum_Item_Scale = 1.0f;
        private const float Minimum_Item_Scale = 0.75f;
        private const float Rotation_Speed_In_Degrees = 10.0f;
    }
}
