using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Timing;

using Bopscotch.Data;

namespace Bopscotch.Interface.Dialogs.RaceJoinScene
{
    public class RaceOpponentListDialog : ButtonDialog
    {
        private Dictionary<string, OpponentSelector> _opponents;
        private OpponentSelector _selectedOpponent;

        private Communication.InterDeviceCommunicator _communicator;
        private Timer _timer;

        private float _displayLeftLimit;
        private float _totalDisplayWidth;

        public Communication.ICommunicator Communicator
        {
            set { if (value is Communication.InterDeviceCommunicator) { _communicator = (Communication.InterDeviceCommunicator)value; } }
        }

        public RaceOpponentListDialog(float displayLeftLimit, float totalDisplayWidth)
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Top_Y_When_Active;

            _displayLeftLimit = displayLeftLimit;
            _totalDisplayWidth = totalDisplayWidth;

            _opponents = new Dictionary<string, OpponentSelector>();

            _defaultButtonCaption = "";

            _timer = new Timer("", HandleTimerExpiration);
            GlobalTimerController.GlobalTimer.RegisterUpdateCallback(_timer.Tick);
        }

        private void HandleTimerExpiration()
        {
            if (!_communicator.Active) { InitializeCommunicator(); }
        }

        public override void Reset()
        {
            base.Reset();
            WorldPosition = new Vector2(0.0f, -(Height + Entry_Margin));
        }

        public override void Activate()
        {
            base.Activate();

            _selectedOpponent = null;
            _communicator.CommsEventCallback = HandleCommunicationData;

            if (!_communicator.Active) { _timer.NextActionDuration = Communicator_Activation_Delay; }
        }

        protected override void HandleDialogExitCompletion()
        {
            if ((_selectedOpponent != null) && (_selectedOpponent.HasAccepted)) { _communicator.OtherPlayerRaceID = _selectedOpponent.ID; }
            base.HandleDialogExitCompletion();
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            base.Update(millisecondsSinceLastUpdate);

            UpdateOpponentSelectors(millisecondsSinceLastUpdate);
        }

        private void UpdateOpponentSelectors(int millisecondsSinceLastUpdate)
        {
            foreach (KeyValuePair<string,OpponentSelector> kvp in _opponents)
            {
                kvp.Value.ParentPosition = WorldPosition;
                kvp.Value.Update(millisecondsSinceLastUpdate);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (_opponents.Count > 0)
            {
                foreach (KeyValuePair<string, OpponentSelector> kvp in _opponents) { kvp.Value.Draw(spriteBatch); }
            }
            else
            {
                DrawStatusMessage(spriteBatch);
            }

            if ((_selectedOpponent != null) && (!_selectedOpponent.Active)) 
            { 
                _selectedOpponent = null;
                ReturnToWaitingForInvite();
            }
        }

        private void DrawStatusMessage(SpriteBatch spriteBatch)
        {
            string message = "Finding Opponents...";

            if (!_communicator.Active) { message = "Opening communications..."; }

            if (!string.IsNullOrEmpty(message))
            {
                TextWriter.Write(Translator.Translation(message), spriteBatch,
                    new Vector2(Definitions.Back_Buffer_Center.X, WorldPosition.Y + (Dialog_Height * 0.3f)),
                    Color.White, Color.Black, 3.0f, Text_Render_Depth, TextWriter.Alignment.Center);
            }

            if ((_communicator.Active) && (_timer.CurrentActionProgress == 1.0f) && (_opponents.Count < 1))
            {
                TextWriter.Write(Translator.Translation("Is Wifi switched on and connected?"), spriteBatch,
                    new Vector2(Definitions.Back_Buffer_Center.X, WorldPosition.Y + (Dialog_Height * 0.75f)),
                    Color.White, Color.Black, 3.0f, 0.65f, Text_Render_Depth, TextWriter.Alignment.Center);
            }

        }

        protected override void CheckForAndHandleSelection(Input.InputProcessorBase inputSource)
        {
            base.CheckForAndHandleSelection(inputSource);
			try
			{
	            foreach (KeyValuePair<string, OpponentSelector> kvp in _opponents)
	            {
					if ((kvp.Value != null) && (kvp.Value.ContainsLocation(inputSource.SelectionLocation)))
	                {
	                    HandleOpponentSelection(kvp.Value);
	                    break;
	                }
	            }
			}
			catch
			{
			}
        }

        private void HandleOpponentSelection(OpponentSelector selected)
        {
            switch (selected.State)
            {
                case OpponentSelector.OpponentSelectionState.WaitingForInvite: SetSelectionState(selected); break;
                case OpponentSelector.OpponentSelectionState.HasBeenInvited: SetSelectionState(selected); break;
                case OpponentSelector.OpponentSelectionState.IsInviting: AcceptInvitation(selected); break;
            }
        }

        private void SetSelectionState(OpponentSelector selected)
        {
            if (_selectedOpponent == selected)
            {
                // If we have touched our current invited opponent, switch back to waiting
                _selectedOpponent = null;
                ReturnToWaitingForInvite();
            }
            else
            {
                // otherwise, start sending invites to the selected opponent
                _selectedOpponent = selected;
                _communicator.Message = string.Concat("cmd=invite&target=", _selectedOpponent.ID);

                // Some stuff to allow testing...
                if ((Data.Profile.Settings.TestingRaceMode) && (selected.Name == "test-opp")) { DismissWithReturnValue(selected.ID); }
            }

            // Switch invited (if any) to has been invited state, all others to waiting
            foreach (KeyValuePair<string, OpponentSelector> kvp in _opponents)
            {
                if (kvp.Value == _selectedOpponent) { kvp.Value.State = OpponentSelector.OpponentSelectionState.HasBeenInvited; }
                else { kvp.Value.State = OpponentSelector.OpponentSelectionState.WaitingForInvite; }
            }
        }

        private void AcceptInvitation(OpponentSelector selected)
        {
            // Touched an opponent who has invited - mark as our selected opponent and set state to accepted
            _selectedOpponent = selected;
            _selectedOpponent.State = OpponentSelector.OpponentSelectionState.InvitationAccepted;

            // ... and start sending "accept" messages back
            _communicator.Message = string.Concat("cmd=accept&target=", _selectedOpponent.ID);

            // ... and switch off all other possibles (no changing selection once accepted unless it all falls through)
            foreach (KeyValuePair<string, OpponentSelector> kvp in _opponents)
            {
                if (kvp.Value != _selectedOpponent) { kvp.Value.Suspended = true; }
            }
        }

        private void InitializeCommunicator()
        {
            _opponents.Clear();

            _communicator.Active = true;
            _communicator.SendOnMessageChange = true;
            _communicator.SendInterval = Milliseconds_Between_Send_Attempts;
            _communicator.OtherPlayerRaceID = "";

            ReturnToWaitingForInvite();

            if (Data.Profile.Settings.TestingRaceMode) { AddOpponentToList("test-opponent", "test-opp"); }

            _timer.NextActionDuration = Check_Wifi_Message_Delay;
        }

        public void TearDownCommunicator()
        {
            _communicator.Active = false;
        }

        private bool RequestIsValid(Dictionary<string, string> data)
        {
            return ((data.ContainsKey("id")) && (data.ContainsKey("cmd")) && 
                ((_opponents.ContainsKey(data["id"])) || (data["cmd"] == "join") || (data["cmd"] == "accept")));
        }

        private bool RequestIsFromSelectedOpponent(Dictionary<string, string> data)
        {
            if (!RequestIsValid(data)) { return false;}
            if (_selectedOpponent == null) { return false; }
            if (_selectedOpponent.ID != data["id"]) { return false; }

            return true;
        }

        private bool RequestIsForUs(Dictionary<string, string> data)
        {
            if (!RequestIsValid(data)) { return false; }
            if (!data.ContainsKey("target")) { return false; }
            if (data["target"] != _communicator.OwnPlayerRaceID) { return false; }

            return true;
        }

        private bool RequestIsNotForUs(Dictionary<string, string> data)
        {
            if (!RequestIsValid(data)) { return false; }
            if (!data.ContainsKey("target")) { return false; }
            if (data["target"] == _communicator.OwnPlayerRaceID) { return false; }

            return true;
        }

        private void HandleCommunicationData(Dictionary<string, string> data)
        {
            if (RequestIsValid(data))
            {
                switch (data["cmd"])
                {
                    case "join": HandleJoinRequest(data); break;
                    case "invite": HandleInvitationRequest(data); break;
                    case "accept": HandleInvitationAccepted(data); break;
                    case "inv-ack": HandleInvitationAcknowledged(data); break;
                }

                if (_opponents.ContainsKey(data["id"])) { _opponents[data["id"]].ResetCommsTimeoutCounter();}
            }
        }

        private void HandleJoinRequest(Dictionary<string, string> data)
        {
            // If join is from unknown source, add to the list
            if ((_opponents.Count < Maximum_Opponents) && (data.ContainsKey("id")) && (data.ContainsKey("name")))
            {
                AddOpponentToList(data["id"], data["name"]);
            }

            // If join is from an opponent that was inviting but has now switched back to join, switch the state back to joining
            if (_opponents[data["id"]].State == OpponentSelector.OpponentSelectionState.IsInviting) 
            { 
                _opponents[data["id"]].State = OpponentSelector.OpponentSelectionState.WaitingForInvite; 
            }
        }

        private void AddOpponentToList(string id, string name)
        {
            if (!_opponents.ContainsKey(id))
            {
                _opponents.Add(id,
                    new OpponentSelector(
                        new Vector2(_displayLeftLimit + (_totalDisplayWidth - Button_X_Margin),
                            Button_Top_Offset + (OpponentSelector.Height * _opponents.Count)),
                        _displayLeftLimit,
                        id, 
                        name));
            }
        }

        private void HandleInvitationRequest(Dictionary<string, string> data)
        {
            if (RequestIsForUs(data) && (!_opponents.ContainsKey(data["id"]))) { AddOpponentToList(data["id"], data["name"]); }

            // Request is valid and targeting us, switch inviting opponent to "is inviting" state
            if ((RequestIsForUs(data)) && (!RequestIsFromSelectedOpponent(data)))
            { 
                _opponents[data["id"]].State = OpponentSelector.OpponentSelectionState.IsInviting; 
            }

            // If request is from an opponent we are trying to accept an invite from but NOT targeting us, switch back to waiting and clear selection
            if ((RequestIsNotForUs(data)) && (_opponents[data["id"]].State != OpponentSelector.OpponentSelectionState.WaitingForInvite))
            {
                _opponents[data["id"]].State = OpponentSelector.OpponentSelectionState.WaitingForInvite;
                if (_opponents[data["id"]] == _selectedOpponent) { _selectedOpponent = null; }
                ReturnToWaitingForInvite();
            }
        }

        private void ReturnToWaitingForInvite()
        {
            foreach (KeyValuePair<string, OpponentSelector> kvp in _opponents) { kvp.Value.Suspended = false; }

            _communicator.Message = string.Concat("cmd=join&name=", Data.Profile.Settings.RaceName);
        }

        private void HandleInvitationAccepted(Dictionary<string, string> data)
        {
            // If we have a selected opponent we are inviting and the data matches
            if ((RequestIsFromSelectedOpponent(data)) && (RequestIsForUs(data)))
            {
                if (Active)
                {
                    // Accepted our invitation - flag that the invitation has been accepted
                    _selectedOpponent.State = OpponentSelector.OpponentSelectionState.HasAcceptedInvite;

                    // ... and start sending "acknowledge" messages back
                    _communicator.Message = string.Concat("cmd=inv-ack&target=", _selectedOpponent.ID);

                    DismissWithReturnValue(_selectedOpponent.ID);
                }
            }
            else
            {
                // Either this is not our selected opponent or they have sent the accept to someone else - either way, remove from list
                _opponents.Remove(data["id"]);

                // If this is our selected opponent accepting someone else's invite, return us to waiting state
                if (RequestIsFromSelectedOpponent(data))
                {
                    _selectedOpponent = null;
                    ReturnToWaitingForInvite();
                }
            }
        }

        private void HandleInvitationAcknowledged(Dictionary<string, string> data)
        {
            if (RequestIsFromSelectedOpponent(data))
            {
                if (RequestIsForUs(data)) 
                {
                    if (Active) { DismissWithReturnValue(_selectedOpponent.ID); }
                }
                else
                {
                    _selectedOpponent.State = OpponentSelector.OpponentSelectionState.WaitingForInvite;
                    _selectedOpponent = null;
                    ReturnToWaitingForInvite();
                }
            }
        }

        private const float Text_Render_Depth = 0.141f;
        private const int Milliseconds_Between_Send_Attempts = 1000;

        private const int Maximum_Opponents = 4;
        private const float Button_X_Margin = 220.0f;
        private const float Button_Top_Offset = 80.0f;

        private const int Communicator_Activation_Delay = 300;
        private const int Check_Wifi_Message_Delay = 10000;

        public const int Dialog_Height = 450;
        public const float Top_Y_When_Active = 125.0f;
    }
}

