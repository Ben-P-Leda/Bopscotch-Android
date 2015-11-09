using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Controllers.Camera;

namespace Bopscotch.Gameplay.Controllers
{
    public class PlayerTrackingCameraController : MobileObjectTrackingCameraController
    {
        private Objects.Characters.Player.Player _playerToTrack;

        public Objects.Characters.Player.Player PlayerToTrack { set { base.ObjectToTrack = value; _playerToTrack = value; } }
        public bool LockedOntoStartPoint { get; private set; }

        public PlayerTrackingCameraController()
        {
            SnapSpeed = Scroll_Snap_Speed;
            LockedOntoStartPoint = true;
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            if (!_playerToTrack.CanMoveHorizontally) { LockCameraOntoStationaryPlayer(); }

            base.Update(millisecondsSinceLastUpdate);
        }

        private void LockCameraOntoStationaryPlayer()
        {
            SnapSpeed = Lock_Snap_Speed;
            LockedOntoStartPoint = false;

            Vector2 updatedPosition = WorldPosition;
            int playerDirection = _playerToTrack.IsMovingLeft ? -1 : 1;

            updatedPosition.X = UpdatePositionWithinWorld(
                WorldPosition.X,
                _playerToTrack.WorldPosition.X,
                playerDirection,
                WorldDimensions.X,
                Viewport.Width,
                ScrollBoundaryViewportFractions.X,
                Overspill.X);

            updatedPosition.Y = UpdatePositionWithinWorld(
                WorldPosition.Y,
                _playerToTrack.WorldPosition.Y,
                0.0f,
                WorldDimensions.Y,
                Viewport.Height,
                ScrollBoundaryViewportFractions.Y,
                Overspill.Y);

            if (WorldPosition == updatedPosition) { LockedOntoStartPoint = true; }

            WorldPosition = updatedPosition;

            SnapSpeed = Scroll_Snap_Speed;

        }

        public void PositionForPlayStart()
        {
            if (!_playerToTrack.IsMovingLeft) { SetStartingPosition(new Vector2(-(Viewport.Width * ScrollBoundaryViewportFractions.X) / 2.0f, 0.0f)); }
            else { SetStartingPosition(new Vector2((Viewport.Width * ScrollBoundaryViewportFractions.X) / 2.0f, 0.0f)); }
            //else { SetStartingPosition(new Vector2(-(Viewport.Width * (1.0f - ScrollBoundaryViewportFractions.X)) / 2.0f, 0.0f)); }
        }

        private const float Scroll_Snap_Speed = 4.0f;
        private const float Lock_Snap_Speed = 25.0f;
    }
}
