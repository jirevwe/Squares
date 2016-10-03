using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuChooser : MonoBehaviour {

    public string level = "LEVEL_TO_LOAD";

	void Start () {
	
	}
	
	void Update () {
        if (Input.GetKeyUp(KeyCode.Escape))
            SceneManager.LoadScene(0);
    }

    public void LoadLevel_0()
    {
        PlayerPrefs.SetString(level, "4");
        PlayerPrefs.Save();
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    public void LoadLevel_1()
    {
        PlayerPrefs.SetString(level, "5");
        PlayerPrefs.Save();
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    public void LoadLevel_2()
    {
        PlayerPrefs.SetString(level, "2");
        PlayerPrefs.Save();
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    public void LoadLevel_3()
    {
        PlayerPrefs.SetString(level, "3");
        PlayerPrefs.Save();
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }
}
