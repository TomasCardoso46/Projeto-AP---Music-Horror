using UnityEngine;
using System.Collections;

public class Jumpscare : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;  // The audio source to play the jumpscare sound
    public AudioClip jumpscareClip;  // The sound to play

    [Header("Camera Settings")]
    public GameObject jumpscareCamera;  // The 2nd camera to enable

    [Header("Shake Settings")]
    public GameObject objectToShake;   // Any object to shake
    public float shakeDuration = 0.5f;  // How long the shake lasts
    public float shakeMagnitude = 15f;  // Max rotation in degrees
    public enum ShakeAxis { X, Y, Z }
    public ShakeAxis shakeAxis = ShakeAxis.Y; // Choose which axis to shake

    [Header("Enemy Settings")]
    public GameObject enemyToDisable;  // The chasing enemy to hide

    [Header("Quit Settings")]
    public float quitDelay = 2f;  // Time in seconds before quitting the game

    private Quaternion originalRotation;

    // Call this function to trigger the jumpscare
    public void TriggerJumpscare()
    {
        if (jumpscareCamera != null)
        {
            jumpscareCamera.SetActive(true);
        }

        if (audioSource != null && jumpscareClip != null)
        {
            audioSource.PlayOneShot(jumpscareClip);
        }

        if (enemyToDisable != null)
        {
            enemyToDisable.SetActive(false);
        }

        if (objectToShake != null)
        {
            originalRotation = objectToShake.transform.localRotation;
            StartCoroutine(ShakeObjectRotation());
        }

        StartCoroutine(QuitAfterDelay());
    }

    private IEnumerator ShakeObjectRotation()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float angleOffset = Random.Range(-shakeMagnitude, shakeMagnitude);
            Vector3 rotation = originalRotation.eulerAngles;

            switch (shakeAxis)
            {
                case ShakeAxis.X: rotation.x = originalRotation.eulerAngles.x + angleOffset; break;
                case ShakeAxis.Y: rotation.y = originalRotation.eulerAngles.y + angleOffset; break;
                case ShakeAxis.Z: rotation.z = originalRotation.eulerAngles.z + angleOffset; break;
            }

            objectToShake.transform.localRotation = Quaternion.Euler(rotation);
            elapsed += Time.deltaTime;
            yield return null;
        }

        objectToShake.transform.localRotation = originalRotation;
    }

    private IEnumerator QuitAfterDelay()
    {
        yield return new WaitForSeconds(quitDelay);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in editor
#else
        Application.Quit(); // Quit the build
#endif
    }
}
