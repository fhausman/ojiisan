using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverScreen : MonoBehaviour
{
    void PlayEnd()
    {
        GetComponent<Animator>().SetTrigger("End");
    }
}
