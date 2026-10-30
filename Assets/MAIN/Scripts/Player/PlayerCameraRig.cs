using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Citlali.Player
{
    public class PlayerCameraRig : NetworkBehaviour
    {
        public static PlayerCameraRig Local { get; private set; }

        [SerializeField] float distance = 8f;
        [SerializeField] float sensitivity = 0.1f;
        [SerializeField] float maxShakeAngle = 3f;
        [SerializeField] float maxShakeOffset = 0.15f;
        [SerializeField] float traumaDecay = 1.8f;

        InputAction look;
        Transform cam;
        float yaw, pitch = 25f;
        float trauma;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) { enabled = false; return; }
            Local = this;
            cam = Camera.main.transform;
            look = new InputAction("Look", binding: "<Pointer>/delta");
            look.Enable();  
        }

        public override void OnNetworkDespawn()
        {
            look?.Dispose();
            if (Local == this) Local = null;
        }

        public void AddTrauma(float amount) => trauma = Mathf.Clamp01(trauma + amount);

        void LateUpdate()
        {
            if (CursorController.GameplayActive)
            {
                Vector2 d = look.ReadValue<Vector2>();
                yaw += d.x * sensitivity;
                pitch = Mathf.Clamp(pitch - d.y * sensitivity, 5f, 60f);
            }

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focus = transform.position + Vector3.up;
            cam.position = focus + rot * new Vector3(0f, 0f, -distance);
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
    }
}
