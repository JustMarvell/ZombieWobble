using Citlali.Carry;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Citlali.Player
{
    public class GrabArmIK : MonoBehaviour
    {
        [SerializeField] NetworkObject owner;      // the player's root NetworkObject
        [SerializeField] Animator animator;        // the model's Animator
        [SerializeField] TwoBoneIKConstraint rightArm;
        [SerializeField] TwoBoneIKConstraint leftArm;
        [SerializeField] Transform rightTarget;
        [SerializeField] Transform leftTarget;
        [SerializeField] float handSpread = 0.15f;      // distance between the two hands on the handle
        [SerializeField] float blendSpeed = 8f;         // how fast the arms switch on and off
        [SerializeField] float followSharpness = 18f;   // lower = looser, more lag

        Transform rightBone, leftBone;
        float weight;

        void Start()
        {
            rightBone = animator.GetBoneTransform(HumanBodyBones.RightHand);
            leftBone = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        }

        Transform FindHandle()
        {
            if (!owner.IsSpawned) return null;
            foreach (var obj in CarryObject.All)
                for (int i = 0; i < obj.Occupants.Count; i++)
                    if (obj.Occupants[i] == owner.NetworkObjectId) return obj.GetHandle(i);
            return null;
        }

        void Update()
        {
            Transform handle = FindHandle();
            weight = Mathf.MoveTowards(weight, handle ? 1f : 0f, blendSpeed * Time.deltaTime);
            rightArm.weight = weight;
            leftArm.weight = weight;

            if (handle != null)
            {
                Vector3 side = owner.transform.right * handSpread;
                float k = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
                rightTarget.position = Vector3.Lerp(rightTarget.position, handle.position + side, k);
                leftTarget.position = Vector3.Lerp(leftTarget.position, handle.position - side, k);
            }
            else
            {
                rightTarget.position = rightBone.position;   // start the next reach from the current hands
                leftTarget.position = leftBone.position;
            }
        }
    }
}