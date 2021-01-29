using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGameManager : MonoBehaviour
{
    [SerializeField]
    private int _score = 0;

    [SerializeField]
    private Furnace[] furnaces = null;

    public void OnGameLost()
    {
        //Show lose screen
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
