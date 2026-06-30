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
            if (_instance == null) {
                _instance = Instantiate(Resources.Load<SetupManager>("SetupManager"));
            }

            return _instance;
        }
    }

    [SerializeField] Slider _masterVolumeSetup;
    [SerializeField] Slider _bgmVolumeSetup;
    [SerializeField] Slider _sfxVolumeSetup;
    [SerializeField] AudioMixer _mixer;

    private void Awake()
    {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
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
            _masterVolumeSetup.value = Mathf.Pow(10, masterValue / 40f);

        if (_mixer.GetFloat("BGM", out float bgmValue)) 
            _bgmVolumeSetup.value = Mathf.Pow(10, bgmValue / 40f);

        if (_mixer.GetFloat("SFX", out float sfxValue))
            _sfxVolumeSetup.value = Mathf.Pow(10, sfxValue / 40f);

        _masterVolumeSetup.onValueChanged.AddListener(SetMasterVolume);
        _bgmVolumeSetup.onValueChanged.AddListener(SetBGMVolume);
        _sfxVolumeSetup.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMasterVolume(float value)
    {
        if (value <= 0f) {
            _mixer.SetFloat("Master", -80f);
        }
        else {
            float decibel = Mathf.Log10(value) * 40f;
            decibel = Mathf.Clamp(decibel, -80f, 0f);
            _mixer.SetFloat("Master", decibel);
        }
    }

    public void SetBGMVolume(float value)
    {
        if (value <= 0f) {
            _mixer.SetFloat("BGM", -80f);
        }
        else {
            float decibel = Mathf.Log10(value) * 40f;
            decibel = Mathf.Clamp(decibel, -80f, 0f);
            _mixer.SetFloat("BGM", decibel);
        }
    }

    public void SetSFXVolume(float value)
    {
        if (value <= 0f) {
            _mixer.SetFloat("SFX", -80f);
        }
        else {
            float decibel = Mathf.Log10(value) * 40f;
            decibel = Mathf.Clamp(decibel, -80f, 0f);
            _mixer.SetFloat("SFX", decibel);
        }
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

    public void Closed()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }
}
