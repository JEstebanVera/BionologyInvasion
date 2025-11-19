using UnityEngine;
using System.Collections;

public static class SafeDestroy
{

    public static IEnumerator DestroyEndOfFrameCoroutine(GameObject obj)
    {
        yield return new WaitForEndOfFrame();
        if (obj != null)
            Object.Destroy(obj);
    }


    public static IEnumerator DestroyAfterSecondsSafeCoroutine(GameObject obj, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (obj != null)
            yield return DestroyEndOfFrameCoroutine(obj);
    }

    public static void DestroyEndOfFrame(MonoBehaviour caller, GameObject obj)
    {
        if (caller == null || obj == null) return;
        caller.StartCoroutine(DestroyEndOfFrameCoroutine(obj));
    }

    public static void DestroyAfterSecondsSafe(MonoBehaviour caller, GameObject obj, float seconds)
    {
        if (caller == null || obj == null) return;
        caller.StartCoroutine(DestroyAfterSecondsSafeCoroutine(obj, seconds));
    }
}
