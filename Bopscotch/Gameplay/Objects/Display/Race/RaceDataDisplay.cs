using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;

using Bopscotch.Interface;
using Bopscotch.Communication;
using Bopscotch.Gameplay.Coordination;

namespace Bopscotch.Gameplay.Objects.Display.Race
{
    public class RaceDataDisplay : StatusDisplay
    {
        private string _lapCountText;
        private string _totalTimeText;

        public RaceDataDisplay()
            : base()
        {
            _lapCountText = "";
            _totalTimeText = "";
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            TextWriter.Write(_lapCountText, spriteBatch, _position, Color.White, Color.Black, Outline_Thickness, 
                Text_Scale, Render_Depth, TextWriter.Alignment.Left);

            TextWriter.Write(_totalTimeText, spriteBatch, _position + new Vector2(0.0f, Time_Text_Y_Offset), Color.White, Color.Black, 
                Outline_Thickness, Text_Scale, Render_Depth, TextWriter.Alignment.Left);
        }

        public void SetLapCountText(int completed, int toComplete)
        {
                _lapCountText = Translator.Translation("race-lap")
                    .Replace("[COMPLETED]", (Math.Min(completed + 1, toComplete)).ToString())
                    .Replace("[TOTAL]", toComplete.ToString());
        }

        public void SetTotalTimeText(int raceTimeInMilliseconds)
        {
            string hundredths = string.Concat("00", (raceTimeInMilliseconds / 10) % 100);
            string seconds = string.Concat("0", (raceTimeInMilliseconds / 1000) % 60);
            string minutes = (raceTimeInMilliseconds / 60000).ToString();

            _totalTimeText = string.Format("{0}:{1}:{2}", minutes, seconds.Substring(seconds.Length - 2, 2), hundredths.Substring(hundredths.Length - 2, 2));
        }

        private const float Time_Text_Y_Offset = 50.0f;
    }
}
