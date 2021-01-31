using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuyController : MonoBehaviour
{
    private Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    void Bump()
    {
        Debug.Log("BUMP");
        _animator.SetTrigger("Bump");
    }
}
