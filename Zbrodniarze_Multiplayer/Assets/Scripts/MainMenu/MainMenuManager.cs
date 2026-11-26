using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public Animator mainMenuAnimator;

    public GameObject serverList;

    private bool isIntroPlaying;


    void Update()
    {
        if (mainMenuAnimator.GetCurrentAnimatorStateInfo(0).IsName("IntroAnimation") && Input.GetKeyDown(KeyCode.Space))
        {
            mainMenuAnimator.Play("IntroAnimation", 0, 0.8f);
        }
    }

    public void openServerList()
    {
        serverList.SetActive(true);
    }

    public void openWindowCustomization()
    {
        mainMenuAnimator.SetTrigger("menu_Customization");
    }

    public void goBack()
    {
        mainMenuAnimator.SetTrigger("menu_GoBack");
    }
}
