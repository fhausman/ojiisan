using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartController : MonoBehaviour
{
    [SerializeField]
    private PlayerController _player;

    [SerializeField]
    private RectTransform _mask;

    [SerializeField]
    private RectTransform _indicator;

    void Update()
    {
        var offset = 180.0f * ((100.0f - _player.CurrentHealth) / 100.0f);
        _mask.anchoredPosition = new Vector2(0.0f, -offset);
        _indicator.anchoredPosition = new Vector2(0.0f, offset);
    }
}
