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

[Serializable]
public class SoundCategory
{
    public string categoryName;
    public List<SoundInfo> soundInfos = new List<SoundInfo>();
}

public class SoundManager : MonoBehaviour
{
    #region public
    public static SoundManager Instance;
    #endregion

    #region private
    [Header("사운드 목록 (구역별 관리)")]
    [SerializeField] List<SoundCategory> _soundCategories = new List<SoundCategory>();

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
        foreach (var category in _soundCategories)
        {
            foreach (var info in category.soundInfos)
            {
                // 오디오 클립이 비어있으면 에러가 나므로 건너뛰기 방어 코드 추가
                if (info.clip == null) continue;

                GameObject obj = new GameObject(info.clip.name);
                obj.transform.SetParent(this.transform);

                AudioSource source = obj.AddComponent<AudioSource>();
                source.clip = info.clip;

                if (info.type == Sounds.BGM)
                {
                    source.loop = true;
                    source.playOnAwake = true;
                }
                else
                    source.playOnAwake = false;

                var targetGroups = _mixer.FindMatchingGroups(info.type.ToString());

                if (targetGroups.Length > 0)
                {
                    source.outputAudioMixerGroup = targetGroups[0];
                }
                else
                {
                    Debug.LogError($"오디오 믹서에서 '{info.type.ToString()}' 그룹을 찾을 수 없습니다!");
                }

                if (!_audioSourceDic.ContainsKey(info.clip.name))
                    _audioSourceDic.Add(info.clip.name, source);
            }
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
            source.loop = false;
            source.PlayOneShot(source.clip);
        }
        else
            Debug.LogWarning($"재생하려는 SFX가 없습니다: {sfxName}");
    }

    public void PlaySFX(string sfxName, float volume)
    {
        if (_audioSourceDic.TryGetValue(sfxName, out AudioSource source))
        {
            source.loop = false;
            source.PlayOneShot(source.clip, volume);
        }
        else
            Debug.LogWarning($"재생하려는 SFX가 없습니다: {sfxName}");
    }

    /// <summary>효과음 루프 재생 </summary>
    public void PlayLoopSFX(string sfxName)
    {
        if (_audioSourceDic.TryGetValue(sfxName, out AudioSource source))
        {
            source.loop = true;
            if (!source.isPlaying)
                source.Play();
        }
        else
            Debug.LogWarning($"재생하려는 Loop SFX가 없습니다: {sfxName}");
    }

    public void PlayLoopSFX(string sfxName, float volume)
    {
        if (_audioSourceDic.TryGetValue(sfxName, out AudioSource source))
        {
            source.loop = true;
            source.volume = Mathf.Clamp01(volume);

            if (!source.isPlaying)
                source.Play();
        }
        else
            Debug.LogWarning($"재생하려는 Loop SFX가 없습니다: {sfxName}");
    }

    /// <summary> 특정 사운드 재생 중단 </summary>
    public void StopSound(string soundName)
    {
        if (_audioSourceDic.TryGetValue(soundName, out AudioSource source))
            source.Stop();
        else
            Debug.LogWarning($"중단하려는 사운드가 없습니다: {soundName}");
    }

    /// <summary> 재생 중인 모든 사운드 일괄 중단 </summary>
    public void StopAllSounds()
    {
        foreach (var source in _audioSourceDic.Values)
            source.Stop();
    }
}
