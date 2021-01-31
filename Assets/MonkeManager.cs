using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonkeManager : MonoBehaviour
{
    [SerializeField]
    private Transform spawnPoint;

    [SerializeField]
    private Transform[] waypoints;

    [SerializeField]
    private Monke[] monkePool;

    [SerializeField]
    private IPickable[] itemPool;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
