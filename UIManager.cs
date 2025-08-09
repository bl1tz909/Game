using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainPanel;
    public GameObject playGamePanel;
    public GameObject menuPanel;
    public GameObject HostPanel;

    [Header("Post-Processing")]
    public Volume globalVolume;
    private DepthOfField dof;

    void Start()
    {
        if (globalVolume != null && globalVolume.profile.TryGet(out dof))
        {
            dof.active = false;
        }

        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainPanel.SetActive(true);
        playGamePanel.SetActive(false);
        menuPanel.SetActive(false);
        HostPanel.SetActive(false);
        ToggleDepthOfField(false);
    }

    public void ShowPlayGame()
    {
        mainPanel.SetActive(false);
        playGamePanel.SetActive(true);
        menuPanel.SetActive(false);
        HostPanel.SetActive(false);
        ToggleDepthOfField(false);
    }

    public void ShowMenu()
    {
        mainPanel.SetActive(false);
        playGamePanel.SetActive(false);
        menuPanel.SetActive(true);
        HostPanel.SetActive(false);
        ToggleDepthOfField(true);
    }

    public void ShowHost()
    {
        mainPanel.SetActive(false);
        playGamePanel.SetActive(false);
        menuPanel.SetActive(false);
        HostPanel.SetActive(true);
        ToggleDepthOfField(true);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("MainGame"); // Replace with your actual gameplay scene name
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }

    private void ToggleDepthOfField(bool enable)
    {
        if (dof != null)
        {
            dof.active = enable;
        }
    }
}
