using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Citlali.Player
{
    public class PlayerInputSender : NetworkBehaviour
    {
        [SerializeField] PlayerMotor motor;
        [SerializeField] PlayerCombat combat;
        [SerializeField] PlayerGrab grab;

        InputAction move, jump, punch, grabAction;
        Transform cam;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) { enabled = false; return; }
            cam = Camera.main.transform;

            move = new InputAction("Move");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            move.AddBinding("<Gamepad>/leftStick");

            jump = new InputAction("Jump", binding: "<Keyboard>/space");
            jump.AddBinding("<Gamepad>/buttonSouth");

            punch = new InputAction("Punch", binding: "<Mouse>/leftButton");
            punch.AddBinding("<Gamepad>/buttonWest");

            grabAction = new InputAction("Grab", binding: "<Keyboard>/e");
            grabAction.AddBinding("<Gamepad>/buttonEast");

            move.Enable(); jump.Enable(); punch.Enable(); grabAction.Enable();
        }

        public override void OnNetworkDespawn()
        {
            move?.Dispose(); jump?.Dispose(); punch?.Dispose(); grabAction?.Dispose();
        }

        void Update()
        {
            if (jump.WasPressedThisFrame()) JumpRpc();
            if (punch.WasPressedThisFrame()) PunchRpc();
            if (grabAction.WasPressedThisFrame()) GrabRpc();
        }

        void FixedUpdate()
        {
            Vector2 i = move.ReadValue<Vector2>();
            Vector3 fwd = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
            MoveRpc(fwd * i.y + right * i.x);   // camera-relative world direction
        }

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
        void MoveRpc(Vector3 dir) => motor.SetMoveDir(dir);

        [Rpc(SendTo.Server)]
        void JumpRpc() => motor.Jump();

        [Rpc(SendTo.Server)]
        void PunchRpc() => combat.TryPunch();

        [Rpc(SendTo.Server)]
        void GrabRpc() => grab.ToggleGrab();
    }
}