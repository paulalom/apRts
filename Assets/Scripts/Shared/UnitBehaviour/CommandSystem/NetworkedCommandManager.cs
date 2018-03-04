﻿using System;
using UnityEngine;
using System.Collections.Generic;

public class NetworkedCommandManager : ICommandManager{

    public PlayerManager playerManager;
    public NetworkStateManager netStateManager;
    CommandQueue nonNetworkedCommandQueue = new CommandQueue(); // non game affecting ui commands
    
    public override void AddCommand(Command command)
    {
        MyPair<List<long>, Command> unitCommandPair = BuildUnitCommandPair(command);
        long stepToRunCommands = StepManager.CurrentStep + StepManager.numStepsToDelayInputProcessing;

        netStateManager.AddCommand(stepToRunCommands, unitCommandPair);
    }

    public override void AddCommand(Command command, List<long> unitIds)
    {
        long stepToRunCommands = StepManager.CurrentStep + StepManager.numStepsToDelayInputProcessing;
        MyPair<List<long>, Command> unitCommandPair = new MyPair<List<long>, Command>();
        unitCommandPair.Key = unitIds;
        unitCommandPair.Value = command;

        netStateManager.AddCommand(stepToRunCommands, unitCommandPair);
    }

    public override void AddCommands(List<MyPair<List<long>, Command>> commands)
    {
        long stepToRunCommands = StepManager.CurrentStep + StepManager.numStepsToDelayInputProcessing;
        foreach (MyPair<List<long>, Command> command in commands)
        {
            netStateManager.AddCommand(stepToRunCommands, command);
        }
    }

    public override void AddNonNetworkedCommand(Command command)
    {
        MyPair<List<long>, Command> unitCommandPair = BuildUnitCommandPair(command);
        nonNetworkedCommandQueue.AddCommand(StepManager.CurrentStep, unitCommandPair);
    }
    
    MyPair<List<long>, Command> BuildUnitCommandPair(Command command)
    {
        List<long> units;
        MyPair<List<long>, Command> unitCommandPair;
        
        units = playerManager.GetOrderableSelectedUnitIds();
        unitCommandPair = new MyPair<List<long>, Command>(units, command);

        return unitCommandPair;
    }

    public override void ProcessCommandsForStep(long step, GameManager gameManager)
    {
        foreach (MyPair<List<long>, Command> command in GetCommandsForStep(step))
        {
            Command comm = command.Value;
            Command.GetAction(comm.action, gameManager).Invoke();
            Command.GetRayCastHitAction(comm.raycastHitAction, gameManager).Invoke(comm.orderData.target, comm.orderData.targetPosition);
            Command.GetUnitCommandAction(comm.raycastHitUnitAction, gameManager).Invoke(command.Key, comm);

            Order order = Command.GetNextDefaultOrderFunction(comm.getOrder, gameManager).Invoke();
            if (order != null)
            {
                if (comm.overrideDefaultOrderData)
                {
                    order.orderData = comm.orderData;
                }
                gameManager.ProcessOrder(command.Key, command.Value, order);
            }
        }
    }

    public override List<MyPair<List<long>, Command>> GetCommandsForStep(long step)
    {
        List<MyPair<List<long>, Command>> commands;
        commands = nonNetworkedCommandQueue.GetCommandsForStep(step);
        commands.AddRange(netStateManager.localCommands.GetCommandsForStep(step));
        return commands;
    }
}