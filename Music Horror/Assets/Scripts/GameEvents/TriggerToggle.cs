using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriggerToggle : MonoBehaviour
{
    public enum ActionType
    {
        Activate,
        Deactivate
    }

    [Header("Settings")]
    [SerializeField] private ActionType action = ActionType.Activate;
    [SerializeField] private GameObject targetObject;
    [SerializeField] private GameObject targetObject2;
    [SerializeField] private bool isMethodTrigger = false;

    [Header("Safety")]
    [SerializeField] private string requiredTag = "Player"; 
    [SerializeField] private bool triggerOnce = true;

    [Header("Dissolve Settings")]
    [SerializeField] private float dissolveSpeed = 1f;
    [SerializeField] private string dissolveProperty = "_Dissolve";

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered) return;

        if (isMethodTrigger)
        {
            StartCoroutine(DissolveChildrenAndDisable(targetObject2));
            hasTriggered = true;
            
        }

        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
            return;

        if (targetObject == null)
        {
            Debug.LogWarning("TriggerToggle: No target object assigned.", this);
            return;
        }

        switch (action)
        {
            case ActionType.Activate:
                targetObject.SetActive(true);
                break;

            case ActionType.Deactivate:
                targetObject.SetActive(false);
                break;
        }

        hasTriggered = true;
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