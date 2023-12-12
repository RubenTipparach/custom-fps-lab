using UnityEngine;

public class FPSGun : MonoBehaviour
{
    public int totalAmmo = 100;
    public int ammoInMagazine = 30;
    public Transform bulletSpawnPoint;
    public Timing shotCooldown;
    public Timing reloadCooldown;
    public AnimationCurve recoilCurve;
    public AnimationCurve reloadCurve;

    private bool isReloading = false;
    private float recoilTime = 0f;
    private float reloadTime = 0f;
    private Vector3 originalPosition;
    public float recoildMoveAmount = .1f;
    public float reloadMoveAmount = 1f;

    void Start()
    {
        //shotCooldown = new Timing { duration = recoilCurve.keys[recoilCurve.length - 1].time }; // Use the last key time in the recoil curve as the shot cooldown
        //reloadCooldown = new Timing { duration = reloadCurve.keys[reloadCurve.length - 1].time }; // Use the last key time in the reload curve as the reload cooldown

        originalPosition = transform.localPosition;
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && ammoInMagazine > 0 && !isReloading && shotCooldown.Completed())
        {
            FireGun();
        }

        if (Input.GetKeyDown(KeyCode.R) && !isReloading)
        {
            StartReload();
        }

        if (isReloading)
        {
            if (reloadCooldown.Completed())
            {
                FinishReload();
            }
            else
            {
                // Apply reload animation curve
                float reloadStep = reloadCurve.Evaluate(reloadCooldown.GetProgressClamped);
                transform.localPosition = originalPosition + Vector3.up * reloadStep * reloadMoveAmount ;
                //reloadTime += Time.deltaTime;
            }
        }
        else if (!shotCooldown.Completed())
        {
            // Apply recoil animation curve
            float recoilStep = recoilCurve.Evaluate(shotCooldown.GetProgressClamped);
            //transform.localPosition = originalPosition + Vector3.up * recoilStep * recoildMoveAmount;
            transform.localRotation = Quaternion.Euler(recoilStep * recoildMoveAmount, 0, 0);
            //recoilTime += Time.deltaTime;
        }
    }

    void FireGun()
    {
        // Implement the visual and audio effects for firing the gun here.
        shotCooldown.Init(); // Start shot cooldown
        recoilTime = 0f; // Reset recoil animation time

        ammoInMagazine--;

        // Raycast from the center of the screen to detect hits
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
        {
            // Implement hit logic
        }

        // For visual bullet spawning, you can instantiate a bullet prefab here if necessary
    }

    void StartReload()
    {
        isReloading = true;
        reloadCooldown.Init();
        reloadTime = 0f; // Reset reload animation time
        // Implement visual and audio feedback for reloading here
    }

    void FinishReload()
    {
        isReloading = false;
        int ammoNeeded = Mathf.Clamp(ammoInMagazine - totalAmmo, 0, ammoInMagazine);
        totalAmmo -= ammoNeeded;
        ammoInMagazine += ammoNeeded;
        transform.localPosition = originalPosition; // Reset position after reloading
        // Implement any final visual or audio feedback for reloading completion
    }
}