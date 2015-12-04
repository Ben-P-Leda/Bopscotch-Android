using System;
using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Tile_Map;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;
using Leda.Core.Serialization;
using Leda.Core.Motion;
using Leda.Core.Shapes;
using Leda.Core.Timing;
using Leda.Core;
using Leda.Core.Game_Objects.Controllers.Collisions;

using Bopscotch.Data.Avatar;
using Bopscotch.Gameplay.Coordination;
using Bopscotch.Gameplay.Objects.Environment;
using Bopscotch.Gameplay.Objects.Environment.Blocks;
using Bopscotch.Gameplay.Objects.Environment.Signposts;
using Bopscotch.Gameplay.Objects.Environment.Flags;
using Bopscotch.Gameplay.Objects.Display.Survival;
using Bopscotch.Gameplay.Objects.Display.Race;

namespace Bopscotch.Gameplay.Objects.Characters.Player
{
    public class Player : Base.Character, ICircularCollidable
    {
        public delegate void PlayerEventHandler();

        private bool _hasLandedOnBlock;
        private bool _didNotLandSafely;
        private bool _hasTouchedGoalFlag;
        private bool _hasAlreadyTouchedGoalFlag;
        private bool _isSmashingSmashBlocks;
        private Rectangle _mapBounds;

        private Definitions.PowerUp _activePowerUp;

        private PlayerMotionEngine _motionEngine;
        public Input.InputProcessorBase InputProcessor { set { _motionEngine.InputProcessor = value; } }
        public bool IsMovingLeft { get { return _motionEngine.IsMovingLeft; } }
        public override bool Mirror
        {
            get { return base.Mirror; }
            set { base.Mirror = value; if (_motionEngine != null) { _motionEngine.IsMovingLeft = value; } }
        }
        public bool CanMoveHorizontally
        {
            get { return _motionEngine.CanMoveHorizontally; }
            set { _motionEngine.CanMoveHorizontally = value; }
        }
        public TimerController.TickCallbackRegistrationHandler TimerTickCallback { set { _motionEngine.TimerTickCallback = value; } }

        private PlayerEvent _lastEvent;
        public PlayerEventHandler PlayerEventCallback { private get; set; }

        public Circle CollisionBoundingCircle { get; private set; }
        public Circle PositionedCollisionBoundingCircle { get; set; }
        private OneToManyCollisionController _collisionController;
        public OneToManyCollisionController CollisionController { set { value.ObjectToTest = this; _collisionController = value; } }
        
        private BlockMap _map;
        public BlockMap Map { set { _map = value; SetCurrentMapBounds(); } }
        public float DistanceToGround
        {
            get
            {
                float distance = float.PositiveInfinity;
                bool groundFound = false;
                int offset = _mapBounds.Height;

                while ((!groundFound) && (offset < _map.Height))
                {
                    if (_map.CellIsOccupied(_mapBounds.X, offset))
                    {
                        groundFound = true;
                        distance = _map.GetTile(_mapBounds.X, offset).WorldPosition.Y - WorldPosition.Y;
                    }
                    else if ((_mapBounds.X != _mapBounds.Width) && (_map.CellIsOccupied(_mapBounds.Width, offset)))
                    {
                        groundFound = true;
                        distance = _map.GetTile(_mapBounds.Width, offset).WorldPosition.Y - WorldPosition.Y;
                    }
                    else
                    {
                        offset++;
                    }
                }
                return distance;
            }
        }

        public TutorialRunner.TutorialRunnerStepTrigger TutorialStepTrigger { private get; set; }

        public float DeathByFallingThreshold { private get; set; }
        public bool IsDead { get; private set; }
        public bool IsExitingLevel { private get; set; }

        public int CustomSkinSlotIndex { private get; set; }

        public PlayerEvent LastEvent
        {
            get { return _lastEvent; }
            private set { _lastEvent = value; PlayerEventCallback(); }
        }

        public Flag LastRaceRestartPointTouched { get; private set; }

        public Player()
            : base()
        {
            ID = "player";
            PlayerEventCallback = null;
            _lastEvent = PlayerEvent.None;

            RenderLayer = Render_Layer;
            RenderDepth = Render_Depth;

            _mapBounds = Rectangle.Empty;
            CollisionBoundingCircle = new Circle(Vector2.Zero, Body_Collision_Radius);
            Collidable = true;
            Visible = true;

            _motionEngine = new PlayerMotionEngine();
            MotionEngine = _motionEngine;

            _isSmashingSmashBlocks = false;
            _hasTouchedGoalFlag = false;
            IsExitingLevel = false;

            LastRaceRestartPointTouched = null;

            TutorialStepTrigger = null;

            ResetCollisionFlags();
        }

        public void SetForMovement()
        {
            _motionEngine.CanMoveHorizontally = true;
            _motionEngine.CanTriggerJump = true;
            _motionEngine.VerticalMovementIsEnabled = true;

            _isSmashingSmashBlocks = false;
        }

        private void ResetCollisionFlags()
        {
            _hasLandedOnBlock = false;
            _didNotLandSafely = true;

            _hasAlreadyTouchedGoalFlag = _hasTouchedGoalFlag;
            _hasTouchedGoalFlag = false;
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            _motionEngine.DistanceToGround = DistanceToGround;
            base.Update(millisecondsSinceLastUpdate);

            UpdateMotionAnimationSequence();

            if (LifeCycleState == LifeCycleStateValue.Active)
            {
                SetCurrentMapBounds();
                CheckAndHandleMapTileImpacts();
                CheckAndHandleBadLandingLastUpdate();
            }

            CheckAndHandleGoalPassed();
            CheckAndHandleFallingOutOfLevel();

            ResetCollisionFlags();

            if (TutorialStepTrigger != null) { TutorialStepTrigger(WorldPosition); }
        }

        private void SetCurrentMapBounds()
        {
            _mapBounds = new Rectangle(
                (int)Math.Floor(WorldPosition.X - Body_Collision_Radius) / Definitions.Grid_Cell_Pixel_Size,
                (int)Math.Floor(WorldPosition.Y - Body_Collision_Radius) / Definitions.Grid_Cell_Pixel_Size,
                (int)Math.Floor(WorldPosition.X + Body_Collision_Radius) / Definitions.Grid_Cell_Pixel_Size,
                (int)Math.Floor(WorldPosition.Y + Body_Collision_Radius) / Definitions.Grid_Cell_Pixel_Size);
        }

        private void CheckAndHandleMapTileImpacts()
        {
            CheckAndHandleTileImpact(_mapBounds.X, _mapBounds.Y);
            if ((Collidable) && (_mapBounds.X != _mapBounds.Width)) { CheckAndHandleTileImpact(_mapBounds.Width, _mapBounds.Y); }
            if ((Collidable) && (_mapBounds.Y != _mapBounds.Height)) { CheckAndHandleTileImpact(_mapBounds.X, _mapBounds.Height); }
            if ((Collidable) && (_mapBounds.X != _mapBounds.Width) && (_mapBounds.Y != _mapBounds.Height)) { CheckAndHandleTileImpact(_mapBounds.Width, _mapBounds.Height); }

            if ((Collidable) && (!_motionEngine.VerticalMovementCanSmash))
            {
                int rowBelow = Math.Max(_mapBounds.Y, _mapBounds.Height) + 1;
                CheckAndHandleSpringLaunch(_mapBounds.X, rowBelow);
                if ((!_motionEngine.VerticalMovementCanSmash) && (_mapBounds.X != _mapBounds.Width)) { CheckAndHandleSpringLaunch(_mapBounds.Width, rowBelow); }
            }
        }

        private void CheckAndHandleTileImpact(int mapX, int mapY)
        {
            if (!_map.CellIsOccupied(mapX, mapY)) { return; }

            IBoxCollidable collisionTile = (IBoxCollidable)_map.GetTile(mapX, mapY);
            if ((collisionTile.Collidable) && (_collisionController.BoxAndCircularCollidersHaveCollided(collisionTile, this)))
            {
                collisionTile.HandleCollision(this);
                this.HandleCollision(collisionTile);
            }
        }

        private void CheckAndHandleSpringLaunch(int mapX, int mapY)
        {
            if (_map.CellIsOccupied(mapX, mapY))
            {
                SpringBlock launcher = _map.GetTile(mapX, mapY) as SpringBlock;
                if ((launcher != null) && (launcher.HasBeenLandedOnSquarely(WorldPosition)))
                { 
                    HandleSpringBlockLaunch(launcher); 
                }
            }
        }

        private void HandleSpringBlockLaunch(SpringBlock launcher)
        {
            SoundEffectManager.PlayEffect("spring-launch");
            _motionEngine.SetForBoostedJump();
            launcher.TriggerLaunchAnimation();
        }

        private void HandleIceBlockImpact()
        {
            _motionEngine.FreezeSpeedChanges();
            SoundEffectManager.PlayEffect("ice-crunch");
        }

        private void HandleBombBlockImpact(BombBlock bomb)
        {
            bomb.TriggerByImpact();
        }

        private void CheckAndHandleBadLandingLastUpdate()
        {
            if ((_hasLandedOnBlock) && (_didNotLandSafely)) { StartDeathSequence(); }
        }

        private void StartDeathSequence()
        {
            DisableAllMovement();
            IsDead = true;
            LifeCycleState = Leda.Core.LifeCycleStateValue.Exiting;
            SoundEffectManager.PlayEffect("player-death");

            Data.Profile.HandlePlayerDeath();
        }

        private void DisableAllMovement()
        {
            _motionEngine.CanMoveHorizontally = false;
            _motionEngine.CanTriggerJump = false;
            _motionEngine.VerticalMovementIsEnabled = false;
        }

        protected override void StartExitSequence()
        {
            if (IsDead)
            {
                LastEvent = PlayerEvent.Died;
                Visible = false;
                Collidable = false;
            }
            else
            {
                IsExitingLevel = true;
                _motionEngine.CanMoveHorizontally = false;
            }
        }

        private void UpdateMotionAnimationSequence()
        {
            if (_motionEngine.VerticalDirectionChanged)
            {
                if (_motionEngine.Delta.Y > 0.0f)
                {
                    if (IsExitingLevel) { SetForLevelExitSequence(); }
                    else { AnimationEngine.Sequence = AnimationDataManager.Sequences["player-fall"]; }
                }
                else if (!IsExitingLevel) { AnimationEngine.Sequence = AnimationDataManager.Sequences["player-jump"]; }
            }
        }

        private void SetForLevelExitSequence()
        {
            _motionEngine.VerticalMovementIsEnabled = false;

            Bones.Clear();
            PrimaryBoneID = "";
            CreateBonesFromDataManager(Definitions.Avatar_Skeleton_Front);
            SkinBones(AvatarComponentManager.FrontFacingAvatarSkin(CustomSkinSlotIndex));

            AnimationEngine.Sequence = AnimationDataManager.Sequences["player-front-win"];
        }

        private void CheckAndHandleGoalPassed()
        {
            if ((_hasAlreadyTouchedGoalFlag) & (!_hasTouchedGoalFlag))
            {
                LastEvent = PlayerEvent.Goal_Passed;
                if (!Data.Profile.PlayingRaceMode) { StartExitSequence(); }
            }
        }

        private void CheckAndHandleFallingOutOfLevel()
        {
            if ((WorldPosition.Y >= DeathByFallingThreshold) && (!IsDead)) { StartDeathSequence(); }
        }

        public override void HandleCollision(ICollidable collider)
        {
            if (collider is Block) { HandleBlockCollision((Block)collider); }
            else if (collider is SignpostBase) { HandleSignpostCollision((SignpostBase)collider); }
            else if (collider is Flag) { HandleFlagCollision((Flag)collider); }
            else if (collider is BombBlockBlastCollider) { StartDeathSequence(); }
        }

        private void HandleBlockCollision(Block collidingBlock)
        {
            if (collidingBlock is ObstructionBlock) { HandleObstructionBlockCollision((ObstructionBlock)collidingBlock); }
            else if (collidingBlock is SmashBlock) { HandleSmashBlockCollision((SmashBlock)collidingBlock); }
            else { HandleSolidBlockCollision(collidingBlock); }
        }

        private void HandleObstructionBlockCollision(ObstructionBlock collidingObstructionBlock)
        {
            if ((_activePowerUp == Definitions.PowerUp.Boots) && (collidingObstructionBlock is SpikeBlock))
            {
                HandleSolidBlockCollision(collidingObstructionBlock);
            }
            else
            {
                StartDeathSequence();
            }
        }

        private void HandleSmashBlockCollision(SmashBlock collidingSmashBlock)
        {
            if ((_motionEngine.VerticalMovementCanSmash) || (_isSmashingSmashBlocks))
            {
                collidingSmashBlock.HandleSmash();
                _map.RecordBlockHasBeenSmashed(collidingSmashBlock);
                _motionEngine.PreventNextJump();
                _isSmashingSmashBlocks = true;
            }
            else
            {
                HandleSolidBlockCollision(collidingSmashBlock);
                _motionEngine.PlayerHasJustLandedOnSmashBlock = true;
            }
        }

        private void HandleSolidBlockCollision(Block collidingBlock)
        {
            _hasLandedOnBlock = true;
            _isSmashingSmashBlocks = false;

            if (collidingBlock.HasBeenLandedOnSquarely(WorldPosition))
            {
                _motionEngine.PlayerIsOnGround = true;
                _didNotLandSafely = false;

                if (_motionEngine.Delta.Y > 0)
                {
                    WorldPosition -= new Vector2(0.0f, WorldPosition.Y - (collidingBlock.TopSurfaceY - Body_Collision_Radius));
                    bool specialBlock = false;

                    SpringBlock launcher = collidingBlock as SpringBlock;
                    if (launcher != null) { HandleSpringBlockLaunch(launcher); specialBlock = true; }

                    if (!specialBlock)
                    {
                        IceBlock ice = collidingBlock as IceBlock;
                        if (ice != null) { HandleIceBlockImpact(); specialBlock = true; }
                    }

                    if (!specialBlock)
                    {
                        BombBlock bomb = collidingBlock as BombBlock;
                        if (bomb != null) { HandleBombBlockImpact(bomb); }
                    }
                }
            }
            else if ((CornerHasBeenClipped(collidingBlock.LeftSurfaceX, collidingBlock.TopSurfaceY)) ||
                (CornerHasBeenClipped(collidingBlock.RightSurfaceX, collidingBlock.TopSurfaceY)))
            {
                _hasLandedOnBlock = false;
            }
            else if ((FacingAwayFromBlock(collidingBlock)) && (WorldPosition.Y < collidingBlock.WorldPosition.Y))
            {
                if (Math.Abs(WorldPosition.X - (collidingBlock.WorldPosition.X + (collidingBlock.CollisionBoundingBox.Width / 2.0f))) < Rear_Edge_Clip_Tolerance)
                {
                    _motionEngine.PlayerIsOnGround = true;
                }
                _didNotLandSafely = false;
            }
        }

        private bool CornerHasBeenClipped(float cornerX, float cornerY)
        {
            return ((WorldPosition.Y < cornerY) &&
                (Vector2.DistanceSquared(new Vector2(cornerX, cornerY), WorldPosition) < (Body_Collision_Radius * Body_Collision_Radius)));
        }

        private bool FacingAwayFromBlock(Block collidingBlock)
        {
            if ((_motionEngine.IsMovingLeft) && (WorldPosition.X < collidingBlock.WorldPosition.X)) { return true; }
            if ((!_motionEngine.IsMovingLeft) && (WorldPosition.X > collidingBlock.WorldPosition.X)) { return true; }
            return false;
        }

        private void HandleSignpostCollision(SignpostBase collider)
        {
            if (collider is OneWaySignpost) { HandleOneWaySignpostCollision((OneWaySignpost)collider); }
            if (collider is SpeedLimitSignpost) { HandleSpeedLimitSignpostCollision((SpeedLimitSignpost)collider); }
        }

        private void HandleOneWaySignpostCollision(OneWaySignpost collider)
        {
            if ((collider.Mirror != _motionEngine.IsMovingLeft) && (IsHorizontallyCloseEnoughForEffect(collider, Signpost_Effect_Distance)))
            {
                SoundEffectManager.PlayEffect("turn-round");
                ChangeHorizontalMovementDirection();
            }
        }

        private void ChangeHorizontalMovementDirection()
        {
            Mirror = !_motionEngine.IsMovingLeft;
        }

        private bool IsHorizontallyCloseEnoughForEffect(ICollidable collider, float maximumDistance)
        {
            return (Math.Abs(collider.WorldPosition.X - WorldPosition.X) < maximumDistance);
        }

        private void HandleSpeedLimitSignpostCollision(SpeedLimitSignpost collider)
        {
            collider.InitiateCollisionEffect();

            if (_motionEngine.AvailableSpeedRangeSteps != collider.SpeedRange)
            {
                if (!_motionEngine.SpeedIsOverridden)
                {
                    if ((collider.SpeedRange.Minimum > _motionEngine.AvailableSpeedRangeSteps.Minimum) ||
                        (collider.SpeedRange.Maximum > _motionEngine.AvailableSpeedRangeSteps.Maximum))
                    {
                        SoundEffectManager.PlayEffect("speed-up");
                    }
                    else if ((collider.SpeedRange.Minimum < _motionEngine.AvailableSpeedRangeSteps.Minimum) ||
                        (collider.SpeedRange.Maximum < _motionEngine.AvailableSpeedRangeSteps.Maximum))
                    {
                        SoundEffectManager.PlayEffect("slow-down");
                    }
                }

                _motionEngine.AvailableSpeedRangeSteps = collider.SpeedRange;
            }
        }

        private void HandleFlagCollision(Flag collider)
        {
            if (IsHorizontallyCloseEnoughForEffect(collider, Flag_Effect_Distance))
            {
                bool colliderIsNotLastRestartPointPassed = (LastRaceRestartPointTouched != collider);

                LastRaceRestartPointTouched = collider;

                if (Data.Profile.PlayingRaceMode)
                {
                    if (IsMovingLeft != collider.ActivatedWhenMovingLeft)
                    {
                        SoundEffectManager.PlayEffect("turn-round");
                        ChangeHorizontalMovementDirection();
                        LastEvent = PlayerEvent.Restart_Point_Changed_Direction; ;
                    }
                    else if ((colliderIsNotLastRestartPointPassed) && (!(collider is GoalFlag)))
                    {
                        LastEvent = PlayerEvent.Restart_Point_Touched;
                    }
                }

                if (collider is GoalFlag) { _hasTouchedGoalFlag = true; }
            }
        }

        protected override XElement Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.AddDataItem("has-landed-on-block", _hasLandedOnBlock);
            serializer.AddDataItem("did-not-land-safely", _didNotLandSafely);
            serializer.AddDataItem("has-touched-goal", _hasTouchedGoalFlag);
            serializer.AddDataItem("already-touched-goal", _hasAlreadyTouchedGoalFlag);
            serializer.AddDataItem("smashing-blocks", _isSmashingSmashBlocks);
            serializer.AddDataItem("motion-engine", MotionEngine);
            serializer.AddDataItem("last-event", LastEvent);

            return serializer.SerializedData;
        }

        protected override Serializer Deserialize(Serializer serializer)
        {
            serializer.KnownSerializedObjects.Add(_motionEngine);

            base.Deserialize(serializer);

            _hasLandedOnBlock = serializer.GetDataItem<bool>("has-landed-on-block");
            _didNotLandSafely = serializer.GetDataItem<bool>("did-not-land-safely");
            _hasTouchedGoalFlag = serializer.GetDataItem<bool>("has-touched-goal");
            _hasAlreadyTouchedGoalFlag = serializer.GetDataItem<bool>("already-touched-goal");
            _isSmashingSmashBlocks = serializer.GetDataItem<bool>("smashing-blocks");
            _motionEngine = serializer.GetDataItem<PlayerMotionEngine>("motion-engine");
            _lastEvent = serializer.GetDataItem<PlayerEvent>("last-event");

            return serializer;
        }

        public void Activate()
        {
            _motionEngine.SetForStartSequence(IsMovingLeft);

            LifeCycleState = LifeCycleStateValue.Active;
            Visible = true;
            IsDead = false;
            Collidable = true;

            ResetAllPowerUpEffects();
        }

        public void TriggerLevelSkip()
        {
            DisableAllMovement();
            Collidable = false;
            LastEvent = PlayerEvent.Goal_Passed;
        }

        public void SetActivePowerUp(Definitions.PowerUp powerUpToActivate)
        {
            switch (powerUpToActivate)
            {
                case Definitions.PowerUp.None: ResetAllPowerUpEffects(); break;
                case Definitions.PowerUp.Chilli: _motionEngine.ForceMaximumSpeed(PowerUpTimer.Chilli_Duration_In_Milliseconds); break;
                case Definitions.PowerUp.Wheel: ChangeHorizontalMovementDirection(); break;
                case Definitions.PowerUp.Shell: _motionEngine.ForceMinimumSpeed(Slow_PowerDown_Duration_In_Milliseconds); break;
                case Definitions.PowerUp.Horn: _motionEngine.ForceMaximumSpeed(Speed_PowerDown_Duration_In_Milliseconds); break;
                default: _activePowerUp = powerUpToActivate; break;
            }
        }

        public void ResetAllPowerUpEffects()
        {
            _activePowerUp = Definitions.PowerUp.None;
            _motionEngine.ClearForcedMovementSpeed();
        }

        public enum PlayerEvent
        {
            None,
            Goal_Passed,
            Restart_Point_Touched,
            Restart_Point_Changed_Direction,
            Died,
            Resurrected
        }

        public const string In_Play_Skeleton_Name = "player-ingame";
        public const string Cleared_Skeleton_Name = "player-dialog";

        private const int Render_Layer = 2;
        private const float Render_Depth = 0.4f;

        private const float Body_Collision_Radius = 35.0f;
        private const float Signpost_Effect_Distance = 40.0f;
        private const float Flag_Effect_Distance = 35.0f;
        private const float Rear_Edge_Clip_Tolerance = 70.0f;

        private const int Slow_PowerDown_Duration_In_Milliseconds = 2000;
        private const int Speed_PowerDown_Duration_In_Milliseconds = 2000;
    }
}
