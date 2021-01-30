using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum MonkeState
{
    Idle,
    GoingForItem,
    Walking,
    Angry,
    Hit
}

public class MonkeIdle : BaseState
{
    public Monke monke;

    private float _monkeyIdleCooldownBase = 2.0f;
    private float _monkeyIdleCooldown = 2.0f;
    private float _cooldownElapsed = 0.0f;

    public override void onInit(params object[] args)
    {
        _monkeyIdleCooldown = _monkeyIdleCooldownBase + Random.Range(-1.0f, 1.0f);
        _cooldownElapsed = 0.0f;
    }

    public override void onUpdate(float deltaTime)
    {
        _cooldownElapsed += deltaTime;
        if(_cooldownElapsed > _monkeyIdleCooldown)
        {
            Debug.Log("start walking");
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
    }

    public override void onFixedUpdate(float deltaTime)
    {
        if(!Physics2D.Raycast(monke.transform.position + Vector3.right * monke.Direction * 1.0f, Vector2.down, 2.0f, LayerMask.GetMask("Ground")))
        {
            monke.Direction = -monke.Direction;
        }

        monke.Rigidbody.velocity = monke.MonkeSpeed * Vector2.right * monke.Direction;
    }

    public override void onExit()
    {
        monke.Rigidbody.velocity = Vector2.zero;
    }
}

public class MonkeJump : BaseState
{
    public Monke monke;

    public override void onInit(params object[] args)
    {
    }

    public override void onUpdate(float deltaTime)
    {
    }

    public override void onFixedUpdate(float deltaTime)
    {
    }
}

public class MonkeFall : BaseState
{
    public Monke monke;

    public override void onInit(params object[] args)
    {
    }

    public override void onFixedUpdate(float deltaTime)
    {
    }
}

public class MonkeHit : BaseState
{
    public Monke monke;

    public override void onInit(params object[] args)
    {
    }

    public override void onFixedUpdate(float deltaTime)
    {
    }
}


public class Monke : MonoBehaviour
{
    [SerializeField]
    private float _monkeSpeed = 10.0f;
    public float MonkeSpeed { get => _monkeSpeed; }

    public float Direction { get => transform.localScale.x; set => transform.localScale = new Vector3(value, 1.0f, 1.0f); }

    [SerializeField]
    private GameObject _itemSlot = null;
    public GameObject ItemSlot { get => _itemSlot; }
    public SpriteRenderer ItemSprite { get => _itemSlot.GetComponent<SpriteRenderer>(); }

    public IPickable HeldObject { get; set; } = null;

    public Rigidbody2D Rigidbody { get; private set; } = null;

    public StateMachine<MonkeState> StateMachine { get; private set; } = new StateMachine<MonkeState>();

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
        StateMachine.AddState(MonkeState.Idle, new MonkeIdle() { monke = this });
        StateMachine.AddState(MonkeState.Walking, new MonkeWalking() { monke = this });

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
            StateMachine.ChangeState(MonkeState.Idle);
        }    
    }
}
