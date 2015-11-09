using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Timing;

using Bopscotch.Data;
using Bopscotch.Communication;
using Bopscotch.Interface;
using Bopscotch.Effects.Popups;
using Bopscotch.Gameplay.Objects.Display.Race;
using Bopscotch.Gameplay.Objects.Characters.Player;
using Bopscotch.Gameplay.Objects.Environment.Flags;

namespace Bopscotch.Gameplay.Coordination
{
    public class RaceProgressCoordinator : RacePlayerCommunicationData, IGameObject, IPausable
    {
        private Timer _sequenceTimer;
        private bool _activated;
        private int _lastLapStartTime;
        private Dictionary<string, int> _ownCheckpointTimes;
        private Dictionary<string, int> _opponentCheckpointTimes;
        private int _lastOpponentAttackTime;

        public bool Paused { private get; set; }

        public int LapsToComplete { private get; set; }
        public Vector2 RestartPosition { get; private set; }
        public bool RestartsFacingLeft { get; private set; }

        public Player Player { private get; set; }
        public override Vector2 PlayerWorldPosition { get { return Player.WorldPosition; } }

        public ICommunicator Communicator { private get; set; }
        public RaceInfoPopup StatusPopup { private get; set; }
        public RaceDataDisplay StatusDisplay { private get; set; }

        public TimerController.UpdateCallback SequenceTimerTick { get { return _sequenceTimer.Tick; } }

        public int CurrentLapTimeInMilliseconds { get { return TotalRaceTimeElapsedInMilliseconds - CurrentLapTimeInMilliseconds; } }

        public Definitions.PowerUp NextOpponentAttackPowerUp { get { return Communicator.OtherPlayerData.LastAttackPowerUp; } }
        public bool OpponentAttackPending
        {
            get
            {
                if (Communicator.OtherPlayerData.LastAttackPowerUp == Definitions.PowerUp.None) { return false; }
                if (Communicator.OtherPlayerData.LastAttackPowerUpTimeInMilliseconds <= _lastOpponentAttackTime) { return false; }
                if (TotalRaceTimeElapsedInMilliseconds < Communicator.OtherPlayerData.LastAttackPowerUpTimeInMilliseconds + Attack_Lag_In_Milliseconds) { return false; }
                return true;
            }
        }

        public RaceProgressCoordinator()
            : base()
        {
            _sequenceTimer = new Timer("sequence-timer", HandleTimedSequenceCompletion);
            _activated = false;

            _lastLapStartTime = 0;
            _ownCheckpointTimes = new Dictionary<string, int>();
            _opponentCheckpointTimes = new Dictionary<string, int>();

            LapsToComplete = 0;
        }

        private void HandleTimedSequenceCompletion()
        {
            Player.SetForMovement();
            _activated = true;
        }

        public void StartTimerToBeginningOfRace()
        {
            _sequenceTimer.NextActionDuration = Race_Start_Sequence_Duration_In_Milliseconds;
        }

        public void Initialize()
        {
        }

        public void Reset()
        {
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            if ((_activated) && (!Paused))
            {
                TotalRaceTimeElapsedInMilliseconds += millisecondsSinceLastUpdate;
                
                UpdateProgressData(_ownCheckpointTimes, this);
                UpdateProgressData(_opponentCheckpointTimes, Communicator.OtherPlayerData);
            }

            StatusDisplay.SetLapCountText(LapsCompleted, LapsToComplete);
            StatusDisplay.SetTotalTimeText(TotalRaceTimeElapsedInMilliseconds);
        }

        private void UpdateProgressData(Dictionary<string,int> progressData, RacePlayerCommunicationData dataSource)
        {
            string lapSectionKey = string.Concat(dataSource.LapsCompleted, ":", dataSource.LastCheckpointIndex);

            if (!progressData.ContainsKey(lapSectionKey)) { progressData.Add(lapSectionKey, 0); }
            progressData[lapSectionKey] = dataSource.TotalRaceTimeElapsedInMilliseconds - dataSource.LastCheckpointTimeInMilliseconds;
        }

        public bool LapCompleted()
        {
            if ((Player.LastRaceRestartPointTouched.ActivatedWhenMovingLeft == Player.IsMovingLeft) && (LastCheckpointIndex > -1))
            {
                LastCheckpointIndex = -1;
                LapsCompleted++;
                _lastLapStartTime = TotalRaceTimeElapsedInMilliseconds;

                UpdatePositionStatus(false);
                return true;
            }

            return false;
        }

        public void CheckAndUpdateRestartPoint()
        {
            if (Player.LastRaceRestartPointTouched.ActivatedWhenMovingLeft == Player.IsMovingLeft)
            {
                if (Player.LastRaceRestartPointTouched is GoalFlag)
                {
                    LastCheckpointTimeInMilliseconds = TotalRaceTimeElapsedInMilliseconds;
                    SetRestartPoint();
                }
                else if (((CheckpointFlag)Player.LastRaceRestartPointTouched).CheckpointIndex > LastCheckpointIndex)
                {
                    LastCheckpointIndex = ((CheckpointFlag)Player.LastRaceRestartPointTouched).CheckpointIndex;
                    LastCheckpointTimeInMilliseconds = TotalRaceTimeElapsedInMilliseconds;
                    SetRestartPoint();

                    UpdatePositionStatus(false);
                }
            }
        }

        private void UpdatePositionStatus(bool dueToResurrection)
        {
            string stateTextureName = "";

            if (Leading) 
            { 
                stateTextureName = Leading_Texture_Name;
            }
            else if (Trailing)
            {
                stateTextureName = Falling_Back_Texture_Name;
                if (!dueToResurrection)
                {
                    int timeDifference = ((LastCheckpointIndex < 0) ? LapTimeDifference : SectionTimeDifference);
                    if (timeDifference < 0) { stateTextureName = Catching_Up_Texture_Name; }
                }
            }

            if (!string.IsNullOrEmpty(stateTextureName)) { StatusPopup.StartPopupForRaceInfo(stateTextureName); }
        }

        private bool Leading
        {
            get
            {
                if (LapsCompleted > Communicator.OtherPlayerData.LapsCompleted) { return true; }
                else if (LapsCompleted < Communicator.OtherPlayerData.LapsCompleted) { return false; }
                else if (LastCheckpointIndex > Communicator.OtherPlayerData.LastCheckpointIndex) { return true; }
                else { return false; }
            }
        }

        private bool Trailing 
        { 
            get 
            {
                if (Leading) { return false; }
                else if (LapsCompleted < Communicator.OtherPlayerData.LapsCompleted) { return true; }
                else if (LastCheckpointIndex < Communicator.OtherPlayerData.LastCheckpointIndex) { return true; }
                else if (LastCheckpointTimeInMilliseconds > Communicator.OtherPlayerData.LastCheckpointTimeInMilliseconds) { return true; }
                else { return false; }
            } 
        }

        private int LapTimeDifference
        {
            get
            {
                return LapTime(_ownCheckpointTimes, LapsCompleted - 1) - LapTime(_opponentCheckpointTimes, LapsCompleted - 1);
            }
        }

        private int LapTime(Dictionary<string, int> sectionTimes, int lapNo)
        {
            int lapTime = 0;
            foreach (KeyValuePair<string, int> kvp in sectionTimes)
            {
                if (kvp.Key.StartsWith(string.Concat(lapNo, ":"))) { lapTime += kvp.Value;}
            }

            return lapTime;
        }

        private int SectionTimeDifference
        {
            get
            {
                string lapSectionKey = string.Concat(LapsCompleted, ":", LastCheckpointIndex - 1);
                if ((_opponentCheckpointTimes.ContainsKey(lapSectionKey)) && (_ownCheckpointTimes.ContainsKey(lapSectionKey)))
                {
                    return _ownCheckpointTimes[lapSectionKey] - _opponentCheckpointTimes[lapSectionKey];
                }
                else
                {
                    return 0;
                }
            }
        }

        public void SetRestartPoint()
        {
            RestartPosition = new Vector2(Player.WorldPosition.X, Player.WorldPosition.Y + Player.DistanceToGround - Player.CollisionBoundingCircle.Radius);
            RestartsFacingLeft = Player.IsMovingLeft;
        }

        public void ResurrectPlayerAtLastRestartPoint()
        {
            Player.WorldPosition = RestartPosition;
            Player.Mirror = RestartsFacingLeft;
            Player.Activate();

            _sequenceTimer.NextActionDuration = Race_Resurrect_Sequence_Duration_In_Milliseconds;

            UpdatePositionStatus(true);
        }

        public void CompleteRace()
        {
            _activated = false;
        }

        public void StartAttackSequence(Definitions.PowerUp attackPowerUp)
        {
            LastAttackPowerUp = attackPowerUp;
            LastAttackPowerUpTimeInMilliseconds = TotalRaceTimeElapsedInMilliseconds;
        }

        public void SetLastOpponentAttackTime()
        {
            _lastOpponentAttackTime = Communicator.OtherPlayerData.LastAttackPowerUpTimeInMilliseconds;
        }

        public Definitions.RaceOutcome Result
        {
            get
            {
                if ((LapsCompleted >= LapsToComplete) && (Communicator.OtherPlayerData.LapsCompleted < LapsToComplete)) 
                {
                    return Definitions.RaceOutcome.OwnPlayerWin;
                }
                else if ((LapsCompleted < LapsToComplete) && (Communicator.OtherPlayerData.LapsCompleted >= LapsToComplete))
                {
                    return Definitions.RaceOutcome.OpponentPlayerWin;
                }
                else if ((LapsCompleted >= LapsToComplete) && (Communicator.OtherPlayerData.LapsCompleted >= LapsToComplete))
                {
                    if (TotalRaceTimeElapsedInMilliseconds < Communicator.OtherPlayerData.TotalRaceTimeElapsedInMilliseconds)
                    {
                        return Definitions.RaceOutcome.OwnPlayerWin;
                    }
                    else if (TotalRaceTimeElapsedInMilliseconds > Communicator.OtherPlayerData.TotalRaceTimeElapsedInMilliseconds)
                    {
                        return Definitions.RaceOutcome.OpponentPlayerWin;
                    }
                    else
                    {
                        return Definitions.RaceOutcome.Draw;
                    }
                }
                else
                {
                    return Definitions.RaceOutcome.Incomplete;
                }
            }
        }

        private const int Race_Resurrect_Sequence_Duration_In_Milliseconds = 750;
        private const int Race_Start_Sequence_Duration_In_Milliseconds = 3500;
        private const int Attack_Lag_In_Milliseconds = 2000;

        private const string Leading_Texture_Name = "popup-race-leading";
        private const string Catching_Up_Texture_Name = "popup-race-catching-up";
        private const string Falling_Back_Texture_Name = "popup-race-falling-back";
    }
}
