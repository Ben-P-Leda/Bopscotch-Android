using System.Collections.Generic;

using Leda.Core.Game_Objects.Behaviours;

namespace Leda.Core.Animation.Skeletons
{
    public interface ISkeleton : ITransformationAnimatable, IWorldObject
    {
        Dictionary<string, IBone> Bones { get; }
        string PrimaryBoneID { set; }
        bool Mirror { get; set; }

        void AddBone(IBone boneToAdd, string parentBoneID);
    }
}
