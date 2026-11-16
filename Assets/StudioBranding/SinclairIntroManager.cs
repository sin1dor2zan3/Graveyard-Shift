using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SinclairIntroManager : MonoBehaviour
{
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private float introDuration = 4.8f;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(introDuration);
        SceneManager.LoadScene(mainMenuScene);
    }
}