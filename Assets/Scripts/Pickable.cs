using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPickable
{
    void OnObjectPick(PlayerController pc);
    void OnObjectPick(Monke monke);
    void OnObjectDrop(GameObject owner);
    GameObject GetObjRef();
}
