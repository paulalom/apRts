using System;
using UnityEngine;
using System.Collections.Generic;

public class OrderData {
    
    public OrderPhase phase = OrderPhase.GetInRange;
    public Vector3 targetPosition;
    public Vector3 orderIssuedPosition;
    public float orderRange = 1f;
    public List<MyPair<Type, int>> items;
    public RTSGameObject target;
    public Ability ability;
    public bool repeatOnComplete = false;
    public float remainingChannelTime;

    public override string ToString()
    {
        string orderDataString =
        "abi: " + (ability == null ? "" : ability.ToString()) + "~" +
        ItemFactory.ItemsToString(items) + "~" +
        "isPos: " + orderIssuedPosition.x + "," + orderIssuedPosition.y + "," + orderIssuedPosition.z + "~" +
        "r: " + orderRange + "~" +
        phase + "~" +
        "chanl: " + remainingChannelTime + "~" +
        "repOnComp: " + repeatOnComplete + "~" +
        "targ: " + (target == null ? "" : target.name) + "~" +
        "targPos: " + targetPosition.x + "," + targetPosition.y + "," + targetPosition.z;

        return orderDataString;
    }

    public string ToNetString()
    {
        string orderDataString = 
        (ability == null ? "" : ability.ToNetString()) + "~" +
        ItemFactory.ItemsToString(items) + "~" +
        orderIssuedPosition.x + "," + orderIssuedPosition.y + "," + orderIssuedPosition.z + "~" +
        orderRange + "~" +
        (int)phase + "~" +
        remainingChannelTime + "~" +
        repeatOnComplete + "~" +
        (target == null ? "" : target.uid.ToString()) + "~" +
        targetPosition.x + "," + targetPosition.y + "," + targetPosition.z;

        return orderDataString;
    }

    public static OrderData FromString(string orderData, PlayerManager playerManager)
    {
        OrderData outOData = new OrderData();
        string[] orderDataComponents = orderData.Split('~');
        string[] orderIssuedPositionComponents = orderDataComponents[2].Split(',');
        string[] targetPositionComponents = orderDataComponents[8].Split(',');
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
        outOData.phase = (OrderPhase)Enum.Parse(typeof(OrderPhase), orderDataComponents[4]);
        outOData.remainingChannelTime = float.Parse(orderDataComponents[5]);
        outOData.repeatOnComplete = bool.Parse(orderDataComponents[6]);
        outOData.target = orderDataComponents[7] == "" ? null : playerManager.GetUnit(long.Parse(orderDataComponents[7]));
        outOData.targetPosition = targetPosition;

        return outOData;
    }
}
