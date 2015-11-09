namespace Bopscotch.Gameplay.Objects.Behaviours
{
    public interface ICanHaveGlowEffect
    {
        void InitializeGlow(int layers, float scale, float scaleStep, float renderDepthOffset);
        void UpdateGlow(int millisecondsSinceLastUpdate);
    }
}
