using System.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Asset_Management;

using Bopscotch.Data.Avatar;
using Bopscotch.Interface.Objects;

namespace Bopscotch.Interface.Content
{
    public class AvatarContent : ContentBase
    {
        private ComponentSetDisplayAvatar _skeleton;

        public override Vector2 Offset
        { 
            protected get { return base.Offset; }
            set { base.Offset = value; if (_skeleton != null) { _skeleton.WorldPosition = _position + Offset; } }
        }

        public override Color Tint 
        { 
            protected get { return base.Tint; }
            set { base.Tint = value; if (_skeleton == null) { _skeleton = new ComponentSetDisplayAvatar(); } _skeleton.Tint = FadedTint; }
        }

        public override float FadeFraction 
        { 
            protected get { return base.FadeFraction; }
            set { base.FadeFraction = value; if (_skeleton == null) { _skeleton = new ComponentSetDisplayAvatar(); } _skeleton.Tint = FadedTint; }
        }

        public override float Scale
        {
            protected get { if (_skeleton == null) { _skeleton = new ComponentSetDisplayAvatar(); } return _skeleton.Scale; }
            set { if (_skeleton == null) { _skeleton = new ComponentSetDisplayAvatar(); } _skeleton.Scale = value; }
        }

        public override bool Visible
        {
            get { if (_skeleton == null) { _skeleton = new ComponentSetDisplayAvatar(); } return _skeleton.Visible; }
            set { if (_skeleton == null) { _skeleton = new ComponentSetDisplayAvatar(); } _skeleton.Visible = value; }
        }

        public override int RenderLayer 
        {
            get { if (_skeleton == null) { _skeleton = new ComponentSetDisplayAvatar(); } return _skeleton.RenderLayer; }
            set { if (_skeleton == null) { _skeleton = new ComponentSetDisplayAvatar(); } _skeleton.RenderLayer = value; }
        }

        public AvatarContent(Vector2 position, string skeletonName)
            : base(position)
        {
            if (_skeleton == null) { _skeleton = new ComponentSetDisplayAvatar(); }
            _skeleton.Name = skeletonName;
            _skeleton.WorldPosition = position;
            _skeleton.CreateBonesFromDataManager(skeletonName);
        }

        public void AddComponent(AvatarComponent toAdd)
        {
            _skeleton.Components.Add(toAdd);
        }

        public void SkinSkeleton()
        {
            _skeleton.SkinFromComponents();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _skeleton.Draw(spriteBatch);
        }
    }
}
