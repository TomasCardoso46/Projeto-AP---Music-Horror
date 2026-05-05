using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Jumpscare : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpscareClip; 

    [Header("Camera Settings")]
    [SerializeField] private GameObject jumpscareCamera;  

    [Header("Shake Settings")]
    [SerializeField] private GameObject objectToShake;
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeMagnitude = 15f;
    [SerializeField] private enum ShakeAxis { X, Y, Z }
    [SerializeField] private ShakeAxis shakeAxis = ShakeAxis.Y;

    [Header("Enemy Settings")]
    [SerializeField] private GameObject enemyToDisable;

    [Header("Quit Settings")]
    [SerializeField] private float quitDelay = 2f;
    [SerializeField] private string sceneToLoad;

    private Quaternion originalRotation;

    // Call this metod to trigger the jumpscare
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
        SceneManager.LoadScene(sceneToLoad);
    }
}
