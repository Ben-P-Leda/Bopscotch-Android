using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;

namespace Bopscotch.Interface.Dialogs.RaceJoinScene
{
    public class OpponentSelector : Button
    {
        private int _millisecondsSinceLastCommunication;
        private Vector2 _relativePosition;
        private bool _isActive;
        private Color _textTint;
        private OpponentSelectionState _state;
        private bool _isSuspended;

        public string ID { get; private set; }
        public string Name { get; private set; }
        public int AvatarSlot { get; private set; }

        public OpponentSelectionState State { get { return _state; } set { _state = value; UpdateSelectionState(); } }
        public bool IsInvited { get { return ((State == OpponentSelectionState.HasBeenInvited) || (State == OpponentSelectionState.IsInviting)); } }
        public bool HasAccepted { get { return ((State == OpponentSelectionState.HasAcceptedInvite) || (State == OpponentSelectionState.InvitationAccepted)); } }

        public bool Suspended
        {
            get
            {
                return _isSuspended;
            }
            set
            {
                _isSuspended = value;
                if ((_isSuspended) && (_isActive)) { SwitchOff(); _isActive = true; }
                else if ((!_isSuspended) && (_isActive)) { UpdateSelectionState(); }
            }
        }

        public bool Active { get { return _isActive; } }

        public bool TimedOut { get { return (_millisecondsSinceLastCommunication > Milliseconds_Before_Time_Out); } }

        public OpponentSelector(Vector2 relativeCenterPosition, float leftDisplayLimit, string id, string name, int avatarSlot)
            : base(Unselected_Caption_Text, false, relativeCenterPosition)
        {
            ID = id;
            Name = name;
            AvatarSlot = avatarSlot;

            Scale = Button_Scale;

            State = OpponentSelectionState.WaitingForInvite;

            _relativePosition = new Vector2(leftDisplayLimit + Text_X_Margin, relativeCenterPosition.Y - Text_Y_Offset);
            _millisecondsSinceLastCommunication = 0;
            _isActive = true;
            _isSuspended = false;
            _textTint = Color.White;
        }

        public void UpdateSelectionState()
        {
            switch (_state)
            {
                case OpponentSelectionState.WaitingForInvite: Icon = ButtonIcon.Play; IconBackgroundTint = Color.DodgerBlue; break;
                case OpponentSelectionState.HasBeenInvited: Icon = ButtonIcon.Play; IconBackgroundTint = Color.Yellow; break;
                case OpponentSelectionState.IsInviting: Icon = ButtonIcon.Play; IconBackgroundTint = Color.LawnGreen; break;
                case OpponentSelectionState.InvitationAccepted: Icon = ButtonIcon.Play; IconBackgroundTint = Color.LawnGreen; break;
            }
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            _millisecondsSinceLastCommunication += millisecondsSinceLastUpdate;

            if ((!Data.Profile.Settings.TestingRaceMode) || (Name != "test-opp"))
            {
                if (_millisecondsSinceLastCommunication > Milliseconds_Before_Switch_Off) { SwitchOff(); }
                else if (!_isActive) { _state = OpponentSelectionState.WaitingForInvite;  SwitchOn(); }
            }
        }

        private void SwitchOff()
        {
            _isActive = false;
            _textTint = Color.Gray;
            Icon = ButtonIcon.None;
            IconBackgroundTint = Color.Gray;
        }

        private void SwitchOn()
        {
            _isActive = true;
            _textTint = Color.White;
            UpdateSelectionState();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            TextWriter.Write(Name, spriteBatch, ParentPosition + _relativePosition, _textTint, Color.Black, 2.0f, Text_Render_Depth, 
                TextWriter.Alignment.Left);
        }

        public bool Selected(Vector2 touchLocation)
        {
            Rectangle container = new Rectangle(0, (int)(ParentPosition.Y + _relativePosition.Y - (Height / 2.0f)),
                (int)Definitions.Back_Buffer_Width, (int)(ParentPosition.Y + _relativePosition.Y + (Height / 2.0f)));

            return ((_isActive) && (!_isSuspended) && (container.Contains((int)touchLocation.X, (int)touchLocation.Y)));
        }

        public void ResetCommsTimeoutCounter()
        {
            _millisecondsSinceLastCommunication = 0;
        }

        public enum OpponentSelectionState
        {
            WaitingForInvite,
            HasBeenInvited,
            IsInviting,
            InvitationAccepted,
            HasAcceptedInvite
        }

        private const string Unselected_Caption_Text = "Race!";
        private const float Button_Scale = 0.6f;
        private const int Milliseconds_Before_Switch_Off = 7500;
        private const int Milliseconds_Before_Time_Out = 10000;
        private const float Text_X_Margin = 30.0f;
        private const float Text_Y_Offset = 60.0f;
        private const float Text_Render_Depth = 0.1f;

        public const float Height = 100.0f;
    }
}
