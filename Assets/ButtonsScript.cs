using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Bridget LaBonney
//blabonney@nevada.unr.edu
public class ButtonsScript : MonoBehaviour
{
    //script built for button functions, will probably reuse
    public void loadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    //function "borrowed" from insane.engineer's blog
    public void quitgame()
    {
        Application.Quit();
    }
}
