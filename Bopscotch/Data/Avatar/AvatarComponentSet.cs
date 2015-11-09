using System.Linq;
using System.Collections.Generic;

namespace Bopscotch.Data.Avatar
{
    public class AvatarComponentSet
    {
        public string Name { get; private set; }
        public string DisplaySkeleton { get; private set; }
        public bool SelectionNotMandatory { get; private set; }

        public List<AvatarComponent> Components { get; private set; }

        public bool HasUnlockedComponents
        {
            get
            {
                foreach (AvatarComponent c in Components)
                {
                    if (c.Unlocked) { return true; break; }
                }
                return false;
            }
        }

        public AvatarComponentSet(string name, string displaySkeleton, bool selectionNotMandatory)
        {
            Name = name;
            DisplaySkeleton = displaySkeleton;
            SelectionNotMandatory = selectionNotMandatory;

            Components = new List<AvatarComponent>();
        }

        public void UnlockComponent(string componentName)
        {
            AvatarComponent toUnlock = GetNamedComponent(componentName);
            if (toUnlock != null) { toUnlock.Unlocked = true; }
        }

        private AvatarComponent GetNamedComponent(string componentName)
        {
            return (from item in Components where item.Name == componentName select item).First();
        }

        public AvatarComponent Component(string componentName)
        {
            return GetNamedComponent(componentName);
        }
    }
}
