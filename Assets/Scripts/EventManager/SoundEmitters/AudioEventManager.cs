using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AudioEventManager : MonoBehaviour
{
    public EventSound3D eventSound3DPrefab;
    public AudioClip bigfootFootstepAudio;
    public AudioClip radioAudio;
    public AudioClip dropAudio;
    public AudioClip pickUpAudio;

    private UnityAction<Vector3> bigfootFootstepEventListener;
    private UnityAction<GameObject> radioAudioEventListener;
    private UnityAction<GameObject> dropAudioEventListener;
    private UnityAction<GameObject> pickUpAudioEventListener;

    // Start is called before the first frame update
    void Start()
    {
        bigfootFootstepEventListener = new UnityAction<Vector3>(bigfootFootstepEventHandler);
        radioAudioEventListener = new UnityAction<GameObject>(radioAudioEventHandler);
        dropAudioEventListener = new UnityAction<GameObject>(dropAudioEventHandler);
        pickUpAudioEventListener = new UnityAction<GameObject>(pickUpAudioEventHandler);

        EventManager.StartListening<BigfootFootstepEvent, Vector3>(bigfootFootstepEventListener);
        EventManager.StartListening<RadioEvent, GameObject>(radioAudioEventListener);
        EventManager.StartListening<DropAudioEvent, GameObject>(dropAudioEventListener);
        EventManager.StartListening<PickUpAudioEvent, GameObject>(pickUpAudioEventListener);
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

    void radioAudioEventHandler(GameObject obj)
    {
        EventSound3D snd = Instantiate(eventSound3DPrefab, obj.transform);

        snd.audioSrc.clip = this.radioAudio;

        snd.audioSrc.minDistance = 5f;
        snd.audioSrc.maxDistance = 100f;
        snd.audioSrc.volume = 0.8f;

        snd.audioSrc.Play();
    }

    void dropAudioEventHandler(GameObject obj)
    {
        EventSound3D snd = Instantiate(eventSound3DPrefab, obj.transform);

        snd.audioSrc.clip = this.dropAudio;

        snd.audioSrc.minDistance = 5f;
        snd.audioSrc.maxDistance = 100f;
        snd.audioSrc.volume = 0.8f;

        snd.audioSrc.Play();
    }

    void pickUpAudioEventHandler(GameObject obj)
    {
        EventSound3D snd = Instantiate(eventSound3DPrefab, obj.transform);

        snd.audioSrc.clip = this.pickUpAudio;

        snd.audioSrc.minDistance = 5f;
        snd.audioSrc.maxDistance = 100f;
        snd.audioSrc.volume = 0.8f;

        snd.audioSrc.Play();
    }
}
