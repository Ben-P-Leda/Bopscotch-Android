using Leda.Core.Gamestate_Management;

namespace Leda.Core.Game_Objects.Behaviours
{
    public interface ITemporary
    {
        bool ReadyForDisposal { get; set; }

        void PrepareForDisposal();
    }
}
