using UnityEngine;

namespace KillerOnline1.Scripts
{
    public sealed class ExitTrigger : MonoBehaviour
    {
        [SerializeField] private GameFlow gameFlow;

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerInventory>() == null)
            {
                return;
            }

            gameFlow.ShowWin();
        }
    }
}
