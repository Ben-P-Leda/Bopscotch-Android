using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Asset_Management;
using Leda.Core.Timing;

namespace Bopscotch.Interface.Dialogs
{
    public class Button
    {
        private string _caption;
        private Vector2 _relativeCenterPosition;
        private Vector2 _topLeft;
        private float _scale;
        private bool _hideCaption;

        private Rectangle _detectionArea;
        private Vector2 _textContainerPosition;
        private Vector2 _textPosition;
        private Vector2 _iconPosition;
        private Vector2 _highlightPosition;
        private Timer _highlightTimer;
        private string _highlightTextureName;
        private State _currentState;

        private string _translatedCaption;
        private int _iconIndex;

        public Vector2 ParentPosition { protected get; set; }
        public Color IconBackgroundTint { private get; set; }
        public ButtonIcon Icon { set { _iconIndex = (int)value; } }
        public float Scale { set { _scale = value; CalculateRenderAndSelectionMetrics(); } }

        public Rectangle Area { get { return _detectionArea; } }
        public string ReturnValue { get { return _caption; } }

        public string CaptionOfButtonActivatedByMovingUp { get; set; }
        public string CaptionOfButtonActivatedByMovingDown { get; set; }
        public string CaptionOfButtonActivatedByMovingLeft { get; set; }
        public string CaptionOfButtonActivatedByMovingRight { get; set; }

        public bool Disabled { get; set; }

        public Button(string caption, bool hideCaption, Vector2 relativeCenterPosition)
        {
            ParentPosition = Vector2.Zero;

            _caption = caption;
            _translatedCaption = "";
            _hideCaption = hideCaption;
            _relativeCenterPosition = relativeCenterPosition;
            _currentState = State.Inactive;

            IconBackgroundTint = Color.Blue;
            Icon = Button.ButtonIcon.None;
            _scale = Default_Scale;

            _highlightTimer = new Timer("button-fade-timer", HandleTimerCompletion);
            GlobalTimerController.GlobalTimer.RegisterUpdateCallback(_highlightTimer.Tick);

            _highlightTextureName = hideCaption ? No_Caption_Highlight_Texture_Name : Highlight_Texture_Name;

            CalculateRenderAndSelectionMetrics();

            CaptionOfButtonActivatedByMovingUp = "";
            CaptionOfButtonActivatedByMovingDown = "";
            CaptionOfButtonActivatedByMovingLeft = "";
            CaptionOfButtonActivatedByMovingRight = "";

            Disabled = false;
        }

        private void HandleTimerCompletion()
        {
            if (_currentState == State.Activating) { _currentState = State.Active; }
            else if (_currentState == State.Deactivating) { _currentState = State.Inactive; }
        }

        private void CalculateRenderAndSelectionMetrics()
        {
            if (_hideCaption)
            {
                _topLeft = _relativeCenterPosition - ((new Vector2(Total_Height, Total_Height) / 2.0f) * _scale);

                _detectionArea = new Rectangle((int)_topLeft.X, (int)_topLeft.Y, (int)(Total_Height * _scale), (int)(Total_Height * _scale));
                _detectionArea.Inflate(Detection_Area_Margin, Detection_Area_Margin);
            }
            else
            {
                _topLeft = _relativeCenterPosition - ((new Vector2(Total_Width, Total_Height) / 2.0f) * _scale);

                _detectionArea = new Rectangle((int)_topLeft.X, (int)_topLeft.Y, (int)(Total_Width * _scale), (int)(Total_Height * _scale));
                _textContainerPosition = _topLeft + (new Vector2(Text_Container_Horizontal_Offset, Text_Container_Vertical_Offset) * _scale);
                _textPosition = _textContainerPosition + (new Vector2((Total_Width - Text_Container_Horizontal_Offset) / 2.0f, Text_Vertical_Offset) * _scale);
            }

            _iconPosition = _topLeft + (new Vector2(Icon_Offset) * _scale);
            _highlightPosition = _topLeft - (new Vector2(Highlight_Offset) * _scale);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TextureManager.Textures[_highlightTextureName], GameBase.ScreenPosition(ParentPosition + _highlightPosition), 
                null, TintFromActivationState, 0.0f, Vector2.Zero, GameBase.ScreenScale(_scale), SpriteEffects.None, Highlight_Render_Depth);

            spriteBatch.Draw(TextureManager.Textures[Icon_Container_Texture], GameBase.ScreenPosition(ParentPosition + _topLeft), null,
                TintFromEnabledState(IconBackgroundTint), 0.0f, Vector2.Zero, GameBase.ScreenScale(_scale), SpriteEffects.None, Icon_Container_Render_Depth);

            if (_iconIndex > -1)
            {
                spriteBatch.Draw(TextureManager.Textures[Icon_Texture], GameBase.ScreenPosition(ParentPosition + _iconPosition), 
                    IconFrame(_iconIndex), Color.Black, 0.0f, Vector2.Zero, GameBase.ScreenScale(_scale), SpriteEffects.None, Icon_Render_Depth);
            }

            if (!_hideCaption)
            {
                spriteBatch.Draw(TextureManager.Textures[Text_Container_Texture], GameBase.ScreenPosition(ParentPosition + _textContainerPosition), 
                    null, TintFromEnabledState(Color.White), 0.0f, Vector2.Zero, GameBase.ScreenScale(_scale), SpriteEffects.None, Text_Container_Render_Depth);

                if (string.IsNullOrEmpty(_translatedCaption)) { _translatedCaption = Translator.Translation(_caption); }

                TextWriter.Write(_translatedCaption, spriteBatch, ParentPosition + _textPosition, TintFromEnabledState(Color.Black),
                    _scale, Content_Render_Depth, TextWriter.Alignment.Center);
            }
        }

        private Color TintFromActivationState
        {
            get
            {
                if (_currentState == State.Activating) { return Color.Lerp(Color.Transparent, Color.White, _highlightTimer.CurrentActionProgress); }
                if (_currentState == State.Deactivating) { return Color.Lerp(Color.White, Color.Transparent, _highlightTimer.CurrentActionProgress); }
                if (_currentState == State.Active) { return Color.White; }
                return Color.Transparent;
            }
        }

        private Color TintFromEnabledState(Color enabledColour)
        {
            if (Disabled) { return Color.Lerp(enabledColour, Color.Black, 0.5f); }
            return enabledColour;
        }

        public bool ContainsLocation(Vector2 location)
        {
            Rectangle detectionArea = new Rectangle((int)ParentPosition.X + _detectionArea.X, (int)ParentPosition.Y + _detectionArea.Y,
                _detectionArea.Width, _detectionArea.Height);

            return detectionArea.Contains((int)location.X, (int)location.Y);
        }

        public void Activate()
        {
            if ((_currentState == State.Deactivating) || (_currentState == State.Inactive))
            {
                _currentState = State.Activating;
                _highlightTimer.NextActionDuration = Transition_Duration;
            }
        }

        public void Deactivate()
        {
            if ((_currentState == State.Activating) || (_currentState == State.Active))
            {
                _currentState = State.Deactivating;
                _highlightTimer.NextActionDuration = Transition_Duration;
            }
        }

        private enum State
        {
            Inactive,
            Activating,
            Active,
            Deactivating
        }

        public enum ButtonIcon
        {
            None = -1,
            Play = 0,
            Quit = 1,
            Pause = 2,
            Previous = 3,
            Next = 4,
            Back = 5,
            Help = 6,
            Options = 7,
            Race = 8,
            Adventure = 9,
            Store = 10,
            Character = 11,
            Ticket = 12,
            Music = 13,
            Sound = 14,
            Tick = 15,
            Trash = 16,
            Facebook = 17,
            Twitter = 18,
            Website = 19,
            Rate = 20
        }

        public static Rectangle IconFrame(ButtonIcon icon) { return IconFrame((int)icon); }

        public static Rectangle IconFrame(int iconIndex)
        {
            return new Rectangle(iconIndex * Definitions.Button_Icon_Pixel_Size, 0,
                Definitions.Button_Icon_Pixel_Size, Definitions.Button_Icon_Pixel_Size);
        }

        private const int Total_Width = 625;
        private const int Total_Height = 125;

        private const float Default_Scale = 0.8f;

        private const string Text_Container_Texture = "button-text-container";
        private const float Text_Container_Horizontal_Offset = 62.0f;
        private const float Text_Container_Vertical_Offset = 16.0f;
        private const float Text_Container_Render_Depth = 0.1f;

        private const string Icon_Container_Texture = "button-icon-container";
        private const float Icon_Container_Render_Depth = 0.09f;

        private const float Content_Render_Depth = 0.08f;
#if WINDOWS_PHONE
        private const float Text_Vertical_Offset = -12.0f;
#else
        private const float Text_Vertical_Offset = -16.0f;
#endif
        private const string Icon_Texture = "button-icons";
        private const float Icon_Offset = 32.0f;
        private const float Icon_Render_Depth = 0.085f;

        private const int Detection_Area_Margin = 10;

        private const int Transition_Duration = 250;
        private const string Highlight_Texture_Name = "button-highlight";
        private const string No_Caption_Highlight_Texture_Name = "button-highlight-no-caption";
        private const float Highlight_Offset = 31.0f;
        private const float Highlight_Render_Depth = 0.11f;
    }
}
