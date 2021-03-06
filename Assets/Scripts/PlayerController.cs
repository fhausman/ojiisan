﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum PlayerState
{
    Idle,
    Walk,
    Jump,
    Falling,
    Hit,
    Bump
}

public class PlayerIdle : BaseState
{
    public PlayerController pc;

    public override void onInit(params object[] args)
    {
        Debug.Log("Idle");
        var prevState = pc.StateMachine.PreviousState;
        if (prevState != PlayerState.Falling && prevState != PlayerState.Jump)
        {
            pc.Animator.SetTrigger("Idle");
        }
    }

    public override void onUpdate(float deltaTime)
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            pc.StateMachine.ChangeState(PlayerState.Jump);
            return;
        }

        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f)
        {
            pc.StateMachine.ChangeState(PlayerState.Walk);
            return;
        }

        if(pc.InGirlsZone)
        {
            pc.Animator.SetTrigger("Perv");
        }
    }

    public override void onFixedUpdate(float deltaTime)
    {
        RaycastHit2D hit;
        if (!pc.Grounded(out hit))
        {
            pc.StateMachine.ChangeState(PlayerState.Falling);
            return;
        }

        var velocity = pc.Rigidbody.velocity;
        velocity -= -pc.Friction * pc.transform.localScale.x * Vector2.right;
        velocity = new Vector2(
            pc.transform.localScale.x < 0.0f ? Mathf.Clamp(velocity.x, 0.0f, pc.WalkSpeed) : Mathf.Clamp(velocity.x, -pc.WalkSpeed, 0.0f),
            velocity.y
            );
        pc.Rigidbody.velocity = velocity;
    }

    public override void onExit()
    {
        pc.Animator.ResetTrigger("Idle");
    }
}

public class PlayerWalk : BaseState
{
    public PlayerController pc;

    public override void onInit(params object[] args)
    {
        Debug.Log("Walk");
        pc.Animator.SetTrigger("Walk");
    }

    public override void onUpdate(float deltaTime)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            pc.StateMachine.ChangeState(PlayerState.Jump);
            return;
        }

        var input = Input.GetAxis("Horizontal");
        if (Mathf.Abs(input) < 0.1f)
        {
            pc.StateMachine.ChangeState(PlayerState.Idle);
            return;
        }
    }

    public override void onFixedUpdate(float deltaTime)
    {
        pc.UpdateHorizontal();
        RaycastHit2D hit;
        if (!pc.Grounded(out hit))
        {
            pc.StateMachine.ChangeState(PlayerState.Falling);
        }
    }
}

public class PlayerJump : BaseState
{
    public PlayerController pc;

    private float timeElapsed = 0.0f;

    public override void onInit(params object[] args)
    {
        Debug.Log("Jump");
        timeElapsed = 0.0f;
        pc.Animator.SetTrigger("Jump");
    }

    public override void onUpdate(float deltaTime)
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            pc.StateMachine.ChangeState(PlayerState.Falling);
            return;
        }
    }

    public override void onFixedUpdate(float deltaTime)
    {
        var hit = Physics2D.Raycast(pc.transform.position, Vector2.up, 1.75f, LayerMask.GetMask("Ground"));
        if (hit)
        {
            pc.Animator.SetTrigger("Hit");
            pc.Glasses.SetActive(false);
            pc.DropGlasses();
            pc.StateMachine.ChangeState(PlayerState.Falling);
            return;
        }
        else if(timeElapsed >= pc.JumpTime)
        {
            pc.StateMachine.ChangeState(PlayerState.Falling);
            return;
        }

        pc.UpdateHorizontal();
        pc.Rigidbody.velocity = new Vector3(pc.Rigidbody.velocity.x, pc.JumpForce);

        timeElapsed += deltaTime;
    }
}

public class PlayerBump : BaseState
{
    public PlayerController pc;

    private float timeElapsed = 0.0f;

    public override void onInit(params object[] args)
    {
        Debug.Log("Bump");
        timeElapsed = 0.0f;
        pc.Animator.SetTrigger("Jump");
    }

    public override void onUpdate(float deltaTime)
    {
    }

    public override void onFixedUpdate(float deltaTime)
    {
        if (timeElapsed >= 0.075f)
        {
            pc.StateMachine.ChangeState(PlayerState.Falling);
            return;
        }

        pc.UpdateHorizontal();
        pc.Rigidbody.velocity = new Vector3(pc.Rigidbody.velocity.x, pc.JumpForce * 0.8f);

        timeElapsed += deltaTime;
    }
}

public class PlayerFall : BaseState
{
    public PlayerController pc;

    public override void onInit(params object[] args)
    {
        Debug.Log("Fall");
    }

    public override void onFixedUpdate(float deltaTime)
    {
        RaycastHit2D hit;
        if (pc.Grounded(out hit))
        {
            pc.Rigidbody.velocity = Vector2.zero;
            pc.transform.position = new Vector3(pc.transform.position.x, hit.point.y + 2.0f, pc.transform.position.z);

            pc.Animator.Play("JumpEnd");
            pc.StateMachine.ChangeState(PlayerState.Idle);
            return;
        }

        pc.UpdateHorizontal();
        if (pc.Rigidbody.velocity.y > 0.0f)
        {
            pc.Rigidbody.velocity -= new Vector2(0.0f, 5.0f);
        }
        else
        {
            pc.Rigidbody.velocity = new Vector2(pc.Rigidbody.velocity.x, -pc.FallForce);
        }
    }
}

public class PlayerHit : BaseState
{
    public PlayerController pc;

    float _timer = 0.0f;

    public override void onInit(params object[] args)
    {
        pc.Animator.SetTrigger("Hit");
        pc.Rigidbody.velocity = -pc.transform.localScale.x * pc.transform.right * (pc.FallForce);
        _timer = 0.0f;
    }

    public override void onUpdate(float deltaTime)
    {
        if(_timer >= 0.78f)
        {
            pc.StateMachine.ChangeState(PlayerState.Idle);
        }

        _timer += Time.deltaTime;
    }

    public override void onFixedUpdate(float deltaTime)
    {
        var velocity = pc.Rigidbody.velocity;
        velocity -= -pc.Friction * pc.transform.localScale.x * ((Vector2)pc.transform.right) * (0.78f - _timer);
        velocity = new Vector2(
            pc.transform.localScale.x < 0.0f ? Mathf.Clamp(velocity.x, 0.0f, pc.FallForce) : Mathf.Clamp(velocity.x, -pc.FallForce, 0.0f),
            velocity.y
            );
        pc.Rigidbody.velocity = velocity;
    }
}

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float _jumpForce = 10.0f;
    public float JumpForce { get => _jumpForce; }

    [SerializeField]
    private float _jumpTime = 1.0f;
    public float JumpTime { get => _jumpTime; }

    [SerializeField]
    private float _fallForce = 30.0f;
    public float FallForce { get => _fallForce; }

    [SerializeField]
    private float _walkSpeed = 10.0f;
    public float WalkSpeed { get => _walkSpeed; }

    [SerializeField]
    private float _friction = 5.0f;
    public float Friction { get => _friction; }

    private Rigidbody2D _rb = null;
    public Rigidbody2D Rigidbody { get => _rb; }

    private SpriteRenderer _sr = null;
    public SpriteRenderer SpriteRenderer { get => _sr; }

    private Animator _animator = null;
    public Animator Animator { get => _animator; }

    public StateMachine<PlayerState> StateMachine { get; private set; } = new StateMachine<PlayerState>();

    #region Blur related properties
    [SerializeField]
    private GameObject _glasses = null;
    public GameObject Glasses { get => _glasses; }

    [SerializeField]
    private GameObject _rendTex = null;

    [SerializeField]
    private float _blurSpeed = 4.0f;

    private Material _blur = null;
    private float _blurValue = 0.0f;
    private bool _shouldBlur { get => !_glasses.activeSelf; }
    public Glasses HeldGlasses { get; set; } = null;
    #endregion

    #region Attack
    [SerializeField]
    private GameObject _attackRange = null;
    private float _attackCooldown = 0.55f;
    private float _attackCooldownElapsed = 0.0f;
    private int _attackHelper = 0;
    #endregion

    #region Health
    [SerializeField]
    private float _maxHealth = 100.0f;

    [SerializeField]
    private float _healthLossPerSecond = 2.0f;

    [SerializeField]
    private float _healthGainPerSecond = 5.0f;

    [SerializeField]
    private float _damage = 10.0f;

    [SerializeField]
    private UnityEvent _onHealthLost;

    public float CurrentHealth { get; set; } = 0.0f;

    private float _healthUpdateTimer = 0.0f;
    private bool _isInGirlsZone = false;
    public bool InGirlsZone { get => _isInGirlsZone; }
    #endregion

    public bool Dead = false;

    public void DropGlasses()
    {
        if (HeldGlasses != null)
        {
            HeldGlasses.transform.position = _glasses.transform.position;
            HeldGlasses.gameObject.SetActive(true);
            HeldGlasses.OnObjectDrop(gameObject);
            HeldGlasses = null;
        }
    }

    public bool Grounded(out RaycastHit2D hit)
    {
        hit = Physics2D.Raycast(transform.position - Vector3.right * 0.75f, Vector2.down, 2.6f, LayerMask.GetMask("Ground"));
        if (hit.collider != null)
            return true;

        hit = Physics2D.Raycast(transform.position - Vector3.left * 0.75f, Vector2.down, 2.6f, LayerMask.GetMask("Ground"));
        if (hit.collider != null)
            return true;

        return false;
    }

    public void UpdateHorizontal()
    {
        if (_attackCooldownElapsed < _attackCooldown)
        {
            _rb.velocity = Vector2.zero;
            return;
        }


        var input = Input.GetAxis("Horizontal");
        _rb.velocity = new Vector3(input * WalkSpeed, _rb.velocity.y);
    }

    public void UpdateAttack()
    {
        if (_attackCooldownElapsed > 0.15f && _attackCooldownElapsed < 0.37f)
        {
            if (_attackHelper == 3)
            {
                _attackRange.transform.localPosition += new Vector3(0.1f, 0.0f, 0.0f);
            }
            ++_attackHelper;
            _attackRange.SetActive(true);
        }
        else
        {
            _attackRange.transform.localPosition = new Vector3(1.15f, 0.0f, 0.0f);
            _attackRange.SetActive(false);
            _attackHelper = 0;
        }

        if (_attackCooldownElapsed < 1.1*_attackCooldown)
        {
            _attackCooldownElapsed += Time.deltaTime;
            return;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            //play animation attack
            _attackRange.SetActive(true);
            _attackCooldownElapsed = 0.0f;
            Animator.SetTrigger("Attack");
        }
    }

    public void UpdateHealth()
    {
        if(_healthUpdateTimer >= 1.0f)
        {
            CurrentHealth += !_isInGirlsZone ? -_healthLossPerSecond : _healthGainPerSecond;
            CurrentHealth = Mathf.Clamp(CurrentHealth, -1.0f, _maxHealth);
            _healthUpdateTimer = 0.0f;

            //Debug.Log("Current health: " + _currentHealth);
        }
        else
        {
            _healthUpdateTimer += Time.deltaTime;
        }

        if (CurrentHealth <= 0.0f + Mathf.Epsilon)
        {
            _onHealthLost.Invoke();
            Dead = true;
        }
    }

    public void Damage()
    {
        if (StateMachine.CurrentState != PlayerState.Hit)
        {
            CurrentHealth -= _damage;
            StateMachine.ChangeState(PlayerState.Hit);
        }
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _blur = _rendTex.GetComponent<Renderer>().material;
        CurrentHealth = _maxHealth;

        StateMachine.AddState(PlayerState.Idle, new PlayerIdle() { pc = this });
        StateMachine.AddState(PlayerState.Walk, new PlayerWalk() { pc = this });
        StateMachine.AddState(PlayerState.Jump, new PlayerJump() { pc = this });
        StateMachine.AddState(PlayerState.Falling, new PlayerFall() { pc = this });
        StateMachine.AddState(PlayerState.Hit, new PlayerHit() { pc = this });
        StateMachine.AddState(PlayerState.Bump, new PlayerBump() { pc = this });
        StateMachine.ChangeState(PlayerState.Idle);
    }

    // Update is called once per frame
    private void Update()
    {
        if (Dead)
            return;

        StateMachine.OnUpdate(Time.deltaTime);
        UpdateBlur();
        UpdateDirection();
        UpdateAttack();
        UpdateHealth();

        Animator.SetInteger("CurrentState", (int) StateMachine.CurrentState);
    }

    private void FixedUpdate()
    {
        if (Dead)
            return;

        StateMachine.OnFixedUpdate(Time.fixedDeltaTime);
    }

    private void UpdateBlur()
    {
        if(_shouldBlur)
        {
            _blurValue += _blurSpeed * Time.deltaTime; 
        }
        else
        {
            _blurValue -= _blurSpeed * Time.deltaTime;
        }
        _blurValue = Mathf.Clamp(_blurValue, 0.0f, 4.0f);

        _blur.SetFloat("_Size", _blurValue);
    }

    private void UpdateDirection()
    {
        var input = Input.GetAxis("Horizontal");
        if (input > 0.0f + Mathf.Epsilon)
        {
            _rb.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
        else if (input < 0.0f - Mathf.Epsilon)
        {
            _rb.transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.CompareTag("Guy"))
        {
            StateMachine.ChangeState(PlayerState.Bump);
            collision.gameObject.SendMessage("Bump");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Pickable"))
        {
            collision.gameObject.GetComponentInParent<IPickable>().OnObjectPick(this);
        }
        else if(collision.CompareTag("GirlsZone"))
        {
            Animator.SetTrigger("Perv");
            _isInGirlsZone = true;
        }
        else if(collision.CompareTag("Claws"))
        {
            Damage();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("GirlsZone"))
        {
            Animator.ResetTrigger("Perv");
            _isInGirlsZone = false;
        }
    }
}
