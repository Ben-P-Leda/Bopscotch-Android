using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Bopscotch.Gameplay.Objects.Display.Race;

namespace Bopscotch.Input
{
    public class KeyboardInputProcessor : InputProcessorBase
    {
        private bool _actionHeld;
        private bool _selectHeld;

        private Dictionary<KeyDefinitions, Keys> _individualControls;
        private List<Keys> _selectionControls;

        public override InputProcessorType ProcessorType { get { return InputProcessorType.Keyboard; } }

        public KeyboardInputProcessor()
        {
            _individualControls = new Dictionary<KeyDefinitions, Keys>();
            _selectionControls = new List<Keys>();
        }

        protected override void Reset()
        {
            base.Reset();

            _actionHeld = false;
            _selectHeld = false;
        }

        public void DefineKey(KeyDefinitions toDefine, Keys selectedKey)
        {
            if (toDefine == KeyDefinitions.Select) { _selectionControls.Add(selectedKey); }
            else if (_individualControls.ContainsKey(toDefine)) { _individualControls[toDefine] = selectedKey; }
            else { _individualControls.Add(toDefine, selectedKey); }
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            ResetPublicProperties();
            KeyboardState kbState = Keyboard.GetState();

            MoveLeft = KeyPressedWithoutOpposition(kbState, KeyDefinitions.Left, KeyDefinitions.Right);
            MoveRight = KeyPressedWithoutOpposition(kbState, KeyDefinitions.Right, KeyDefinitions.Left);
            MoveUp = KeyPressedWithoutOpposition(kbState, KeyDefinitions.Up, KeyDefinitions.Down);
            MoveDown = KeyPressedWithoutOpposition(kbState, KeyDefinitions.Down, KeyDefinitions.Up);

            if ((kbState.IsKeyDown(_individualControls[KeyDefinitions.Action])) && (!_actionHeld)) { _actionHeld = true; ActionTriggered = true; }
            else if (!kbState.IsKeyDown(_individualControls[KeyDefinitions.Action])) { _actionHeld = false; }

            if (kbState.IsKeyDown(_individualControls[KeyDefinitions.PowerUp])) { HandlePowerUpControlTrigger(); }

            for (int i = 0; i < _selectionControls.Count; i++)
            {
                if (kbState.IsKeyDown(_selectionControls[i])) { SelectionTriggered = true; }
            }

            if ((SelectionTriggered) && (!_selectHeld)) { _selectHeld = true; }
            else if (!SelectionTriggered) { _selectHeld = false; }
            else { SelectionTriggered = false; }

            SelectionLocation = Vector2.Zero;
        }

        private bool KeyPressedWithoutOpposition(KeyboardState kbState, KeyDefinitions targetKey, KeyDefinitions opposingKey)
        {
            return ((kbState.IsKeyDown(_individualControls[targetKey])) && (!kbState.IsKeyDown(_individualControls[opposingKey])));
        }

        private void HandlePowerUpControlTrigger()
        {
            if ((_inGameButtons.ContainsKey(PowerUpButton.In_Game_Button_Name)) && (_inGameButtons[PowerUpButton.In_Game_Button_Name].Active))
            {
                _inGameButtons[PowerUpButton.In_Game_Button_Name].Active = false;
                LastInGameButtonPressed = PowerUpButton.In_Game_Button_Name;
            }
        }

        public static KeyboardInputProcessor CreateForPlayerOne()
        {
            KeyboardInputProcessor processor = new KeyboardInputProcessor();
            processor.DefineKey(KeyDefinitions.Up, Keys.W);
            processor.DefineKey(KeyDefinitions.Down, Keys.S);
            processor.DefineKey(KeyDefinitions.Left, Keys.A);
            processor.DefineKey(KeyDefinitions.Right, Keys.D);
            processor.DefineKey(KeyDefinitions.Action, Keys.Space);
            processor.DefineKey(KeyDefinitions.Select, Keys.Space);
            processor.DefineKey(KeyDefinitions.PowerUp, Keys.LeftShift);

            return processor;
        }

        public static KeyboardInputProcessor CreateForPlayerTwo()
        {
            KeyboardInputProcessor processor = new KeyboardInputProcessor();
            processor.DefineKey(KeyDefinitions.Up, Keys.Up);
            processor.DefineKey(KeyDefinitions.Down, Keys.Down);
            processor.DefineKey(KeyDefinitions.Left, Keys.Left);
            processor.DefineKey(KeyDefinitions.Right, Keys.Right);
            processor.DefineKey(KeyDefinitions.Action, Keys.Enter);
            processor.DefineKey(KeyDefinitions.Select, Keys.Enter);
            processor.DefineKey(KeyDefinitions.PowerUp, Keys.RightShift);

            return processor;
        }

        public enum KeyDefinitions
        {
            Up,
            Down,
            Left,
            Right,
            Action,
            Select,
            PowerUp
        }
    }
}
