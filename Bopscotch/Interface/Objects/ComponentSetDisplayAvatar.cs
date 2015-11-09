using System.Linq;
using System.Collections.Generic;

using Leda.Core.Game_Objects.Base_Classes;

using Bopscotch.Data.Avatar;
using Bopscotch.Interface.Dialogs.Carousel;

namespace Bopscotch.Interface.Objects
{
    public class ComponentSetDisplayAvatar : CarouselAvatar
    {
        public string Name { private get; set; }

        public List<AvatarComponent> Components { get; private set; }

        public ComponentSetDisplayAvatar() : this("", 1.0f) { }

        public ComponentSetDisplayAvatar(string selectionValue, float initialBrightness)
            : base(selectionValue, initialBrightness)
        {
            Components = new List<AvatarComponent>();
        }

        public void SkinFromComponents()
        {
            CreateSkins();
            SetTintOverrides();
        }

        private void CreateSkins()
        {
            foreach (AvatarComponent item in Components)
            {
                AvatarComponentMapping mapping = (from m in item.Mappings where m.TargetSkeleton == Name select m).First();
                SetSkinForBone(mapping.TargetBone, item.TextureName, mapping.Origin, mapping.Frame);
            }
        }

        private void SetTintOverrides()
        {
            foreach (AvatarComponent item in Components)
            {
                foreach (AvatarComponentChildTintOverride tint in item.TintOverrides) { AttemptTintOverride(tint);}
            }
        }

        private void AttemptTintOverride(AvatarComponentChildTintOverride tintData)
        {
            foreach (AvatarComponent item in Components)
            {
                if ((item.TintedByParentSkin) && (item.Mappings[0].TargetBone == tintData.TargetBone))
                {
                    Bones[tintData.TargetBone].MasterTint = tintData.OverridingTint;
                }
            }
        }
    }
}
