using UnityEngine;

public sealed class ExitTrigger : MonoBehaviour
{
    [SerializeField] private GameFlow _gameFlow;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerInventory>() == null)
        {
            return;
        }

        _gameFlow.ShowWin();
    }
}
