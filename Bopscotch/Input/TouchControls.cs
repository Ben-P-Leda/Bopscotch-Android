using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Shapes;
using Leda.Core.Input;

namespace Bopscotch.Input
{
    public abstract class TouchControls : InputProcessorBase
    {
        private Vector2 _touchStartLocation = Vector2.Zero;
        private Vector2 _touchCurrentLocation = Vector2.Zero;
        private int _touchHoldDuration = 0;

        public Vector2 TouchLocation { get { return _touchCurrentLocation; } }
        public Circle PowerUpButtonArea { protected get; set; }
        public float InGameActionBoundary { protected get; set; }

        public TouchControls()
            : base()
        {
            InGameActionBoundary = 0.0f;
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            ResetPublicProperties();

            if (TouchProcessor.HasTouches)
            {
                if (_touchStartLocation == Vector2.Zero)
                {
                    _touchStartLocation = TouchProcessor.Touches[0].CurrentLocation;
                    _touchHoldDuration = 0;
                }
                else
                {
                    _touchHoldDuration += millisecondsSinceLastUpdate;
                }

                _touchCurrentLocation = TouchProcessor.Touches[0].CurrentLocation;

                MoveLeft = MoveLeftControlHasBeenActivated(_touchCurrentLocation, _touchStartLocation);
                MoveRight = MoveRightControlHasBeenActivated(_touchCurrentLocation, _touchStartLocation);
            }
            else
            {
                if (!ButtonHasBeenPressed(_touchCurrentLocation, _touchStartLocation, _touchHoldDuration))
                {
                    ActionTriggered = ActionControlHasBeenActivated(_touchCurrentLocation, _touchStartLocation, _touchHoldDuration);
                    SelectionTriggered = SelectionControlHasBeenActivated(_touchCurrentLocation, _touchHoldDuration);

                    if (SelectionTriggered) { SelectionLocation = _touchCurrentLocation; }
                }

                _touchStartLocation = Vector2.Zero;
                _touchCurrentLocation = Vector2.Zero;
            }
        }

        protected abstract bool MoveLeftControlHasBeenActivated(Vector2 touchCurrentLocation, Vector2 touchStartLocation);

        protected abstract bool MoveRightControlHasBeenActivated(Vector2 touchCurrentLocation, Vector2 touchStartLocation);

        protected abstract bool ActionControlHasBeenActivated(Vector2 touchCurrentLocation, Vector2 touchStartLocation, int touchHoldDuration);

        protected abstract bool SelectionControlHasBeenActivated(Vector2 touchCurrentLocation, int touchHoldDuration);

        private bool ButtonHasBeenPressed(Vector2 touchCurrentLocation, Vector2 touchStartLocation, int touchHoldDuration)
        {
            if (touchHoldDuration < Milliseconds_Before_Tap_Becomes_Hold)
            {
                foreach (KeyValuePair<string, InGameButtonArea> kvp in _inGameButtons)
                {
                    if ((kvp.Value.Active) && (kvp.Value.Contains(touchStartLocation)) && (kvp.Value.Contains(touchCurrentLocation)))
                    {
                        LastInGameButtonPressed = kvp.Key;
                        return true;
                        break;
                    }
                }
            }

            return false;
        }

        public static TouchControls CreateController()
        {
            //return new ScreenSectionControls();
            return new DragTapControls();
        }

        protected const int Milliseconds_Before_Tap_Becomes_Hold = 500;
    }
}
