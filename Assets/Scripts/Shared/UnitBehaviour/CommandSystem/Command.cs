using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public enum CommandAction
{
    RaiseTerrain,
    SpawnFactory,
    None
}
public enum OrderBuilderFunction
{
    NewUseAbilityOrder,
    NewCancelOrder,
    NewProductionOrder,
    NewConstructionOrder,
    NewResumeConstructionOrder,
    NewJoinOrder,
    NewHarvestOrder,
    NewPatrolOrder,
    NewGuardOrder,
    NewFollowOrder,
    NewMoveOrder,
    NewGiveOrder,
    NewTakeOrder,
    NewCheatSpawnFactoryOrder,
    NewCheatRaiseTerrainOrder,
    None
}

public class Command
{
    public bool queueOrderInsteadOfClearing = true, queueOrderAtFront = false, smartCast = false, overrideDefaultOrderData = false;
    public OrderData orderData = new OrderData();
    public OrderBuilderFunction getOrder = OrderBuilderFunction.None;

    public string ToNetString()
    {
        return (queueOrderInsteadOfClearing ? "1|" : "0|") +
                (queueOrderAtFront ?  "1|" : "0|") +
                (smartCast ? "1|" : "0|") +
                (overrideDefaultOrderData ? "1|" : "0|") +
                orderData.ToNetString() + "|" +
                ((int)getOrder).ToString();
    }

    public static Command FromNetString(string commandString, RTSGameObjectManager rtsGameObjectManager)
    {
        Command command = new Command();
        string[] commandComponents = commandString.Split('|');

        command.queueOrderInsteadOfClearing = commandComponents[0] == "1";
        command.queueOrderAtFront = commandComponents[1] == "1";
        command.smartCast = commandComponents[2] == "1";
        command.overrideDefaultOrderData = commandComponents[3] == "1";
        command.orderData = OrderData.FromString(commandComponents[4], rtsGameObjectManager);
        command.getOrder = (OrderBuilderFunction)Enum.Parse(typeof(OrderBuilderFunction), commandComponents[5]);

        return command;
    }

    public static Func<Order> GetDefaultOrderFunction(OrderBuilderFunction orderFunction, GameManager gameManager)
    {
        switch (orderFunction)
        {
            case OrderBuilderFunction.NewUseAbilityOrder:
                return OrderFactory.GetDefaultUseAbilityOrder;
            case OrderBuilderFunction.NewCancelOrder:
                return OrderFactory.GetDefaultCancelOrder;
            case OrderBuilderFunction.NewProductionOrder:
                return OrderFactory.GetDefaultProductionOrder;
            case OrderBuilderFunction.NewConstructionOrder:
                return OrderFactory.GetDefaultConstructionOrder;
            case OrderBuilderFunction.NewResumeConstructionOrder:
                return OrderFactory.GetDefaultResumeConstructionOrder;
            case OrderBuilderFunction.NewJoinOrder:
                return OrderFactory.GetDefaultJoinOrder;
            case OrderBuilderFunction.NewHarvestOrder:
                return OrderFactory.GetDefaultHarvestOrder;
            case OrderBuilderFunction.NewPatrolOrder:
                return OrderFactory.GetDefaultPatrolOrder;
            case OrderBuilderFunction.NewGuardOrder:
                return OrderFactory.GetDefaultGuardOrder;
            case OrderBuilderFunction.NewFollowOrder:
                return OrderFactory.GetDefaultFollowOrder;
            case OrderBuilderFunction.NewMoveOrder:
                return OrderFactory.GetDefaultMoveOrder;
            case OrderBuilderFunction.NewGiveOrder:
                return OrderFactory.GetDefaultGiveOrder;
            case OrderBuilderFunction.NewTakeOrder:
                return OrderFactory.GetDefaultTakeOrder;
            case OrderBuilderFunction.NewCheatSpawnFactoryOrder:
                return OrderFactory.NewCheatSpawnFactoryOrder;
            case OrderBuilderFunction.NewCheatRaiseTerrainOrder:
                return OrderFactory.NewCheatRaiseTerrainOrder;
            default:
                return delegate { return null; };
        }
    }
}
