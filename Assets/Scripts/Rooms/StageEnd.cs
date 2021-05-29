using UnityEngine;

public class StageEnd : MonoBehaviour
{
    private StageManager stageManager;

    private void Start()
    {
        stageManager = GameObject.Find("Manager").GetComponent<StageManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            stageManager.CompleteStage();
    }
}