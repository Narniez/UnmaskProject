using UnityEngine;

public class AudioTester : MonoBehaviour
{
    [SerializeField] private AudioClip[] audioClip;

    public void PlayAudio()
    {
        SoundManager.instance.PlayRandomSoundClip(audioClip, transform, 1);
    }
}
