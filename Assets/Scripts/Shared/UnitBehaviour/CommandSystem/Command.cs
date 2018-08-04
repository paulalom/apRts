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
    public bool clearExistingOrders = true, smartCast = false, overrideDefaultOrderData = false;
    public OrderData orderData = new OrderData();
    public OrderBuilderFunction getOrder = OrderBuilderFunction.None;

    /*
    public override string ToString()
    {
        return "clrExOrd: " + (clearExistingOrders ? "1|" : "0|") +
               "SmCst: " + (smartCast ? "1|" : "0|") +
               "ovridDefOrdDat: " + (overrideDefaultOrderData ? "1|" : "0|") +
                orderData.ToString() + "|" +
                action.ToString() + "|" +
                getOrder.ToString();
    }*/

    public string ToNetString()
    {
        return (clearExistingOrders ? "1|" : "0|") +
                (smartCast ? "1|" : "0|") +
                (overrideDefaultOrderData ? "1|" : "0|") +
                orderData.ToNetString() + "|" +
                ((int)getOrder).ToString();
    }

    public static Command FromNetString(string commandString, PlayerManager playerManager)
    {
        Command command = new Command();
        string[] commandComponents = commandString.Split('|');

        command.clearExistingOrders = commandComponents[0] == "1";
        command.smartCast = commandComponents[1] == "1";
        command.overrideDefaultOrderData = commandComponents[2] == "1";
        command.orderData = OrderData.FromString(commandComponents[3], playerManager);
        command.getOrder = (OrderBuilderFunction)Enum.Parse(typeof(OrderBuilderFunction), commandComponents[4]);

        return command;
    }

    public static Func<Order> GetNextDefaultOrderFunction(OrderBuilderFunction orderFunction, GameManager gameManager)
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
