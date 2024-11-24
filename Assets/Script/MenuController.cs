using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {
    private GameObject DemoButton;
    private GameObject WebsiteButton;
    private GameObject HaikyuuButton;
    private GameObject PreviousButton;
    
    public float buttonMoveSpeed;
    private bool isButtonMoving = false;
    
    private Vector3 demoButtonTargetPosition;
    private Vector3 websiteButtonTargetPosition;
    private Vector3 haikyuuButtonTargetPosition;
    private Vector3 previosButtonTargetPosition;
    
    void Start() {
        DemoButton = GameObject.Find("DemoButton");
        WebsiteButton = GameObject.Find("WebsiteButton");
        HaikyuuButton = GameObject.Find("HaikyuuButton");
        PreviousButton = GameObject.Find("PreviousButton");
        DemoButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(DemoButtonOnClick);
        PreviousButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(PreviousButtonOnClick);
        HaikyuuButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(HaikyuuButtonOnClick);
        WebsiteButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(WebsiteButtonOnClick);
    }
    
    void Update() {
        if (isButtonMoving) {
            MoveButtonTo(DemoButton, demoButtonTargetPosition);
            MoveButtonTo(WebsiteButton, websiteButtonTargetPosition);
            MoveButtonTo(HaikyuuButton, haikyuuButtonTargetPosition);
            MoveButtonTo(PreviousButton, previosButtonTargetPosition);
            if (DemoButton.transform.localPosition == demoButtonTargetPosition &&
                WebsiteButton.transform.localPosition == websiteButtonTargetPosition &&
                HaikyuuButton.transform.localPosition == haikyuuButtonTargetPosition &&
                PreviousButton.transform.localPosition == previosButtonTargetPosition) {
                isButtonMoving = false;
            }
        }
    }
    
    void MoveButtonTo(GameObject button, Vector3 targetPosition) {
        button.transform.localPosition = Vector3.MoveTowards(button.transform.localPosition,
                                                            targetPosition,
                                                            buttonMoveSpeed * Time.deltaTime);
    }
    
    private void WebsiteButtonOnClick() {
        Application.OpenURL("http://cvprotect.sparrowten.com/");
    }
    
    private void DemoButtonOnClick() {
        if (isButtonMoving) return;
        
        isButtonMoving = true;
        
        demoButtonTargetPosition = new Vector3(-1265,
                                                DemoButton.transform.localPosition.y,
                                                DemoButton.transform.localPosition.z);
        websiteButtonTargetPosition = new Vector3(-1265,
                                                WebsiteButton.transform.localPosition.y,
                                                WebsiteButton.transform.localPosition.z);
        haikyuuButtonTargetPosition = new Vector3(0,
                                                HaikyuuButton.transform.localPosition.y,
                                                HaikyuuButton.transform.localPosition.z);
        previosButtonTargetPosition = new Vector3(0,
                                                PreviousButton.transform.localPosition.y,
                                                PreviousButton.transform.localPosition.z);
    }
    
    private void PreviousButtonOnClick() {
        if (isButtonMoving) return;
        
        isButtonMoving = true;
        
        demoButtonTargetPosition = new Vector3(0,
                                                DemoButton.transform.localPosition.y,
                                                DemoButton.transform.localPosition.z);
        websiteButtonTargetPosition = new Vector3(0,
                                                WebsiteButton.transform.localPosition.y,
                                                WebsiteButton.transform.localPosition.z);
        haikyuuButtonTargetPosition = new Vector3(1265,
                                                HaikyuuButton.transform.localPosition.y,
                                                HaikyuuButton.transform.localPosition.z);
        previosButtonTargetPosition = new Vector3(1265,
                                                PreviousButton.transform.localPosition.y,
                                                PreviousButton.transform.localPosition.z);
    }
    
    private void HaikyuuButtonOnClick() {
        if (isButtonMoving) return;
        SceneManager.LoadScene("Haikyuu");
    }
}
