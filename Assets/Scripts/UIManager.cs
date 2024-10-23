using UnityEngine;
using UnityEngine.SceneManagement;
using Apple.PHASE;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    private PHASESource phaseInstance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SwitchScene(int sceneIndex)
    {
        StopPHASEAudio();

        SceneManager.LoadScene(sceneIndex);
    }

    private void StopPHASEAudio()
    {
        PHASESource phaseInstance = FindObjectOfType<PHASESource>();

        if (phaseInstance != null)
        {
            phaseInstance.Stop();
        }
    }
}
