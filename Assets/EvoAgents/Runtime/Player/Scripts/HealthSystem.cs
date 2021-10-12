using UnityEngine;
using UnityEngine.UI;
using System;

namespace EvoAgents.Player {
    public class HealthSystem : MonoBehaviour {
        public string enemyTag = "Wolf";
        public Image gameOver;
        public StatusBar statusBar;

        public event Action OnPlayerDead;

        // Start is called before the first frame update
        void Start() {
            statusBar.SetStatusValue(20, 20);
        }

        private void Update() {
            if (statusBar.GetStatusValue(20) < 0.1f) {
                statusBar.ResetStatusValue();
                OnPlayerDead?.Invoke();
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag(enemyTag))
                statusBar.SetStatusValue(statusBar.GetStatusValue(20) - 100f * Time.deltaTime, 20);
        }
    }
}
