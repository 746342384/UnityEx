using Ex.UI.Event;
using UnityEngine;

namespace Ex.UI.Button
{
    public class ContinuousPressButton : EventTriggerListener
    {
        protected override float Duration => 0.5f;

        private void Start()
        {
            SetPressEvent(() => { Debug.Log($"连续按下:{ContinuousCount}"); });
        }
    }
}