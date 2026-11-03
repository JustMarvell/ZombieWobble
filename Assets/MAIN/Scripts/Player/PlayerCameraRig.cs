using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Citlali.Player
{
    public class PlayerCameraRig : NetworkBehaviour
    {
        public static PlayerCameraRig Local { get; private set; }
        public static string SpectatingName { get; private set; }

        [SerializeField] float distance = 8f;
        [SerializeField] float sensitivity = 0.1f;
        [SerializeField] float maxShakeAngle = 3f;
        [SerializeField] float maxShakeOffset = 0.15f;
        [SerializeField] float traumaDecay = 1.8f;
        [SerializeField] float cameraRadius = 0.3f;
        [SerializeField] float minDistance = 1.2f;
        [SerializeField] float zoomOutSpeed = 6f;

        InputAction look;
        Transform cam;
        float yaw, pitch = 25f;
        float trauma;
        float currentDistance;
        int obstructionMask;
        PlayerLife life, spectated;
        Transform lastTarget;
        Vector3 focus;
        bool transitioning;
        InputAction switchTarget;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) { enabled = false; return; }
            Local = this;
            currentDistance = distance;
            obstructionMask = ~(LayerMask.GetMask("Player", "Enemy", "Carry", "Ragdoll"));
            cam = Camera.main.transform;
            look = new InputAction("Look", binding: "<Pointer>/delta");
            look.Enable();

            life = GetComponent<PlayerLife>();
            focus = transform.position + Vector3.up;
            lastTarget = transform;
            switchTarget = new InputAction("SpectateNext", binding: "<Mouse>/leftButton");
            switchTarget.Enable();
        }

        public override void OnNetworkDespawn()
        {
            look?.Dispose();
            if (Local == this) Local = null;

            switchTarget?.Dispose();
            SpectatingName = null;
        }

        public void AddTrauma(float amount) => trauma = Mathf.Clamp01(trauma + amount);

        void LateUpdate()
        {
            if (cam == null)
            {
                if (Camera.main == null) return;
                cam = Camera.main.transform;
            }

            if (CursorController.GameplayActive)
            {
                Vector2 d = look.ReadValue<Vector2>();
                yaw += d.x * sensitivity;
                pitch = Mathf.Clamp(pitch - d.y * sensitivity, 5f, 60f);
            }

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);

            Transform target = GetTarget();
            Vector3 desired = target.position + Vector3.up;
            if (target != lastTarget) { lastTarget = target; transitioning = true; }

            if (transitioning)   // glide when switching targets; otherwise follow exactly like before
            {
                focus = Vector3.Lerp(focus, desired, 1f - Mathf.Exp(-6f * Time.deltaTime));
                if ((focus - desired).sqrMagnitude < 0.05f) transitioning = false;
            }
            else focus = desired;

            Vector3 dir = rot * Vector3.back;

            float targetDist = distance;
            if (Physics.SphereCast(focus, cameraRadius, dir, out var hit, distance,
                                   obstructionMask, QueryTriggerInteraction.Ignore))
                targetDist = Mathf.Max(hit.distance, minDistance);

            // snap in instantly (so it never shows the wall), ease back out smoothly
            currentDistance = targetDist < currentDistance
                ? targetDist
                : Mathf.MoveTowards(currentDistance, targetDist, zoomOutSpeed * Time.deltaTime);

            cam.position = focus + dir * currentDistance;
            cam.LookAt(focus);

            if (trauma > 0f)
            {
                float s = trauma * trauma;
                float t = Time.time * 25f;
                float x = Mathf.PerlinNoise(t, 0f) - 0.5f;
                float y = Mathf.PerlinNoise(0f, t) - 0.5f;
                float roll = Mathf.PerlinNoise(t, 7f) - 0.5f;

                cam.position += (cam.right * x + cam.up * y) * 2f * maxShakeOffset * s;
                cam.rotation *= Quaternion.Euler(0f, 0f, roll * 2f * maxShakeAngle * s);
                trauma = Mathf.Max(0f, trauma - traumaDecay * Time.deltaTime);
            }
        }

        Transform GetTarget()
        {
            SpectatingName = null;
            if (life.State.Value != PlayerState.Dead) { spectated = null; return transform; }

            bool next = CursorController.GameplayActive && switchTarget.WasPressedThisFrame();
            if (spectated == null || spectated.State.Value == PlayerState.Dead || next)
                spectated = PickNext(spectated);

            if (spectated == null) return transform;   // nobody left, stay on yourself

            if (spectated.TryGetComponent(out PlayerIdentity id))
                SpectatingName = id.PlayerName.Value.ToString();
            return spectated.transform;
        }

        PlayerLife PickNext(PlayerLife current)
        {
            var all = PlayerLife.All;
            int start = current != null ? all.IndexOf(current) : -1;
            for (int i = 1; i <= all.Count; i++)
            {
                var p = all[(start + i + all.Count) % all.Count];
                if (p != life && p.State.Value != PlayerState.Dead) return p;
            }
            return null;
        }
    }
}
