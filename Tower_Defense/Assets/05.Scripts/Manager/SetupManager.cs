using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static UnityEngine.InputSystem.Controls.AxisControl;
public class SetupManager : MonoBehaviour
{
    public static SetupManager _instance;
    public static SetupManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = Instantiate(Resources.Load<SetupManager>("SetupManager"));

            return _instance;
        }
    }

    [SerializeField] Slider _masterVolumeSetup;
    [SerializeField] Slider _bgmVolumeSetup;
    [SerializeField] Slider _sfxVolumeSetup;
    [SerializeField] AudioMixer _mixer;

    private void Awake()
    {
        if (Instance == this)
            DontDestroyOnLoad(this.gameObject);
        else
            Destroy(this.gameObject);
    }

    private void Start()
    {
        Setup();
    }

    void Setup()
    {
        _masterVolumeSetup.onValueChanged.RemoveAllListeners();
        _bgmVolumeSetup.onValueChanged.RemoveAllListeners();
        _sfxVolumeSetup.onValueChanged.RemoveAllListeners();

        //나중에는 싹 다 PlayerPref으로 바꿀거임
        if (_mixer.GetFloat("Master", out float masterValue)) 
            _masterVolumeSetup.value = Mathf.Pow(10, masterValue / 20f);

        if (_mixer.GetFloat("BGM", out float bgmValue)) 
            _bgmVolumeSetup.value = Mathf.Pow(10, bgmValue / 20f);

        if (_mixer.GetFloat("SFX", out float sfxValue)) 
            _sfxVolumeSetup.value = Mathf.Pow(10, sfxValue / 20f);

        _masterVolumeSetup.onValueChanged.AddListener(SetMasterVolume);
        _bgmVolumeSetup.onValueChanged.AddListener(SetBGMVolume);
        _sfxVolumeSetup.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMasterVolume(float value)
    {           
        float clampedValue = Mathf.Clamp(value, 0.0001f, 1f);
        float decibel = Mathf.Log10(clampedValue) * 20f;
        _mixer.SetFloat("Master", decibel);
    }

    public void SetBGMVolume(float value)
    {
        float clampedValue = Mathf.Clamp(value, 0.0001f, 1f);
        float decibel = Mathf.Log10(clampedValue) * 20f;
        _mixer.SetFloat("BGM", decibel);
    }

    public void SetSFXVolume(float value)
    {
        float clampedValue = Mathf.Clamp(value, 0.0001f, 1f);
        float decibel = Mathf.Log10(clampedValue) * 20f;
        _mixer.SetFloat("SFX", decibel);
    }

    public void GameQuit()
    {
        GameManager.Instance.GameQuit();
    }

    public void DifficultySelect()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GoToDifficultySelect();

        gameObject.SetActive(false);
    }
}
