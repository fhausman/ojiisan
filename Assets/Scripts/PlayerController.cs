using System;
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
    Hit
}

public class PlayerIdle : BaseState
{
    public PlayerController pc;

    public override void onUpdate(float deltaTime)
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            pc.StateMachine.ChangeState(PlayerState.Jump);
        }

        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f)
        {
            pc.StateMachine.ChangeState(PlayerState.Walk);
        }
    }

    public override void onFixedUpdate(float deltaTime)
    {
        RaycastHit2D hit;
        if (!pc.Grounded(out hit))
        {
            pc.StateMachine.ChangeState(PlayerState.Falling);
        }
        var velocity = pc.Rigidbody.velocity;
        velocity -= -pc.Friction * pc.transform.localScale.x * Vector2.right;
        velocity = new Vector2(
            pc.transform.localScale.x < 0.0f ? Mathf.Clamp(velocity.x, 0.0f, pc.WalkSpeed) : Mathf.Clamp(velocity.x, -pc.WalkSpeed, 0.0f),
            velocity.y
            );
        pc.Rigidbody.velocity = velocity;
    }
}

public class PlayerWalk : BaseState
{
    public PlayerController pc;

    public override void onUpdate(float deltaTime)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            pc.StateMachine.ChangeState(PlayerState.Jump);
        }

        var input = Input.GetAxis("Horizontal");
        if (Mathf.Abs(input) < 0.1f)
        {
            pc.StateMachine.ChangeState(PlayerState.Idle);
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
        timeElapsed = 0.0f;
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

public class PlayerFall : BaseState
{
    public PlayerController pc;

    public override void onInit(params object[] args)
    {
    }

    public override void onFixedUpdate(float deltaTime)
    {
        RaycastHit2D hit;
        if (pc.Grounded(out hit))
        {
            pc.Rigidbody.velocity = Vector2.zero;
            pc.transform.position = new Vector3(pc.transform.position.x, hit.point.y + 2.0f, pc.transform.position.z);
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

    public override void onInit(params object[] args)
    {
    }

    public override void onFixedUpdate(float deltaTime)
    {
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

    private StateMachine<PlayerState> _stateMachine = new StateMachine<PlayerState>();
    public StateMachine<PlayerState> StateMachine { get => _stateMachine; }

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
    private float _attackCooldown = 0.1f;
    private float _attackCooldownElapsed = 0.0f;
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

    private float _currentHealth = 0.0f;
    private float _healthUpdateTimer = 0.0f;
    private bool _isInGirlsZone = false;
    #endregion

    public void DropGlasses()
    {
        if (HeldGlasses != null)
        {
            HeldGlasses.transform.position = _glasses.transform.position;
            HeldGlasses.gameObject.SetActive(true);
            HeldGlasses.GetComponent<Rigidbody2D>().AddForce(Vector2.left * transform.localScale.x * 10.0f, ForceMode2D.Impulse);
            HeldGlasses.OnObjectDrop();
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
        var input = Input.GetAxis("Horizontal");
        _rb.velocity = new Vector3(input * WalkSpeed, _rb.velocity.y);
    }

    public void UpdateAttack()
    {
        if (_attackCooldownElapsed < 2*_attackCooldown)
        {
            if(_attackCooldownElapsed > _attackCooldown)
            {
                _attackRange.SetActive(false);
            }

            _attackCooldownElapsed += Time.deltaTime;
            return;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            //play animation attack
            _attackRange.SetActive(true);
            _attackCooldownElapsed = 0.0f;
        }
    }

    public void UpdateHealth()
    {
        if(_healthUpdateTimer >= 1.0f)
        {
            _currentHealth += !_isInGirlsZone ? -_healthLossPerSecond : _healthGainPerSecond;
            _currentHealth = Mathf.Clamp(_currentHealth, -1.0f, _maxHealth);
            _healthUpdateTimer = 0.0f;

            Debug.Log("Current health: " + _currentHealth);
        }
        else
        {
            _healthUpdateTimer += Time.deltaTime;
        }

        if (_currentHealth <= 0.0f + Mathf.Epsilon)
        {
            _onHealthLost.Invoke();
        }
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _blur = _rendTex.GetComponent<Renderer>().material;
        _currentHealth = _maxHealth;

        _stateMachine.AddState(PlayerState.Idle, new PlayerIdle() { pc = this });
        _stateMachine.AddState(PlayerState.Walk, new PlayerWalk() { pc = this });
        _stateMachine.AddState(PlayerState.Jump, new PlayerJump() { pc = this });
        _stateMachine.AddState(PlayerState.Falling, new PlayerFall() { pc = this });
        _stateMachine.ChangeState(PlayerState.Idle);
    }

    // Update is called once per frame
    private void Update()
    {
        _stateMachine.OnUpdate(Time.deltaTime);
        UpdateBlur();
        UpdateDirection();
        UpdateAttack();
        UpdateHealth();
    }

    private void FixedUpdate()
    {
        _stateMachine.OnFixedUpdate(Time.fixedDeltaTime);
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Pickable"))
        {
            collision.gameObject.GetComponentInParent<IPickable>().OnObjectPick(this);
        }
        else if(collision.CompareTag("GirlsZone"))
        {
            _isInGirlsZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("GirlsZone"))
        {
            _isInGirlsZone = false;
        }
    }
}
