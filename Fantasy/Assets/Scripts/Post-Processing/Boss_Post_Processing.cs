using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class Boss_Post_Processing : MonoBehaviour
{
    public GameObject PostProc;
    private Volume _settings;
    private bool _isTriger;


    // Start is called before the first frame update
    void Start()
    {
        _settings = PostProc.GetComponent<Volume>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_isTriger)
        {

            VolumeProfile profile = _settings.sharedProfile;
            if (!profile.TryGet<ColorAdjustments>(out var _color))
            {
                _color = profile.Add<ColorAdjustments>(true);
            }
            _color.contrast.value = Mathf.Lerp(_color.contrast.value, -10, Time.deltaTime);
            _color.saturation.value = Mathf.Lerp(_color.saturation.value, -32, Time.deltaTime);
            _color.hueShift.value = Mathf.Lerp(_color.hueShift.value, 0, Time.deltaTime);


            if (!profile.TryGet<FilmGrain>(out var _film))
            {
                _film = profile.Add<FilmGrain>(true);
            }
            _film.intensity.value = Mathf.Lerp(_film.intensity.value, 0.63f, Time.deltaTime);
            _film.response.value = Mathf.Lerp(_film.response.value, 0.364f, Time.deltaTime);


            if (!profile.TryGet<Vignette>(out var _vig))
            {
                _vig = profile.Add<Vignette>(true);
            }
          _vig.smoothness.value = Mathf.Lerp(_vig.smoothness.value, 0.46f, Time.deltaTime);

        }
        if (!_isTriger)
        {

            VolumeProfile profile = _settings.sharedProfile;
            if (!profile.TryGet<ColorAdjustments>(out var _color))
            {
                _color = profile.Add<ColorAdjustments>(true);
            }
            _color.contrast.value = Mathf.Lerp(_color.contrast.value, 1 , Time.deltaTime);
            _color.saturation.value = Mathf.Lerp(_color.saturation.value, 71, Time.deltaTime);
            _color.hueShift.value = Mathf.Lerp(_color.hueShift.value, 10, Time.deltaTime);

            if (!profile.TryGet<FilmGrain>(out var _film))
            {
                _film = profile.Add<FilmGrain>(true);
            }
            _film.intensity.value = Mathf.Lerp(_film.intensity.value, 0, Time.deltaTime);
            _film.response.value = Mathf.Lerp(_film.response.value, 0, Time.deltaTime);


            if (!profile.TryGet<Vignette>(out var _vig))
            {
                _vig = profile.Add<Vignette>(true);
            }
            _vig.smoothness.value = Mathf.Lerp(_vig.smoothness.value, 0.146f, Time.deltaTime);
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isTriger = true;
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
