using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Citlali.Player
{
    public class PunchArmIK : MonoBehaviour
    {
        [System.Serializable]
        public class Arm
        {
            public TwoBoneIKConstraint constraint;
            public Transform target;
            public Vector3 windup;            // pull-back point
            public Vector3 strike;            // fist at full extension
            [HideInInspector] public Vector3 start;
            [HideInInspector] public float t = -1f;
            [HideInInspector] public Transform bone;
        }

        [SerializeField] Transform owner;     // the player root
        [SerializeField] Animator animator;
        [SerializeField] Arm right = new Arm { windup = new Vector3(0.3f, 0.45f, 0f), strike = new Vector3(0.15f, 0.4f, 1.3f) };
        [SerializeField] Arm left = new Arm { windup = new Vector3(-0.3f, 0.45f, 0f), strike = new Vector3(-0.15f, 0.4f, 1.3f) };
        [SerializeField] float windupTime = 0.10f;
        [SerializeField] float strikeTime = 0.06f;
        [SerializeField] float recoverTime = 0.25f;

        bool useRight = true;

        void Start()
        {
            right.bone = animator.GetBoneTransform(HumanBodyBones.RightHand);
            left.bone = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        }

        public void Play()
        {
            var a = useRight ? right : left;
            useRight = !useRight;
            a.start = owner.InverseTransformPoint(a.bone.position);   // start from where the hand is
            a.t = 0f;
        }

        void Update()
        {
            Tick(right);
            Tick(left);
        }

        void Tick(Arm a)
        {
            if (a.t < 0f) { a.constraint.weight = 0f; return; }

            a.t += Time.deltaTime;
            Vector3 local;
            float w;

            if (a.t < windupTime)
            {
                float k = EaseOut(a.t / windupTime);
                local = Vector3.Lerp(a.start, a.windup, k);
                w = k;
            }
            else if (a.t < windupTime + strikeTime)
            {
                float k = EaseIn((a.t - windupTime) / strikeTime);
                local = Vector3.Lerp(a.windup, a.strike, k);
                w = 1f;
            }
            else if (a.t < windupTime + strikeTime + recoverTime)
            {
                float k = (a.t - windupTime - strikeTime) / recoverTime;
                local = Vector3.Lerp(a.strike, a.start, EaseOut(k));
                w = 1f - k;
            }
            else
            {
                a.t = -1f;
                a.constraint.weight = 0f;
                return;
            }

            a.target.position = owner.TransformPoint(local);
            a.constraint.weight = w;
        }

        static float EaseOut(float x) => 1f - (1f - x) * (1f - x);
        static float EaseIn(float x) => x * x;
    }
}