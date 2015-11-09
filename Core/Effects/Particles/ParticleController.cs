using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core.Game_Objects.Behaviours;

namespace Leda.Core.Effects.Particles
{
    public class ParticleController : IGameObject, ISimpleRenderable, ICameraLinked
    {
        private List<Particle> _particles;

        public Vector2 CameraPosition { set { for (int i = 0; i < _particles.Count; i++) { _particles[i].CameraPosition = value; } } }
        public int RenderLayer { get; set; }
        public bool Visible { get; set; }

        public delegate void ParticleRegistrationHandler(Particle toRegister);

        public ParticleController()
        {
            _particles = new List<Particle>();

            RenderLayer = -1;
            Visible = true;
        }

        public void Initialize()
        {
        }

        public void Reset()
        {
            _particles.Clear();
        }

        public void RegisterParticle(Particle toRegister)
        {
            _particles.Add(toRegister);
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                if (!_particles[i].Visible) { _particles.RemoveAt(i); }
                else { _particles[i].Update(millisecondsSinceLastUpdate); }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _particles.Count; i++)
            {
                if (_particles[i].Visible) { _particles[i].Draw(spriteBatch); }
            }
        }
    }
}
