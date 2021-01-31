using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum MonkeState
{
    Idle,
    GoingForItem,
    Walking,
    Angry,
    Hit,
    Attack
}

public class MonkeIdle : BaseState
{
    public Monke monke;

    private float _monkeyIdleCooldownBase = 1.0f;
    private float _monkeyIdleCooldown = 2.0f;
    private float _cooldownElapsed = 0.0f;

    public override void onInit(params object[] args)
    {
        if (args.Length >= 1)
        {
            _monkeyIdleCooldown = (float)args[0];
        }
        else
        {
            _monkeyIdleCooldown = _monkeyIdleCooldownBase + Random.Range(-0.9f, 0.5f);
        }
        _cooldownElapsed = 0.0f;

        monke.Animator.SetTrigger(monke.ItemSlot.activeSelf ? "IdlePick" : "Idle");
    }

    public override void onUpdate(float deltaTime)
    {
        _cooldownElapsed += deltaTime;
        if(_cooldownElapsed > _monkeyIdleCooldown)
        {
            monke.StateMachine.ChangeState(MonkeState.Walking);
        }
    }
}

public class MonkeWalking : BaseState
{
    public Monke monke;

    public override void onInit(params object[] args)
    {
        monke.Direction = Mathf.Sign(Random.Range(-1.0f, 1.0f));
    }

    public override void onUpdate(float deltaTime)
    {
        monke.Animator.SetTrigger(monke.ItemSlot.activeSelf ? "WalkPick" : "Walk");
        if(!monke.ItemSlot.activeSelf)
        {
            var collider = Physics2D.OverlapCircle(monke.transform.position, 1.5f, LayerMask.GetMask("Player"));
            if (collider != null)
            {
                monke.StateMachine.ChangeState(MonkeState.Attack, collider);
                return;
            }
        }
    }

    public override void onFixedUpdate(float deltaTime)
    {
        if(!Physics2D.Raycast(monke.transform.position + Vector3.right * monke.Direction * 1.0f, Vector2.down, 2.0f, LayerMask.GetMask("Ground")))
        {
            //monke.Direction = -monke.Direction;
            monke.Collider.enabled = false;
            monke.StartCoroutine(monke.ReenableCollider());
            monke.CanLookForGround = false;
        }

        if (monke.Collider.enabled)
            monke.Rigidbody.velocity = monke.MonkeSpeed * Vector2.right * monke.Direction * monke.SpeedMultiplier;
        else
            monke.Rigidbody.velocity = new Vector2(0.0f, monke.Rigidbody.velocity.y);

        if (monke.CanLookForGround)
        {
            if(Physics2D.Raycast(monke.transform.position, Vector2.down, 2.0f, LayerMask.GetMask("Ground")))
            {
                monke.Collider.enabled = true;
            }
        }
    }

    public override void onExit()
    {
        monke.Rigidbody.velocity = Vector2.zero;
        monke.Animator.ResetTrigger("WalkPick");
        monke.Animator.ResetTrigger("Walk");
    }
}

public class MonkeAttack : BaseState
{
    public Monke monke;

    private float _timer = 0.0f;

    public override void onInit(params object[] args)
    {
        if(args.Length > 1)
        {
            var col = (Collider2D)args[0];
            if((monke.transform.position.x > col.gameObject.transform.position.x) && monke.Direction < 0)
            {
                monke.Direction = -monke.Direction;
            }
            else if ((monke.transform.position.x < col.gameObject.transform.position.x) && monke.Direction > 0)
            {
                monke.Direction = -monke.Direction;
            }
        }
        monke.Animator.SetTrigger("Attack");
        _timer = 0.0f;
    }

    public override void onUpdate(float deltaTime)
    {
        _timer += Time.deltaTime;
        if(_timer > 0.55f)
        {
            monke.StateMachine.ChangeState(MonkeState.Idle, 0.1f);
        }

        if(_timer > 0.1666f && _timer < 0.4333f)
        {
            monke.AttackRange.SetActive(true);
        }
        else if(_timer > 0.043333f)
        {
            monke.AttackRange.SetActive(false);
        }
    }

    public override void onFixedUpdate(float deltaTime)
    {
    }

    public override void onExit()
    {
        monke.SpeedMultiplier = Mathf.Clamp(monke.SpeedMultiplier + 0.2f, 1.0f, 2.0f);
    }
}

public class MonkeFall : BaseState
{
    public Monke monke;

    public override void onInit(params object[] args)
    {
    }

    public override void onUpdate(float deltaTime)
    {
    }
}

public class MonkeHit : BaseState
{
    public Monke monke;

    float _timer = 0.0f;

    public override void onInit(params object[] args)
    {
        var direction = (float)args[0];

        monke.Animator.SetTrigger("Hit");
        monke.Rigidbody.AddForce(Vector2.left * 5000f * direction);
        monke.Rigidbody.gravityScale = 0.0f;
        monke.Collider.enabled = false;
        _timer = 0.0f;
    }

    public override void onUpdate(float deltaTime)
    {
        if(_timer > 1.0f)
        {
            monke.gameObject.SetActive(false);
        }

        _timer += deltaTime;
    }
}


public class Monke : MonoBehaviour
{
    [SerializeField]
    private float _monkeSpeed = 10.0f;
    public float MonkeSpeed { get => _monkeSpeed; }

    public float SpeedMultiplier { get; set; } = 1.0f;

    public float Direction { get => transform.localScale.x; set => transform.localScale = new Vector3(value, 1.0f, 1.0f); }

    [SerializeField]
    private GameObject _itemSlot = null;
    public GameObject ItemSlot { get => _itemSlot; }
    public SpriteRenderer ItemSprite { get => _itemSlot.GetComponent<SpriteRenderer>(); }

    [SerializeField]
    public GameObject AttackRange = null;

    public IPickable HeldObject { get; set; } = null;

    public Rigidbody2D Rigidbody { get; private set; } = null;
    public BoxCollider2D Collider { get; private set; } = null;
    public Animator Animator { get; private set; } = null;
    public StateMachine<MonkeState> StateMachine { get; private set; } = new StateMachine<MonkeState>();
    public bool CanLookForGround = false;

    public void Reenable(Vector3 pos)
    {
        transform.position = pos;
        gameObject.SetActive(true);
        Collider.enabled = true;
        Rigidbody.gravityScale = 1.0f;

    }

    public IEnumerator ReenableCollider()
    {
        yield return new WaitForSeconds(0.25f);

        CanLookForGround = true;
        //Collider.enabled = true;
    }

    void DropItem()
    {
        if (HeldObject != null)
        {
            ItemSlot.SetActive(false);
            HeldObject.GetObjRef().transform.position = ItemSlot.transform.position;
            HeldObject.OnObjectDrop(gameObject);
            HeldObject = null;
        }
    }

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        Collider = GetComponent<BoxCollider2D>();
        StateMachine.AddState(MonkeState.Idle, new MonkeIdle() { monke = this });
        StateMachine.AddState(MonkeState.Walking, new MonkeWalking() { monke = this });
        StateMachine.AddState(MonkeState.Attack, new MonkeAttack() { monke = this });
        StateMachine.AddState(MonkeState.Hit, new MonkeHit() { monke = this });

        StateMachine.ChangeState(MonkeState.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        StateMachine.OnUpdate(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        StateMachine.OnFixedUpdate(Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Pickable"))
        {
            if (StateMachine.CurrentState == MonkeState.Walking)
            {
                collision.gameObject.GetComponentInParent<IPickable>().OnObjectPick(this);
            }
        }
        else if (collision.CompareTag("Cane"))
        {
            DropItem();
            StateMachine.ChangeState(MonkeState.Hit, Mathf.Sign(collision.gameObject.transform.position.x - gameObject.transform.position.x));
        }    
    }
}
