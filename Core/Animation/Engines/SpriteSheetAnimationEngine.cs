using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Timing;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;
using Leda.Core.Asset_Management;

namespace Leda.Core.Animation
{
    public class SpriteSheetAnimationEngine : AnimationEngineBase
    {
        private ISpriteSheetAnimatable _target;

        public SpriteSheetAnimationEngine(ISpriteSheetAnimatable target)
            : base()
        {
            _target = target;
            if (_target is ISerializable) { ID = string.Concat(((ISerializable)_target).ID, "-spritesheet-animation-engine"); }
        }

        private SpriteSheetKeyframe _currentKeyFrame { get { return (SpriteSheetKeyframe)_sequence.Keyframes[_currentKeyframeIndex]; } }

        protected override void StartNextKeyframe()
        {
            base.StartNextKeyframe();

            if (!string.IsNullOrEmpty(_currentKeyFrame.TextureName)) 
            { 
                _target.Texture = TextureManager.Textures[_currentKeyFrame.TextureName];
            }

            if (_currentKeyFrame.SourceArea != Rectangle.Empty)
            {
                _target.Frame = ((SpriteSheetKeyframe)_sequence.Keyframes[_currentKeyframeIndex]).SourceArea;
            }
        }

        protected override IKeyframe DeserializeKeyframe(XElement serializedData)
        {
            SpriteSheetKeyframe newKeyframe = new SpriteSheetKeyframe();
            newKeyframe.Deserialize(serializedData);

            return newKeyframe;
        }
    }
}
