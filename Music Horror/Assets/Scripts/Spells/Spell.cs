using UnityEngine;

[CreateAssetMenu(fileName = "NewSpell", menuName = "Spells/BaseSpell")]
public abstract class Spell : ScriptableObject
{
    [Header("General Spell Info")]
    public string spellName;
    [Header("Visual Feedback")]
    public Color spellColor = Color.magenta;


    /// <summary>
    /// Executes the spell. The caster transform is passed so spells know the player's position and direction.
    /// </summary>
    public abstract void Cast(Transform caster);
}
