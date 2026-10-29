using System.Collections;
using UnityEngine;

namespace Citlali.UI
{
    public class PopAnimator : MonoBehaviour
    {
        [SerializeField] float startScale = 2.5f;
        [SerializeField] float duration = 0.45f;
        [SerializeField] AnimationCurve curve;   // 0 to 1 means start to end; going over 1 squashes below the end scale

        Coroutine routine;

        void Reset()
        {
            curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.6f, 1.15f), new Keyframe(1f, 1f));
            for (int i = 0; i < curve.length; i++) curve.SmoothTangents(i, 0f);
        }

        public void Play()
        {
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                float s = Mathf.LerpUnclamped(startScale, 1f, curve.Evaluate(t / duration));
                transform.localScale = Vector3.one * s;
                yield return null;
            }
            transform.localScale = Vector3.one;
        }
    }
}