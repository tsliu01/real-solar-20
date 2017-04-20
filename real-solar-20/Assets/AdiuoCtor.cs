using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdiuoCtor : MonoBehaviour {
    private  AudioSource MyAudio;
    public AudioClip[] AudioClip;
    //public string[] SongNames;
    private int SongIndex = 0;
    //bool LoopPlay = true;
    private GameObject obj;
    
    // Use this for initialization
    void Start () {
        MyAudio = this.GetComponent<AudioSource>();
      // obj= this.gameObject.GetComponent<CursorActivity>().selectedIcon;
	}
	
	// Update is called once per frame
	void Update () {
       
    }
    public void CameraAudio()
    {
        MyAudio.PlayOneShot(AudioClip[2]);
    }
    public void PromptAudio()
    {
        MyAudio.PlayOneShot(AudioClip[1]);
    }
    public void ShallowAudio()
    {
        MyAudio.PlayOneShot(AudioClip[0]);
    }
}
