using UnityEngine;

[CreateAssetMenu(fileName = "ObjectSpawner", menuName = "Spells/ObjectSpawner")]
public class ObjectSpawner : Spell
{
    [Header("Object Settings")]
    [SerializeField] private GameObject objectToSpawn;

    [Header("Spawn Offset Settings")]
    [SerializeField] private float forwardSpawnOffset = 2f; // Distance in front of camera

    private GameObject objectSource;
    
    public override void Cast(Transform caster)
    {
        objectSource = GameObject.FindGameObjectWithTag("MainCamera");

        Vector3 pos = objectSource.transform.position + 
                      objectSource.transform.forward * forwardSpawnOffset;

        Quaternion rot = objectSource.transform.rotation;

        Instantiate(objectToSpawn, pos, rot);
    }
}