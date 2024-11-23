using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;


public class Npc {
    public GameObject NpcObject;
    public GameObject SpoofEffect;
    public GameObject BonaFideEffect;
    
    public Npc(GameObject NpcObject, GameObject SpoofEffect, GameObject BonaFideEffect) {
        this.NpcObject = NpcObject;
        this.SpoofEffect = SpoofEffect;
        this.BonaFideEffect = BonaFideEffect;
    }
}


public class Level {
    private int RandomAudioIndex;
    private AudioClip ABF_Audio;
    private AudioClip AFB_Audio;
    private Npc Npc1;
    private Npc Npc2;
    public string Name;
    private GameObject ScoreBoardTitle = GameObject.Find("ScoreBoardTitle");
    
    public Level(string Name, AudioClip ABF_Audio, AudioClip AFB_Audio, Npc Npc1, Npc Npc2) {
        this.Name = Name;
        this.ABF_Audio = ABF_Audio;
        this.AFB_Audio = AFB_Audio;
        this.Npc1 = Npc1;
        this.Npc2 = Npc2;
    }
    
    private void RandomAudio() {
        Random.InitState(System.DateTime.Now.Millisecond);
        RandomAudioIndex = Random.Range(0, 2);
    }
    
    public void InitScene() {
        RandomAudio();
        Debug.Log("Level: " + Name + " RandomAudioIndex: " + RandomAudioIndex);
        Npc1.SpoofEffect.SetActive(false);
        Npc1.BonaFideEffect.SetActive(false);
        Npc2.SpoofEffect.SetActive(false);
        Npc2.BonaFideEffect.SetActive(false);
        ScoreBoardTitle.SetActive(false);
    }
    
    public AudioClip GetAudio() {
        if (RandomAudioIndex == 0) {
            return AFB_Audio;
        } else {
            return ABF_Audio;
        }
    }
    
    public bool ShowAnswer(int ClickedNpcIndex) {
        if (RandomAudioIndex == 0) {
            Npc1.SpoofEffect.SetActive(true);
            Npc2.BonaFideEffect.SetActive(true);
        } else {
            Npc1.BonaFideEffect.SetActive(true);
            Npc2.SpoofEffect.SetActive(true);
        }
        if (RandomAudioIndex == ClickedNpcIndex) {
            ScoreBoardTitle.GetComponent<Image>().sprite = Resources.Load<Sprite>("ScoreBoard/score_success");
        } else {
            ScoreBoardTitle.GetComponent<Image>().sprite = Resources.Load<Sprite>("ScoreBoard/score_fail");
        }
        ScoreBoardTitle.SetActive(true);
        return RandomAudioIndex == ClickedNpcIndex;
    }
}


public class PlayBar {
    private GameObject Point;
    private readonly GameObject PlayButton;
    private readonly Sprite PlayImage;
    private readonly Sprite StopImage;
    private readonly int PointStart = -700;
    private readonly int PointEnd = 700;
    private readonly int ProcessBarLength = 1400;
    private AudioSource Audio;
    private bool isTempPause = false;
    private bool isDragging = false;
    private bool alreadyPlayed = false;
    
    public PlayBar(AudioSource Audio, GameObject PlayButton, Sprite PlayImage, Sprite StopImage) {
        this.Audio = Audio;
        this.PlayButton = PlayButton;
        this.PlayImage = PlayImage;
        this.StopImage = StopImage;
        this.Point = GameObject.Find("Point");
        PlayButton.GetComponent<Button>().onClick.AddListener(PlayButtonOnClick);
    }
    
    public void PlayButtonOnClick() {
        if (Audio == null || Audio.clip == null) {
            Debug.LogError("AudioSource 或 Audio.clip 為 null。");
            return;
        }
        if (Audio.isPlaying) {
            Audio.Pause();
        } else {
            Audio.Play();
            alreadyPlayed = true;
        }
        UpdateButtonImage();
    }
    
    void UpdateButtonImage() {
        if (Audio.isPlaying) {
            PlayButton.GetComponent<Image>().sprite = StopImage;
        } else {
            PlayButton.GetComponent<Image>().sprite = PlayImage;
        }
    }
    
    public void SetAudio(AudioSource Audio) {
        this.Audio = Audio;
    }
    
    // 將 audio 的播放進度轉換為 point 的座標
    public void MapPointToAudio() {
        if (isDragging) return;
        float total_time = Audio.clip.length;
        float now_time = Audio.time;
        float point_x = now_time / total_time * 100 * ProcessBarLength / 100;
        Point.transform.localPosition = new Vector3(PointStart + point_x, 0, 0);
        UpdateButtonImage();
    }
    
    public void OnDrag(PointerEventData eventData) {
        isDragging = true;
        if (Audio.isPlaying) {
            Audio.Pause();
            isTempPause = true;
        }
        float x = Point.transform.localPosition.x + eventData.delta.x;
        if (x < PointStart) x = PointStart;
        if (x > PointEnd) x = PointEnd;
        Point.transform.localPosition = new Vector3(x, 0, 0);
    }
    
    public void OnEndDrag(PointerEventData eventData) {
        float total_time = Audio.clip.length;
        float point_x = Point.transform.localPosition.x - PointStart;
        float now_time = point_x / ProcessBarLength * total_time;
        
        // audio 未播放過，無法設定時間
        if (!alreadyPlayed) {
            Audio.Play();
            Audio.Pause();
            alreadyPlayed = true;
        }
        
        Audio.time = now_time;
        if (isTempPause) {
            Audio.Play();
            isTempPause = false;
        }
        isDragging = false;
    }
}


public class LevelController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler{
    public Npc Npc1;    // 日向翔陽
    public GameObject Npc1Model;
    public GameObject Npc1SpoofEffect;
    public GameObject Npc1BonaFideEffect;
    
    public Npc Npc2;    // 影山飛雄
    public GameObject Npc2Model;
    public GameObject Npc2SpoofEffect;
    public GameObject Npc2BonaFideEffect;
    
    private int CurrentLevel = 0;  // 目前關卡
    
    private Level[] LevelMap;               // 關卡清單
    private bool AleadyAnswered = false;    // 已回答過
    
    private GameObject PlayButton;  // 播放按鈕
    private GameObject ExitButton;  // 離開按鈕
    
    private AudioSource NowAudio;
    
    public Sprite PlayImage;
    public Sprite StopImage;
    
    public Scene SuccessTitle;
    public Scene FailTitle;
    
    private PlayBar PlayBar1;
    
    void Start() {
        NowAudio = GetComponent<AudioSource>();
        PlayButton = GameObject.Find("PlayButton");
        ExitButton = GameObject.Find("ExitButton");
        
        // 初始化 Npc1 和 Npc2
        Npc1 = new Npc(Npc1Model, Npc1SpoofEffect, Npc1BonaFideEffect);
        Npc2 = new Npc(Npc2Model, Npc2SpoofEffect, Npc2BonaFideEffect);
        
        LevelMap = new Level[] {
            new("第 1 話「終わりと始まり」",
                Resources.Load<AudioClip>("Audio/1/ABF"),
                Resources.Load<AudioClip>("Audio/1/AFB"),
                Npc1,
                Npc2),
            new("第 2 話「烏野高校排球部」",
                Resources.Load<AudioClip>("Audio/2/ABF"),
                Resources.Load<AudioClip>("Audio/2/AFB"),
                Npc1,
                Npc2),
            new("第 3 話「最強の味方」",
                Resources.Load<AudioClip>("Audio/3/ABF"),
                Resources.Load<AudioClip>("Audio/3/AFB"),
                Npc1,
                Npc2),
        };
        
        PlayBar1 = new PlayBar(NowAudio, PlayButton, PlayImage, StopImage);
        UpdateNowAudio();
        
        if (NowAudio.clip == null) {
            NowAudio.clip = LevelMap[CurrentLevel].GetAudio();
            if (NowAudio.clip == null) {
                Debug.LogError("Audio clip 未正確載入。請檢查 Resources 路徑。");
            }
        }
        
        ExitButton.GetComponent<Button>().onClick.AddListener(ExitButtonOnClick);
        
        LevelMap[CurrentLevel].InitScene();
    }
    
    void Update() {
        // update audio point
        PlayBar1.MapPointToAudio();
        NpcOnClickHandler();
    }
    
    private void UpdateNowAudio() {
        NowAudio.clip = LevelMap[CurrentLevel].GetAudio();
        PlayBar1.SetAudio(NowAudio);
    }
    
    // 延遲換場景
    private void DelayChangeLevel() {
        UpdateNowAudio();
        LevelMap[CurrentLevel].InitScene();
        AleadyAnswered = false;
        PlayButton.GetComponent<Button>().interactable = true;
    }
    
    private void NpcOnClickHandler() {
        if (AleadyAnswered) return;
        
        bool isCurrent = false;
        GameObject[] NpcModels = {Npc1.NpcObject, Npc2.NpcObject};
        
        Ray ray = new();    // 射線
        
        // phone mode
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
            ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
        }
        // pc mode
        if (Input.GetMouseButtonDown(0)) {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }
        
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            for (int i = 0; i < NpcModels.Length; i++) {
                if (hit.collider.gameObject == NpcModels[i]) {
                    Debug.Log("Npc " + i + " clicked.");
                    
                    isCurrent = LevelMap[CurrentLevel].ShowAnswer(i);
                    AleadyAnswered = true;
                    PlayButton.GetComponent<Button>().interactable = false;
                    
                    // next level
                    if (isCurrent) {
                        CurrentLevel++;
                        Debug.Log("CurrentLevel: " + CurrentLevel);
                        if (CurrentLevel >= LevelMap.Length) {
                            // StartCoroutine(DelayChangeScene("SuccessTitle")); // 延遲換場景
                        } else {
                            Invoke("DelayChangeLevel", 5f);
                        }
                    }
                }
            }
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData) { }
    
    public void OnDrag(PointerEventData eventData) {
        if (AleadyAnswered) return;
        PlayBar1.OnDrag(eventData);
    }
    
    public void OnEndDrag(PointerEventData eventData) {
        if (AleadyAnswered) return;
        PlayBar1.OnEndDrag(eventData);
    }
    
    private void ExitButtonOnClick() {
        SceneManager.LoadScene("MainMenu");
    }
}
