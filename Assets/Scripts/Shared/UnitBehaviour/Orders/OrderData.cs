using System;
using UnityEngine;
using System.Collections.Generic;

public class OrderData {
    public static RTSGameObjectManager rtsGameObjectManager;

    public Vector3 targetPosition;
    public Vector3 orderIssuedPosition;
    public float orderRange = 1f;
    public List<MyPair<Type, int>> items;
    public RTSGameObject target;
    public Ability ability;
    public bool repeatOnComplete = false, isJoinable = false;
    public int remainingChannelTime;

    // Only the information needed to create a new order is required for transfer.
    // eg. OrderPhase is assumed to start at GetInRange
    // Might be able to trim it a bit more by removing things like repeatOnComplete which are generally static
    // Later will want to convert it into a minimal bit string then run compression on it 
    // (Lossless general compression cant remove unnecessary non-duplicated information, but we can)
    // (Also I'm not confident in my ability to not duplicate any info, so compression is likely to add value)
    // Should benchmark and weigh cpu time vs server bandwidth
    public string ToNetString()
    {
        string orderDataString = 
        (ability == null ? "" : ability.ToNetString()) + "~" +
        ItemFactory.ItemsToString(items) + "~" +
        orderIssuedPosition.x + "," + orderIssuedPosition.y + "," + orderIssuedPosition.z + "~" +
        orderRange + "~" +
        remainingChannelTime + "~" +
        repeatOnComplete + "~" +
        (target == null ? "" : target.unitId.ToString()) + "~" +
        targetPosition.x + "," + targetPosition.y + "," + targetPosition.z;

        return orderDataString;
    }

    public static OrderData FromString(string orderData)
    {
        OrderData outOData = new OrderData();
        string[] orderDataComponents = orderData.Split('~');
        string[] orderIssuedPositionComponents = orderDataComponents[2].Split(',');
        string[] targetPositionComponents = orderDataComponents[7].Split(',');
        Vector3 orderIssuedPosition = new Vector3();
        orderIssuedPosition.x = float.Parse(orderIssuedPositionComponents[0]);
        orderIssuedPosition.y = float.Parse(orderIssuedPositionComponents[1]);
        orderIssuedPosition.z = float.Parse(orderIssuedPositionComponents[2]);
        Vector3 targetPosition = new Vector3();
        targetPosition.x = float.Parse(targetPositionComponents[0]);
        targetPosition.y = float.Parse(targetPositionComponents[1]);
        targetPosition.z = float.Parse(targetPositionComponents[2]);

        outOData.ability = orderDataComponents[0] == "" ? null : Ability.FromString(orderDataComponents[0]);
        outOData.items = ItemFactory.GetItemsFromString(orderDataComponents[1]);
        outOData.orderIssuedPosition = orderIssuedPosition;
        outOData.orderRange = float.Parse(orderDataComponents[3]);
        outOData.remainingChannelTime = int.Parse(orderDataComponents[4]);
        outOData.repeatOnComplete = bool.Parse(orderDataComponents[5]);
        outOData.target = orderDataComponents[6] == "" ? null : rtsGameObjectManager.GetUnit(long.Parse(orderDataComponents[6]));
        outOData.targetPosition = targetPosition;

        return outOData;
    }
}
