using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

using Leda.Core.Game_Objects.Behaviours;

namespace Leda.Core.Game_Objects.Controllers.Rendering
{
    public class RenderLayer
    {
        public BlendState Blending { get; private set; }
        public List<ISimpleRenderable> SimpleObjects { get; private set; }

        public RenderLayer(BlendState blendState)
        {
            Blending = blendState;
            SimpleObjects = new List<ISimpleRenderable>();
        }
    }
}
