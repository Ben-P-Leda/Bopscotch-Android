using Microsoft.Xna.Framework;

using Leda.Core.Gamestate_Management;

namespace Leda.Core.Game_Objects.Behaviours
{
    public interface ICameraLinked
    {
        Vector2 CameraPosition { set; }
    }
}
