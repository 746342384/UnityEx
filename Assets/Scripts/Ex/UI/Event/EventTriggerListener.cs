using System;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Ex.UI.Event
{
    public class EventTriggerListener : EventTrigger
    {
        protected virtual float Duration => 1;
        protected virtual float DecreasingRate => 0;
        private float PressTime { get; set; }
        private Action OnPressEvent { get; set; }
        private bool IsContinuousPress { get; set; }
        protected int ContinuousCount { get; private set; }

        protected void SetPressEvent(Action action)
        {
            OnPressEvent = action;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            OnClickEvent(eventData);
        }

        protected virtual void OnClickEvent(PointerEventData pointerEventData)
        {
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            OnPointerDownEvent(eventData);
            IsContinuousPress = true;
            StartCoroutine(nameof(ContinuousPress));
        }

        protected virtual void OnPointerDownEvent(PointerEventData pointerEventData)
        {
        }

        private IEnumerator ContinuousPress()
        {
            while (IsContinuousPress)
            {
                yield return new WaitForSeconds(Duration);
                PressTime += Duration;
                ContinuousCount += 1;
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            IsContinuousPress = false;
            StopCoroutine(nameof(ContinuousPress));

            if (PressTime >= Duration)
            {
                OnPressEvent?.Invoke();
            }
            else
            {
                OnPointerUpEvent(eventData);
            }

            ContinuousCount = 0;
            PressTime = 0;
        }

        protected virtual void OnPointerUpEvent(PointerEventData pointerEventData)
        {
        }
    }
}