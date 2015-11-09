using Microsoft.Xna.Framework.Graphics;

using Leda.Core.Serialization;

namespace Leda.Core.Game_Objects.Behaviours
{
    public interface ITextureManaged : ISerializable
    {
        string TextureReference { get; }
        Texture2D Texture { set; }
    }
}
