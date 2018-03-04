using System.Collections.Generic;

public interface Plan {

    List<Order> GetPlanSteps(RTSGameObject unit);
    //void PreparePlan();

}
