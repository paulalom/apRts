using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public enum CommandAction
{
    OnActionButtonPress,
    RaiseCamera,
    LowerCamera,
    None
}
public enum CommandRaycastHitUnitAction
{
    OnMoveButtonRelease,
    OnActionButtonRelease,
    None
}
public enum CommandRaycastHitAction
{
    RaiseTerrain,
    SpawnFactory,
    None
}
public enum CommandGetOrderFunction
{
    GetDefaultUseAbilityOrder,
    GetDefaultCancelOrder,
    GetDefaultConstructionOrder,
    GetDefaultHarvestOrder,
    GetDefaultPatrolOrder,
    GetDefaultGuardOrder,
    GetDefaultFollowOrder,
    GetDefaultMoveOrder,
    GetDefaultGiveOrder,
    GetDefaultTakeOrder,
    None
}

public class Command
{
    public bool clearExistingOrders = true, smartCast = false, overrideDefaultOrderData = false, clientSideOnly = false;
    public OrderData orderData = new OrderData();
    public CommandAction action = CommandAction.None;
    public CommandRaycastHitAction raycastHitAction = CommandRaycastHitAction.None;
    public CommandRaycastHitUnitAction raycastHitUnitAction = CommandRaycastHitUnitAction.None;
    public CommandGetOrderFunction getOrder = CommandGetOrderFunction.None;

    public override string ToString()
    {
        return "clrExOrd: " + (clearExistingOrders ? "1|" : "0|") +
               "SmCst: " + (smartCast ? "1|" : "0|") +
               "ovridDefOrdDat: " + (overrideDefaultOrderData ? "1|" : "0|") +
               "cliSide: " + (clientSideOnly ? "1|" : "0|") +
                orderData.ToString() + "|" +
                action.ToString() + "|" +
                raycastHitAction.ToString() + "|" +
                raycastHitUnitAction.ToString() + "|" +
                getOrder.ToString();
    }

    public string ToNetString()
    {
        return (clearExistingOrders ? "1|" : "0|") +
                (smartCast ? "1|" : "0|") +
                (overrideDefaultOrderData ? "1|" : "0|") +
                (clientSideOnly ? "1|" : "0|") +
                orderData.ToNetString() + "|" +
                ((int)action).ToString() + "|" +
                ((int)raycastHitAction).ToString() + "|" +
                ((int)raycastHitUnitAction).ToString() + "|" +
                ((int)getOrder).ToString();
    }

    public static Command FromNetString(string commandString, PlayerManager playerManager)
    {
        Command command = new Command();
        string[] commandComponents = commandString.Split('|');

        command.clearExistingOrders = commandComponents[0] == "1";
        command.smartCast = commandComponents[1] == "1";
        command.overrideDefaultOrderData = commandComponents[2] == "1";
        command.clientSideOnly = commandComponents[3] == "1";
        command.orderData = OrderData.FromString(commandComponents[4], playerManager);
        command.action = (CommandAction)Enum.Parse(typeof(CommandAction), commandComponents[5]);
        command.raycastHitAction = (CommandRaycastHitAction)Enum.Parse(typeof(CommandRaycastHitAction), commandComponents[6]);
        command.raycastHitUnitAction = (CommandRaycastHitUnitAction)Enum.Parse(typeof(CommandRaycastHitUnitAction), commandComponents[7]);
        command.getOrder = (CommandGetOrderFunction)Enum.Parse(typeof(CommandGetOrderFunction), commandComponents[8]);

        return command;
    }

    public static Func<Order> GetNextDefaultOrderFunction(CommandGetOrderFunction orderFunction, GameManager gameManager)
    {
        switch (orderFunction)
        {
            case CommandGetOrderFunction.GetDefaultUseAbilityOrder:
                return OrderFactory.GetDefaultUseAbilityOrder;
            case CommandGetOrderFunction.GetDefaultCancelOrder:
                return OrderFactory.GetDefaultCancelOrder;
            case CommandGetOrderFunction.GetDefaultConstructionOrder:
                return OrderFactory.GetDefaultConstructionOrder;
            case CommandGetOrderFunction.GetDefaultHarvestOrder:
                return OrderFactory.GetDefaultHarvestOrder;
            case CommandGetOrderFunction.GetDefaultPatrolOrder:
                return OrderFactory.GetDefaultPatrolOrder;
            case CommandGetOrderFunction.GetDefaultGuardOrder:
                return OrderFactory.GetDefaultGuardOrder;
            case CommandGetOrderFunction.GetDefaultFollowOrder:
                return OrderFactory.GetDefaultFollowOrder;
            case CommandGetOrderFunction.GetDefaultMoveOrder:
                return OrderFactory.GetDefaultMoveOrder;
            case CommandGetOrderFunction.GetDefaultGiveOrder:
                return OrderFactory.GetDefaultGiveOrder;
            case CommandGetOrderFunction.GetDefaultTakeOrder:
                return OrderFactory.GetDefaultTakeOrder;
            default:
                return delegate { return null; };
        }
    }

    public static Action GetAction(CommandAction commandAction, GameManager gameManager)
    {
        switch (commandAction)
        {
            case CommandAction.LowerCamera:
                return gameManager.mainCamera.LowerCamera;
            case CommandAction.RaiseCamera:
                return gameManager.mainCamera.RaiseCamera;
            case CommandAction.OnActionButtonPress:
                return gameManager.OnActionButtonPress;
            default:
                return delegate { };
        }
    }

    public static Action<RTSGameObject, Vector3> GetRayCastHitAction(CommandRaycastHitAction commandRaycastHitAction, GameManager gameManager)
    {
        switch (commandRaycastHitAction)
        {
            case CommandRaycastHitAction.RaiseTerrain:
                return gameManager.RaiseTerrain;
            case CommandRaycastHitAction.SpawnFactory:
                return gameManager.SpawnFactory;
            default:
                return delegate { };
        }
    }


    public static Action<List<long>, Command> GetUnitCommandAction(CommandRaycastHitUnitAction commandRaycastHitUnitAction, GameManager gameManager)
    {
        switch (commandRaycastHitUnitAction)
        {
            case CommandRaycastHitUnitAction.OnMoveButtonRelease:
                return gameManager.OnMoveButtonRelease;
            case CommandRaycastHitUnitAction.OnActionButtonRelease:
                return gameManager.OnActionButtonRelease;
            default:
                return delegate { };
        }
    }
}
