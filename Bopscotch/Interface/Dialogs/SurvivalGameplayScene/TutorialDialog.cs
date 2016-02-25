using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Timing;

using Bopscotch.Data;

namespace Bopscotch.Interface.Dialogs.SurvivalGameplayScene
{
    public class TutorialDialog : ButtonDialog
    {
        public string StepText { private get; set; }

        public TutorialDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Top_Y_When_Active;

            AddButton("OK", new Vector2(Definitions.Back_Buffer_Center.X, 350), Button.ButtonIcon.Tick, Color.LawnGreen, 0.7f);

            _cancelButtonCaption = "OK";
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            TextWriter.Write(StepText, spriteBatch, new Vector2(Text_Indent, WorldPosition.Y + Top_Margin),
                Color.White, Color.Black, 3.0f, 0.7f, Text_Render_Depth, TextWriter.Alignment.Left);

        }

        private const float Text_Render_Depth = 0.141f;
        private const int Dialog_Height = 450;
        private const float Top_Y_When_Active = 400.0f;
        private const float Text_Indent = 200.0f;
        private const float Top_Margin = 50.0f;
    }
}

