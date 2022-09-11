using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Targetable : NetworkBehaviour     //Questa classe la useremo come una specie di tag, da attaccare a qualunque cosa
{                                              //possa essere targettata dalla fazione avversaria

    [SerializeField] private Transform aimAtPoint = null;

    public Transform GetAimAtPoint()
    {
        return aimAtPoint;
    }
    
}
