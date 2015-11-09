using Microsoft.Xna.Framework;

using Leda.Core.Input;

namespace Bopscotch.Input
{
    public class DragTapControls : TouchControls
    {
        private float ControlSensitivitySquared { get { return Data.Profile.Settings.ControlSensitivity * Data.Profile.Settings.ControlSensitivity; } }

        public override InputProcessorType ProcessorType { get { return InputProcessorType.Touch; } }

        protected override bool MoveLeftControlHasBeenActivated(Vector2 touchCurrentLocation, Vector2 touchStartLocation)
        {
            return ((touchCurrentLocation.X < touchStartLocation.X) && 
                (Vector2.DistanceSquared(touchStartLocation, touchCurrentLocation) > ControlSensitivitySquared));
        }

        protected override bool MoveRightControlHasBeenActivated(Vector2 touchCurrentLocation, Vector2 touchStartLocation)
        {
            return ((touchCurrentLocation.X > touchStartLocation.X) && 
                (Vector2.DistanceSquared(touchStartLocation, touchCurrentLocation) > ControlSensitivitySquared));
        }

        protected override bool ActionControlHasBeenActivated(Vector2 touchCurrentLocation, Vector2 touchStartLocation, int touchHoldDuration)
        {
            if ((touchStartLocation != Vector2.Zero) && (touchCurrentLocation != Vector2.Zero))
            {
                return ((Vector2.DistanceSquared(touchStartLocation, touchCurrentLocation) < ControlSensitivitySquared) &&
                    (touchHoldDuration < Milliseconds_Before_Tap_Becomes_Hold));
            }

            return false;
        }

        protected override bool SelectionControlHasBeenActivated(Vector2 touchCurrentLocation, int touchHoldDuration)
        {
            return ((touchCurrentLocation != Vector2.Zero) && (touchHoldDuration < Milliseconds_Before_Tap_Becomes_Hold));
        }
    }
}
