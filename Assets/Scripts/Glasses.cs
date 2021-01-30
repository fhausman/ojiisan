using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glasses : MonoBehaviour, IPickable
{
    public void OnObjectPick(PlayerController pc)
    {
        gameObject.SetActive(false);
        pc.Glasses.SetActive(true);
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
