using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DestroySigilSpell", menuName = "Spells/DestroySigilSpell")]
public class DestroySigilSpell : Spell
{
    [Header("Sigil Settings")]
    [SerializeField] private string sigilColor; // "Red" or "Green"
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
            if (door != null)
            {
                Transform sigils = door.transform.Find("Sigils");
                if (sigils != null)
                {
                    foreach (Transform child in sigils)
                    {
                        if (child.name == sigilColor)
                        {
                            MonoBehaviour runner = door.GetComponent<MonoBehaviour>();
                            if (runner != null)
                            {
                                runner.StartCoroutine(DissolveChildrenAndDisable(child.gameObject));
                            }
                            else
                            {
                                Debug.LogWarning("No MonoBehaviour found to run coroutine.");
                            }

                            return;
                        }
                    }
                }
            }
        }
    }

    private IEnumerator DissolveChildrenAndDisable(GameObject parentSigil)
    {
        Renderer[] renderers = parentSigil.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length == 0)
        {
            Debug.LogWarning("No renderers found in sigil children.");
            parentSigil.SetActive(false);
            yield break;
        }

        // Create material instances and track dissolve values
        List<Material> materials = new List<Material>();
        List<float> dissolveValues = new List<float>();

        foreach (Renderer r in renderers)
        {
            Material mat = r.material; // instance
            materials.Add(mat);

            float value = mat.HasProperty(dissolveProperty) ? mat.GetFloat(dissolveProperty) : 0.5f;

            // Ensure minimum start
            if (value < 0.5f)
                value = 0.5f;

            mat.SetFloat(dissolveProperty, value);
            dissolveValues.Add(value);
        }

        bool allDone = false;

        while (!allDone)
        {
            allDone = true;

            for (int i = 0; i < materials.Count; i++)
            {
                if (dissolveValues[i] < 1f)
                {
                    dissolveValues[i] += Time.deltaTime * dissolveSpeed;
                    if (dissolveValues[i] > 1f)
                        dissolveValues[i] = 1f;

                    materials[i].SetFloat(dissolveProperty, dissolveValues[i]);

                    allDone = false;
                }
            }

            yield return null;
        }

        // Ensure all are exactly 1
        foreach (var mat in materials)
        {
            mat.SetFloat(dissolveProperty, 1f);
        }

        parentSigil.SetActive(false);

        Debug.Log($"{sigilColor} sigil destroyed!");
    }
}