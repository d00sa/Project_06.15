using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

public class LoadSceneManager : MonoBehaviour
{
    public static LoadSceneManager _instance;
    public static LoadSceneManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = Instantiate(Resources.Load<LoadSceneManager>("SceneManager"));
            return _instance;
        }
    }
    [SerializeField] private CanvasGroup _blackPanel;
    [SerializeField] private Slider _loading_Bar;
    [SerializeField] private TMP_Text _percentage;
    [SerializeField] private GameObject _loadingUI;

    private string _loadSceneName;

    private void Awake()
    {
        if (Instance == this) 
            DontDestroyOnLoad(this.gameObject);
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        _loading_Bar.onValueChanged.AddListener(OnSliderChanged);
        OnSliderChanged(_loading_Bar.value);
    }

    public void LoadScene(string sceneName)
    {
        gameObject.SetActive(true);
        _loadSceneName = sceneName;
        SceneManager.sceneLoaded += OnSceneLoaded;

        StartCoroutine(LoadProcess());
    }

    public void Test()
    {
        gameObject.SetActive(true);
        StartCoroutine(Fade(true));
    }

    private IEnumerator LoadProcess()
    {       
        yield return StartCoroutine(Fade(true));

        // 2. 로딩 UI 표시
        _loadingUI.SetActive(true);

        // 3. 실제 씬 로딩
        yield return StartCoroutine(LoadSceneProcess());
    }

    private IEnumerator LoadSceneProcess()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(_loadSceneName);
        
        op.allowSceneActivation = false;        

        float timer = 0f;
        //씬 구성이 다 끝날 때 까지 반복.
        while (!op.isDone) {
            yield return null;
            if (op.progress < 0.9f)
                _loading_Bar.value = op.progress * 100f;
            else {
                timer += Time.unscaledDeltaTime;
                _loading_Bar.value = Mathf.Clamp(_loading_Bar.value + timer, 0f, 100f);
                if (_loading_Bar.value >= 100f) {
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (arg0.name == _loadSceneName) {
            StartCoroutine(Fade(false));
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private IEnumerator Fade(bool isFade)
    {
        //검은 화면이 사라지게 요청을 받았을 때 로딩창을 없애기.
        if (!isFade)
            _loadingUI.gameObject.SetActive(false);

        float timer = 0f;
        while (timer <= 1f) {
            yield return null;
            timer += Time.unscaledDeltaTime * 0.8f;
            _blackPanel.alpha = isFade ? Mathf.Clamp(timer, 0f, 1f) : Mathf.Clamp(1 - timer, 0f, 1f);
        }

        //검은 화면이 다 사라지면 오브젝트 끄기
        if (!isFade)
            gameObject.SetActive(false);
        //검은 화면이 다 생기면 로딩창 키기
        else
            _loadingUI.gameObject.SetActive(true);
    }

    private void OnSliderChanged(float value)
    {
        int percent = Mathf.RoundToInt(value);

        _percentage.text = $"{percent}%";
    }
}