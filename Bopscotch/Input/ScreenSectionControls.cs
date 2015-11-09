using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Input;

namespace Bopscotch.Input
{
    public class ScreenSectionControls : TouchControls
    {
        public override InputProcessorType ProcessorType { get { return InputProcessorType.Touch; } }

        private bool TouchIsWithinButtonArea(Vector2 touchCurrentLocation)
        {
            foreach (KeyValuePair<string, InGameButtonArea> kvp in _inGameButtons)
            {
                if (kvp.Value.Contains(touchCurrentLocation)) { return true; break; }
            }
            return false;
        }

        private bool TouchIsWithinScreenSection(Vector2 touchLocation, float leftFraction, float rightFraction)
        {
            return ((touchLocation.X >= Definitions.Back_Buffer_Width * leftFraction) &&
                (touchLocation.X < Definitions.Back_Buffer_Width * rightFraction) &&
                (touchLocation.Y >= InGameActionBoundary));
        }

        protected override bool MoveLeftControlHasBeenActivated(Vector2 touchCurrentLocation, Vector2 touchStartLocation)
        {
            return ((TouchIsWithinScreenSection(touchCurrentLocation, 0.0f, Section_Boundary_Fraction)) &&
                (!TouchIsWithinButtonArea(touchCurrentLocation)));
        }

        protected override bool MoveRightControlHasBeenActivated(Vector2 touchCurrentLocation, Vector2 touchStartLocation)
        {
            return ((TouchIsWithinScreenSection(touchCurrentLocation, 1.0f - Section_Boundary_Fraction, 1.0f)) &&
                (!TouchIsWithinButtonArea(touchCurrentLocation)));
        }

        protected override bool ActionControlHasBeenActivated(Vector2 touchCurrentLocation, Vector2 touchStartLocation, int touchHoldDuration)
        {
            return ((touchCurrentLocation.X >= Definitions.Back_Buffer_Width * Section_Boundary_Fraction) &&
                (touchCurrentLocation.X <= Definitions.Back_Buffer_Width * (1.0f - Section_Boundary_Fraction)) &&
                (touchCurrentLocation.Y >= InGameActionBoundary) && 
                (touchHoldDuration < Milliseconds_Before_Tap_Becomes_Hold));
        }

        protected override bool SelectionControlHasBeenActivated(Vector2 touchCurrentLocation, int touchHoldDuration)
        {
            return ((touchCurrentLocation != Vector2.Zero) && (touchHoldDuration < Milliseconds_Before_Tap_Becomes_Hold));
        }

        private const float Section_Boundary_Fraction = 0.25f;
    }
}
