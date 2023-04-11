using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Modules.Card
{
    public abstract class DragBaseView : MonoBehaviour,
        IBeginDragHandler,
        IEndDragHandler,
        IDragHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerClickHandler,
        IPointerUpHandler
    {
        protected virtual float PressTime { get; set; }

        private readonly List<Transform> parentPointerHandlers = new();
        private bool _isPress;

        protected void AddParentPointerHandler(Transform parent)
        {
            parentPointerHandlers.Add(parent);
        }

        protected void PassEventToParent<T>(Transform parent, PointerEventData eventData,
            System.Action<T, PointerEventData> eventAction) where T : IEventSystemHandler
        {
            if (parent == null) return;
            var parentHandler = parent.GetComponent<T>();
            if (parentHandler != null)
            {
                eventAction(parentHandler, eventData);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            foreach (var parentPointerHandler in parentPointerHandlers)
            {
                PassEventToParent<IBeginDragHandler>(parentPointerHandler, eventData, (p, d) => { p.OnBeginDrag(d); });
            }

            OnBeginDragging(eventData);
        }

        protected virtual void OnBeginDragging(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            foreach (var parentPointerHandler in parentPointerHandlers)
            {
                PassEventToParent<IEndDragHandler>(parentPointerHandler, eventData, (p, d) => { p.OnEndDrag(d); });
            }

            OnEndDragging(eventData);
            StopAllCoroutines();
        }

        protected virtual void OnEndDragging(PointerEventData eventData)
        {
        }

        public void OnDrag(PointerEventData eventData)
        {
            foreach (var parentPointerHandler in parentPointerHandlers)
            {
                PassEventToParent<IDragHandler>(parentPointerHandler, eventData, (p, d) => { p.OnDrag(d); });
            }

            OnDragging(eventData);
        }

        protected virtual void OnDragging(PointerEventData eventData)
        {
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            foreach (var parentPointerHandler in parentPointerHandlers)
            {
                PassEventToParent<IPointerEnterHandler>(parentPointerHandler, eventData,
                    (p, d) => { p.OnPointerEnter(d); });
            }

            OnPointerEntering(eventData);
            StopCoroutine(nameof(OnPointerEnterCoroutine));
            StartCoroutine(nameof(OnPointerEnterCoroutine), eventData);
        }

        private IEnumerator OnPointerEnterCoroutine(PointerEventData eventData)
        {
            yield return new WaitForSecondsRealtime(PressTime);
            _isPress = true;
            OnPointerPress(eventData);
        }

        protected virtual void OnPointerPress(PointerEventData eventData)
        {
        }

        protected virtual void OnPointerEntering(PointerEventData eventData)
        {
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            foreach (var parentPointerHandler in parentPointerHandlers)
            {
                PassEventToParent<IPointerExitHandler>(parentPointerHandler, eventData,
                    (p, d) => { p.OnPointerExit(d); });
            }

            OnPointerExiting(eventData);
            StopAllCoroutines();
        }

        protected virtual void OnPointerExiting(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            foreach (var parentPointerHandler in parentPointerHandlers)
            {
                PassEventToParent<IPointerUpHandler>(parentPointerHandler, eventData,
                    (p, d) => { p.OnPointerUp(d); });
            }

            if (_isPress)
            {
                OnPressPointerUpping(eventData);
                _isPress = false;
            }
            OnPointerUpping(eventData);
        }

        protected virtual void OnPointerUpping(PointerEventData eventData)
        {
        }
        
        protected virtual void OnPressPointerUpping(PointerEventData eventData)
        {
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            foreach (var parentPointerHandler in parentPointerHandlers)
            {
                PassEventToParent<IPointerDownHandler>(parentPointerHandler, eventData,
                    (p, d) => { p.OnPointerDown(d); });
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            foreach (var parentPointerHandler in parentPointerHandlers)
            {
                PassEventToParent<IPointerClickHandler>(parentPointerHandler, eventData,
                    (p, d) => { p.OnPointerClick(d); });
            }
        }
    }
}