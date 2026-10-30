using UnityEngine;

namespace Citlali.Level
{
    public class LevelChunk : MonoBehaviour
    {
        [Header("Connections (blue Z arrow = direction of travel)")]
        public Transform entry;              // not needed on the start chunk
        public Transform exit;               // not needed on the finish chunk

        [Header("Spawn markers (all optional)")]
        public Transform[] playerSpawns;     // start chunk
        public Transform carrySpawn;         // start chunk
        public Transform[] medkitSpawns;
        public Transform[] powerUpSpawns;
        public Transform[] enemySpawns;

        void OnDrawGizmos()
        {
            DrawArrow(entry, Color.green);
            DrawArrow(exit, Color.red);
        }

        static void DrawArrow(Transform t, Color c)
        {
            if (t == null) return;
            Gizmos.color = c;
            Vector3 p = t.position, tip = p + t.forward * 2f;
            Gizmos.DrawWireSphere(p, 0.3f);
            Gizmos.DrawLine(p, tip);
            Gizmos.DrawLine(tip, tip - t.forward * 0.5f + t.right * 0.3f);
            Gizmos.DrawLine(tip, tip - t.forward * 0.5f - t.right * 0.3f);
        }
    }
}