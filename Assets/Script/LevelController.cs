using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    private GameObject ExitButton;
    private AudioSource Audio;
    public GameObject Npc1;
    public GameObject Npc2;
    private GameObject PlayButton;
    public Sprite PlayImage;
    public Sprite StopImage;

    private int PointStart = -700;
    private int PointEnd = 700;
    private int ProcessBarLength = 1500;

    private GameObject Point;
    bool isDragging;
    bool isTempPause;


    void Start() {
        Audio = GetComponent<AudioSource>();
        Point = GameObject.Find("Point");
        PlayButton = GameObject.Find("PlayButton");
        ExitButton = GameObject.Find("ExitButton");
        ExitButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(ExitButtonOnClick);
        PlayButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(PlayButtonOnClick);
    }

    void Update() {
        // phone mode
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                if (hit.collider.gameObject == Npc1) {
                    NpcOnClick(Npc1);
                } else if (hit.collider.gameObject == Npc2) {
                    NpcOnClick(Npc2);
                }
            }
        }
        // pc mode
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                if (hit.collider.gameObject == Npc1) {
                    NpcOnClick(Npc1);
                } else if (hit.collider.gameObject == Npc2) {
                    NpcOnClick(Npc2);
                }
            }
        }
        // update audio point
        MapPointToAudio();
    }

    void NpcOnClick(GameObject npc) {
        Debug.Log("NpcOnClick: " + npc.name);
    }

    void ExitButtonOnClick() {
        SceneManager.LoadScene("MainMenu");
    }

    void PlayButtonOnClick() {
        if (Audio.isPlaying) {
            Audio.Pause();
            PlayButton.GetComponent<UnityEngine.UI.Image>().sprite = PlayImage;
        } else {
            Audio.Play();
            PlayButton.GetComponent<UnityEngine.UI.Image>().sprite = StopImage;
        }
    }

    void MapPointToAudio() {
        if (isDragging) return;
        // 將 audio 的播放進度轉換為 point 的座標
        float now_time = Audio.GetComponent<AudioSource>().time;
        float total_time = Audio.GetComponent<AudioSource>().clip.length;
        float point_x = now_time / total_time * 100 * ProcessBarLength / 100;
        Point.transform.localPosition = new Vector3(PointStart + point_x, 0, 0);
    }

    public void OnBeginDrag(PointerEventData eventData) { }

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
        float total_time = Audio.GetComponent<AudioSource>().clip.length;
        float point_x = Point.transform.localPosition.x - PointStart;
        float now_time = point_x / ProcessBarLength * total_time;
        Audio.GetComponent<AudioSource>().time = now_time;
        isDragging = false;
        if (isTempPause) {
            Audio.Play();
            isTempPause = false;
        }
    }
}
