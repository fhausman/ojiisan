using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glasses : MonoBehaviour, IPickable
{
    [SerializeField]
    private float _pickCooldown = 1.0f;
    private float _cooldownElapsed = 1.0f;

    private BoxCollider2D _pickTrigger = null;

    public void OnObjectPick(PlayerController pc)
    {
        pc.Glasses.SetActive(true);
        pc.HeldGlasses = this;

        gameObject.SetActive(false);
        _pickTrigger.enabled = false;
    }

    public void OnObjectDrop()
    {
        _cooldownElapsed = 0.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        _pickTrigger = GetComponentsInChildren<BoxCollider2D>()[1];
    }

    // Update is called once per frame
    void Update()
    {
        _cooldownElapsed += Time.deltaTime;
        if(_cooldownElapsed >= _pickCooldown)
        {
            _pickTrigger.enabled = true;
        }
    }
}
