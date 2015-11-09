using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Effects.Particles;
using Leda.Core.Game_Objects.Controllers.Camera;

using Bopscotch.Gameplay.Objects.Characters.Player;
using Bopscotch.Gameplay.Objects.Environment.Blocks;
using Bopscotch.Gameplay.Objects.Environment.Flags;

namespace Bopscotch.Effects.Particles
{
    public class OpaqueLayerParticleEffectManager : ParticleEffectManager
    {
        private List<Color> _checkpointEffectColours;
        private List<Color> _attackPowerUpColours;
        private List<Color> _defencePowerUpColours;

        public OpaqueLayerParticleEffectManager(CameraControllerBase cameraController)
            : base(Render_Layer, cameraController)
        {
            _checkpointEffectColours = new List<Color>() { Color.Red, Color.Yellow, Color.LightGreen, Color.LightBlue, Color.Orange, Color.Pink };
            _attackPowerUpColours = new List<Color>() { Color.Red, Color.Crimson, Color.DarkRed };
            _defencePowerUpColours = new List<Color>() { Color.LawnGreen, Color.Green, Color.LightGreen };
        }

        public void LaunchFlagStars(Flag targetFlag)
        {
            Emitter effectEmitter = CreateEffectAtObjectPosition("star-burst", "particle-star", targetFlag);
            effectEmitter.Tints = _checkpointEffectColours;
            effectEmitter.Activate();
        }

        public void LaunchPowerUpStars(Player player, bool attackingPowerUp)
        {
            Emitter effectEmitter = CreateEffectAtObjectPosition("star-burst", "particle-star", player);
            effectEmitter.Tints = attackingPowerUp ? _attackPowerUpColours : _defencePowerUpColours;
            effectEmitter.Activate();
        }

        public void LaunchCrateSmash(SmashBlock targetCrate)
        {
            CreateEffectAtObjectPosition("crate-smash", "particle-wood", targetCrate).Activate();
        }

        public const int Render_Layer = 2;
    }
}
