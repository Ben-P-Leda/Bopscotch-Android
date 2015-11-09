using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core.Game_Objects.Behaviours;

namespace Leda.Core.Animation.Skeletons
{
    public interface IBone : ITransformationAnimatable, IColourAnimatable, IWorldObject, ICameraRelative, ITextureManaged
    {
        IBone Parent { get; set; }
        List<IBone> Children { get; }

        Vector2 RelativePosition { get; set; }
        float RelativeRotation { get; set; }
        float RelativeScale { get; set; }
        float RelativeDepth { get; set; }
        Color MasterTint { get; set; }
        bool Mirror { get; set; }
        float RenderDepth { get; set; }

        Rectangle Frame { set; }
        Vector2 Origin { set; }

        void CalculateWorldPosition();
        void CalculateRenderDepth();

        void ApplySkin(string skinTextureName, Vector2 originWithinSkinFrame, Rectangle frameAreaWithinSkinTexture);
        void ClearSkin();

        void SetMetricsAndCreateChildrenFromXml(ISkeleton skeleton, XElement boneData);
    }
}
