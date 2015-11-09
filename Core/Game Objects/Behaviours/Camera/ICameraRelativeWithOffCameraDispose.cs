using Microsoft.Xna.Framework;

namespace Leda.Core.Game_Objects.Behaviours
{
    public interface ICameraRelativeWithOffCameraDispose: ICameraRelative, ITemporary
    {
        bool HasBeenInShot { get; set; }
        Rectangle OutOfShotTolerance { get; set; }
    }
}
