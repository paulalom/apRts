using UnityEngine;
using System.Collections;

public class Ability : MyMonoBehaviour {

    public RTSGameObject target;
    public Vector3 targetPosition;
    public int range;
    public int cooldown;
    public bool isJoinable = false;

    public override string ToString()
    {
        return GetType().ToString() + "-" +
            "targ: " + target.name + "-" +
            "targPos: " + targetPosition.x + "," + targetPosition.y + "," + targetPosition.z + "-" +
            "range: " + range.ToString() + "-" +
            "cd: " + cooldown.ToString();
    }

    public string ToNetString()
    {
        return GetType().ToString() + "-" + 
            target.unitId.ToString() + "-" + 
            targetPosition.x + "," + targetPosition.y + "," + targetPosition.z + "-" + 
            range.ToString() + "-" + 
            cooldown.ToString();
    }

    public static Ability FromString(string abilityString)
    {
        string[] abilityComponents = abilityString.Split('-');
        string[] targetPositionComponents = abilityComponents[2].Split(',');
        Vector3 targetPosition = new Vector3();
        targetPosition.x = float.Parse(targetPositionComponents[0]);
        targetPosition.y = float.Parse(targetPositionComponents[1]);
        targetPosition.z = float.Parse(targetPositionComponents[2]);

        Ability ability = AbilityFactory.GetAbilityFromString(abilityComponents[0]);
        ability.target = GameObject.Find(abilityComponents[1]).GetComponent<RTSGameObject>();
        ability.targetPosition = targetPosition;
        ability.range = int.Parse(abilityComponents[3]);
        ability.cooldown = int.Parse(abilityComponents[4]);

        return ability;
    }
}
