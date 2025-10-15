using UnityEngine;

[CreateAssetMenu(fileName = "DestroySigilSpell", menuName = "Spells/DestroySigilSpell")]
public class DestroySigilSpell : Spell
{
    [Header("Sigil Settings")]
    [SerializeField] private string sigilColor; // "Red" or "Green"
    [SerializeField] private float maxDistance = 10f;

    public override void Cast(Transform caster)
    {
        Ray ray = new Ray(caster.position, caster.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            // Check if the object hit has a DoorInteraction script in parent
            DoorInteraction door = hit.collider.GetComponentInParent<DoorInteraction>();
            if (door != null)
            {
                Transform sigils = door.transform.Find("Sigils");
                if (sigils != null)
                {
                    foreach (Transform child in sigils)
                    {
                        if (child.name == sigilColor)
                        {
                            child.gameObject.SetActive(false);
                            Debug.Log($"{sigilColor} sigil destroyed!");
                        }
                    }
                }
            }
        }
    }
}
