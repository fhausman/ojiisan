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

    private float _currentHeat = 0.0f;
    private float _timer = 0.0f;

    void Start()
    {
        _currentHeat = _initialHeat;
    }

    // Update is called once per frame
    void Update()
    {
        if(_timer >= _coolingInterval)
        {
            _currentHeat -= _coolingDiff;
            _timer = 0.0f;
        }
        else
        {
            _timer += Time.deltaTime;
        }

        if(_currentHeat <= 0.0f)
        {
            _onHeatZeroed.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Cane"))
        {
            if (_currentHeat < _initialHeat)
            {
                _timer = 0.0f;
                _currentHeat += _heatingDiff;
                if(_currentHeat > _initialHeat)
                {
                    _currentHeat = _initialHeat;
                }
            }
        }
    }
}
