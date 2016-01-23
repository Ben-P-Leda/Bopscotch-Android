using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;
using Leda.Core.Motion;
using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Animation.Skeletons;

using Bopscotch.Communication;
using Bopscotch.Gameplay.Coordination;
using Bopscotch.Data.Avatar;
using Bopscotch.Effects.Particles;

namespace Bopscotch.Gameplay.Objects.Characters
{
    public class RaceOpponent : DisposableSkeleton, ICameraRelative, IMobile
    {
        private long _millisecondsSinceLastComms;
        private Vector2 _peerVelocity;
        private Vector2 _expectedPosition;
        private Vector2 _lastCommsPosition;
        private int _lastCommsUpdateTime;
        private int _packetsAtCurrentPosition;
        private int _restartDirection;
        private Vector2 _clientVelocity;
        private int _millisecondsToRestart;
        private int _currentLap;
        private int _currentCheckpoint;
        private float _fadeFraction;
        private Vector2 _displayPosition;

        public InterDeviceCommunicator Communicator { private get; set; }
        public IMotionEngine MotionEngine { get { return null; } }
        public AdditiveLayerParticleEffectManager ParticleManager { private get; set; }


        public RaceOpponent()
            : base()
        {
            RenderLayer = 2;
            WorldPositionIsFixed = false;
            Scale = 0.9f;
            RenderDepth = 0.495f;
        }

        public override void Reset()
        {
            _fadeFraction = 0.0f;

            _currentLap = 0;
            _currentCheckpoint = 0;
            _millisecondsSinceLastComms = 0;
            _peerVelocity = Vector2.Zero;

            if (Bones.Count < 1)
            {
                CreateBonesFromDataManager(Definitions.Avatar_Skeleton_Side);
                AddBone(CreateCloudBone(), "body");
            }

            SkinBones(AvatarComponentManager.SideFacingAvatarSkin(Communicator.OtherPlayerAvatarSlot));
            Rectangle bounds = TextureManager.Textures["race-p2-cloud"].Bounds;
            Bones["cloud"].ApplySkin("race-p2-cloud", new Vector2(bounds.Width, bounds.Height) * 0.5f, bounds);

            _displayPosition = Vector2.Zero;
        }

        private StorableBone CreateCloudBone()
        {
            StorableBone cloud = new StorableBone();
            cloud.ID = "cloud";
            cloud.RelativePosition = new Vector2(0.0f, 30.0f);
            cloud.RelativeDepth = -0.1f;

            return cloud;
        }

        public void SetForRaceStart(Vector2 startPosition, bool facingLeft)
        {
            _restartDirection = (facingLeft ? -1 : 1);
            _expectedPosition = startPosition;
            _clientVelocity = Vector2.Zero;

            _lastCommsPosition = startPosition;
            _packetsAtCurrentPosition = 0;

            _displayPosition = startPosition;
            Visible = true;
            Mirror = facingLeft;
        }

        public void StartMovement()
        {
            _clientVelocity = new Vector2(Player.PlayerMotionEngine.Minimum_Movement_Speed * _restartDirection, 0.0f) * 0.925f;
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            if (_millisecondsSinceLastComms > Communicator.MillisecondsSinceLastReceive)
            {
                LogPositionUpdates();
                UpdateProgress();
                UpdateLifeState();

                if (Visible)
                {
                    Vector2 step = ((Communicator.OtherPlayerData.PlayerWorldPosition - _expectedPosition) / _millisecondsSinceLastComms);
                    _peerVelocity = (_peerVelocity * 0.5f) + (step * 0.5f);
                }
            }
            else
            {
                CheckForRestart(millisecondsSinceLastUpdate);
            }

            _expectedPosition += _peerVelocity * millisecondsSinceLastUpdate;
            _millisecondsSinceLastComms = Communicator.MillisecondsSinceLastReceive;

            UpdateClientVelocity();
            UpdateDisplaySettings();

            _displayPosition += _clientVelocity * millisecondsSinceLastUpdate;
            WorldPosition = _displayPosition - new Vector2(0.0f, 40.0f);
        }

        private void LogPositionUpdates()
        {
            if (Communicator.OtherPlayerData.PlayerWorldPosition != _lastCommsPosition)
            {
                _lastCommsPosition = Communicator.OtherPlayerData.PlayerWorldPosition;
                _lastCommsUpdateTime = Communicator.OtherPlayerData.TotalRaceTimeElapsedInMilliseconds;
                _packetsAtCurrentPosition = 0;
            }
            else
            {
                _packetsAtCurrentPosition++;
            }
        }

        private void UpdateProgress()
        {
            if ((Communicator.OtherPlayerData.LapsCompleted > _currentLap) || (Communicator.OtherPlayerData.LastCheckpointIndex > _currentCheckpoint))
            {
                _currentCheckpoint = Communicator.OtherPlayerData.LastCheckpointIndex;
                _currentLap = Communicator.OtherPlayerData.LapsCompleted;

                _restartDirection = Math.Sign(_peerVelocity.X);
            }
        }

        private void UpdateLifeState()
        {
            if ((Visible) && (_packetsAtCurrentPosition > 1))
            {
                _peerVelocity = Vector2.Zero;
                _millisecondsToRestart = RaceProgressCoordinator.Race_Resurrect_Sequence_Duration_In_Milliseconds - (int)_millisecondsSinceLastComms;

                Visible = false;
                ParticleManager.LaunchCloudBurst(this);
            }
            else if ((!Visible) && (_packetsAtCurrentPosition < 1))
            {
                _expectedPosition = Communicator.OtherPlayerData.PlayerWorldPosition;
                _clientVelocity = Vector2.Zero;
                _peerVelocity = Vector2.Zero;

                _displayPosition = Communicator.OtherPlayerData.PlayerWorldPosition;
                _fadeFraction = 0.0f;
                Mirror = (_restartDirection < 0);
                Visible = true;
            }
        }

        private void CheckForRestart(int millisecondsSinceLastUpdate)
        {
            if (_millisecondsToRestart > 0)
            {
                _millisecondsToRestart -= millisecondsSinceLastUpdate;

                if (_millisecondsToRestart < 1)
                {
                    StartMovement();
                }
            }
        }

        private void UpdateClientVelocity()
        {
            if (_peerVelocity.X != 0.0f)
            {
                if (_clientVelocity.X == 0)
                {
                    StartMovement();
                }

                _clientVelocity = (_clientVelocity * 0.5f) + (_peerVelocity * 0.5f);
            }
        }

        private void UpdateDisplaySettings()
        {
            if (_millisecondsSinceLastComms > Latency_Threshold)
            {
                _fadeFraction = Math.Min(_fadeFraction + Fade_Step, 0.65f);
            }
            else
            {
                _fadeFraction = Math.Max(0.2f, _fadeFraction - Fade_Step);
            }

            Tint = Color.Lerp(Color.White, Color.Transparent, _fadeFraction);

            if (_clientVelocity.X != 0.0f)
            {
                Mirror = _clientVelocity.X < 0.0f;
            }
        }

        private const int Latency_Threshold = 500;
        private const float Fade_Step = 0.025f;
        private const float Maximum_Tilt = 0.35f;
    }
}
