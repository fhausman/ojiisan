using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour, IPickable
{
    [SerializeField]
    private float _score = 100.0f;

    [SerializeField]
    private float _pickCooldown = 1.0f;
    private float _cooldownElapsed = 1.0f;

    private BoxCollider2D _pickTrigger = null;
    private Rigidbody2D _rigidbody = null;
    private SpriteRenderer _spriteRenderer = null;

    public GameObject GetObjRef()
    {
        return gameObject;
    }

    public void OnObjectDrop(GameObject owner)
    {
        gameObject.SetActive(true);
    }

    public void OnObjectPick(PlayerController pc)
    {
        gameObject.SetActive(false);
        //return to the pool
    }

    public void OnObjectPick(Monke monke)
    {
        monke.ItemSlot.SetActive(true);
        monke.ItemSprite.sprite = _spriteRenderer.sprite;
        monke.HeldObject = this;

        gameObject.SetActive(false);
        _pickTrigger.enabled = false;
    }
    
    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _pickTrigger = GetComponentsInChildren<BoxCollider2D>()[1];
    }

    void Update()
    {
        _cooldownElapsed += Time.deltaTime;
        if (_cooldownElapsed >= _pickCooldown)
        {
            _pickTrigger.enabled = true;
        }
    }
}
