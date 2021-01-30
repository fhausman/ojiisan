using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorController : MonoBehaviour
{
    [SerializeField]
    private MainGameManager _gameManager = null;

    [SerializeField]
    private RectTransform _transform = null;

    private float maxRot = -100.0f;
    private float minRot = 190.0f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var scale = _gameManager.GetNormalizedTotalHeat();
        _transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, Mathf.Lerp(190.0f, -100.0f, scale) + Random.Range(-2.0f, 2.0f)));
    }
}
