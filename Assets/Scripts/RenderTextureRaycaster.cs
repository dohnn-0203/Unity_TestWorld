using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RenderTextureRaycaster : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public RawImage rawImage;
    private bool isDragging;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (ConvertToViewportPoint(eventData, out var viewportPos))
        {
            SingletonSceneController.instance.HandlePointerDown(viewportPos);
            isDragging = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging && ConvertToViewportPoint(eventData, out var viewportPos))
            SingletonSceneController.instance.HandleDrag(viewportPos);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isDragging)
        {
            SingletonSceneController.instance.HandlePointerUp();
            isDragging = false;
        }
    }

    private bool ConvertToViewportPoint(PointerEventData eventData, out Vector2 viewportPos)
    {
        viewportPos = Vector2.zero;
        var controller = SingletonSceneController.instance;
        var cam = controller?.sceneCamera;
        if (cam == null || rawImage == null) return false;

        RectTransform rect = rawImage.rectTransform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out var local))
            return false;

        var pixelRect = rect.rect;
        float rw = Mathf.Abs(pixelRect.width);
        float rh = Mathf.Abs(pixelRect.height);

        float camAspect = (float)cam.pixelWidth / Mathf.Max(1, cam.pixelHeight);
        float imgAspect = rw / Mathf.Max(1f, rh);

        float usedW = rw;
        float usedH = rh;
        float padX = 0f;
        float padY = 0f;

        if (imgAspect > camAspect)
        {
            usedH = rw / camAspect;
            padY = (rh - usedH) * 0.5f;
        }
        else if (imgAspect < camAspect)
        {
            usedW = rh * camAspect;
            padX = (rw - usedW) * 0.5f;
        }

        float u = (local.x + rw * 0.5f - padX) / Mathf.Max(1e-6f, usedW);
        float v = (local.y + rh * 0.5f - padY) / Mathf.Max(1e-6f, usedH);

        if (u < 0f || u > 1f || v < 0f || v > 1f) return false;

        viewportPos = new Vector2(u, v);
        return true;
    }
}
