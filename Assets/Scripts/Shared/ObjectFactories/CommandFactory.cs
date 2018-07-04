
using System;
using UnityEngine;
using System.Collections;

public class CommandFactory {

    public static Command GetCommandFromOrder(Order order)
    {
        Command command = new Command();
        command.clearExistingOrders = false;
        command.orderData = order.orderData;
        command.overrideDefaultOrderData = true;
        command.getOrder = (OrderBuilderFunction)Enum.Parse(typeof(OrderBuilderFunction), "New" + order.GetType().ToString());
        return command;
    }
}
