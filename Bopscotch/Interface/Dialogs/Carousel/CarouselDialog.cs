using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;

using Bopscotch.Input;

namespace Bopscotch.Interface.Dialogs.Carousel
{
    public class CarouselDialog : ButtonDialog
    {
        public delegate void ActionHandler(string selectedButtonCaption);
        private float _rotationStep;
        private float _selectorRotation;
        private float _targetRotation;
        private Scene.ObjectRegistrationHandler _registerObject;
        private Scene.ObjectUnregistrationHandler _unregisterObject;

        protected List<ICarouselDialogItem> _items;
        protected Range _itemRenderDepths;
        protected Range _itemScales;
        protected List<string> _nonSpinButtonCaptions;
        protected Dictionary<string, string> _buttonSoundEffectOverrides;

        public float TopYWhenInactive { private get; set; }
        public Vector2 CarouselCenter { private get; set; }
        public Vector2 CarouselRadii { private get; set; }
        public float RotationSpeedInDegrees { private get; set; }

        public ActionHandler ActionButtonPressHandler { private get; set; }
        protected List<string> _captionsForButtonsNotActivatedByGamepadStartButton;

        public string Selection { get { return _items[SelectedItem].SelectionValue; } }

        protected int SelectedItem { get; private set; }
        protected bool Rotating { get; private set; }

        public override bool Visible
        {
            get { return base.Visible; }
            set { base.Visible = value; if (_items != null) { for (int i = 0; i < _items.Count; i++) { _items[i].Visible = value; } } }
        }

        public CarouselDialog(Scene.ObjectRegistrationHandler registrationHandler, Scene.ObjectUnregistrationHandler unregistrationHandler)
            : base()
        {
            SelectedItem = 0;
            Rotating = false;

            _selectorRotation = 0.0f;
            _targetRotation = _selectorRotation;

            _items = new List<ICarouselDialogItem>();
            _registerObject = registrationHandler;
            _unregisterObject = unregistrationHandler;

            _nonSpinButtonCaptions = new List<string>();
            _nonSpinButtonCaptions.Add("Back");
            _nonSpinButtonCaptions.Add("Select");
            _buttonSoundEffectOverrides = new Dictionary<string, string>();

            _captionsForButtonsNotActivatedByGamepadStartButton = new List<string>();

            RotationSpeedInDegrees = Default_Rotation_Speed_In_Degrees;
        }

        protected virtual void SetupButtonLinkagesAndDefaultValues()
        {
            SetMovementLinksForButton("previous", "", "Back", "", "next");
            SetMovementLinksForButton("next", "", "Select", "previous", "");
            SetMovementLinksForButton("Back", "previous", "", "", "Select");
            SetMovementLinksForButton("Select", "next", "", "Back", "");

            _defaultButtonCaption = "Select";
            _activeButtonCaption = "Select";
            _cancelButtonCaption = "Back";
        }

        public override void Reset()
        {
            base.Reset();
            WorldPosition = new Vector2(WorldPosition.X, TopYWhenInactive);
        }

        protected void AddItem(ICarouselDialogItem item)
        {
            if (!_items.Contains(item))
            {
                item.CarouselCenter = CarouselCenter;
                item.CarouselRadii = CarouselRadii;
                item.DepthRange = _itemRenderDepths;
                item.ScaleRange = _itemScales;

                _items.Add(item);
                _registerObject(item);
            }
        }

        protected void FlushItems()
        {
            foreach (ICarouselDialogItem item in _items) { _unregisterObject(item); }
            _items.Clear();
        }

        protected void InitializeForSpin()
        {
            _rotationStep = MathHelper.TwoPi / _items.Count;

            for (int i = 0; i < _items.Count; i++)
            {
                _items[i].AngleOffsetAtZeroRotation = MathHelper.PiOver2 + (i * _rotationStep);
            }
        }

        protected void SetInitialSelection(string selection)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].SelectionValue == selection) { SetInitialSelection(i); break; }
            }
        }

        protected void SetInitialSelection(int selection)
        {
            SelectedItem = selection;
            _targetRotation = -(_rotationStep * selection);
            _selectorRotation = -(_rotationStep * selection);
        }

        private void UpdateItemPositions()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                _items[i].PositionRelativeToDialog(WorldPosition, _selectorRotation);
            }
        }

        protected override void CheckForAndHandleSelection(Input.InputProcessorBase inputSource)
        {
            if ((inputSource.SelectionTriggered) && (!Rotating))
            {
                if (inputSource.SelectionLocation != Vector2.Zero) { base.CheckForAndHandleSelection(inputSource); }
                else if (_nonSpinButtonCaptions.Contains(_activeButtonCaption)) 
                {
                    if ((!(inputSource is GamePadInputProcessor)) ||
                        (!((GamePadInputProcessor)inputSource).SelectionIsByStartButton) ||
                        (!_captionsForButtonsNotActivatedByGamepadStartButton.Contains(_activeButtonCaption)))
                    {
                        ActionButtonPressHandler(_activeButtonCaption);
                        if (!string.IsNullOrEmpty(ActivateSelectionSoundEffectName)) { SoundEffectManager.PlayEffect(ActivateSelectionSoundEffectName); }
                    }
                }
                else { HandleStepSelection(_activeButtonCaption); }
            }
        }

        protected override bool HandleButtonTouch(string buttonCaption)
        {
            if (_nonSpinButtonCaptions.Contains(buttonCaption)) 
            { 
                ActionButtonPressHandler(buttonCaption);

                string soundEffectName = ActivateSelectionSoundEffectName;
                if (_buttonSoundEffectOverrides.ContainsKey(buttonCaption)) { soundEffectName = _buttonSoundEffectOverrides[buttonCaption]; }

                if (!string.IsNullOrEmpty(soundEffectName)) { SoundEffectManager.PlayEffect(soundEffectName); }
            }

            HandleStepSelection(buttonCaption);
            return false;
        }

        protected virtual void HandleStepSelection(string buttonCaption)
        {
            if (buttonCaption == "previous") { _targetRotation += _rotationStep; Rotating = true; SoundEffectManager.PlayEffect("carousel-spin"); }
            if (buttonCaption == "next") { _targetRotation -= _rotationStep; Rotating = true; SoundEffectManager.PlayEffect("carousel-spin"); }
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            base.Update(millisecondsSinceLastUpdate);

            if (Rotating) { UpdateRotation(); }

            UpdateItemPositions();
        }

        private void UpdateRotation()
        {
            if (StepWillEndRotation) { HandleRotationComplete(); }
            else { _selectorRotation += (MathHelper.ToRadians(RotationSpeedInDegrees) * Math.Sign(_targetRotation - _selectorRotation)); }
        }

        private bool StepWillEndRotation
        {
            get
            {
                if ((_targetRotation < _selectorRotation) && (_targetRotation >= _selectorRotation - MathHelper.ToRadians(RotationSpeedInDegrees)))
                {
                    return true;
                }
                else if ((_targetRotation > _selectorRotation) && (_targetRotation <= _selectorRotation + MathHelper.ToRadians(RotationSpeedInDegrees)))
                {
                    return true;
                }

                return false;
            }
        }

        protected virtual void HandleRotationComplete()
        {
            SelectedItem = (SelectedItem - Math.Sign(_targetRotation - _selectorRotation) + _items.Count) % _items.Count;
            _targetRotation = Utility.RectifyAngle(_targetRotation);

            if ((_targetRotation < Rotation_Zero_Point_Tolerance) || (_targetRotation > MathHelper.TwoPi - Rotation_Zero_Point_Tolerance))
            {
                _targetRotation = 0.0f;
            }

            _selectorRotation = _targetRotation;
            Rotating = false;
        }

        public void ClearLastSelection()
        {
            SelectedItem = 0;
            _selectorRotation = 0.0f;
            _targetRotation = 0.0f;
        }

        private const float Default_Rotation_Speed_In_Degrees = 5.0f;
        private const float Rotation_Zero_Point_Tolerance = 0.0001f;
    }
}
