using System;

using Microsoft.Xna.Framework;

using Leda.Core.Timing;

using Bopscotch.Effects.Particles;

namespace Bopscotch.Gameplay.Objects.Environment.Blocks
{
    public class BombBlock : Block
    {
        private Timer _actionTimer;
        
        public BlockMap Map { private get; set; }
        public Point MapLocation { private get; set; }
        public bool ShouldRegenerate { private get; set; }
        public BombBlockBlastCollider BlastCollider { get; private set; }

        public AdditiveLayerParticleEffectManager.CloudBurstEffectInitiator RegenerationParticleEffect { private get; set; }
        public AdditiveLayerParticleEffectManager.FireballEffectInitiator DetonationParticleEffect { private get; set; }

        public TimerController.TickCallbackRegistrationHandler TickCallback
        {
            set
            {
                if (_actionTimer == null) { _actionTimer = new Timer("action-timer", UpdateState); }
                value(_actionTimer.Tick);
                BlastCollider.TickCallback = value;
            }
        }

        public BombBlock()
            : base()
        {
            _actionTimer = null;

            BlastCollider = new BombBlockBlastCollider();
        }

        public override void Reset()
        {
            _actionTimer.Reset();
            BlastCollider.Reset();
            base.Reset();
        }

        public void TriggerByImpact()
        {
            if (Visible)
            {
                _actionTimer.NextActionDuration = Impact_Detonation_Delay;
            }
        }

        public void TriggerByChain()
        {
            if (Visible)
            {
                _actionTimer.NextActionDuration = Chain_Detonation_Delay;
            }
        }

        private void UpdateState()
        {
            if (Visible)
            {
                Detonate();
            }
            else if (ShouldRegenerate)
            {
                Regenerate();
            }
        }

        private void Detonate()
        {
            Visible = false;
            Collidable = false;

            BlastCollider.WorldPosition = WorldPosition + (new Vector2(Definitions.Grid_Cell_Pixel_Size, Definitions.Grid_Cell_Pixel_Size) * 0.5f);
            BlastCollider.Collidable = true;

            DetonationParticleEffect(BlastCollider);

            TriggerNext(-1);
            TriggerNext(1);

            if (ShouldRegenerate)
            {
                _actionTimer.NextActionDuration = Regeneration_Delay;
            }
        }

        private void TriggerNext(int direction)
        {
            int position = MapLocation.X;
            bool finished = false;

            while (!finished)
            {
                position += direction;
                if ((position < 0) || (position >= Map.MapDimensions.X))
                {
                    finished = true;
                }
                else
                {
                    Type blockType = Map.GetTileType(position, MapLocation.Y);
                    if (blockType == null)
                    {
                        finished = true;
                    }
                    else if (blockType == typeof(BombBlock))
                    {
                        ((BombBlock)Map.GetTile(position, MapLocation.Y)).TriggerByChain();
                        finished = true;
                    }
                }
            }
        }

        private void Regenerate()
        {
            RegenerationParticleEffect(BlastCollider);
            Visible = true;
            Collidable = true;
        }

        public new const string Data_Node_Name = "bomb-block";

        private const float Impact_Detonation_Delay = 200.0f;
        private const float Chain_Detonation_Delay = 65.0f;
        private const float Regeneration_Delay = 5000.0f;
    }
}
