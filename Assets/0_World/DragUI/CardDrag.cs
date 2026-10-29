using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDrag : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public FlagstoneManager manager;
    public RectTransform RT;

    private bool checkCard;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Transform originalParent;

    private int originalIndex;

    private CanvasGroup canvasGroup;

    public void SetUp(FlagstoneManager manager)
    {
        this.manager = manager;

        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
        originalParent = transform.parent;
        originalIndex = transform.GetSiblingIndex(); // 获取初始索引
        checkCard = true;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log(this.name + " Begin Drag");

        originalParent = transform.parent;
        originalIndex = transform.GetSiblingIndex(); // 记录初始位置
        transform.SetParent(manager.canvas.transform, true);
        transform.SetAsLastSibling(); // 将被拖拽物体移到最前面
        canvasGroup.blocksRaycasts = false;   //禁用射线检测
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(manager.MoveLimitRt, eventData.position, eventData.pressEventCamera, out Vector2 MousePosition))
        {
            Vector3 globalMousePos = manager.canvas.transform.TransformPoint(MousePosition);
            RT.position = globalMousePos;
            checkCard = false;

            // 确保拖拽的卡牌始终在最高层级
            transform.SetAsLastSibling();

            // 动态调整子物体的位置
            //AdjustSiblingIndex(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log(this.name + " End Drag");

        checkCard = true;
        transform.localScale = originalScale; // 恢复初始缩放比例
        canvasGroup.blocksRaycasts = true;     //恢复射线检测

        if (RectTransformUtility.RectangleContainsScreenPoint(manager.UpperListArea, eventData.position, eventData.pressEventCamera))
        {
            Debug.Log(this.name + " 1");
            if (manager.UpperListParent.childCount < manager.maxCardsInUpperList)
            {
                Debug.Log(this.name + " 1-1");
                // 如果在目标区域内，并且目标区域内卡牌数量小于最大限制，插入目标区域
                transform.SetParent(manager.UpperListParent, true);
                transform.SetSiblingIndex(GetNewSiblingIndexInTarget(eventData));
            }
            else
            {
                Debug.Log(this.name + " 1-2");
                // 替换上面列表中的卡牌
                Transform cardToReplace = GetCardToReplace(eventData);
                if (cardToReplace != null)
                {
                    int replaceIndex = cardToReplace.GetSiblingIndex();

                    cardToReplace.SetParent(manager.LowerListParent, true);
                    cardToReplace.SetAsLastSibling();

                    transform.SetParent(manager.UpperListParent, true);
                    transform.SetSiblingIndex(replaceIndex);
                }
                else
                {
                    // 如果没有找到可替换的卡片，恢复到原始位置
                    RT.localPosition = originalPosition;
                    transform.SetParent(originalParent, true);
                    transform.SetSiblingIndex(originalIndex); // 恢复初始索引
                }
            }
        }
        else if (RectTransformUtility.RectangleContainsScreenPoint(manager.LowerListArea, eventData.position, eventData.pressEventCamera))
        {
            Debug.Log(this.name + " 2");
            // 如果在下列表区域内，插入下列表
            transform.SetParent(manager.LowerListParent, true);
            transform.SetSiblingIndex(GetNewSiblingIndexInTarget(eventData));
        }
        else if (IsWithinParentBounds(eventData))
        {
            Debug.Log(this.name + " 3");
            // 如果在原始父物体范围内，恢复初始位置
            transform.SetParent(originalParent, true);
            transform.SetSiblingIndex(GetNewSiblingIndex(eventData)); // 设置新位置
        }
        else
        {
            Debug.Log(this.name + " 4");
            // 如果不在任何有效区域内，恢复到原始位置
            RT.localPosition = originalPosition;
            transform.SetParent(originalParent, true);
            transform.SetSiblingIndex(originalIndex); // 恢复初始索引
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = new Vector3(1.1f, 1.1f, 1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (checkCard)
        {
            transform.localScale = originalScale;
        }
    }

    void AdjustSiblingIndex(PointerEventData eventData)
    {
        int newIndex = GetNewSiblingIndex(eventData);
        if (newIndex != transform.GetSiblingIndex())
        {
            transform.SetSiblingIndex(newIndex);
        }
    }

    int GetNewSiblingIndex(PointerEventData eventData)
    {
        Transform currentParent = originalParent;

        // 检查是否在上列表区域
        if (RectTransformUtility.RectangleContainsScreenPoint(manager.UpperListArea, eventData.position, eventData.pressEventCamera))
        {
            currentParent = manager.UpperListArea;
        }
        // 检查是否在下列表区域
        else if (RectTransformUtility.RectangleContainsScreenPoint(manager.LowerListArea, eventData.position, eventData.pressEventCamera))
        {
            currentParent = manager.LowerListParent;
        }

        for (int i = 0; i < currentParent.childCount; i++)
        {
            if (currentParent.GetChild(i) == transform) continue;

            RectTransform child = currentParent.GetChild(i) as RectTransform;
            if (RectTransformUtility.RectangleContainsScreenPoint(child, eventData.position, manager.canvas.worldCamera))
            {
                return i < transform.GetSiblingIndex() ? i : i + 1;
            }
        }
        return currentParent.childCount - 1;
    }

    int GetNewSiblingIndexInTarget(PointerEventData eventData)
    {
        RectTransform targetParent = manager.UpperListArea;

        // 检查是否在上列表区域
        if (RectTransformUtility.RectangleContainsScreenPoint(manager.UpperListArea, eventData.position, eventData.pressEventCamera))
        {
            targetParent = manager.UpperListParent;
        }
        // 检查是否在下列表区域
        else if (RectTransformUtility.RectangleContainsScreenPoint(manager.LowerListArea, eventData.position, eventData.pressEventCamera))
        {
            targetParent = manager.LowerListParent;
        }

        for (int i = 0; i < targetParent.childCount; i++)
        {
            RectTransform child = targetParent.GetChild(i) as RectTransform;
            if (RectTransformUtility.RectangleContainsScreenPoint(child, eventData.position, manager.canvas.worldCamera))
            {
                return i;
            }
        }
        return targetParent.childCount;
    }

    bool IsWithinParentBounds(PointerEventData eventData)
    {
        // 检查是否在原始父物体或者移动限制范围内
        return RectTransformUtility.RectangleContainsScreenPoint(originalParent as RectTransform, eventData.position, manager.canvas.worldCamera)
            || RectTransformUtility.RectangleContainsScreenPoint(manager.MoveLimitRt, eventData.position, manager.canvas.worldCamera);
    }

    Transform GetCardToReplace(PointerEventData eventData)
    {
        RectTransform targetParent = manager.UpperListParent;

        for (int i = 0; i < targetParent.childCount; i++)
        {
            RectTransform child = targetParent.GetChild(i) as RectTransform;
            if (RectTransformUtility.RectangleContainsScreenPoint(child, eventData.position, manager.canvas.worldCamera))
            {
                return child;
            }
        }
        return null;
    }
}
