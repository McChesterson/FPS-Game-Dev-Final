using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWepon : NetworkBehaviour
{
    public List<APlayerWeapon> weapons = new List<APlayerWeapon>();
    public List<APlayerWeapon> heldWeapons = new List<APlayerWeapon>();

    [SerializeField] APlayerWeapon currentWeapon;
    private readonly SyncVar<int> currentWeaponIndex = new(-1);

    [SerializeField] int maxInvSize;
    bool holdsWeapon(AGroundWeapon _groundWeapon)
    {
        return false;
    }

    private void Awake()
    {
        currentWeaponIndex.OnChange += OnCurrentWeaponIndexChanged;
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            enabled = false;
            return;
        }
    }
    private void Update()
    {
        
        if (currentWeapon != null)
        {
            if (currentWeapon.isAuto && Input.GetKey(KeyCode.Mouse0))
            {
                FireWeapon();
            }
            else if(Input.GetKeyDown(KeyCode.Mouse0))
            {
                FireWeapon();
            }

        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (heldWeapons.Count < 1) return;
            InitializeWeapon(heldWeapons[0].referenceIndex);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (heldWeapons.Count < 2) return;
            InitializeWeapon(heldWeapons[1].referenceIndex);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (heldWeapons.Count < 3) return;
            InitializeWeapon(heldWeapons[2].referenceIndex);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (heldWeapons.Count < 4) return;
            InitializeWeapon(heldWeapons[3].referenceIndex);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            if (heldWeapons.Count < 5) return;
            InitializeWeapon(heldWeapons[4].referenceIndex);
        }
    }
    public void InitializeWeapons(Transform parentOfWeapons)
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            weapons[i].transform.SetParent(parentOfWeapons);
            weapons[i].transform.rotation *= Quaternion.Euler(new Vector3(-1, -1, -1));
        }
    }
    //adds a weapon from the weapon list to the held weapons list
    public void AddWeapon(AGroundWeapon _AGroundWeapon)
    {
        //checks if the player already has the weapon their grabbing in their inventory
        if (holdsWeapon(_AGroundWeapon) || heldWeapons.Count == maxInvSize)
        {
            Debug.Log("Player already has weapon being added");
            return;
        }

        heldWeapons.Add(weapons[_AGroundWeapon.weaponIndex]);

        //inializes the weapon that was just added
        InitializeWeapon(_AGroundWeapon.weaponIndex);
    }
    //public void ResetInventory()
    //{
    //    heldWeapons.Clear();
    //    SetWeaponIndex(-1);
    //}
    public bool ContainsInInventory(int groundWeaponIndex)
    {
        for (int i = 0; i < heldWeapons.Count; i++)
        {
            if (heldWeapons[i] == weapons[groundWeaponIndex])
            {
                return true;
            }
        }
        return false;
    }
    //sets the current weapon being held
    public void InitializeWeapon(int weaponIndex)
    {
        SetWeaponIndex(weaponIndex);
    }
    [ServerRpc] private void SetWeaponIndex(int weaponIndex) => currentWeaponIndex.Value = weaponIndex;
    private void OnCurrentWeaponIndexChanged(int oldIndex, int newIndex, bool asServer)
    {
        for (int i = 0; i < weapons.Count; i++)
            weapons[i].gameObject.SetActive(false);

        if (weapons.Count > newIndex)
        {
            currentWeapon = weapons[newIndex];
            currentWeapon.gameObject.SetActive(true);
        }
    }
    private void FireWeapon()
    {
        if (heldWeapons.Count == 0) return;
        currentWeapon.Fire();
    }
}