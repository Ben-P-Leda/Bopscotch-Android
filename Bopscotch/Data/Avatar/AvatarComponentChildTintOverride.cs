using Microsoft.Xna.Framework;

namespace Bopscotch.Data.Avatar
{
    public class AvatarComponentChildTintOverride
    {
        public string TargetBone { get; private set; }
        public Color OverridingTint { get; private set; }

        public AvatarComponentChildTintOverride(string bone, int red, int green, int blue, int alpha)
        {
            TargetBone = bone;
            OverridingTint = new Color(red, green, blue, alpha);
        }
    }
}
