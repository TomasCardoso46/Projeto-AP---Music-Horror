using UnityEngine;

[CreateAssetMenu(fileName = "TogglePlatformSpell", menuName = "Spells/TogglePlatformSpell")]
public class TogglePlatformSpell : Spell
{
    [Header("Target Settings")]
    [SerializeField] private int targetID;
    [SerializeField] private float maxDistance = 10f;

    public override void Cast(Transform caster)
    {
        Ray ray = new Ray(caster.position, caster.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            MoveBetweenPoints mover = hit.collider.GetComponentInParent<MoveBetweenPoints>();

            if (mover == null)
                return;

            if (mover.ID != targetID)
                return;

            mover.ToggleMove();
            Debug.Log($"Platform with ID {targetID} toggled.");
        }
    }
}
