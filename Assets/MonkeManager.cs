using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonkeManager : MonoBehaviour
{
    [SerializeField]
    private float spawnRate = 10.0f;

    [SerializeField]
    private Transform spawnPoint;

    [SerializeField]
    private GameObject[] itemPool;

    [SerializeField]
    private Monke monkePrefab;

    [SerializeField]
    private GameObject glassesPrefab;

    private float _timer = 0.0f;

    void Start()
    {
        _timer = 0.0f;
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if(_timer >= spawnRate)
        {
            _timer = 0.0f;
            Spawn();
        }
    }

    void Spawn()
    {
        Monke monke = Instantiate<Monke>(monkePrefab);
        if (Random.value < 0.33f)
        {
            GameObject obj;
            if (GameObject.Find("Glasses") || GameObject.Find("Glasses(Clone)"))
            {
                obj = Instantiate(itemPool[Random.Range(0, itemPool.Length)]);
            }
            else
            {
                obj = Instantiate(glassesPrefab);
            }

            obj.GetComponent<IPickable>().OnObjectPick(monke);
        }
        monke.gameObject.SetActive(true);
        monke.gameObject.transform.position = spawnPoint.transform.position;
        monke.Direction = -transform.localScale.x;
        //monke.StateMachine.ChangeState(MonkeState.Walking);
    }
}
