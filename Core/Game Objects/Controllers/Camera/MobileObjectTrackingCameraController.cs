using System;

using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Controllers;
using Leda.Core.Game_Objects.Behaviours;

namespace Leda.Core.Game_Objects.Controllers.Camera
{
    public class MobileObjectTrackingCameraController : CameraControllerBase
    {
        public IMobile ObjectToTrack { private get; set; }
        public Point WorldDimensions { protected get; set; }
        public Vector2 ScrollBoundaryViewportFractions { protected get; set; }
        public Vector2 Overspill { protected get; set; }
        public float SnapSpeed { private get; set; }

        public MobileObjectTrackingCameraController()
            : base()
        {
            ObjectToTrack = null;
            WorldDimensions = Point.Zero;
            ScrollBoundaryViewportFractions = new Vector2(0.5f);
            Overspill = Vector2.Zero;
            SnapSpeed = Default_Snap_Speed;
        }

        public virtual void Update(int millisecondsSinceLastUpdate)
        {
            if ((ObjectToTrack != null) && (Viewport != Rectangle.Empty))
            {
                Vector2 updatedPosition = WorldPosition;

                updatedPosition.X = UpdatePositionWithinWorld(
                    WorldPosition.X, 
                    ObjectToTrack.WorldPosition.X, 
                    ObjectToTrack.MotionEngine.Delta.X,
                    WorldDimensions.X,
                    Viewport.Width,
                    ScrollBoundaryViewportFractions.X,
                    Overspill.X);

                updatedPosition.Y = UpdatePositionWithinWorld(
                    WorldPosition.Y,
                    ObjectToTrack.WorldPosition.Y,
                    ObjectToTrack.MotionEngine.Delta.Y,
                    WorldDimensions.Y,
                    Viewport.Height,
                    ScrollBoundaryViewportFractions.Y,
                    Overspill.Y);

                WorldPosition = updatedPosition;
            }
        }

        protected float UpdatePositionWithinWorld(float cameraPosition, float trackedObjectPosition, float movementDelta, 
            int worldSideLength, int cameraSideLength, float boundaryFraction, float overspill)
        {
            if (movementDelta > 0.0f)
            {
                if (trackedObjectPosition - cameraPosition > cameraSideLength * boundaryFraction)
                {
                    float updatedPosition = trackedObjectPosition - cameraSideLength * boundaryFraction;

                    cameraPosition = MathHelper.Clamp(
                        Math.Min(updatedPosition, cameraPosition + (movementDelta * SnapSpeed)), 
                        -overspill, 
                        (worldSideLength - cameraSideLength) + overspill);
                }
            }
            else if (movementDelta < 0.0f)
            {
                if (trackedObjectPosition - cameraPosition < cameraSideLength * (1.0f - boundaryFraction))
                {
                    float updatedPosition = trackedObjectPosition - (cameraSideLength * (1.0f - boundaryFraction));

                    cameraPosition = MathHelper.Clamp(
                        Math.Max(updatedPosition, cameraPosition + (movementDelta * SnapSpeed)),
                        -overspill, 
                        (worldSideLength - cameraSideLength) + overspill);
                }
            }

            return cameraPosition;
        }

        public void SetStartingPosition(Vector2 offset)
        {
            Vector2 startPosition = (ObjectToTrack.WorldPosition - (new Vector2(Viewport.Width, Viewport.Height) / 2.0f)) + offset;

            startPosition.X = MathHelper.Clamp(startPosition.X, -Overspill.X, (WorldDimensions.X - Viewport.Width) + Overspill.X);
            startPosition.Y = MathHelper.Clamp(startPosition.Y, -Overspill.Y, (WorldDimensions.Y - Viewport.Height) + Overspill.Y);

            WorldPosition = startPosition;
        }

        private const float Default_Snap_Speed = 2.0f;
    }
}
