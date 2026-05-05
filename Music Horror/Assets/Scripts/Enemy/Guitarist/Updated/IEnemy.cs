public interface IEnemy
{
    void AlertToPosition(UnityEngine.Vector3 worldPos);
    void AlertToTarget(UnityEngine.Transform target);
    void TakeDamage(int amount, UnityEngine.Vector3 hitPoint);
    bool IsAlive { get; }
}
