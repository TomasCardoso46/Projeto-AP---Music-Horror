using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawnpoint : MonoBehaviour
{
    [SerializeField] private HighSoundReaction highSoundReaction;
    private void OnTriggerEnter(Collider other)
    {

        EnemyAttack enemyAttack = other.GetComponent<EnemyAttack>();
        EnemyAudioEmitter enemyAudioEmitter = this.GetComponent<EnemyAudioEmitter>();

        if (enemyAttack != null && !enemyAttack.canAttack)
        {
            enemyAttack.canAttack = true;
            enemyAudioEmitter.StopSound();
            highSoundReaction.ResetReaction();
            other.gameObject.SetActive(false);
        }
    }
}
