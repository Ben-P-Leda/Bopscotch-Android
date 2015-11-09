using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Effects.Particles;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Game_Objects.Controllers.Camera;
using Leda.Core.Asset_Management;

namespace Bopscotch.Effects.Particles
{
    public abstract class ParticleEffectManager : ParticleController
    {
        public CameraControllerBase CameraController { private get; set; }

        public ParticleEffectManager(int renderLayer, CameraControllerBase cameraController)
            : base()
        {
            RenderLayer = renderLayer;
            CameraController = cameraController;
        }

        protected Emitter CreateEffectAtObjectPosition(string effectName, string textureName, IWorldObject targetObject)
        {
            Emitter effectEmitter = EmitterFactoryManager.EmitterFactories[effectName].CreateEmitter();
            effectEmitter.ParticleRegistrationCallback = RegisterParticle;
            effectEmitter.ParticleTexture = TextureManager.Textures[textureName];
            effectEmitter.WorldPosition = targetObject.WorldPosition;
            if (targetObject is ICameraRelative) { effectEmitter.CameraPosition = CameraController.WorldPosition; }

            return effectEmitter;
        }
    }
}
