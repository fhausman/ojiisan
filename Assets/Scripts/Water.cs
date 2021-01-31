using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Water : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Pickable"))
        {
            Destroy(collision.gameObject);
        }
        else if(collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerController>().CurrentHealth -= 100.0f;
        }
    }
}
