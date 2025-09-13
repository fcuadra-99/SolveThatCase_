using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Controls : MonoBehaviour
{
    public SettingsData settings;

    [Header("References")]
    public GraphicRaycaster uiRaycaster;
    public EventSystem eventSystem;
    public Transform bg;
    public Transform ch;
    public Camera cam;

    [Header("Systems")]
    public FileCollection itemCollector;

    [Header("Settings")]
    public float doubleClickTime = 0.3f;
    public Vector3 chScale = new Vector3(1.5f, 1.5f, 1);
    [Range(0.001f, 1.0f)]
    public float dragThresholdPercent = 0.02f;
    [Range(0.01f, 1.5f)]
    public float edgeThreshold = 0.1f;
    public float crosshairLerpSpeed = 10f;

    private Vector3 lastMousePos;
    private float lastClickTime = 0f;
    private Vector3 dcfScale;
    private Vector3 crosshairVelocity = Vector3.zero;

    private Vector2 touchStartPos;
    private bool isDragging = false;

    private Vector3 chTargetPos;

    void Start()
    {
        if (ch != null)
        {
            dcfScale = ch.localScale;
            chTargetPos = ch.position;
        }
    }

    void Update()
    {
        HandleTouchInput();
        HandleMouseInput();

        if (ch != null)
        {
            ch.position = Vector3.Lerp(ch.position, chTargetPos, Time.deltaTime * crosshairLerpSpeed);
        }

        CHClamp();
    }

    private float GetDragTresh()
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

            if (Input.touchCount > 0 && IsPointerOverUI(touch.position))
            {
                return;
            }

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
                isDragging = false;

                if (Time.time - lastClickTime < doubleClickTime)
                {
                    RaycastSelect();
                }
                lastClickTime = Time.time;
            }

            if (touch.phase == TouchPhase.Moved)
            {
                if (!isDragging && Vector2.Distance(touch.position, touchStartPos) > GetDragTresh())
                    isDragging = true;

                if (isDragging)
                {
                    chTargetPos = cam.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, cam.nearClipPlane));
                    MoveCrosshair();
                    CamClamp();
                }

                CHScale();
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
                RaycastSelect();
            }
            lastClickTime = Time.time;
            lastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            if (!isDragging && Vector2.Distance((Vector2)Input.mousePosition, touchStartPos) > GetDragTresh())
                isDragging = true;

            if (isDragging)
            {
                chTargetPos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane));
                MoveCrosshair();
                CamClamp();
                CHScale();
            }
        }
    }

    private void RaycastSelect()
    {
        if (ch == null) return;

        Vector2 worldPoint = ch.position;
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Items") || hit.collider.name == "YourObjectName")
            {
                Debug.Log("Pressed: " + hit.collider.name);

                if (itemCollector != null)
                {
                    bool isNew = itemCollector.CollectItem(hit.collider.name);

                    if (!isNew)
                    {
                        Debug.Log("Popup: Already collected " + hit.collider.name);
                    }
                }

                hit.collider.gameObject.SendMessage("OnTouchDown", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    private void CHScale()
    {
        if (ch == null || cam == null) return;

        Vector2 worldPoint = ch.position;
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

    private void MoveCrosshair()
    {
        Vector3 vp = cam.WorldToViewportPoint(chTargetPos);
        Vector3 targetMove = Vector3.zero;

        if (vp.x < edgeThreshold)
        {
            float t = 1f - (vp.x / edgeThreshold);
            targetMove.x = t;
        }
        else if (vp.x > 1f - edgeThreshold)
        {
            float t = 1f - ((1f - vp.x) / edgeThreshold);
            targetMove.x = -t;
        }

        if (vp.y < edgeThreshold)
        {
            float t = 1f - (vp.y / edgeThreshold);
            targetMove.y = t;
        }
        else if (vp.y > 1f - edgeThreshold)
        {
            float t = 1f - ((1f - vp.y) / edgeThreshold);
            targetMove.y = -t;
        }

        crosshairVelocity = Vector3.Lerp(crosshairVelocity, targetMove, Time.deltaTime * 5f);

        bg.Translate(crosshairVelocity * settings.crosshairSpeed * Time.deltaTime, Space.World);
    }
}
