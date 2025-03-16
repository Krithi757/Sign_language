using UnityEngine;
using UnityEngine.EventSystems;

public class WordDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Rigidbody2D rb; // Reference to Rigidbody2D
    private BoxCollider2D boxCollider; // Reference to BoxCollider2D

    private Vector2 originalPosition;
    private bool isDragging = false;

    public float fallSpeed = 50f; // You can tweak this in the Inspector
    public float minX, maxX, minY, maxY; // For clamping the word within screen bounds

    private GameManager gameManager; // Reference to GameManager

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rb = GetComponent<Rigidbody2D>(); // Get Rigidbody2D
        boxCollider = GetComponent<BoxCollider2D>(); // Get BoxCollider2D
        originalPosition = rectTransform.anchoredPosition;

        // Get reference to GameManager
        gameManager = GameManager.instance;
    }

    void Start()
    {
        // Disable Rigidbody2D until the game starts
        rb.isKinematic = true; // Prevent physics from affecting it initially
    }

    void Update()
    {
        // ✅ STOP FALLING IF THE GAME HASN'T STARTED
        if (!gameManager.gameStarted)
            return;

        // ✅ Enable Rigidbody2D once the game starts
        if (gameManager.gameStarted && rb.isKinematic)
        {
            rb.isKinematic = false; // Allow physics after game starts
        }

        // ✅ Only fall if NOT dragging
        if (!isDragging)
        {
            rectTransform.anchoredPosition += Vector2.down * fallSpeed * Time.deltaTime;

            // ✅ OPTIONAL: Destroy if it goes off-screen (adjust as needed)
            if (rectTransform.anchoredPosition.y < -600f) // Adjust based on your canvas
            {
                gameManager.WrongAnswer(); // Deduct a life when a word falls off-screen
                ResetPosition(); // Reuse the word
            }
        }

        // Ensure the word doesn't get dragged out of screen bounds
        Vector2 clampedPosition = rectTransform.anchoredPosition;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
        rectTransform.anchoredPosition = clampedPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // ✅ OPTIONAL: Snap back if not dropped somewhere valid
        if (!eventData.pointerEnter)
        {
            ResetPosition();
        }
    }

    public void ResetPosition()
    {
        rectTransform.anchoredPosition = originalPosition; // Reset to original position
    }
}
