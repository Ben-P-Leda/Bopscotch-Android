using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Effects.Particles;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Game_Objects.Controllers.Camera;

using Bopscotch.Gameplay.Objects.Characters.Player;

namespace Bopscotch.Effects.Particles
{
    public class AdditiveLayerParticleEffectManager : ParticleEffectManager
    {
        private List<Color> _attackHitColours;

        public delegate void CloudBurstEffectInitiator(IWorldObject targetObject);

        public AdditiveLayerParticleEffectManager(CameraControllerBase cameraController)
            : base(Render_Layer, cameraController)
        {
            _attackHitColours = new List<Color>() { Color.LawnGreen, Color.Green, Color.LightGreen };
        }

        public void LaunchCloudBurst(IWorldObject targetObject)
        {
            CreateEffectAtObjectPosition("cloud-burst", "particle-cloud", targetObject).Activate();
        }

        public void LaunchEnemyAttack(Player player)
        {
            Emitter effectEmitter = CreateEffectAtObjectPosition("attack-hit", "particle-cloud", player);
            effectEmitter.Tints = _attackHitColours;
            effectEmitter.Activate();
        }

        public const int Render_Layer = 3;
    }
}
