using Leda.Core.Effects.Particles;
using Leda.Core.Timing;

namespace Leda.Core.Game_Objects.Behaviours
{
    public interface IHasParticleEffects
    {
        ParticleController.ParticleRegistrationHandler ParticleRegistrationCallback { set; }
    }
}
