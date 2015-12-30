using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

using Microsoft.Xna.Framework;

namespace Bopscotch.Interface.Dialogs.RankingScene
{
    public class NavigationDialog : ButtonDialog
    {
        public delegate void PagingHandler(string areaName);
        public PagingHandler PageChangeCallback { private get; set; }

        private int _selectedAreaIndex;
        private List<string> _areaNames;

        public bool PagingComplete { private get; set; }

        public NavigationDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Definitions.Back_Buffer_Height - (Dialog_Height + Bottom_Margin);
            PagingComplete = true;

            _selectedAreaIndex = 0;
            _areaNames = new List<string>();
            _cancelButtonCaption = "Back";
        }

        public override void Activate()
        {
            ClearButtons();
            GetAvailableAreas();

            AddButton("Back", new Vector2(Definitions.Back_Buffer_Center.X, Button_Y), Button.ButtonIcon.Back, Color.Red, 0.6f);

            float offset = Definitions.Back_Buffer_Center.X * 0.85f;

            AddIconButton("previous", new Vector2(Definitions.Back_Buffer_Center.X - offset, Button_Y), Button.ButtonIcon.Previous, Color.DodgerBlue, 0.6f);
            AddIconButton("next", new Vector2(Definitions.Back_Buffer_Center.X + offset, Button_Y), Button.ButtonIcon.Next, Color.DodgerBlue, 0.6f);

            SetPagingButtonStates();

            base.Activate();
        }

        public void GetAvailableAreas()
        {
            _areaNames.Clear();
            foreach (XElement a in Data.Profile.SimpleAreaData)
            {
                if (!Unranked_Areas.Contains(a.Attribute("name").Value.ToLower()))
                {
                    _areaNames.Add(a.Attribute("name").Value);
                }
            }
        }

        private void SetPagingButtonStates()
        {
            _buttons["previous"].Disabled = ((_areaNames.Count < 1) || (_selectedAreaIndex == 0));
            _buttons["next"].Disabled = ((_areaNames.Count < 1) || (_selectedAreaIndex == _areaNames.Count - 1));
        }

        public override void Reset()
        {
            base.Reset();
            WorldPosition = new Vector2(0.0f, Definitions.Back_Buffer_Height);
        }

        protected override bool HandleButtonTouch(string buttonCaption)
        {
            if (PagingComplete)
            {
                if (buttonCaption == "previous")
                {
                    HandleAreaStep(-1);
                }
                else if (buttonCaption == "next")
                {
                    HandleAreaStep(1);
                }
            }

            return buttonCaption == "Back";
        }

        public void HandleAreaStep(int direction)
        {
            if ((_selectedAreaIndex + direction > -1) && (_selectedAreaIndex + direction < _areaNames.Count))
            {
                _selectedAreaIndex += direction;

                PagingComplete = false;
                PageChangeCallback(_areaNames[_selectedAreaIndex]);

                SetPagingButtonStates();
            }
        }

        private const int Dialog_Height = 150;
        private const float Bottom_Margin = 20.0f;
        private const float Button_Y = 75.0f;
        private const string Unranked_Areas = "tutorial";
    }
}
