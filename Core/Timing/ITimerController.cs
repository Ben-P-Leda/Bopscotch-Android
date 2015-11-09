using Leda.Core.Game_Objects.Behaviours;

namespace Leda.Core.Timing
{
    public interface ITimerController : IPausable
    {
        void RegisterUpdateCallback(TimerController.UpdateCallback toRegister);
    }
}
