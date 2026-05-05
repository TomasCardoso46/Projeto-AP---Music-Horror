using UnityEngine;

public class EnemyGone : MonoBehaviour
{
    [SerializeField] private GameObject enemy;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            enemy.active = false;
        }
    }
}
