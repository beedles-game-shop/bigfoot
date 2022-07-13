using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioEmitter : MonoBehaviour
{
    void PlayRadio()
    {
        EventManager.TriggerEvent<BigfootFootstepEvent, Vector3>(transform.position);
    }
}
