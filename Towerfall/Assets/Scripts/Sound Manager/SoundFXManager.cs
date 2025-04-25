using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
   public static SoundFXManager instance;

   [SerializeField] private AudioSource soundFXObject;

   private void Awake (){
    if(instance == null){
        instance = this;
    }
   }

   public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume){

        //spawn sound object
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        //assign audioClip
        audioSource.clip = audioClip;

        //assign volume
        audioSource.volume = volume;

        //play sound
        audioSource.Play();

        //get length of sound FX Clip
        float clipLength = audioSource.clip.length;

        //destroy sound object after it is done playing
        Destroy(audioSource.gameObject, clipLength);

   }

}
