using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Furnace : MonoBehaviour
{
    [SerializeField]
    private float _initialHeat = 100.0f;

    [SerializeField]
    private float _coolingDiff = 5.0f;

    [SerializeField]
    private float _heatingDiff = 10.0f;

    [SerializeField]
    private float _coolingInterval = 10.0f;

    [SerializeField]
    private UnityEvent _onHeatZeroed;

    public float CurrentHeat { get; private set; } = 0.0f;

    private float _timer = 0.0f;
    private Animator _animator = null;

    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        CurrentHeat = _initialHeat;
    }

    // Update is called once per frame
    void Update()
    {
        if(_timer >= _coolingInterval)
        {
            CurrentHeat -= _coolingDiff;
            _timer = 0.0f;
        }
        else
        {
            _timer += Time.deltaTime;
        }

        if(CurrentHeat <= 0.0f)
        {
            _onHeatZeroed.Invoke();
        }

        _animator.SetFloat("Heat", CurrentHeat);
        CurrentHeat = Mathf.Clamp(CurrentHeat, -1.0f, 100.0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("WWWW");
        if(collision.CompareTag("Cane"))
        {
            CurrentHeat += _heatingDiff;
        }
    }
}
