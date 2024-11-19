namespace GameFramework
{
    public interface IReference
    {
        void OnAcquire();
        void OnRelease();
    }
}