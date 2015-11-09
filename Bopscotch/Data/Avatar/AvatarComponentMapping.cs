using Microsoft.Xna.Framework;

namespace Bopscotch.Data.Avatar
{
    public class AvatarComponentMapping
    {
        public string TargetSkeleton { get; private set; }
        public string TargetBone { get; private set; }
        public Vector2 Origin { get; private set; }
        public Rectangle Frame { get; private set; }

        public AvatarComponentMapping(string skeleton, string bone, float originX, float originY, int frameX, int frameY, int frameWidth, int frameHeight)
        {
            TargetSkeleton = skeleton;
            TargetBone = bone;
            Origin = new Vector2(originX, originY);
            Frame = new Rectangle(frameX, frameY, frameWidth, frameHeight);
        }
    }
}
