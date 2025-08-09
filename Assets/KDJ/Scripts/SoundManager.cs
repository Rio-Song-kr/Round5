using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.VisualScripting;
using Photon.Pun;
using Photon.Realtime;

public class SoundManager : MonoBehaviourPun
{
    [Serializable]
    public struct SoundList
    {
        public string Name;
        public AudioClip Clip;
    }

    [SerializeField] private List<SoundList> _bgmList = new List<SoundList>();
    [SerializeField] private List<SoundList> _sfxList = new List<SoundList>();
    [SerializeField] private AudioSource BGMPlayer;
    [SerializeField] private AudioSource SFXPlayer;
    [SerializeField] private AudioSource LoopPlayer;
    [SerializeField] private Dictionary<string, AudioClip> BGMDic = new Dictionary<string, AudioClip>();
    [SerializeField] private Dictionary<string, AudioClip> SFXDic = new Dictionary<string, AudioClip>();

    public static SoundManager Instance;

    private void Awake()
    {
        Init();
        SetBGMVolume(PlayerPrefs.GetFloat("BGMVolume", 1f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 1f));
    }

    private void Init()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        ListInit();
    }

    /// <summary>
    /// 실행 시 넣어놓은 BGM과 SFX를 Dictionary에 초기화합니다.
    /// </summary>
    private void ListInit()
    {
        foreach (var sound in _bgmList)
        {
            if (!BGMDic.ContainsKey(sound.Name))
            {
                BGMDic.Add(sound.Name, sound.Clip);
            }
        }

        foreach (var sound in _sfxList)
        {
            if (!SFXDic.ContainsKey(sound.Name))
            {
                SFXDic.Add(sound.Name, sound.Clip);
            }
        }
    }

    /// <summary>
    /// BGM을 한번만 재생합니다.
    /// BGM이 재생 중일 경우, 새로 지정한 BGM으로 교체합니다.
    /// </summary>
    /// <param name="name">리스트에 있는 BGM의 이름</param>
    public void PlayBGMOnce(string name)
    {
        if (BGMDic.ContainsKey(name))
        {
            BGMPlayer.clip = BGMDic[name];
            LoopPlayer.Stop();
            BGMPlayer.Play();
        }
    }

    /// <summary>
    /// RPC용 오버라이드 메서드 입니다. 뒤의 int값은 아무거나 넣으면 됩니다.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void PlayBGMOnce(string name, int value)
    {
        photonView.RPC("RPC_PlayBGMOnce", RpcTarget.All, name);
    }

    /// <summary>
    /// RPC를 통해 BGM을 한번만 재생합니다.
    /// </summary>
    /// <param name="name"></param>
    [PunRPC]
    public void RPC_PlayBGMOnce(string name)
    {
        if (BGMDic.ContainsKey(name))
        {
            BGMPlayer.clip = BGMDic[name];
            LoopPlayer.Stop();
            BGMPlayer.Play();
        }
    }

    /// <summary>
    /// BGM을 반복 재생합니다.
    /// </summary>
    /// <param name="name"></param>
    public void PlayBGMLoop(string name)
    {
        if (BGMDic.ContainsKey(name))
        {
            LoopPlayer.clip = BGMDic[name];
            BGMPlayer.Stop();
            LoopPlayer.Play();
        }
    }

    /// <summary>
    /// RPC용 오버라이드 메서드 입니다. 뒤의 int값은 아무거나 넣으면 됩니다.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void PlayBGMLoop(string name, int value)
    {
        photonView.RPC("RPC_PlayBGMLoop", RpcTarget.All, name);
    }

    /// <summary>
    /// RPC를 통해 BGM을 반복 재생합니다.
    /// </summary>
    /// <param name="name"></param>
    [PunRPC]
    public void RPC_PlayBGMLoop(string name)
    {
        if (BGMDic.ContainsKey(name))
        {
            LoopPlayer.clip = BGMDic[name];
            BGMPlayer.Stop();
            LoopPlayer.Play();
        }
    }

    /// <summary>
    /// 메인 메뉴 BGM을 재생합니다.
    /// Loop를 위해 2개의 AudioSource를 사용합니다.
    /// </summary>
    public void PlayMainMenuBGM()
    {
        BGMPlayer.clip = BGMDic["MainMenuStart"];
        BGMPlayer.Play();
        double introLength = AudioSettings.dspTime + BGMPlayer.clip.length;
        LoopPlayer.clip = BGMDic["MainMenuLoop"];
        LoopPlayer.PlayScheduled(introLength);
    }

    /// <summary>
    /// SFX를 재생합니다.
    /// </summary>
    /// <param name="name">리스트에 있는 SFX의 이름</param>
    public void PlaySFX(string name)
    {
        if (SFXDic.ContainsKey(name))
        {
            SFXPlayer.PlayOneShot(SFXDic[name]);
        }
    }

    /// <summary>
    /// RPC를 통해 SFX를 재생합니다.
    /// </summary>
    /// <param name="name"></param>
    [PunRPC]
    public void RPC_PlaySFX(string name)
    {
        if (SFXDic.ContainsKey(name))
        {
            SFXPlayer.PlayOneShot(SFXDic[name]);
        }
    }

    /// <summary>
    /// SFX를 정지합니다.
    /// </summary>
    public void StopSFX()
    {
        SFXPlayer.Stop();
    }

    /// <summary>
    /// BGM을 정지합니다.
    /// </summary>
    public void StopBGM()
    {
        BGMPlayer.Stop();
    }

    /// <summary>
    /// BGM의 볼륨을 설정합니다.
    /// </summary>
    /// <param name="volume">BGM의 볼륨</param>
    public void SetBGMVolume(float volume)
    {
        BGMPlayer.volume = volume;
        LoopPlayer.volume = volume; // LoopPlayer의 볼륨도 동일하게 설정
    }

    /// <summary>
    /// SFX의 볼륨을 설정합니다.
    /// </summary>
    /// <param name="volume">SFX의 볼륨</param>
    public void SetSFXVolume(float volume)
    {
        SFXPlayer.volume = volume;
    }
}
