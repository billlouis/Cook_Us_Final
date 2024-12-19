using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vegetable : Interactable
{
    private Damageable damageable;

    void Start()
    {
        damageable = GetComponent<Damageable>();
    }

    protected override void Interact()
    {
        if (damageable != null && damageable.isParalyzed)
        {
            PlayerInteract playerInteract = FindObjectOfType<PlayerInteract>();
            damageable.PickUp(playerInteract.pickupPosition);
        }
        else
        {
            Debug.Log("This object cannot be picked up yet.");
        }
    }
}

