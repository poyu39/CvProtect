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
    
    public Level(string Name, AudioClip ABF_Audio, AudioClip AFB_Audio, Npc Npc1, Npc Npc2) {
        this.Name = Name;
        this.ABF_Audio = ABF_Audio;
        this.AFB_Audio = AFB_Audio;
        this.Npc1 = Npc1;
        this.Npc2 = Npc2;
        RandomAudio();
    }
    
    private void RandomAudio() {
        RandomAudioIndex = UnityEngine.Random.Range(0, 2);
    }
    
    public void InitScene() {
        Npc1.SpoofEffect.SetActive(false);
        Npc1.BonaFideEffect.SetActive(false);
        Npc2.SpoofEffect.SetActive(false);
        Npc2.BonaFideEffect.SetActive(false);
    }
    
    public AudioClip GetAudio() {
        if (RandomAudioIndex == 0) {
            return ABF_Audio;
        } else {
            return AFB_Audio;
        }
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
    bool isTempPause = false;
    bool isDragging = false;
    bool alreadyPlayed = false;
    
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
    
    // 關卡清單
    private Level[] LevelMap;
    
    private GameObject PlayButton;  // 播放按鈕
    private GameObject ExitButton;  // 離開按鈕
    
    private AudioSource NowAudio;
    
    public Sprite PlayImage;
    public Sprite StopImage;
    
    private GameObject ScoreBoardTitle;
    public Scene SuccessTitle;
    public Scene FailTitle;
    
    PlayBar PlayBar1;
    
    void UpdateNowAudio() {
        NowAudio.clip = LevelMap[CurrentLevel].GetAudio();
        PlayBar1.SetAudio(NowAudio);
    }
    
    void Start() {
        NowAudio = GetComponent<AudioSource>();
        PlayButton = GameObject.Find("PlayButton");
        ExitButton = GameObject.Find("ExitButton");
        
        ScoreBoardTitle = GameObject.Find("Title");
        
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
        // phone mode
        // if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
        //     Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
        //     RaycastHit hit;
        //     if (Physics.Raycast(ray, out hit)) {
        //         if (hit.collider.gameObject == Npc1) {
        //             NpcOnClick(Npc1);
        //         } else if (hit.collider.gameObject == Npc2) {
        //             NpcOnClick(Npc2);
        //         }
        //     }
        // }
        // // pc mode
        // if (Input.GetMouseButtonDown(0)) {
        //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //     RaycastHit hit;
        //     if (Physics.Raycast(ray, out hit)) {
        //         if (hit.collider.gameObject == Npc1) {
        //             NpcOnClick(Npc1);
        //         } else if (hit.collider.gameObject == Npc2) {
        //             NpcOnClick(Npc2);
        //         }
        //     }
        // }
    }
    
    void NpcOnClick(GameObject npc) {
        // Debug.Log("NpcOnClick: " + npc.name);
        // if (npc == Npc1) {
        //     Effect1.SetActive(true);
        //     Effect1.GetComponent<Animator>().enabled = true;
        //     Effect1.GetComponent<Animator>().Play("Effect1", 0, 0);
        //     Effect2.SetActive(false);
        //     Effect2.GetComponent<Animator>().enabled = false;
        //     Effect2.GetComponent<Animator>().Play("Effect2", 0, 0);
        //     ScoreBoardTitle.GetComponent<Image>().sprite = FailTitle;
        //     ScoreBoardTitle.SetActive(true);
        //     BackButton.SetActive(true);
        // } else if (npc == Npc2) {
        //     Effect1.SetActive(false);
        //     Effect1.GetComponent<Animator>().enabled = false;
        //     Effect1.GetComponent<Animator>().Play("Effect1", 0, 0);
        //     Effect2.SetActive(true);
        //     Effect2.GetComponent<Animator>().enabled = true;
        //     Effect2.GetComponent<Animator>().Play("Effect2", 0, 0);
        //     ScoreBoardTitle.GetComponent<Image>().sprite = SuccessTitle;
        //     ScoreBoardTitle.SetActive(true);
        //     NextButton.SetActive(true);
        // }
    }
    
    public void OnBeginDrag(PointerEventData eventData) { }
    
    public void OnDrag(PointerEventData eventData) {
        PlayBar1.OnDrag(eventData);
    }
    
    public void OnEndDrag(PointerEventData eventData) {
        PlayBar1.OnEndDrag(eventData);
    }
    
    void ExitButtonOnClick() {
        SceneManager.LoadScene("MainMenu");
    }
}
