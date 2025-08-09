using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class VolumePanel : MonoBehaviour
{
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;

    private void Start()
    {
        // 슬라이더 값 변경 리스너 등록
        _masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        _bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        _sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        _masterSlider.value = GameManager.Instance.MasterVolume * 100f;
        _bgmSlider.value = GameManager.Instance.BGMVolume * 100f;
        _sfxSlider.value = GameManager.Instance.SFXVolume * 100f;
    }

    void OnDisable()
    {
        PlayerPrefs.SetFloat("MasterVolume", GameManager.Instance.MasterVolume);
        PlayerPrefs.SetFloat("BGMVolume", GameManager.Instance.BGMVolume);
        PlayerPrefs.SetFloat("SFXVolume", GameManager.Instance.SFXVolume);
        PlayerPrefs.Save();
    }

    private void UpdateBGMVolume()
    {
        float volume = GameManager.Instance.BGMVolume * GameManager.Instance.MasterVolume;
        SoundManager.Instance.SetBGMVolume(volume);
    }

    private void UpdateSFXVolume()
    {
        float volume = GameManager.Instance.SFXVolume * GameManager.Instance.MasterVolume;
        SoundManager.Instance.SetSFXVolume(volume);
    }

    private void OnMasterVolumeChanged(float value)
    {
        GameManager.Instance.MasterVolume = value / 100f;
        UpdateBGMVolume();
        UpdateSFXVolume();
    }

    private void OnBGMVolumeChanged(float value)
    {
        GameManager.Instance.BGMVolume = value / 100f;
        UpdateBGMVolume();
    }

    private void OnSFXVolumeChanged(float value)
    {
        GameManager.Instance.SFXVolume = value / 100f;
        UpdateSFXVolume();
    }
}
