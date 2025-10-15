using UnityEngine;

[CreateAssetMenu(fileName = "ObjectSpawner", menuName = "Spells/ObjectSpawner")]
public class ObjectSpawner : Spell
{
    [Header("Object Settings")]
    [SerializeField] private GameObject objectToSpawn;
    private GameObject objectSource;
    
    public override void Cast(Transform caster)
    {
        objectSource = GameObject.FindGameObjectWithTag("MainCamera");
        Vector3 pos = objectSource.transform.position;
        Quaternion rot = objectSource.transform.rotation;
        Instantiate(objectToSpawn, pos, rot);
    }
}
