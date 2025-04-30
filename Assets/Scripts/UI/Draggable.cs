using UnityEngine;
using UnityEngine.UIElements;

public static class Draggable
{
    public static void MakeDraggable(VisualElement element)
    {
        Vector2 dragOffset = default;
        bool isDragging = false;

        // Ensure layout is updated before dragging starts
        element.RegisterCallback<GeometryChangedEvent>(evt =>
        {
            if (isDragging) return; // Avoid multiple calculations while dragging

            // Register pointer down only once layout is finalized
            element.RegisterCallback<PointerDownEvent>(pointerEvt =>
            {
                if (pointerEvt.button == 0 && element.parent != null)
                {
                    var parent = element.parent;

                    parent.Remove(element);
                    parent.Add(element);

                    Vector2 mouseInParent = parent.WorldToLocal(pointerEvt.position);
                    Vector2 elementTopLeft = element.layout.position;

                    dragOffset = mouseInParent - elementTopLeft + new Vector2(20,20);

                    element.CapturePointer(pointerEvt.pointerId);
                    isDragging = true;
                    pointerEvt.StopPropagation();
                }
            });
        });

        // Handle dragging
        element.RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (isDragging && element.parent != null)
            {
                var parent = element.parent;
                Vector2 mouseInParent = parent.WorldToLocal(evt.position);
                Vector2 newTopLeft = mouseInParent - dragOffset;

                element.style.left = newTopLeft.x;
                element.style.top = newTopLeft.y;
            }
        });

        // Stop dragging
        element.RegisterCallback<PointerUpEvent>(evt =>
        {
            if (isDragging && evt.button == 0)
            {
                isDragging = false;
                element.ReleasePointer(evt.pointerId);
                evt.StopPropagation();
            }
        });
    }
}
