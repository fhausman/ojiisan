using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glasses : MonoBehaviour, IPickable
{
    [SerializeField]
    private float _pickCooldown = 1.0f;
    private float _cooldownElapsed = 1.0f;

    private BoxCollider2D _pickTrigger = null;
    private Rigidbody2D _rigidbody = null;
    private SpriteRenderer _spriteRenderer = null;

    public void OnObjectPick(PlayerController pc)
    {
        pc.Glasses.SetActive(true);
        pc.HeldGlasses = this;

        gameObject.SetActive(false);
        _pickTrigger.enabled = false;
    }

    public void OnObjectPick(Monke monke)
    {
        monke.ItemSlot.SetActive(true);
        monke.ItemSprite.sprite = _spriteRenderer.sprite;
        monke.HeldObject = this;

        gameObject.SetActive(false);
        _pickTrigger.enabled = false;
    }

    public void OnObjectDrop(GameObject owner)
    {
        _cooldownElapsed = 0.0f;
        _rigidbody.AddForce(Vector2.left * owner.transform.localScale.x * 10.0f, ForceMode2D.Impulse);
        gameObject.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
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

    public GameObject GetObjRef()
    {
        return gameObject;
    }
}
