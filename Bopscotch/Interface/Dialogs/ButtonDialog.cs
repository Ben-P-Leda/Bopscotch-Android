using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;
using Leda.Core.Motion;
using Leda.Core.Motion.Engines;

using Bopscotch.Input;

namespace Bopscotch.Interface.Dialogs
{
    public class ButtonDialog : ISimpleRenderable, IMobile
    {
        public delegate void ButtonSelectionHandler(string selectedButtonCaption);

        private Rectangle _frame;
        private Vector2 _worldPosition;
        private float _topYWhenActive;
        private BounceEntryMotionEngine _entryMotionEngine;
        private BounceExitMotionEngine _exitMotionEngine;

        private int _millisecondsSinceLastMovement;
        private MovementDirection _lastMovementDirection;

        private List<string> _buttonCaptions;

        protected Dictionary<string, Button> _buttons;
        protected string _defaultButtonCaption;
        protected string _cancelButtonCaption;
        protected string _activeButtonCaption;
        protected string _boxCaption;

        public string ChangeSelectionSoundEffectName { private get; set; }
        public string ActivateSelectionSoundEffectName { protected get; set; }

        public int Height
        {
            get { return _frame.Height; }
            set { _frame.Height = value; if (_topYWhenActive == 0.0f) { TopYWhenActive = Definitions.Back_Buffer_Center.Y - (value / 2.0f); } }
        }
        public float TopYWhenActive { set { _topYWhenActive = value; } }
        public List<InputProcessorBase> InputSources { get; set; }
        public ButtonSelectionHandler SelectionCallback { private get; set; }
        public ButtonSelectionHandler ExitCallback { private get; set; }

        public Vector2 WorldPosition
        {
            get { return _worldPosition; }
            set { _worldPosition = value; foreach (KeyValuePair<string, Button> kvp in _buttons) { kvp.Value.ParentPosition = value; } }
        }
        public bool WorldPositionIsFixed { get { return false; } }

        public int RenderLayer { get { return Render_Layer; } set { } }

        public virtual bool Visible { get; set; }
        public bool Active { get; private set; }

        public IMotionEngine MotionEngine { get; private set; }

        public ButtonDialog()
            : base()
        {
            _buttonCaptions = new List<string>();
            _buttons = new Dictionary<string, Button>();
            _worldPosition = Vector2.Zero;
            _frame = new Rectangle(0, 0, (int)Definitions.Back_Buffer_Width, 0);
            _defaultButtonCaption = "";
            _cancelButtonCaption = "";

            _entryMotionEngine = new BounceEntryMotionEngine();
            _entryMotionEngine.ObjectToTrack = this;
			_entryMotionEngine.RecoilMultiplier = Recoil_Multiplier;
            _entryMotionEngine.CompletionCallback = HandleDialogEntryCompletion;

            _exitMotionEngine = new BounceExitMotionEngine();
            _exitMotionEngine.ObjectToTrack = this;
			_exitMotionEngine.RecoilMultiplier = Recoil_Multiplier;
            _exitMotionEngine.CompletionCallback = HandleDialogExitCompletion;

            MotionEngine = null;
            Active = false;
            Visible = false;
            TopYWhenActive = 0.0f;
            SelectionCallback = null;
            ExitCallback = null;

            InputSources = new List<InputProcessorBase>();

            _boxCaption = "";

            ChangeSelectionSoundEffectName = Default_Selection_Change_Sound_Effect;
            ActivateSelectionSoundEffectName = Default_Selection_Activate_Sound_Effect;
        }

        protected virtual void HandleDialogEntryCompletion()
        {
            Active = true;
        }

        protected virtual void HandleDialogExitCompletion()
        {
            Visible = false;
            MotionEngine = null;

            if (ExitCallback != null) { ExitCallback(_activeButtonCaption); }
        }

        public void AddButton(string caption, Vector2 relativeCenterPosition, Button.ButtonIcon icon, Color iconContainerTint)
        {
            AddButton(caption, relativeCenterPosition, icon, iconContainerTint, -1.0f);
        }

        public void AddButton(string caption, Vector2 relativeCenterPosition, Button.ButtonIcon icon, Color iconContainerTint, float scale)
        {
            AddButton(caption, relativeCenterPosition, icon, iconContainerTint, scale, false);
        }

        private void AddButton(string caption, Vector2 relativeCenterPosition, Button.ButtonIcon icon, Color iconContainerTint, float scale, bool hideCaption)
        {
            Button newButton = new Button(caption, hideCaption, relativeCenterPosition);
            newButton.IconBackgroundTint = iconContainerTint;
            newButton.Icon = icon;

            if (scale > 0.0f) { newButton.Scale = scale; }

            _buttonCaptions.Add(caption);
            _buttons.Add(caption, newButton);

            if (string.IsNullOrEmpty(_defaultButtonCaption)) { _defaultButtonCaption = caption; }
        }

        public void AddIconButton(string pressValue, Vector2 relativeCenterPosition, Button.ButtonIcon icon, Color containerTint)
        {
            AddButton(pressValue, relativeCenterPosition, icon, containerTint, -1.0f, true);
        }

        public void AddIconButton(string pressValue, Vector2 relativeCenterPosition, Button.ButtonIcon icon, Color containerTint, float scale)
        {
            AddButton(pressValue, relativeCenterPosition, icon, containerTint, scale, true);
        }

        public void SetMovementLinksForButton(string targetCaption, string buttonAboveCaption, string buttonBelowCaption,
            string buttonToLeftCaption, string buttonToRightCaption)
        {
            if (_buttons.ContainsKey(targetCaption))
            {
                _buttons[targetCaption].CaptionOfButtonActivatedByMovingUp = buttonAboveCaption;
                _buttons[targetCaption].CaptionOfButtonActivatedByMovingDown = buttonBelowCaption;
                _buttons[targetCaption].CaptionOfButtonActivatedByMovingLeft = buttonToLeftCaption;
                _buttons[targetCaption].CaptionOfButtonActivatedByMovingRight = buttonToRightCaption;
            }
        }

        public void ClearButtons()
        {
            _buttonCaptions.Clear();
            _buttons.Clear();
        }

        public void Initialize()
        {
        }

        public virtual void Reset()
        {
            WorldPosition = new Vector2(0.0f, Definitions.Back_Buffer_Height + Entry_Margin);

            ActivateButton(_defaultButtonCaption);

            _activeButtonCaption = _defaultButtonCaption;
            _lastMovementDirection = MovementDirection.None;
            _millisecondsSinceLastMovement = 0;

            Active = false;
        }

        protected void EnableButton(string caption)
        {
            if (_buttons.ContainsKey(caption)) { _buttons[caption].Disabled = false; }
        }

        protected void DisableButton(string caption)
        {
            if (_buttons.ContainsKey(caption)) { _buttons[caption].Disabled = true; }
        }

        protected virtual void ActivateButton(string caption)
        {
            if ((_buttons.ContainsKey(caption)) && (!_buttons[caption].Disabled))
            {
                foreach (KeyValuePair<string, Button> kvp in _buttons)
                {
                    if (kvp.Key == caption) { kvp.Value.Activate(); }
                    else { kvp.Value.Deactivate(); }
                }

                _activeButtonCaption = caption;
            }
        }

        public virtual void Activate()
        {
            Activate(false);
        }

        public virtual void Activate(bool skipEntrySequence)
        {
            Reset();

            _exitMotionEngine.TargetWorldPosition = new Vector2(0.0f, WorldPosition.Y);

            if (skipEntrySequence)
            {
                WorldPosition = new Vector2(WorldPosition.X, _topYWhenActive);
                MotionEngine = null;
                Active = true;
            }
            else
            {
                _entryMotionEngine.TargetWorldPosition = new Vector2(0.0f, _topYWhenActive);
                _entryMotionEngine.Activate();
                MotionEngine = _entryMotionEngine;
            }

            Visible = true;
        }

        public virtual void Update(int millisecondsSinceLastUpdate)
        {
            if (MotionEngine != null) { MotionEngine.Update(millisecondsSinceLastUpdate); }
            if (MotionEngine != null) { WorldPosition += MotionEngine.Delta; }

            if (Active) { CheckForAndHandleInput(millisecondsSinceLastUpdate); }
        }

        private void CheckForAndHandleInput(int millisecondsSinceLastUpdate)
        {
            for (int i = 0; i < InputSources.Count; i++) { CheckForAndHandleInputFromSingleSource(InputSources[i], millisecondsSinceLastUpdate); }
        }

        private void CheckForAndHandleInputFromSingleSource(InputProcessorBase inputSource, int millisecondsSinceLastUpdate)
        {
            if ((inputSource.ProcessorType == InputProcessorBase.InputProcessorType.Keyboard) || 
                (inputSource.ProcessorType == InputProcessorBase.InputProcessorType.Gamepad))
            {
                CheckDirectionalMovementAndUpdateActiveButton(inputSource, millisecondsSinceLastUpdate);
            }

            CheckForAndHandleSelection(inputSource);
        }

        private void CheckDirectionalMovementAndUpdateActiveButton(InputProcessorBase inputSource, int millisecondsSinceLastUpdate)
        {
            _millisecondsSinceLastMovement = Math.Min(_millisecondsSinceLastMovement + millisecondsSinceLastUpdate, Movement_Repeat_Time_In_Milliseconds);

            MovementDirection direction = MovementDirection.None;
            if (inputSource.MoveUp) { direction = MovementDirection.Up; }
            else if (inputSource.MoveDown) { direction = MovementDirection.Down; }
            else if (inputSource.MoveLeft) { direction = MovementDirection.Left; }
            else if (inputSource.MoveRight) { direction = MovementDirection.Right; }

            if ((direction == _lastMovementDirection) && (_millisecondsSinceLastMovement < Movement_Repeat_Time_In_Milliseconds))
            {
                direction = MovementDirection.None;
            }

            if (direction != MovementDirection.None)
            {
                switch (direction)
                {
                    case MovementDirection.Up: AttemptToActivateButton(_buttons[_activeButtonCaption].CaptionOfButtonActivatedByMovingUp); break;
                    case MovementDirection.Down: AttemptToActivateButton(_buttons[_activeButtonCaption].CaptionOfButtonActivatedByMovingDown); break;
                    case MovementDirection.Left: AttemptToActivateButton(_buttons[_activeButtonCaption].CaptionOfButtonActivatedByMovingLeft); break;
                    case MovementDirection.Right: AttemptToActivateButton(_buttons[_activeButtonCaption].CaptionOfButtonActivatedByMovingRight); break;
                }

                _lastMovementDirection = direction;
                _millisecondsSinceLastMovement = 0;
            }
        }

        private void AttemptToActivateButton(string activatingButtonCaption)
        {
            if (!string.IsNullOrEmpty(activatingButtonCaption)) 
            {
                if (!string.IsNullOrEmpty(ChangeSelectionSoundEffectName)) { SoundEffectManager.PlayEffect(ChangeSelectionSoundEffectName); }
                ActivateButton(activatingButtonCaption); 
            }
        }

        protected virtual void CheckForAndHandleSelection(Input.InputProcessorBase inputSource)
        {
            bool selectionMade = inputSource.SelectionTriggered;

            if ((selectionMade) && (inputSource.SelectionLocation != Vector2.Zero))
            {
                selectionMade = CheckForSelectionAtTouchLocation(inputSource.SelectionLocation);
            }

            if (selectionMade)
            {
                Dismiss();
                if (SelectionCallback != null) { SelectionCallback(_activeButtonCaption); }
                if (!string.IsNullOrEmpty(ActivateSelectionSoundEffectName)) { SoundEffectManager.PlayEffect(ActivateSelectionSoundEffectName); }
            }
        }

        private bool CheckForSelectionAtTouchLocation(Vector2 location)
        {
            bool buttonTouched = false;

            foreach (KeyValuePair<string, Button> kvp in _buttons)
            {
                if (kvp.Value.ContainsLocation(location))
                {
                    if (!kvp.Value.Disabled) { buttonTouched = HandleButtonTouch(kvp.Key); }
                    break;
                }
            }

            return buttonTouched;
        }

        protected virtual bool HandleButtonTouch(string buttonCaption)
        {
            _activeButtonCaption = buttonCaption;
            return true;
        }

        protected virtual void Dismiss()
        {
            Active = false;

            _exitMotionEngine.Activate();
            MotionEngine = _exitMotionEngine;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if ((WorldPosition.Y + Height > 0.0f) && (WorldPosition.Y < Definitions.Back_Buffer_Height))
            {
                spriteBatch.Draw(TextureManager.Textures[Trim_Texture_Name], GameBase.ScreenPosition(WorldPosition), null, Color.White, 
                    0.0f, Vector2.Zero, GameBase.ScreenScale(), SpriteEffects.None, Trim_Render_Depth);

                spriteBatch.Draw(TextureManager.Textures[Trim_Texture_Name], GameBase.ScreenPosition(WorldPosition + new Vector2(0.0f, Height)),
                    null, Color.White, 0.0f, new Vector2(0.0f, TextureManager.Textures[Trim_Texture_Name].Height), GameBase.ScreenScale(), SpriteEffects.None, 
                    Trim_Render_Depth);

                Vector2 topLeft = GameBase.ScreenPosition(_frame.X + WorldPosition.X, _frame.Y + WorldPosition.Y);
                spriteBatch.Draw(TextureManager.Textures[Background_Texture_Name],
                    new Rectangle((int)topLeft.X, (int) topLeft.Y, (int)GameBase.ScreenScale(_frame.Width), (int)GameBase.ScreenScale(_frame.Height)), 
                    null, Color.Lerp(Color.Black, Color.Transparent, 0.5f), 0.0f, Vector2.Zero, SpriteEffects.None, Background_Render_Depth);

                int buttonCount = _buttonCaptions.Count;
                for (int i = 0; i < buttonCount; i++) { _buttons[_buttonCaptions[i]].Draw(spriteBatch); }

                if (!string.IsNullOrEmpty(_boxCaption))
                {
                    TextWriter.Write(Translator.Translation(_boxCaption), spriteBatch, new Vector2(Definitions.Back_Buffer_Center.X, WorldPosition.Y),
                        Color.White, Color.Black, 3.0f, Trim_Render_Depth, TextWriter.Alignment.Center);
                }
            }
        }

        public void Cancel()
        {
            if ((!string.IsNullOrEmpty(_cancelButtonCaption)) && (Active))
            {
                DismissWithReturnValue(_cancelButtonCaption);
            }
        }

        public void DismissWithReturnValue(string valueToReturn)
        {
            _activeButtonCaption = valueToReturn;

            Dismiss();
            if (SelectionCallback != null) { SelectionCallback(valueToReturn); }
        }

        private enum MovementDirection
        {
            None,
            Up,
            Down,
            Left,
            Right
        }

        private const string Background_Texture_Name = "pixel";
        private const float Background_Render_Depth = 0.15f;
        private const string Trim_Texture_Name = "dialog-presenter";
        private const float Trim_Render_Depth = 0.14f;
        private const int Render_Layer = 4;
        private const int Recoil_Multiplier = 5;

        private const int Movement_Repeat_Time_In_Milliseconds = 750;
        private const string Default_Selection_Change_Sound_Effect = "menu-move";
        private const string Default_Selection_Activate_Sound_Effect = "menu-select";

        protected const float Entry_Margin = 100.0f;
    }
}
