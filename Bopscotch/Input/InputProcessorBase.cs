using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Bopscotch.Input
{
    public abstract class InputProcessorBase
    {
        protected Dictionary<string, InGameButtonArea> _inGameButtons;

        public abstract InputProcessorType ProcessorType { get; }

        public bool MoveUp { get; protected set; }
        public bool MoveDown { get; protected set; }
        public bool MoveLeft { get; protected set; }
        public bool MoveRight { get; protected set; }

        public bool ActionTriggered { get; protected set; }

        public bool SelectionTriggered { get; protected set; }
        public Vector2 SelectionLocation { get; protected set; }

        public string LastInGameButtonPressed { get; protected set; }

        public InputProcessorBase()
        {
            _inGameButtons = new Dictionary<string, InGameButtonArea>();
        }

        public void AddButtonArea(string name, Vector2 center, float radius, bool startsActive)
        {
            if (!_inGameButtons.ContainsKey(name)) { _inGameButtons.Add(name, null); }
            _inGameButtons[name] = new InGameButtonArea(center, radius) { Active = startsActive };
        }

        public void ActivateButton(string name)
        {
            _inGameButtons[name].Active = true;
        }

        public void DeactivateButton(string name)
        {
            _inGameButtons[name].Active = false;
        }

        protected virtual void Reset()
        {
            ResetPublicProperties();
        }

        protected void ResetPublicProperties()
        {
            MoveUp = false;
            MoveDown = false;
            MoveLeft = false;
            MoveRight = false;
            ActionTriggered = false;
            SelectionTriggered = false;
            SelectionLocation = Vector2.Zero;
            LastInGameButtonPressed = "";
        }

        public abstract void Update(int millsecondsSinceLastUpdate);

        public enum InputProcessorType
        {
            Keyboard,
            Gamepad,
            Touch
        }

        public const float Default_Control_Sensitivity = 25.0f;
    }
}
