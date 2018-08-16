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

        public static void IssueCommand(int orderTypeId, bool overrideDefaultOrderData = false)
        {
            Command command = new Command() { getOrder = (OrderBuilderFunction)orderTypeId, overrideDefaultOrderData = overrideDefaultOrderData };
            
            command.queueOrderInsteadOfClearing = Input.GetKey(Setting.queueOrderInsteadOfClearing);
            command.queueOrderAtFront = Input.GetKey(Setting.addOrderToFrontOfQueue);
            
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
                RTSGameObject objectClicked = GetClickedRTSGameObject();
                selectionManager.CheckSingleSelectionEvent(objectClicked);
            }
            else
            {
                selectionManager.CheckBoxSelectionEvent();
            }
            selectionManager.menuClicked = false;

            // For now everything is smart cast, Once that changes, regular cast abilities will go here.
        }
        internal static RTSGameObject GetClickedRTSGameObject()
        {
            RTSGameObject objectClicked = null;
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool rayCast = Physics.Raycast(ray, out hit);

            if (rayCast)
            {
                objectClicked = hit.collider.GetComponent<RTSGameObject>();
                if (objectClicked == null)
                {
                    objectClicked = hit.collider.GetComponentInParent<RTSGameObject>();
                }
            }
            return objectClicked;
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

        internal static void OnMoveButtonRelease()
        {
            RTSGameObject clickedObject = GetClickedRTSGameObject();
            if (clickedObject != null && clickedObject is Structure && clickedObject.GetComponent<ConstructionInfo>() != null)
            {
                List<RTSGameObject> selectedUnits = gameManager.playerManager.GetOrderableSelectedUnits();
                if (!selectedUnits.Any(x => x.GetComponent<Worker>() == null))
                {
                    IssueCommand((int)OrderBuilderFunction.NewResumeConstructionOrder, true);
                }
            }
            else if (clickedObject != null)
            {
                List<RTSGameObject> selectedUnits = gameManager.playerManager.GetOrderableSelectedUnits();
                if (!selectedUnits.Any(x => x.GetComponent<Mover>() == null))
                {
                    IssueCommand((int)OrderBuilderFunction.NewJoinOrder, true);
                }
            }
            else
            {
                IssueCommand((int)OrderBuilderFunction.NewMoveOrder);
            }
        }
    }
}
