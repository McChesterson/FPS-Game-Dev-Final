using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerPickup : NetworkBehaviour
{
    [SerializeField] private float pickupRange = 4f;
    [SerializeField] private KeyCode pickupKey = KeyCode.E;
    [SerializeField] private LayerMask pickupLayers;
    [SerializeField] private TextMeshProUGUI pickupPrompt;
    public bool pickupPrompOn;
    private Transform cameraTransform;
    private PlayerWepon playerWeapon;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            enabled = false;
            return;
        }
        cameraTransform = Camera.main.transform;
        if (TryGetComponent(out PlayerWepon pWeapon))
            playerWeapon = pWeapon;
        else
            Debug.LogError("Couldn't get player weapon script", gameObject);
    }
    private void Update()
    {
        //if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, pickupRange, pickupLayers))
        //{
        //    if (hit.transform.TryGetComponent(out AGroundWeapon groundWeapon))
        //    {
        //        Debug.Log("Looking at Ground Weapon");
        //    }
        //}
        if (Input.GetKeyDown(pickupKey))
        {
            Pickup();
        }
    }
    private void Pickup()
    {
        Debug.Log("Pressed Pickup Key");
        if (!Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, pickupRange, pickupLayers))
        {
            Debug.Log("Didn't hit anything");
            return;
        }
        //when the item is a weapon
        if (hit.transform.TryGetComponent(out AGroundWeapon groundWeapon))
        {
            Debug.Log("Hit Ground Weapon");
            


            playerWeapon.AddWeapon(groundWeapon);
            DestroyWeapon(hit.transform.gameObject);
        }
        else
            Debug.Log("Didn't hit ground weapon");
    }
    [ServerRpc] void DestroyWeapon(GameObject weapon)
    {
        Destroy(weapon);
    }
}