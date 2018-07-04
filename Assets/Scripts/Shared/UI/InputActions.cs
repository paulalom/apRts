using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Shared.UI
{
    public static class InputActions
    {
        public static GameManager gameManager;
        public static MenuManager menuManager;
        public static SelectionManager selectionManager;

        public static void IssueCommand(int orderTypeId)
        {
            Command command = new Command() { getOrder = (OrderBuilderFunction)orderTypeId };

            if (Input.GetKey(Setting.dontClearExistingOrdersToggle))
            {
                command.clearExistingOrders = false;
            }

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool rayCast = Physics.Raycast(ray, out hit);

            if (rayCast)
            {
                command.orderData.targetPosition = hit.point;
                command.orderData.target = hit.collider.GetComponent<RTSGameObject>();
            }

            gameManager.commandManager.AddCommand(command);
        }

        internal static void OnActionButtonPress()
        {
            selectionManager.mouseDown = Input.mousePosition;
        }

        internal static void OnActionButtonRelease()
        {
            if (selectionManager.mouseDown == Input.mousePosition)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                bool rayCast = Physics.Raycast(ray, out hit);

                if (rayCast)
                {
                    selectionManager.CheckSingleSelectionEvent(hit);
                }
            }
            else
            {
                selectionManager.CheckBoxSelectionEvent();
            }
            selectionManager.menuClicked = false;

            // For now everything is smart cast, Once that changes, regular cast abilities will go here.
        }

        internal static void RaiseCamera()
        {
            gameManager.mainCamera.RaiseCamera();
        }

        internal static void LowerCamera()
        {
            gameManager.mainCamera.LowerCamera();
        }

        internal static void NumericMenuButton(int keyNum)
        {
            Type[] menuTypes = menuManager.GetNumericMenuTypes();
            List<MyPair<Type, int>> items = new List<MyPair<Type, int>>();;

            if (keyNum == 0)
            {
                keyNum = 10;
            }
            if (menuTypes.Length >= keyNum)
            {
                gameManager.ProduceFromMenu(menuTypes[keyNum - 1]);
            }
        }

        internal static void OnMoveButtonPress()
        {
        }
    }
}
