using UnityEngine;
using UnityEngine.UI;

namespace EvoAgents {
    [AddComponentMenu("EvoAgents/UI/Status Bar")]
    public class StatusBar : MonoBehaviour {
        public Image statusBar;
        public Text statusText;
        public void SetStatusValue(float currentValue, float maxValue) {
            statusBar.fillAmount = currentValue / maxValue;
            statusText.text = Mathf.RoundToInt(currentValue) + "/" + maxValue;
        }
        public float GetStatusValue(float maxValue) {
            return statusBar.fillAmount * maxValue;
        }
        public void ResetStatusValue() {
            statusBar.fillAmount = 1f;
            statusText.text = "20/20";
        }
    }
}
