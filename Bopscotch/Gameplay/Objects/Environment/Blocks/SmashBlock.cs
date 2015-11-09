using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;
using Leda.Core.Serialization;
using Leda.Core.Timing;
using Leda.Core;

using Bopscotch.Effects;

namespace Bopscotch.Gameplay.Objects.Environment.Blocks
{
    public abstract class SmashBlock : Block
    {
        public delegate void SmashCallbackMethod(SmashBlock smashedBlock);

        public List<Data.SmashBlockItemData> Contents { get; private set; }

        public SmashCallbackMethod SmashCallback { private get; set; }

        public SmashBlock()
            : base()
        {
            Texture = TextureManager.Textures[Texture_Name];

            Contents = new List<Data.SmashBlockItemData>();

            SmashCallback = null;
        }


        public virtual void HandleSmash()
        {
            SmashCallback(this);

            Visible = false;
            Collidable = false;
            SoundEffectManager.PlayEffect("crate-smash");
        }

        private const string Texture_Name = "block-smash";
        private const float Player_Impact_Tolerance = 30.0f;

        public new const string Data_Node_Name = "smash-block";
    }
}
