using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour
{

    public AudioClip sound;
    AudioSource audio;
    private bool _isTriger;
    // Start is called before the first frame update
    void Start()
    {
        audio = gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_isTriger)
        {
            audio.volume = Mathf.Lerp(audio.volume, 1, Time.deltaTime);
        }

        if (!_isTriger)
        {
            audio.volume = Mathf.Lerp(audio.volume, 0, Time.deltaTime);
            if(audio.volume <= 0.1f)
            {
                audio.Stop();
            }
        }



    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
                _isTriger = true;
                audio.PlayOneShot(sound);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isTriger = false;

        }
    }

}
