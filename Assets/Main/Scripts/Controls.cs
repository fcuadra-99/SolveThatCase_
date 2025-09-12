using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Controls : MonoBehaviour
{
    [Header("References")]
    public GraphicRaycaster uiRaycaster;
    public EventSystem eventSystem;
    public Transform bg;
    public Transform ch;
    public Camera cam;

    [Header("Settings")]
    public float panSpeed = 0.5f;
    public float doubleClickTime = 0.3f;
    public Vector3 chScale = new Vector3(1.5f, 1.5f, 1);
    [Range(0.001f, 1.0f)]
    public float dragThresholdPercent = 0.02f;
    [Range(0.01f, 1.5f)]
    public float edgeThreshold = 0.1f;

    private Vector3 lastMousePos;
    private float lastClickTime = 0f;
    private Vector3 dcfScale;

    private Vector2 touchStartPos;
    private bool isDragging = false;

    void Start()
    {
        if (ch != null) dcfScale = ch.localScale;
    }

    void Update()
    {
        HandleTouchInput();
        HandleMouseInput();
        CHClamp();
    }

    private float GetDragThreshold()
    {
        float diag = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height);
        return diag * dragThresholdPercent;
    }

    private bool IsPointerOverUI(Vector2 screenPos)
    {
        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = screenPos;

        List<RaycastResult> results = new List<RaycastResult>();
        uiRaycaster.Raycast(pointerData, results);

        return results.Count > 0;
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (Input.touchCount > 0)
            {
                if (IsPointerOverUI(touch.position))
                {
                    return;
                }
            }


            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
                isDragging = false;

                if (Time.time - lastClickTime < doubleClickTime)
                {
                    RaycastSelect(touch.position);
                }
                lastClickTime = Time.time;
            }

            if (touch.phase == TouchPhase.Moved)
            {
                if (!isDragging)
                {
                    if (Vector2.Distance(touch.position, touchStartPos) > GetDragThreshold())
                        isDragging = true;
                }

                if (isDragging)
                {
                    ch.position = cam.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, cam.nearClipPlane));
                    MoveBGWithCrosshair();
                    CamClamp();
                }

                CHScale(touch.position);
            }
        }
    }

    private void HandleMouseInput()
    {
        if (IsPointerOverUI(Input.mousePosition))
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
            isDragging = false;

            if (Time.time - lastClickTime < doubleClickTime)
            {
                RaycastSelect(Input.mousePosition);
            }
            lastClickTime = Time.time;
            lastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            if (!isDragging)
            {
                if (Vector2.Distance((Vector2)Input.mousePosition, touchStartPos) > GetDragThreshold())
                    isDragging = true;
            }

            if (isDragging)
            {
                ch.position = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane));
                MoveBGWithCrosshair();
                CamClamp();
                CHScale(Input.mousePosition);
            }
        }
    }

    private void RaycastSelect(Vector3 screenPos)
    {
        Vector2 worldPoint = cam.ScreenToWorldPoint(screenPos);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Items") || hit.collider.name == "YourObjectName")
            {
                Debug.Log("Pressed: " + hit.collider.name);
                hit.collider.gameObject.SendMessage("OnTouchDown", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    private void CHScale(Vector3 screenPos)
    {
        Vector2 worldPoint = cam.ScreenToWorldPoint(screenPos);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        if (hit.collider != null && (hit.collider.CompareTag("Items") || hit.collider.name == "YourObjectName"))
        {
            ch.localScale = Vector3.Lerp(ch.localScale, chScale, Time.deltaTime * 10f);
        }
        else
        {
            ch.localScale = Vector3.Lerp(ch.localScale, dcfScale, Time.deltaTime * 10f);
        }
    }

    private void CHClamp()
    {
        if (ch == null || cam == null) return;

        Vector3 chPos = ch.position;
        Vector3 viewportPos = cam.WorldToViewportPoint(chPos);

        viewportPos.x = Mathf.Clamp01(viewportPos.x);
        viewportPos.y = Mathf.Clamp01(viewportPos.y);

        ch.position = cam.ViewportToWorldPoint(viewportPos);
    }

    private void CamClamp()
    {
        if (bg == null || cam == null) return;

        SpriteRenderer sr = bg.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        float spriteWidth = sr.bounds.size.x;
        float spriteHeight = sr.bounds.size.y;

        Vector3 pos = bg.position;

        float minX = cam.transform.position.x - (spriteWidth / 2f - camWidth / 2f);
        float maxX = cam.transform.position.x + (spriteWidth / 2f - camWidth / 2f);

        float minY = cam.transform.position.y - (spriteHeight / 2f - camHeight / 2f);
        float maxY = cam.transform.position.y + (spriteHeight / 2f - camHeight / 2f);

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        bg.position = pos;
    }

    private void MoveBGWithCrosshair()
    {
        Vector3 vp = cam.WorldToViewportPoint(ch.position);
        Vector3 move = Vector3.zero;

        if (vp.x < edgeThreshold)
        {
            float t = 1f - (vp.x / edgeThreshold);
            move.x = t;
        }
        else if (vp.x > 1f - edgeThreshold)
        {
            float t = 1f - ((1f - vp.x) / edgeThreshold);
            move.x = -t;
        }

        if (vp.y < edgeThreshold)
        {
            float t = 1f - (vp.y / edgeThreshold);
            move.y = t;
        }
        else if (vp.y > 1f - edgeThreshold)
        {
            float t = 1f - ((1f - vp.y) / edgeThreshold);
            move.y = -t;
        }

        bg.Translate(move * panSpeed * Time.deltaTime);
    }

}
