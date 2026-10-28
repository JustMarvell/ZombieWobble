using Citlali.Carry;
using Unity.Netcode;
using UnityEngine;

namespace Citlali.Game
{
    [RequireComponent(typeof(BoxCollider))]
    public class GoalZone : MonoBehaviour
    {
        void Reset()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }

        void OnTriggerStay(Collider other)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null || !nm.IsServer) return;
            if (!other.attachedRigidbody || !other.attachedRigidbody.TryGetComponent(out CarryObject _)) return;

            Bounds zone = GetComponent<Collider>().bounds;
            if (zone.Contains(other.bounds.min) && zone.Contains(other.bounds.max))
                GameManager.Instance.ReportGoalReached();
        }
    }
}
