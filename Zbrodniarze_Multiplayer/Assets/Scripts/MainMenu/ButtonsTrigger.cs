using UnityEngine;
using UnityEngine.SceneManagement;
public class ButtonsTrigger : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
}
