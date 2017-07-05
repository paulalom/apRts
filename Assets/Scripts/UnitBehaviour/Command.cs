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
public enum CommandRaycastAction
{
    OnMoveButtonRelease,
    OnActionButtonRelease,
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
    public CommandRaycastAction raycastHitAction = CommandRaycastAction.None;
    public CommandGetOrderFunction getOrder = CommandGetOrderFunction.None;
    
    public override string ToString()
    {
        return (clearExistingOrders ? "1|" : "0|") +
                (smartCast ? "1|" : "0|") +
                (overrideDefaultOrderData ? "1|" : "0|") +
                (clientSideOnly ? "1|" : "0|") +
                orderData.ToString() + "|" +
                ((int)action).ToString() + "|" +
                ((int)raycastHitAction).ToString() + "|" +
                ((int)getOrder).ToString();
    }

    public static Command FromString(string commandString, PlayerManager playerManager)
    {
        Command command = new Command();
        string[] commandComponents = commandString.Split('|');

        command.clearExistingOrders = commandComponents[0] == "1";
        command.smartCast = commandComponents[1] == "1";
        command.overrideDefaultOrderData = commandComponents[2] == "1";
        command.clientSideOnly = commandComponents[3] == "1";        
        command.orderData = OrderData.FromString(commandComponents[4], playerManager);
        command.action = (CommandAction)Enum.Parse(typeof(CommandAction), commandComponents[5]);
        command.raycastHitAction = (CommandRaycastAction)Enum.Parse(typeof(CommandRaycastAction), commandComponents[6]);
        command.getOrder = (CommandGetOrderFunction)Enum.Parse(typeof(CommandGetOrderFunction), commandComponents[7]);

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

    public static Action<RTSGameObject, Vector3> GetRayCastHitAction(CommandRaycastAction commandRaycastAction, GameManager gameManager)
    {
        switch (commandRaycastAction)
        {
            case CommandRaycastAction.OnMoveButtonRelease:
                return gameManager.OnMoveButtonRelease;
            case CommandRaycastAction.OnActionButtonRelease:
                return gameManager.OnActionButtonRelease;
            case CommandRaycastAction.RaiseTerrain:
                return gameManager.RaiseTerrain;
            case CommandRaycastAction.SpawnFactory:
                return gameManager.SpawnFactory;
            default:
                return delegate { };
        }
    }
}
