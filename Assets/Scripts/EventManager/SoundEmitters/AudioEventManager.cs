using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AudioEventManager : MonoBehaviour
{
    public EventSound3D eventSound3DPrefab;
    public AudioClip bigfootFootstepAudio;

    private UnityAction<Vector3> bigfootFootstepEventListener;

    // Start is called before the first frame update
    void Start()
    {
        bigfootFootstepEventListener = new UnityAction<Vector3>(bigfootFootstepEventHandler);
        EventManager.StartListening<BigfootFootstepEvent, Vector3>(bigfootFootstepEventListener);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void bigfootFootstepEventHandler(Vector3 pos)
    {
        
        EventSound3D snd = Instantiate(eventSound3DPrefab, pos, Quaternion.identity, null);

        snd.audioSrc.clip = this.bigfootFootstepAudio;

        snd.audioSrc.minDistance = 5f;
        snd.audioSrc.maxDistance = 100f;
        snd.audioSrc.volume = 0.25f;

        snd.audioSrc.Play();
        
    }
}
