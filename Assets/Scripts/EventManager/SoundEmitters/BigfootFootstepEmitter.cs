using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigfootFootstepEmitter : MonoBehaviour
{
    public void ExecuteFootstep()
    {
        EventManager.TriggerEvent<BigfootFootstepEvent, Vector3>(transform.position);
    }
}
