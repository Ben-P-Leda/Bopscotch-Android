using System.Collections.Generic;

using Microsoft.Xna.Framework.Audio;

namespace Leda.Core.Asset_Management
{
    public static class SoundEffectManager
    {
        private static Dictionary<string, SoundEffect> _effects = null;
        private static Dictionary<string, SoundEffectInstance> _effectInstances = null;

        private static bool MuteDueToObscuring = false;

        public static bool Muted = false;

        public static void AddEffect(string effectName, SoundEffect effect, bool playFromInstance)
        {
            if (playFromInstance)
            {
                if (_effectInstances == null) { _effectInstances = new Dictionary<string, SoundEffectInstance>(); }

                if (!_effectInstances.ContainsKey(effectName)) { _effectInstances.Add(effectName, effect.CreateInstance()); }
                else { _effectInstances[effectName] = effect.CreateInstance(); }
            }
            else
            {
                if (_effects == null) { _effects = new Dictionary<string, SoundEffect>(); }

                if (!_effects.ContainsKey(effectName)) { _effects.Add(effectName, effect); }
                else { _effects[effectName] = effect; }
            }
        }

        public static SoundEffectInstance PlayEffect(string effectName)
        {
            if ((!Muted) && (!MuteDueToObscuring))
            {
				if ((_effectInstances != null) && (_effectInstances.ContainsKey(effectName))) { _effectInstances[effectName].Play(); return _effectInstances[effectName]; }
				if ((_effects != null) && (_effects.ContainsKey(effectName))) { _effects[effectName].Play(); }
            }

            return null;
        }

        public static void HandleGameObscured() { MuteDueToObscuring = true; }
        public static void HandleGameUnobscured() { MuteDueToObscuring = false; }
    }
}
