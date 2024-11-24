using UnityEngine;
using UnityEngine.SceneManagement;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

public class ScoreController : MonoBehaviour {
    public Sprite Score06;
    public Sprite Score16;
    public Sprite Score26;
    public Sprite Score36;
    public Sprite Score46;
    public Sprite Score56;
    public Sprite Score66;
    public Sprite[] ScoreSprites;
    private GameObject Score;
    
    private GameObject ExitButton;  // 離開按鈕
    
    void Start() {
        Debug.Log("Score: " + ScoreBoard.score);
        Score = GameObject.Find("Score");
        ScoreSprites = new Sprite[] {Score06, Score16, Score26, Score36, Score46, Score56, Score66};
        Score.GetComponent<Image>().sprite = ScoreSprites[ScoreBoard.score];
        
        ExitButton = GameObject.Find("ExitButton");
        ExitButton.GetComponent<Button>().onClick.AddListener(ExitButtonOnClick);
        ExitButton.GetComponent<Button>().interactable = true;
    }
    
    void Update() {
        // pass
    }
    
    private void ExitButtonOnClick() {
        Debug.Log("Exit button clicked.");
        SceneManager.LoadScene("MainMenu");
    }
}
