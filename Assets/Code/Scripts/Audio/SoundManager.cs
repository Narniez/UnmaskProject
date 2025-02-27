using Unity.VisualScripting;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] private AudioSource soundObject;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public void PlaySoundClip(AudioClip audioClip, Transform spawnPosition, float volume)
    {
        //spawn in game object
        AudioSource audioSource = Instantiate(soundObject, spawnPosition.position, Quaternion.identity);

        //assignt the clip
        audioSource.clip = audioClip;

        //assign volume
        audioSource.volume = volume;

        //play the clip
        audioSource.Play();

        //get the length
        float clipLength = audioSource.clip.length;

        //destroy game object
        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlayRandomSoundClip(AudioClip[] audioClip, Transform spawnPosition, float volume)
    {

        //get random index 
        int random = Random.Range(0, audioClip.Length);

        //spawn in game object
        AudioSource audioSource = Instantiate(soundObject, spawnPosition.position, Quaternion.identity);

        //assignt the clip
        audioSource.clip = audioClip[random];

        //assign volume
        audioSource.volume = volume;

        //play the clip
        audioSource.Play();

        //get the length
        float clipLength = audioSource.clip.length;

        //destroy game object
        Destroy(audioSource.gameObject, clipLength);
    }

}
