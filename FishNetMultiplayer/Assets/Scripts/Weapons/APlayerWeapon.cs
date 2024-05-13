using FishNet.Component.Animating;
using FishNet.Object;
using FishNet.Serializing.Helping;
using System.Collections;
using UnityEngine;

public abstract class APlayerWeapon : NetworkBehaviour
{
    public int referenceIndex = -1;

    public bool isAuto = false;
    public int damage;
    public float fireRate = 0.5f;
    public float maxRange = 20f;
    [SerializeField] public float muzzleFlashTime = 0.2f;

    public LayerMask weaponHitLayer;

    [SerializeField] private ParticleSystem muzzleFlashParticles;
    [SerializeField] private GameObject muzzleFlashSprite;
    private Transform cameraTransform;

    private float lastFireTime;
    private NetworkAnimator networkAnimator;
    private void Awake()
    {
        cameraTransform = Camera.main.transform;
        if (TryGetComponent(out NetworkAnimator networkAni))
            networkAnimator = networkAni;
    }
    public void Fire()
    {
        if (Time.time < lastFireTime + fireRate)
        {
            return;
        }

        lastFireTime = Time.time;
        AnimateWeapon();
        if (!Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, maxRange, weaponHitLayer))
        {
            return;
        }
        if(hit.transform.gameObject.TryGetComponent(out PlayerHealth health))
        {
            health.TakeDamage(damage, OwnerId);
        }
    }
    [ServerRpc]
    public virtual void AnimateWeapon()
    {
        //MuzzleFlash(muzzleFlashSprite);
        PlayerAnimationObserver();
    }
    [ObserversRpc]
    private void PlayerAnimationObserver()
    {
        //muzzleFlashParticles.Play();
        networkAnimator.SetTrigger("OnFire");
    }
    private void MuzzleFlash(GameObject muzzleSprite)
    {
        muzzleSprite.SetActive(true);
        float shotTime = Time.time;
        if (Time.time > shotTime + muzzleFlashTime)
        {
            muzzleSprite.SetActive(false);
        }
    }
}
