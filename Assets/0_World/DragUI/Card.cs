using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerClickHandler
{
    public int index;
    private void Start()
    {
        Color randomColor = new Color(Random.value, Random.value, Random.value);
        this.GetComponent<Image>().color = randomColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Click on " + this.name);
    }
}
