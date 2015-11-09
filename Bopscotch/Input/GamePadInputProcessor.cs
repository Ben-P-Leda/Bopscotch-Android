using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Bopscotch.Gameplay.Objects.Display.Race;

namespace Bopscotch.Input
{
    public class GamePadInputProcessor : InputProcessorBase
    {
        private bool _actionHeld;
        private bool _selectHeld;
        private PlayerIndex _playerIndex;

        public bool SelectionIsByStartButton;
        public override InputProcessorType ProcessorType { get { return InputProcessorType.Gamepad; } }

        public GamePadInputProcessor(PlayerIndex playerIndex)
            : base()
        {
            _playerIndex = playerIndex;
        }

        public override void Update(int millsecondsSinceLastUpdate)
        {
            ResetPublicProperties();
            GamePadState padState = GamePad.GetState(_playerIndex);

            if (padState.IsConnected)
            {
                MoveUp = ((padState.ThumbSticks.Left.Y < -Data.Profile.Settings.ControlSensitivity) || (padState.DPad.Up == ButtonState.Pressed));
                MoveDown = ((padState.ThumbSticks.Left.Y > Data.Profile.Settings.ControlSensitivity) || (padState.DPad.Down == ButtonState.Pressed));
                if (MoveUp && MoveDown) { MoveUp = false; MoveDown = false; }

                MoveLeft = ((padState.ThumbSticks.Left.X < -Data.Profile.Settings.ControlSensitivity) || (padState.DPad.Left == ButtonState.Pressed));
                MoveRight = ((padState.ThumbSticks.Left.X > Data.Profile.Settings.ControlSensitivity) || (padState.DPad.Right == ButtonState.Pressed));
                if (MoveLeft && MoveRight) { MoveLeft = false; MoveRight = false; }

                if (((padState.Buttons.A == ButtonState.Pressed) || (padState.Buttons.B == ButtonState.Pressed)) && (!_actionHeld))
                {
                    _actionHeld = true;
                    ActionTriggered = true;
                }
                else if ((padState.Buttons.A == ButtonState.Released) && (padState.Buttons.B == ButtonState.Released))
                {
                    _actionHeld = false;
                }

                if ((padState.Buttons.X == ButtonState.Pressed) || (padState.Buttons.Y == ButtonState.Pressed)) { HandlePowerUpControlTrigger(); }

                if (((padState.Buttons.A == ButtonState.Pressed) || (padState.Buttons.B == ButtonState.Pressed) || (padState.Buttons.Start == ButtonState.Pressed))
                    && (!_selectHeld))
                {
                    _selectHeld = true;
                    SelectionTriggered = true;
                    SelectionIsByStartButton = padState.Buttons.Start == ButtonState.Pressed;
                }
                else if ((padState.Buttons.A == ButtonState.Released) && (padState.Buttons.B == ButtonState.Released) && (padState.Buttons.Start == ButtonState.Released))
                {
                    _selectHeld = false;
                }
            }
        }

        private void HandlePowerUpControlTrigger()
        {
            if ((_inGameButtons.ContainsKey(PowerUpButton.In_Game_Button_Name)) && (_inGameButtons[PowerUpButton.In_Game_Button_Name].Active))
            {
                _inGameButtons[PowerUpButton.In_Game_Button_Name].Active = false;
                LastInGameButtonPressed = PowerUpButton.In_Game_Button_Name;
            }
        }
    }
}
