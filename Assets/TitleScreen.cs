using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        AsyncOperation asyncOperation = null;
        if(Input.anyKey)
        {
            asyncOperation = SceneManager.LoadSceneAsync("Final4");
            asyncOperation.allowSceneActivation = true;
        }

        //if (asyncOperation != null && asyncOperation.isDone)
        //{
        //    asyncOperation.allowSceneActivation = true;
        //}
    }
}
