using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

public enum Sounds
{
    Master, BGM, SFX
}

[Serializable]
public class SoundInfo
{
    public Sounds type;
    public AudioClip clip;
}

public class SoundManager : MonoBehaviour
{
    #region public
    public static SoundManager Instance;
    #endregion

    #region private
    [SerializeField] List<SoundInfo> _soundInfos;
    [SerializeField] AudioMixer _mixer;
    private Dictionary<string, AudioSource> _audioSourceDic = new Dictionary<string, AudioSource>();
    #endregion

    void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);

        Init();
    }

    void Init()
    {
        for (int i = 0; i < _soundInfos.Count; i++) {
            GameObject obj = new GameObject(_soundInfos[i].clip.name);
            obj.transform.SetParent(this.transform);

            AudioSource source = obj.AddComponent<AudioSource>();
            source.clip = _soundInfos[i].clip;

            if (_soundInfos[i].type == Sounds.BGM) {
                source.loop = true;
                source.playOnAwake = true;
            }
            else
                source.playOnAwake = false;

            var targetGroups = _mixer.FindMatchingGroups(_soundInfos[i].type.ToString());

            if (targetGroups.Length > 0) {
                source.outputAudioMixerGroup = targetGroups[0];
            }
            else {
                Debug.LogError($"오디오 믹서에서 '{_soundInfos[i].type.ToString()}' 그룹을 찾을 수 없습니다!");
            }

            if (!_audioSourceDic.ContainsKey(_soundInfos[i].clip.name))
                _audioSourceDic.Add(_soundInfos[i].clip.name, source);
        }
    }

    public void PlayBGM(string bgmName)
    {
        if (_audioSourceDic.TryGetValue(bgmName, out AudioSource source)) {           
            if (source.isPlaying) 
                return;

            source.Play();
        }
        else
            Debug.LogWarning($"재생하려는 BGM이 없습니다: {bgmName}");
    }

    public void PlaySFX(string sfxName)
    {
        if (_audioSourceDic.TryGetValue(sfxName, out AudioSource source)) {
            source.PlayOneShot(source.clip);
        }
        else
            Debug.LogWarning($"재생하려는 SFX가 없습니다: {sfxName}");
    }
}
