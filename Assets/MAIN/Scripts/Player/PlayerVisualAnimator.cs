using System;
using UnityEngine;

namespace Citlali.Player
{
    public class PlayerVisualAnimator : MonoBehaviour
    {
        [SerializeField] Animator animator;
        [SerializeField] float dampTime = 0.1f;

        static readonly int SpeedHash = Animator.StringToHash("Speed");
        Vector3 lastPos;

        void Start()
        {
            lastPos = transform.position;
        }

        void Update()
        {
            Vector3 delta = transform.position - lastPos;
            lastPos = transform.position;
            delta.y = 0f;

            float speed = Time.deltaTime > 0f ? delta.magnitude / Time.deltaTime : 0f;
            if (speed > 20f) speed = 0f;

            animator.SetFloat(SpeedHash, speed, dampTime, Time.deltaTime);
        }
    }
}