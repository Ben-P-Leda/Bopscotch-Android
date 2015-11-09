using Microsoft.Xna.Framework;

namespace Leda.Core
{
    public sealed class Random : System.Random
    {
        private static Random _random = null;

        public static Random Generator
        {
            get
            {
                if (_random == null) { _random = new Random(); }
                return _random;
            }
        }

        public float Next(float maxValue)
        {
            return Next(0.0f, maxValue);
        }

        public float Next(float minValue, float maxValue)
        {
            return MathHelper.Lerp(minValue, maxValue, (float)NextDouble());
        }

        public int NextInt(int maxValue)
        {
            return base.Next(maxValue);
        }

        public int NextInt(int minValue, int maxValue)
        {
            return base.Next(minValue, maxValue);
        }

        public int NextSign()
        {
            return (base.Next(2) * 2) - 1;
        }

        public int NextPercentile()
        {
            return NextInt(1, 101);
        }
    }
}
