using System.Collections.Generic;

namespace Leda.Core.Game_Objects.Behaviours
{
    public interface ILinkedToOtherObjects
    {
        List<IGameObject> LinkedObjects { get; }
    }
}
