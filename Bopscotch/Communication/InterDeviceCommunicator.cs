using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core;

namespace Bopscotch.Communication
{
    public class InterDeviceCommunicator : GameComponent, ICommunicator
    {
        private TimeSpan _lastUpdateTime;
        private int _millisecondsToNextSend;
        private string _message;
        private string[] _expectedKeys;
        private int _lastReceivedTimestamp;
        private int _millisecondsSinceLastTimestampChange;

        private OpponentRaceProgressCoordinator _otherPlayerData;
        private string _selectedCourse;

        private CommunicationHandler _communicationHandler;

        private Dictionary<string,Definitions.PowerUp> _powerUpTranslator;

        public bool Active
        {
            get 
            {
                return Enabled;
            }
            set 
            {
                Enabled = value;
                if (value) { _millisecondsToNextSend = 0; MillisecondsSinceLastReceive = 0; }
                else { if (_communicationHandler != null) { _communicationHandler.Close(); } _communicationHandler = null; }
            }
        }

        public long MillisecondsSinceLastReceive { get; private set; }

        public string OwnPlayerRaceID { get; private set; }
        public string OtherPlayerRaceID { get; set; }
        public string OtherPlayerName { get; set; }
        public int OtherPlayerAvatarSlot { get; set; }

        public int SendInterval { private get; set; }
        public bool SendOnMessageChange { private get; set; }
        public string Message { set { _message = value; if (SendOnMessageChange) { SendMessage(); } } }
        public CommunicationHandler.CommsEventHandler CommsEventCallback { private get; set; }

        public Data.RacePlayerCommunicationData OwnPlayerData { get; set; }
        public Data.RacePlayerCommunicationData OtherPlayerData { get { return _otherPlayerData; } }

        public bool RaceInProgress { private get; set; }

        public bool ConnectionLost
        {
            get
            {
                if (MillisecondsSinceLastReceive > Connection_Loss_Timeout_In_Milliseconds) { return true; }
                if ((RaceInProgress) && (_millisecondsSinceLastTimestampChange > Connection_Loss_Timeout_In_Milliseconds))
                {
                    return true;
                }

                return false;
            }
        }

        public InterDeviceCommunicator()
            : base(GameBase.Instance)
        {
            GameBase.Instance.Components.Add(this);

            _expectedKeys = Expected_Race_Keys.Split(',');
            _lastUpdateTime = TimeSpan.Zero;
            _message = "";

            OwnPlayerRaceID = Guid.NewGuid().ToString();
            OtherPlayerRaceID = "";
            SendInterval = Default_Send_Interval;

            Enabled = false;
            SendOnMessageChange = true;

            _powerUpTranslator = new Dictionary<string, Definitions.PowerUp>();
            foreach (Definitions.PowerUp p in Enum.GetValues(typeof(Definitions.PowerUp))) { _powerUpTranslator.Add(p.ToString().ToLower(), p); }
        }

        public void Update()
        {
            Message = string.Format("target={0}&elapsed={1}&laps={2}&pos-x={3}&pos-y={4}&cp-time={5}&cp-no={6}&pwr={7}&pwr-time={8}",
                OtherPlayerRaceID,
                OwnPlayerData.TotalRaceTimeElapsedInMilliseconds,
                OwnPlayerData.LapsCompleted,
                OwnPlayerData.PlayerWorldPosition.X,
                OwnPlayerData.PlayerWorldPosition.Y,
                OwnPlayerData.LastCheckpointTimeInMilliseconds,
                OwnPlayerData.LastCheckpointIndex,
                OwnPlayerData.LastAttackPowerUp,
                OwnPlayerData.LastAttackPowerUpTimeInMilliseconds);

            if (!string.IsNullOrEmpty(_selectedCourse)) { Message = string.Concat(_message, "&course=", _selectedCourse); }
        }

        public override void Update(GameTime gameTime)
        {
            if (_communicationHandler == null) { OpenCommunicationHandler(); }
            else if (_communicationHandler.CommsCallback != CommsEventCallback) { _communicationHandler.CommsCallback = CommsEventCallback; }

            int lastUpdateDuration = 0;
            if (_lastUpdateTime != TimeSpan.Zero)
            {
                TimeSpan difference = (gameTime.TotalGameTime - _lastUpdateTime);
                lastUpdateDuration = difference.Milliseconds + (difference.Seconds / 1000);
            }

            _millisecondsToNextSend -= lastUpdateDuration;
            if (_millisecondsToNextSend < 1) { SendMessage(); }

            base.Update(gameTime);

            MillisecondsSinceLastReceive += lastUpdateDuration;
			if (RaceInProgress) { _millisecondsSinceLastTimestampChange += lastUpdateDuration; }
            _lastUpdateTime = gameTime.TotalGameTime;
        }

        private void OpenCommunicationHandler()
        {
            try
            {
                _communicationHandler = new Communication.CommunicationHandler();
                _communicationHandler.CommsCallback = CommsEventCallback;
                _communicationHandler.MyID = OwnPlayerRaceID;
                _communicationHandler.Open();
            }
            catch
            {
                if (_communicationHandler != null)
                {
                    _communicationHandler.Close();
                    _communicationHandler = null;
                }
            }
        }

        private void SendMessage()
        {
            if ((Enabled) && (!string.IsNullOrEmpty(_message)))
            {
                if (_communicationHandler == null)
                {
                    OpenCommunicationHandler();
                }
                else
                {
                    try { _communicationHandler.SendData(_message); }
                    catch { _communicationHandler.Close(); _communicationHandler = null; }
                }
                _millisecondsToNextSend = SendInterval;
            }
        }

        public void SetForRace(string selectedCourse)
        {
            SendInterval = Default_Send_Interval;
            Message = "";
            Active = true;
            SendOnMessageChange = false;
            CommsEventCallback = HandleCommsEvent;
            RaceInProgress = true;

            MillisecondsSinceLastReceive = 0;
            _lastReceivedTimestamp = 0;
            _millisecondsSinceLastTimestampChange = 0;
            _otherPlayerData = new OpponentRaceProgressCoordinator();
            _selectedCourse = selectedCourse;
        }

        private void HandleCommsEvent(Dictionary<string, string> data)
        {
            if ((RaceIDIsValid(OwnPlayerRaceID, "target", data)) && (RaceIDIsValid(OtherPlayerRaceID, "id", data)))
            {
                if (AllRaceKeysArePresent(data))
                {
                    bool dataIsValid = true;

                    int laps = 0;
                    if ((!int.TryParse(data["laps"], out laps)) || (laps < 0)) { dataIsValid = false; }

                    float xpos = -1.0f;
                    if (!float.TryParse(data["pos-x"], out xpos)) { dataIsValid = false; }
                    float ypos = -1.0f;
                    if (!float.TryParse(data["pos-y"], out ypos)) { dataIsValid = false; }

                    int elapsed = -1;
                    if (!int.TryParse(data["elapsed"], out elapsed)) { dataIsValid = false; }
                    int lastCheckIndex = -1;
                    if (!int.TryParse(data["cp-no"], out lastCheckIndex)) { dataIsValid = false; }
                    int lastCheckTime = -1;
                    if (!int.TryParse(data["cp-time"], out lastCheckTime)) { dataIsValid = false; }

                    if (dataIsValid)
                    {
                        MillisecondsSinceLastReceive = 0;
                        _otherPlayerData.Populate(elapsed, laps, lastCheckIndex, lastCheckTime, xpos, ypos);

                        if ((data.ContainsKey("pwr")) && (_powerUpTranslator.ContainsKey(data["pwr"].ToLower())))
                        {
                            int attackTime = 0;
                            if (data.ContainsKey("pwr-time")) { int.TryParse(data["pwr-time"], out attackTime); }

                            if ((attackTime > _otherPlayerData.LastAttackPowerUpTimeInMilliseconds) ||
                                (_powerUpTranslator[data["pwr"].ToLower()] == Definitions.PowerUp.None))
                            {
                                _otherPlayerData.LastAttackPowerUp = _powerUpTranslator[data["pwr"].ToLower()];
                                _otherPlayerData.LastAttackPowerUpTimeInMilliseconds = attackTime;
                            }
                        }

                        _selectedCourse = "";

                        if (elapsed > _lastReceivedTimestamp)
                        {
                            _lastReceivedTimestamp = elapsed;
                            _millisecondsSinceLastTimestampChange = 0;
                        }
                    }
                }
                else if ((!OtherPlayerData.ReadyToRace) && (data.ContainsKey("course")))
                {
                    MillisecondsSinceLastReceive = 0;
                }
            }
        }

        private bool AllRaceKeysArePresent(Dictionary<string, string> data)
        {
            for (int i = 0; i < _expectedKeys.Length; i++)
            {
                if (!data.ContainsKey(_expectedKeys[i])) { return false; break; }
            }
            return true;
        }

        private bool RaceIDIsValid(string raceID, string fieldName, Dictionary<string, string> data)
        {
            Guid temp = Guid.Empty;
            if (!data.ContainsKey(fieldName)) { return false; }
            if (!Guid.TryParse(data[fieldName], out temp)) { return false; }
            if (data[fieldName] != raceID) { return false; }

            return true;
        }

        public void ResetLastReceiveTime()
        {
            MillisecondsSinceLastReceive = 0;
        }

        private const int Default_Send_Interval = 64;
        private const string Expected_Race_Keys = "id,target,elapsed,laps,pos-x,pos-y,cp-time,cp-no";
        private const int Connection_Loss_Timeout_In_Milliseconds = 5000;
    }
}
