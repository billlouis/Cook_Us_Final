using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerLook : NetworkBehaviour
{
    [SerializeField] public Camera cam;
    private float xRotation = 0f;

    [SerializeField] private float xSensitivity = 30f;
    [SerializeField] private float ySensitivity = 30f;
    [SerializeField] private AudioListener listener;
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            // Disable camera and input for other players
            cam.gameObject.SetActive(false);
            enabled = false;
        }
        else
        {
            listener.enabled = true;
            // Activate local player's camera and input
            cam.gameObject.SetActive(true);
            enabled = true;
        }
    }


    public void ProcessLook(Vector2 input)
    {
        if (!IsOwner) return;

        float mouseX = input.x;
        float mouseY = input.y;

        xRotation -= mouseY * Time.deltaTime * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * mouseX * Time.deltaTime * xSensitivity);
    }
}
