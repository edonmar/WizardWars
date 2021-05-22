using UnityEngine;

public class BarrierTrigger : MonoBehaviour
{
    [SerializeField] private BarrierStats barrierStats;

    private void OnTriggerEnter(Collider other)
    {
        barrierStats.DestroyThis();
    }
}