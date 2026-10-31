using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.SceneManagement;

namespace Citlali.Player
{
    public class RagdollController : MonoBehaviour
    {
        [SerializeField] Animator animator;
        [SerializeField] RigBuilder rigBuilder;
        [SerializeField] Transform owner;
        [SerializeField] Behaviour[] disableWhileRagdoll;
        [SerializeField] SkinnedMeshRenderer[] skins;
        [SerializeField] float followStrength = 60f;
        [SerializeField] float followDamping = 6f;

        public bool IsRagdoll { get; private set; }

        Rigidbody[] bones;
        Collider[] boneColliders;
        Transform hips, hipsParent;
        Rigidbody hipsBody;
        Vector3 hipsLocalPos, hipsLocalScale;
        Quaternion hipsLocalRot;
        Vector3 lastPos, velocity;
        bool following, inited;

        void Start()
        {
            var sm = NetworkManager.Singleton != null ? NetworkManager.Singleton.SceneManager : null;
            if (sm != null) sm.OnLoad += OnSceneLoad;
        }

        void Init()
        {
            inited = true;
            hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            if (hips == null) { Debug.LogError("RagdollController: No hips bone (Is the rig Humanoid?)"); return; }

            hipsParent = hips.parent;
            hipsLocalPos = hips.localPosition;
            hipsLocalRot = hips.localRotation;
            hipsLocalScale = hips.localScale;
            hipsBody = hips.GetComponent<Rigidbody>();

            bones = animator.GetComponentsInChildren<Rigidbody>(true);
            boneColliders = animator.GetComponentsInChildren<Collider>(true);

            int layer = LayerMask.NameToLayer("Ragdoll");
            foreach (var c in boneColliders) { c.gameObject.layer = layer; c.enabled = false; }
            foreach (var b in bones) b.isKinematic = true;
            foreach (var s in skins) s.updateWhenOffscreen = true;

            lastPos = owner.position;
        }

        void OnSceneLoad(ulong clientId, string sceneName, LoadSceneMode mode, AsyncOperation op)
        {
            if (clientId != NetworkManager.Singleton.LocalClientId || mode != LoadSceneMode.Single) return;
            if (IsRagdoll) SetRagdoll(false);   // reattach the hips before the old scene unloads
        }


        public void SetRagdoll(bool on, bool follow = false)
        {
            if (!inited) Init();
            if (hips == null) return;

            following = on && follow;
            if (on == IsRagdoll) return;
            IsRagdoll = on;
            if (on) Enable(); else Disable();
        }

        void Enable()
        {
            if (rigBuilder) rigBuilder.enabled = false;
            animator.enabled = false;
            foreach (var b in disableWhileRagdoll) b.enabled = false;

            hips.SetParent(null, true);
            foreach (var c in boneColliders) c.enabled = true;
            foreach (var b in bones)
            {
                b.isKinematic = false;
                b.linearVelocity = velocity;
                b.angularVelocity = Vector3.zero;
            }
        }

        void Disable()
        {
            foreach (var b in bones)
            {
                b.linearVelocity = Vector3.zero;
                b.angularVelocity = Vector3.zero;
                b.isKinematic = true;
            }
            foreach (var c in boneColliders) c.enabled = false;

            hips.SetParent(hipsParent, false);
            hips.localPosition = hipsLocalPos;
            hips.localRotation = hipsLocalRot;
            hips.localScale = hipsLocalScale;

            animator.enabled = true;
            if (rigBuilder) rigBuilder.enabled = true;
            foreach (var b in disableWhileRagdoll) b.enabled = true;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            Vector3 v = (owner.position - lastPos) / dt;
            lastPos = owner.position;
            if (v.magnitude < 25f) velocity = Vector3.Lerp(velocity, v, 0.5f);
        }

        void FixedUpdate()
        {
            if (!IsRagdoll || !following || hipsBody == null) return;

            Vector3 err = owner.position - hipsBody.position;
            err.y = 0f;
            hipsBody.AddForce(err * followStrength - hipsBody.linearVelocity * followDamping, ForceMode.Acceleration);
        }

        void OnDestroy()
        {
            var sm = NetworkManager.Singleton != null ? NetworkManager.Singleton.SceneManager : null;
            if (sm != null) sm.OnLoad -= OnSceneLoad;

            if (IsRagdoll && hips != null) Destroy(hips.gameObject);   // unchanged: clean up detached hips
        }
    }
}