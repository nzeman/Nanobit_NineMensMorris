using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnReturnedToMainMenu : MonoBehaviour
{
    void Start()
    {
        AudioManager.Instance.StopGameMusic();
        AudioManager.Instance.PlayMainMenuMusic(AudioManager.Instance.GetAudioData().mainMenuMusic);
    }

}
