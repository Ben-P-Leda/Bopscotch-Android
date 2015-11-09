using System;
using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Timing;
using Leda.Core.Motion;
using Leda.Core.Serialization;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;

namespace Bopscotch.Gameplay.Objects.Characters.Player
{
    public class PlayerMotionEngine : IMotionEngine, ISerializable
    {
        private float _gravity;
        private float _verticalVelocity;
        private int _horizontalMovementDirection;
        private int _verticalMovementDirection;
        private Vector2 _delta;
        private bool _jumpTriggered;
        private bool _jumpInProgress;
        private float _distanceToGround;
        private Range _availableSpeedSteps;

        private Timer _fixedSpeedTimer;
        private bool _forceMaximumSpeed;
        private Timer _speedChangeLockoutTimer;

        public string ID { get; set; }
        public float Speed { get; private set; }
        public bool PlayerIsOnGround { private get; set; }
        public bool PlayerHasJustLandedOnSmashBlock { private get; set; }
        public bool CanMoveHorizontally { get; set; }
        public bool CanTriggerJump { get; set; }
        public bool VerticalMovementIsEnabled { get; set; }

        public bool SpeedIsOverridden { get { return _fixedSpeedTimer.CurrentActionProgress < 1.0f; } }
        public bool SpeedChangesAreLockedOut { get { return _speedChangeLockoutTimer.CurrentActionProgress < 1.0f; } }

        public Range AvailableSpeedRangeSteps 
        {
            get 
            {
                if (_fixedSpeedTimer.CurrentActionProgress < 1.0f) 
                { 
                    return _forceMaximumSpeed ? new Range(Maximum_Speed_Range_Steps - 1, Maximum_Speed_Range_Steps) : Range.Empty; 
                }

                return _availableSpeedSteps;
            }
            set { _availableSpeedSteps = value; }
        }

        public int DifficultySpeedBoosterUnit { private get; set; }

        public Input.InputProcessorBase InputProcessor { private get; set; }

        public float DistanceToGround { set { _distanceToGround = value;}}

        public bool IsMovingLeft
        {
            get { return _horizontalMovementDirection < 0; }
            set { if (value) { _horizontalMovementDirection = -1; } else { _horizontalMovementDirection = 1; } }
        }

        public Vector2 Delta { get { return _delta; } }
        public bool VerticalDirectionChanged { get { return (Math.Sign(_delta.Y) != _verticalMovementDirection); } }
        public bool VerticalMovementCanSmash { get { return ((_jumpInProgress) || (_jumpTriggered)); } }

        public TimerController.TickCallbackRegistrationHandler TimerTickCallback { set { value(_fixedSpeedTimer.Tick); value(_speedChangeLockoutTimer.Tick); } }

        private float MaximumSpeed { get { return Minimum_Movement_Speed + (Speed_Limit_Range_Step * (AvailableSpeedRangeSteps.Maximum + 1)); } }
        private float MinimumSpeed { get { return Minimum_Movement_Speed + (Speed_Limit_Range_Step * AvailableSpeedRangeSteps.Minimum); } }

        public PlayerMotionEngine()
        {
            ID = "player-motion-engine";

            _horizontalMovementDirection = 0;
            _verticalMovementDirection = 0;
            _delta = Vector2.Zero;

            _fixedSpeedTimer = new Timer("fix-speed-timer");
            _forceMaximumSpeed = false;
            _speedChangeLockoutTimer = new Timer("input-lock-timer");

            InputProcessor = null;

            DifficultySpeedBoosterUnit = 0;
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            _verticalMovementDirection = Math.Sign(_delta.Y);

            if (CanMoveHorizontally) { CheckForAndHandleSpeedChange(); }

            if (_fixedSpeedTimer.CurrentActionProgress < 1.0f)
            {
                if (_forceMaximumSpeed) { Speed = Minimum_Movement_Speed + (Speed_Limit_Range_Step * Maximum_Speed_Range_Steps);}
                else { Speed = Minimum_Movement_Speed; }
            }

            if ((CanTriggerJump) && (JumpTriggerIsAllowed)) { CheckForAndHandleJumpStart(); }

            int steps = millisecondsSinceLastUpdate + ((millisecondsSinceLastUpdate / Milliseconds_Per_Speed_Boost_Unit) * DifficultySpeedBoosterUnit);

            SetHorizontalMovementDelta(steps);
            SetVerticalMovementDelta(steps);
        }

        private bool SpeedUpInputReceived
        {
            get
            {
                if (SpeedChangesAreLockedOut) { return false; }
                if (IsMovingLeft && InputProcessor.MoveLeft) { return true; }
                if (!IsMovingLeft && InputProcessor.MoveRight) { return true; }
                return false;
            }
        }

        private bool SlowDownInputReceived
        {
            get
            {
                if (SpeedChangesAreLockedOut) { return false; }
                if (IsMovingLeft && InputProcessor.MoveRight) { return true; }
                if (!IsMovingLeft && InputProcessor.MoveLeft) { return true; }
                return false;
            }
        }

        private void CheckForAndHandleSpeedChange()
        {
            if (SpeedUpInputReceived || Speed < MinimumSpeed) { Speed = Math.Min(Speed + Speed_Change_Rate, MaximumSpeed); }
            else if (SlowDownInputReceived || (Speed > MaximumSpeed)) { Speed = Math.Max(Speed - Speed_Change_Rate, MinimumSpeed); }
        }

        private bool JumpTriggerIsAllowed
        {
            get
            {
                //if ((_jumpInProgress) || (_jumpTriggered)) { return false; }
                if (_jumpTriggered) { return false; }
                if (_verticalVelocity < Bounce_Vertical_Velocity) { return false; }
                return (_distanceToGround < Maximum_Allowed_Height_For_Jump_Start);
            }
        }

        private void CheckForAndHandleJumpStart()
        {
            if (InputProcessor.ActionTriggered)
            {
                _gravity = Jump_Start_Gravity_Value;
                _jumpTriggered = true;
            }
        }

        public void ForceMaximumSpeed(int duration)
        {
            _fixedSpeedTimer.NextActionDuration = duration;
            _forceMaximumSpeed = true;
        }

        public void ForceMinimumSpeed(int duration)
        {
            _fixedSpeedTimer.NextActionDuration = duration;
            _forceMaximumSpeed = false;
        }

        public void ClearForcedMovementSpeed()
        {
            _fixedSpeedTimer.NextActionDuration = 0;
        }

        public void FreezeSpeedChanges()
        {
            _speedChangeLockoutTimer.NextActionDuration = Ice_Block_Speed_Change_Freeze_In_Milliseconds;
        }

        public void PreventNextJump()
        {
            _jumpTriggered = false;
        }

        private void SetHorizontalMovementDelta(int steps)
        {
            if (!CanMoveHorizontally) { _delta.X = 0.0f; }
            else { _delta.X = Speed * steps * _horizontalMovementDirection; }
        }

        private void SetVerticalMovementDelta(int steps)
        {
            if ((PlayerIsOnGround) && (_verticalVelocity >= 0.0f))
            {
                if ((!_jumpTriggered) || (!PlayerHasJustLandedOnSmashBlock))
                {
                    _verticalVelocity = Bounce_Vertical_Velocity;
                    _gravity = Definitions.Normal_Gravity_Value;

                    if (_jumpInProgress) { _jumpInProgress = false; }

                    if (_jumpTriggered)
                    {
                        _verticalVelocity = Normal_Jump_Vertical_Velocity;
                        _jumpTriggered = false;
                        _jumpInProgress = true;
                        SoundEffectManager.PlayEffect("player-jump");
                    }
                    else
                    {
                        SoundEffectManager.PlayEffect("player-move-bounce");
                    }
                }
            }

            _delta.Y = 0.0f;
            if (VerticalMovementIsEnabled)
            {
                while (steps-- > 0)
                {
                    _delta.Y += _verticalVelocity;
                    if (_verticalVelocity <= -Definitions.Zero_Vertical_Velocity) { _verticalVelocity *= _gravity; }
                    else if (_verticalVelocity >= Definitions.Zero_Vertical_Velocity) { _verticalVelocity /= _gravity; }
                    else { _verticalVelocity = Definitions.Zero_Vertical_Velocity; }
                }

                _delta.Y = Math.Min(_delta.Y, Definitions.Terminal_Velocity);
            }

            PlayerIsOnGround = false;
            PlayerHasJustLandedOnSmashBlock = false;
        }

        public void SetForBoostedJump()
        {
            _jumpTriggered = false;
            _jumpInProgress = true;
            _gravity = Definitions.Normal_Gravity_Value;
            _verticalVelocity = Boosted_Jump_Vertical_Velocity;
        }

        public XElement Serialize()
        {
            Serializer serializer = new Serializer(this);
            serializer.AddDataItem("speed", Speed);
            serializer.AddDataItem("gravity", _gravity);
            serializer.AddDataItem("vertical-velocity", _verticalVelocity);
            serializer.AddDataItem("horizontal-movement-direction", _horizontalMovementDirection);
            serializer.AddDataItem("vertical-movement-direction", _verticalMovementDirection);
            serializer.AddDataItem("delta", _delta);
            serializer.AddDataItem("jump-triggered", _jumpTriggered);
            serializer.AddDataItem("jump-in-progess", _jumpInProgress);
            serializer.AddDataItem("is-on-ground", PlayerIsOnGround);
            serializer.AddDataItem("can-move-horizontally", CanMoveHorizontally);
            serializer.AddDataItem("can-trigger-jump", CanTriggerJump);
            serializer.AddDataItem("vertical-move-enabled", VerticalMovementIsEnabled);
            serializer.AddDataItem("moving-left", IsMovingLeft);
            serializer.AddDataItem("speed-step-range", _availableSpeedSteps);
            serializer.AddDataItem("difficulty-booster", DifficultySpeedBoosterUnit);
            serializer.AddDataItem("lockout-timer", _speedChangeLockoutTimer);

            return serializer.SerializedData;
        }

        public void Deserialize(XElement serializedData)
        {
            Serializer serializer = new Serializer(serializedData);

            Speed = serializer.GetDataItem<float>("speed");
            _gravity = serializer.GetDataItem<float>("gravity");
            _verticalVelocity = serializer.GetDataItem<float>("vertical-velocity");
            _horizontalMovementDirection = serializer.GetDataItem<int>("horizontal-movement-direction");
            _verticalMovementDirection = serializer.GetDataItem<int>("vertical-movement-direction");
            _delta = serializer.GetDataItem<Vector2>("delta");
            _jumpTriggered = serializer.GetDataItem<bool>("jump-triggered");
            _jumpInProgress = serializer.GetDataItem<bool>("jump-in-progress");
            PlayerIsOnGround = serializer.GetDataItem<bool>("is-on-ground");
            CanMoveHorizontally = serializer.GetDataItem<bool>("can-move-horizontally");
            CanTriggerJump = serializer.GetDataItem<bool>("can-trigger-jump");
            VerticalMovementIsEnabled = serializer.GetDataItem<bool>("vertical-move-enabled");
            IsMovingLeft = serializer.GetDataItem<bool>("moving-left");
            _availableSpeedSteps = serializer.GetDataItem<Range>("speed-step-range");
            DifficultySpeedBoosterUnit = serializer.GetDataItem<int>("difficulty-booster");
            _speedChangeLockoutTimer = serializer.GetDataItem<Timer>("lockout-timer");
        }

        public void SetForStartSequence(bool startsMovingLeft)
        {
            AvailableSpeedRangeSteps = new Range(0, Starting_Maximum_Speed_Range_Step);

            IsMovingLeft = startsMovingLeft;
            CanMoveHorizontally = false;
            CanTriggerJump = false;
            VerticalMovementIsEnabled = true;

            Speed = Minimum_Movement_Speed;
            _gravity = Definitions.Normal_Gravity_Value;
            _verticalVelocity = Bounce_Vertical_Velocity;
            _jumpTriggered = false;
            _jumpInProgress = false;
        }

        private const int Milliseconds_Per_Speed_Boost_Unit = 16;

        private const float Speed_Change_Rate = 0.015f;

        private const float Jump_Start_Gravity_Value = 0.90f;
        private const float Maximum_Allowed_Height_For_Jump_Start = 120.0f;

        private const float Bounce_Vertical_Velocity = -0.35f;
        private const float Normal_Jump_Vertical_Velocity = -1.5f;
        private const float Boosted_Jump_Vertical_Velocity = -2.75f;
        private const float Gliding_Fall_Velocity = 2.0f;

        public const float Minimum_Movement_Speed = 0.475f;

        public const float Speed_Limit_Range_Step = 0.0775f;
        public const int Maximum_Speed_Range_Steps = 4;
        private const int Starting_Maximum_Speed_Range_Step = 1;

        public const int Ice_Block_Speed_Change_Freeze_In_Milliseconds = 1500;
    }
}
