using Leda.Core.Motion;

namespace Leda.Core.Game_Objects.Behaviours
{
    public interface IMobile : IWorldObject
    {
        IMotionEngine MotionEngine { get; }

        void Update(int millisecondsSinceLastUpdate);
    }
}
