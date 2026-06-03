using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DestroySigilSpell", menuName = "Spells/DestroySigilSpell")]
public class DestroySigilSpell : Spell
{
    [Header("Sigil Settings")]
    [SerializeField] private string sigilColor;
    [SerializeField] private float maxDistance = 10f;

    [Header("Dissolve Settings")]
    [SerializeField] private float dissolveSpeed = 1f;
    [SerializeField] private string dissolveProperty = "_Dissolve";

    public override void Cast(Transform caster)
    {
        Ray ray = new Ray(caster.position, caster.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance) && hit.collider.isTrigger)
        {
            DoorInteraction door = hit.collider.GetComponentInParent<DoorInteraction>();

            if (door == null)
                return;

            Transform sigilsRoot = door.transform.Find("Sigils");

            if (sigilsRoot == null)
                return;

            // Find the target sigil
            Transform targetSigil = null;

            foreach (Transform child in sigilsRoot)
            {
                if (child.name == sigilColor)
                {
                    targetSigil = child;
                    break;
                }
            }

            if (targetSigil == null)
                return;

            // IMPORTANT: count remaining ACTIVE sigils BEFORE destroying this one
            int activeSigils = 0;

            foreach (Transform child in sigilsRoot)
            {
                if (child.gameObject.activeSelf && child != targetSigil)
                    activeSigils++;
            }

            bool isLastSigil = activeSigils == 0;

            // Auto-open ONLY if this is the last sigil
            if (isLastSigil)
            {
                door.TriggerAutoOpenFromSpell();
            }

            // Start dissolve
            MonoBehaviour runner = door.GetComponent<MonoBehaviour>();

            if (runner != null)
            {
                runner.StartCoroutine(DissolveChildrenAndDisable(targetSigil.gameObject));
            }
        }
    }

    private IEnumerator DissolveChildrenAndDisable(GameObject parentSigil)
    {
        Renderer[] renderers = parentSigil.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length == 0)
        {
            parentSigil.SetActive(false);
            yield break;
        }

        List<Material> materials = new List<Material>();
        List<float> dissolveValues = new List<float>();

        foreach (Renderer r in renderers)
        {
            Material mat = r.material;
            materials.Add(mat);

            float value = mat.HasProperty(dissolveProperty)
                ? mat.GetFloat(dissolveProperty)
                : 0.5f;

            if (value < 0.5f)
                value = 0.5f;

            mat.SetFloat(dissolveProperty, value);
            dissolveValues.Add(value);
        }

        bool done = false;

        while (!done)
        {
            done = true;

            for (int i = 0; i < materials.Count; i++)
            {
                if (dissolveValues[i] < 1f)
                {
                    dissolveValues[i] += Time.deltaTime * dissolveSpeed;
                    dissolveValues[i] = Mathf.Min(dissolveValues[i], 1f);

                    materials[i].SetFloat(dissolveProperty, dissolveValues[i]);
                    done = false;
                }
            }

            yield return null;
        }

        foreach (var mat in materials)
            mat.SetFloat(dissolveProperty, 1f);

        parentSigil.SetActive(false);
    }
}