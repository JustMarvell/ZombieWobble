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
        [SerializeField] PlayerLife life;
        [SerializeField] PlayerInventory inventory;

        InputAction move, jump, punch, grabAction, useAction;
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

            useAction = new InputAction("UseItem", binding: "<Keyboard>/q");
            useAction.AddBinding("<Gamepad>/buttonNorth");

            move.Enable(); jump.Enable(); punch.Enable(); grabAction.Enable(); useAction.Enable();
        }

        public override void OnNetworkDespawn()
        {
            move?.Dispose(); jump?.Dispose(); punch?.Dispose(); grabAction?.Dispose(); useAction?.Dispose();
        }

        void Update()
        {
            if (jump.WasPressedThisFrame()) JumpRpc();
            if (punch.WasPressedThisFrame() && CursorController.GameplayActive) PunchRpc();
            if (grabAction.WasPressedThisFrame()) GrabRpc();
            if (useAction.WasPressedThisFrame()) UseRpc(true);
            if (useAction.WasReleasedThisFrame()) UseRpc(false);
        }

        void FixedUpdate()
        {
            Vector2 i = move.ReadValue<Vector2>();
            Vector3 fwd = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
            MoveRpc(fwd * i.y + right * i.x);   // camera-relative world direction
        }

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
        void MoveRpc(Vector3 dir) => motor.SetMoveDir(life.IsAlive ? dir : Vector3.zero);

        [Rpc(SendTo.Server)]
        void JumpRpc() { if (life.IsAlive) motor.Jump(); }

        [Rpc(SendTo.Server)]
        void PunchRpc() { if (life.IsAlive) combat.TryPunch(); }

        [Rpc(SendTo.Server)]
        void GrabRpc() { if (life.IsAlive) grab.ToggleGrab(); }

        [Rpc(SendTo.Server)]
        void UseRpc(bool holding) => inventory.SetUsing(holding);
    }
}