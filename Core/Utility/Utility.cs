using System;

using Microsoft.Xna.Framework;

namespace Leda.Core
{
    public static class Utility
    {
        public static float PointToPointAngle(Vector2 location, Vector2 target)
        {
            float angle = MathHelper.Pi + (Math.Sign(target.Y - location.Y) * MathHelper.PiOver2);

            if (location.X > target.X) { angle = (float)Math.Atan((target.Y - location.Y) / (target.X - location.X)); }
            else if (location.X < target.X) { angle = MathHelper.Pi + (float)Math.Atan((target.Y - location.Y) / (target.X - location.X)); }

            return (MathHelper.Pi + angle + MathHelper.TwoPi) % MathHelper.TwoPi;
        }

        public static float RectifyAngle(float angleToRectify)
        {
            while (angleToRectify < 0.0f) { angleToRectify += MathHelper.TwoPi; }

            return (angleToRectify % MathHelper.TwoPi);
        }

        public static float AngleDifference(float angleOne, float angleTwo)
        {
            float difference = RectifyAngle(angleTwo) - RectifyAngle(angleOne);

            if (difference > MathHelper.Pi) { difference -= MathHelper.TwoPi; }
            else if (difference < -MathHelper.Pi) { difference += MathHelper.TwoPi; }

            return difference;
        }

        public static float VectorAngle(Vector2 trajectory)
        {
            return PointToPointAngle(Vector2.Zero, trajectory);
        }

        public static Vector2 Rotate(Vector2 toRotate, float angle)
        {
            return Vector2.Transform(toRotate, Quaternion.CreateFromYawPitchRoll(0.0f, 0.0f, angle));
        }

        public static Vector2 RotatedNormal(float angle)
        {
            return Rotate(Vector2.UnitX, angle);
        }

        //public static float ToFloat(string toConvert)
        //{
        //    toConvert = toConvert.ToLower().Replace("f", "");
        //    return float.Parse(toConvert);
        //}

        public static bool Between(float value, float lowerLimit, float upperLimit)
        {
            return ((value >= lowerLimit) && (value < upperLimit));
        }

        public static float Extremity(float valueOne, float valueTwo)
        {
            if (Math.Sign(valueOne) == -Math.Sign(valueTwo)) { return 0; }

            return Math.Max(Math.Abs(valueOne), Math.Abs(valueTwo)) * (Math.Sign(valueOne) + Math.Sign(valueTwo));
        }
    }
}
