using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public AnimationCurve curve;
    public float duration = 0.8f;  

    public void Shake()
    {
        StopAllCoroutines(); // evita que se acumulen shakes
        StartCoroutine(Shaking());
    }

    IEnumerator Shaking()
    {
        Vector3 startPosition = transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float strength = curve.Evaluate(elapsedTime / duration);
            transform.localPosition = startPosition + Random.insideUnitSphere * strength;
            yield return null;
        }

        transform.localPosition = startPosition;
    }
}
