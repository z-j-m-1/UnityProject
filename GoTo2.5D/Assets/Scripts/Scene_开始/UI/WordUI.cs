using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class WordUI : MonoBehaviour, IPointerEnterHandler
{
    private string wordText = "";

    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;

    void Start()
    {
        TextMeshProUGUI textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        if (textMeshPro != null)
        {
            wordText = textMeshPro.text.Trim();
            gameObject.name = "Word_" + wordText;
        }
        if(clickSound == null)
        {
            Debug.LogWarning("Click sound is not assigned in WordUI.");
        }
        if(hoverSound == null)
        {
            Debug.LogWarning("Hover sound is not assigned in WordUI.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

        MusicManager.Instance?.PlaySFX(hoverSound);

    }
}