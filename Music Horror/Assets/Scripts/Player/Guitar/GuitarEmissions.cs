using System.Collections;
using UnityEngine;

public class GuitarEmissionController : MonoBehaviour
{
    [Header("Guitar Materials (2)")]
    [SerializeField] private Material guitarMaterialA;
    [SerializeField] private Material guitarMaterialB;

    [Header("Emission Settings")]
    [SerializeField] private Color normalEmissionColor = Color.black;
    [SerializeField] private float emissionIntensity = 2f;

    private Coroutine glowRoutine;

    void Awake()
    {
        SetEmission(normalEmissionColor);
    }

    public void TriggerSpellGlow(Color spellColor, float holdTime, float fadeTime)
    {
        if (glowRoutine != null)
            StopCoroutine(glowRoutine);

        glowRoutine = StartCoroutine(SpellGlowRoutine(spellColor, holdTime, fadeTime));
    }

    private IEnumerator SpellGlowRoutine(Color spellColor, float holdTime, float fadeTime)
    {
        // Instant glow
        SetEmission(spellColor);

        yield return new WaitForSeconds(holdTime);

        // Fade back to normal
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            Color current = Color.Lerp(spellColor, normalEmissionColor, t / fadeTime);
            SetEmission(current);
            yield return null;
        }

        SetEmission(normalEmissionColor);
    }

    private void SetEmission(Color color)
    {
        Color finalColor = color * emissionIntensity;

        if (guitarMaterialA != null)
        {
            guitarMaterialA.SetColor("_EmissionColor", finalColor);
            guitarMaterialA.EnableKeyword("_EMISSION");
        }

        if (guitarMaterialB != null)
        {
            guitarMaterialB.SetColor("_EmissionColor", finalColor);
            guitarMaterialB.EnableKeyword("_EMISSION");
        }
    }
}
