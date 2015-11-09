using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Timing;
using Leda.Core.Asset_Management;

using Bopscotch.Effects.Particles;

namespace Bopscotch.Gameplay.Objects.Environment.Blocks
{
    public class RaceModePowerUpSmashBlock : SmashBlock
    {
        private Timer _regenerationTimer;
        private List<string> _powerUpTextureNames;

        public AdditiveLayerParticleEffectManager.CloudBurstEffectInitiator RegenerationParticleEffect { private get; set; }

        public TimerController.TickCallbackRegistrationHandler TickCallback
        {
            set
            {
                if (_regenerationTimer == null) { _regenerationTimer = new Timer("regen-timer", Regenerate); }
                value(_regenerationTimer.Tick);
            }
        }

        public RaceModePowerUpSmashBlock()
            : base()
        {
            _regenerationTimer = null;
            RegenerationParticleEffect = null;
            _powerUpTextureNames = new List<string>();
            foreach (KeyValuePair<string, Texture2D> kvp in TextureManager.Textures)
            {
                if (kvp.Key.StartsWith("power")) { _powerUpTextureNames.Add(kvp.Key); }
            }
        }

        private void Regenerate()
        {
            if (RegenerationParticleEffect != null) { RegenerationParticleEffect(this); }

            Visible = true;
            Collidable = true;
        }

        public override void HandleSmash()
        {
            if (Contents.Count > 0) { SetPowerUpContents(); }
            _regenerationTimer.NextActionDuration = Regeneration_Delay_in_Milliseconds;

            base.HandleSmash();
        }

        private void SetPowerUpContents()
        {
            Contents.Clear();

            int powerUpIndex = (int)Random.Generator.Next(_powerUpTextureNames.Count);

            Contents.Add(new Data.SmashBlockItemData()
            {
                TextureName = _powerUpTextureNames[powerUpIndex],
                Count = 1,
                AffectsItem = Data.SmashBlockItemData.AffectedItem.PowerUp,
                Value = powerUpIndex
            });
        }

        private const int Regeneration_Delay_in_Milliseconds = 5000;
    }
}
