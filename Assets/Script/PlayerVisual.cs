using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{


    [SerializeField] private MeshRenderer meshRenderer;


    private Material material;

    private void Awake()
    {
        material = new Material(meshRenderer.material);
        meshRenderer.material = material;
    }

    public void SetPlayerColor(Color color)
    {
        material.color = color;
    }

}