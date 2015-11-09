namespace Leda.Core.Game_Objects.Behaviours
{
    public interface IHasLifeCycle : ITemporary
    {
        LifeCycleStateValue LifeCycleState { get; set; }
    }
}
