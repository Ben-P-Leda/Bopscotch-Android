namespace Leda.Core.Game_Objects.Behaviours
{
    public interface ISerializableWithPostDeserialize : ISerializable
    {
        void HandlePostDeserialize();
    }
}
