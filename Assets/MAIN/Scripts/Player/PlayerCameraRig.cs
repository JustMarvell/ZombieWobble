using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Citlali
{
    public class PlayerCameraRig : NetworkBehaviour
    {
        [SerializeField] float distance = 8f;
        [SerializeField] float sensitivity = 0.1f;

        InputAction look;
        Transform cam;
        float yaw, pitch = 25f;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) { enabled = false; return; }
            cam = Camera.main.transform;
            look = new InputAction("Look", binding: "<Pointer>/delta");
            look.Enable();
            Cursor.lockState = CursorLockMode.Locked;
        }

        public override void OnNetworkDespawn()
        {
            look?.Dispose();
        }

        void LateUpdate()
        {
            Vector2 d = look.ReadValue<Vector2>();
            yaw += d.x * sensitivity;
            pitch = Mathf.Clamp(pitch - d.y * sensitivity, 5f, 60f);

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focus = transform.position + Vector3.up;
            cam.position = focus + rot * new Vector3(0f, 0f, -distance);
            cam.LookAt(focus);
        }
    }
}
