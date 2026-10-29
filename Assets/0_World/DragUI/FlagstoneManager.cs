using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagstoneManager : MonoBehaviour
{
    public Canvas canvas;

    public RectTransform UpperListParent; // 上列表区域
    public RectTransform UpperListArea; // 上列表区域
    public RectTransform LowerListParent; // 下列表的父物体
    public RectTransform LowerListArea; // 下列表区域
    public RectTransform MoveLimitRt; //移动范围

    public int maxCardsInUpperList = 5; // 上面列表的最大卡牌数量

    public GameObject flagstonePrefab;

    private List<int> upperList;
    private List<int> lowerList;

    private void Start()
    {
        for (int i = 0; i < 7; i++)
        {
            GameObject prefab = Instantiate(flagstonePrefab, LowerListParent);
            prefab.GetComponent<CardDrag>().SetUp(this);
            prefab.GetComponent<Card>().index = i;
            prefab.name = i.ToString();
        }

        UpdateInventory();
    }


    public void UpdateInventory()
    {
        upperList = new List<int>();
        lowerList = new List<int>();

        for (int i = 0; i < UpperListParent.childCount; i++)
        {
            Transform child = UpperListParent.GetChild(i);
            Card card = child.GetComponent<Card>(); // 获取 Card 组件
            if (card != null)
            {
                upperList.Add(child.childCount);
            }
        }

        for (int i = 0; i < LowerListParent.childCount; i++)
        {
            Transform child = LowerListParent.GetChild(i);
            Card card = child.GetComponent<Card>(); // 获取 Card 组件
            if (card != null)
            {
                lowerList.Add(child.childCount);
            }
        }
    }
}
