using UnityEngine;

public static class EnemyUtilities
{
    // Sample a random point on NavMesh around center
    public static bool RandomNavSphere(Vector3 origin, float dist, out Vector3 result)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        if (UnityEngine.AI.NavMesh.SamplePosition(randDirection, out UnityEngine.AI.NavMeshHit navHit, 20.0f, UnityEngine.AI.NavMesh.AllAreas))
        {
            result = navHit.position;
            return true;
        }
        result = origin;
        return false;
    }
}
