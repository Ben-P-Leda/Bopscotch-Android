using System;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Gamestate_Management;

using Bopscotch.Interface.Dialogs.Carousel;

namespace Bopscotch.Interface.Dialogs.RaceJoinScene
{
    public class CourseSelectionCarouselDialog : AreaSelectionCarouselDialog
    {
        private string _opponentCourseSelection;
        private Communication.InterDeviceCommunicator _communicator;
        private string _ownCourseSelection;

        public string SelectedCourseName { get; private set; }
        public Communication.ICommunicator Communicator
        {
            set { if (value is Communication.InterDeviceCommunicator) { _communicator = (Communication.InterDeviceCommunicator)value; } }
        }

        public CourseSelectionCarouselDialog(Scene.ObjectRegistrationHandler registrationHandler, Scene.ObjectUnregistrationHandler unregistrationHandler)
            : base(registrationHandler, unregistrationHandler)
        {
            _boxCaption = Translator.Translation("Select Course");

            Height = Dialog_Height;
            TopYWhenInactive = Definitions.Back_Buffer_Height;
            CarouselCenter = new Vector2(Definitions.Back_Buffer_Center.X, Carousel_Center_Y);
            CarouselRadii = new Vector2(Carousel_Horizontal_Radius, Carousel_Vertical_Radius);

            SetupButtonLinkagesAndDefaultValues();

            registrationHandler(this);
        }

        protected override void CreateButtons()
        {
            AddIconButton("previous", new Vector2(Definitions.Back_Buffer_Center.X - 450, 305), Button.ButtonIcon.Previous, Color.DodgerBlue);
            AddIconButton("next", new Vector2(Definitions.Back_Buffer_Center.X + 450, 305), Button.ButtonIcon.Next, Color.DodgerBlue);

            AddButton("Back", new Vector2(Definitions.Left_Button_Column_X, 625), Button.ButtonIcon.Back, Color.Red, 0.7f);
            AddButton("Select", new Vector2(Definitions.Right_Button_Column_X, 625), Button.ButtonIcon.Play, Color.LawnGreen, 0.7f);

            base.CreateButtons();
        }

        public override void Activate()
        {
            base.Activate();

            SelectedCourseName = "";

            _ownCourseSelection = No_Course_Selected_Message;
            _opponentCourseSelection = "";
            _communicator.CommsEventCallback = HandleCommunicationData;
			_communicator.RaceInProgress = false;
            _communicator.ResetLastReceiveTime();
            SetCourseToTransmit(_ownCourseSelection);

            UpdateAnnotations();
            ActivateButton("Select");
        }

        private void HandleCommunicationData(Dictionary<string, string> data)
        {
            if (CommunicationDataIsValid(data))
            {
                _communicator.ResetLastReceiveTime();
                if (data["course"] != _opponentCourseSelection)
                {
                    _opponentCourseSelection = data["course"];
                    UpdateAnnotations();
                }
            }
        }

        private bool CommunicationDataIsValid(Dictionary<string, string> data)
        {
            foreach (string s in Communication_Keys.Split(','))
            {
                if (!data.ContainsKey(s)) { return false; break; }
            }

            if (data["id"] != _communicator.OtherPlayerRaceID) { return false; }
            if (data["target"] != _communicator.OwnPlayerRaceID) { return false; }

            return true;
        }

        private void SetCourseToTransmit(string courseName)
        {
            _communicator.Message = string.Concat("target=", _communicator.OtherPlayerRaceID, "&course=", courseName);
        }

        private void UpdateAnnotations()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].SelectionValue == _opponentCourseSelection) { ((CarouselAreaImage)_items[i]).Annotation = "P2"; }
                else { ((CarouselAreaImage)_items[i]).Annotation = ""; }
            }
        }

        protected override void GetAreaData()
        {
            base.GetAreaData();

            for (int i = _dataSource.Count - 1; i >= 0; i--)
            {
                if ((bool)_dataSource[i].Attribute("no-race")) { _dataSource.RemoveAt(i); }
                else { _dataSource[i].SetAttributeValue("locked", false); }
            }
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            base.Update(millisecondsSinceLastUpdate);

            if (Active)
            {
                if (_communicator.ConnectionLost) 
                { 
                    DismissWithReturnValue("Disconnected"); 
                }
                else if ((_ownCourseSelection != No_Course_Selected_Message) && (_ownCourseSelection == _opponentCourseSelection)) 
                { 
                    DismissWithReturnValue("Select"); 
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            DrawOpponentSelectionDetails(spriteBatch);
        }

        protected override void DrawAreaDetails(SpriteBatch spriteBatch)
        {
            string areaText = _dataSource[SelectedItem].Attribute("name").Value;

            if ((bool)_dataSource[SelectedItem].Attribute("locked"))
            {
                areaText = string.Concat(areaText, " (", Translator.Translation("locked"), ")");
            }

            if (!string.IsNullOrEmpty(SelectedCourseName)) { areaText = string.Concat(areaText, " (", Translator.Translation("selected"), ")"); }

            TextWriter.Write(areaText, spriteBatch, new Vector2(Definitions.Back_Buffer_Center.X, WorldPosition.Y + 425.0f),
                TransitionTint(_textTint), TransitionTint(Color.Black), 3.0f, 0.1f, TextWriter.Alignment.Center);
        }

        private void DrawOpponentSelectionDetails(SpriteBatch spriteBatch)
        {
            string opponentText = "";

            if ((string.IsNullOrEmpty(_opponentCourseSelection)) || (_opponentCourseSelection == No_Course_Selected_Message))
            {
                opponentText = Translator.Translation("awaiting opponent selection");
            }
            else if ((!string.IsNullOrEmpty(SelectedCourseName)) && (_opponentCourseSelection != SelectedCourseName))
            {
                opponentText = Translator.Translation("awaiting opponent confirmation");
            }

            if (!string.IsNullOrEmpty(opponentText))
            {
                TextWriter.Write(opponentText, spriteBatch, new Vector2(Definitions.Back_Buffer_Center.X, WorldPosition.Y + 510.0f), 
                    TransitionTint(_textTint), TransitionTint(Color.Black), 3.0f, 0.75f, 0.1f, TextWriter.Alignment.Center);
            }
        }

        protected override void HandleStepSelection(string buttonCaption)
        {
            if ((buttonCaption == "previous") || (buttonCaption == "next")) 
            { 
                SelectedCourseName = "";
                _ownCourseSelection = No_Course_Selected_Message;
            }

            base.HandleStepSelection(buttonCaption);
        }

        protected override void HandleRotationComplete()
        {
            base.HandleRotationComplete();

            for (int i = 0; i < _dataSource.Count; i++)
            {
                if (_dataSource[i].Attribute("name").Value == Selection)
                {
                    _buttons["Select"].Disabled = (bool)_dataSource[i].Attribute("locked");
                }
            }
        }

        protected override void HandleNonSpinButtonAction(string buttonCaption)
        {
            if (buttonCaption == "Select") { HandleCourseSelection(); }
            else { DismissWithReturnValue(buttonCaption); }
        }

        private void HandleCourseSelection()
        {
            if ((Data.Profile.Settings.TestingRaceMode) && (_opponentCourseSelection == ""))
            {
                _opponentCourseSelection = Selection;
                UpdateAnnotations();
            }
            else
            {
                SelectedCourseName = Selection;
                SetCourseToTransmit(Selection);
                _ownCourseSelection = Selection;
            }
        }

        private const float Carousel_Center_Y = 280.0f;
        private const float Carousel_Horizontal_Radius = 275.0f;
        private const float Carousel_Vertical_Radius = 90.0f;
        private const int Dialog_Height = 700;
        private const string Communication_Keys = "id,target,course";
        private const int Milliseconds_Before_Time_Out = 5000;
        private const string No_Course_Selected_Message = "NONE";
    }
}
