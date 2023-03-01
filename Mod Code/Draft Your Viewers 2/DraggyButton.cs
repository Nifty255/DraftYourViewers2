using UnityEngine;
using UnityEngine.EventSystems;

namespace CodeNifty.DraftYourViewers2
{
    public class DraggyButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public RectTransform Container;
        private RectTransform dragButton;
        private bool isMouseDown = false;
        private Vector3 startMousePosition;
        private Vector3 startPosition;

        // Use this for initialization
        void Start()
        {
            dragButton = GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData dt)
        {
            isMouseDown = true;

            startPosition = Container.position;
            startMousePosition = Input.mousePosition;
        }

        public void OnPointerUp(PointerEventData dt)
        {
            isMouseDown = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (isMouseDown)
            {
                Vector3 currentPosition = Input.mousePosition;

                Vector3 diff = currentPosition - startMousePosition;

                Vector3 pos = startPosition + diff;

                Container.position = pos;
            }
        }
    }
}
