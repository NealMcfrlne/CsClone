using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Photon.Pun;
using UnityEngine.UI;

public class Weapon : MonoBehaviourPunCallbacks
{
    public Gun[] loadout;
    public Transform weaponParent;
    public GameObject bullletholePrefab;
    public LayerMask canBeShot;
    public bool isAiming = false;

    private float currentCooldown;
    private int currentIndex;
    private GameObject currentWeapon;
    private bool isReloading = false;
    // Start is called before the first frame update
    void Start()
    {
        foreach (Gun g in loadout) g.Initialize();
        Equip(0);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha1))
        {
            photonView.RPC("Equip", RpcTarget.All, 0);
        }
        if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha2))
        {
            photonView.RPC("Equip", RpcTarget.All, 1);
        }

        if (currentWeapon != null)
        {
            if(photonView.IsMine)
            {
                Aim(Input.GetMouseButton(1));

                if(loadout[currentIndex].burst != 1)
                {
                    if (Input.GetMouseButtonDown(0) && currentCooldown <= 0)
                    {
                        if (!isReloading)
                        {
                            if (loadout[currentIndex].FireBullet()) photonView.RPC("Shoot", RpcTarget.All);
                            else StartCoroutine(Reload(loadout[currentIndex].reloadTime));
                        }
                    }

                }
                else
                {
                    if (Input.GetMouseButton(0) && currentCooldown <= 0)
                    {
                        if (!isReloading)
                        {
                            if (loadout[currentIndex].FireBullet()) photonView.RPC("Shoot", RpcTarget.All);
                            else StartCoroutine(Reload(loadout[currentIndex].reloadTime));
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.R)) StartCoroutine(Reload(loadout[currentIndex].reloadTime));

                //if(loadout[currentIndex].GetClip()==0&& loadout[currentIndex].GetCurrentAmmo()>0) StartCoroutine(Reload(loadout[currentIndex].reloadTime));

                //Cooldown
                if (currentCooldown > 0)
                {
                    currentCooldown -= Time.deltaTime;
                }
            }
            //Weapon elasticity
            currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);

        }            
    }

    IEnumerator Reload(float pWait)
    {
        isReloading = true;
        currentWeapon.SetActive(false);
        yield return new WaitForSeconds(pWait);
        loadout[currentIndex].Reload();

        currentWeapon.SetActive(true);
        isReloading = false;        
    }

    //Sets this function as a remote procedural call
    [PunRPC]
    void Equip(int pIndex)
    {
        if (currentWeapon != null)
        {
            if(isReloading) StopCoroutine("Reload");
            Destroy(currentWeapon);
        }

        currentIndex = pIndex;

        GameObject newWeapon = Instantiate(loadout[pIndex].prefab, weaponParent.position, weaponParent.rotation,weaponParent) as GameObject;
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localEulerAngles = Vector3.zero;
        newWeapon.GetComponent<Sway>().isMine = photonView.IsMine;

        currentWeapon = newWeapon;
    }

    void Aim(bool pisAiming)
    {
        isAiming = pisAiming;
        Transform anchor = currentWeapon.transform.Find("Anchor");
        Transform stateADS = currentWeapon.transform.Find("States/ADS");
        Transform stateHip = currentWeapon.transform.Find("States/Hip");

        if (pisAiming)
        {
            //ADS
            anchor.position = Vector3.Lerp(anchor.position, stateADS.position, Time.deltaTime * loadout[currentIndex].aimSpeed);
        }
        else
        {
            //Hip
            anchor.position = Vector3.Lerp(anchor.position, stateHip.position, Time.deltaTime * loadout[currentIndex].aimSpeed);
        }
    }

    [PunRPC]
    void Shoot()
    {
        Transform spawn = transform.Find("Cameras/Normal Camera");
        //Transform spawn = transform.Find("Weapon/Pistol(Clone)/Anchor/Design/Barrel");

        //Bloom
        Vector3 bloom = spawn.position + spawn.forward * 1000f;
        bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * spawn.up;
        bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * spawn.right;
        bloom -= spawn.position;
        bloom.Normalize();


        //Raycast
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(spawn.position, bloom, out hit, 1000f, canBeShot))
        {
            GameObject newBulletHole = Instantiate(bullletholePrefab, hit.point + hit.normal * 0.001f, Quaternion.identity) as GameObject;
            newBulletHole.transform.LookAt(hit.point + hit.normal);
            Destroy(newBulletHole, 5f);
            
            if(photonView.IsMine)
            {
                //If another player is hit
                if(hit.collider.gameObject.layer==11)
                {
                    hit.collider.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, loadout[currentIndex].damage);
                }
            }
        }

        //Recoil
        currentWeapon.transform.Rotate(-loadout[currentIndex].recoil, 0, 0);
        //Kickback
        currentWeapon.transform.position -= currentWeapon.transform.forward * loadout[currentIndex].kickback;

        //Cooldown
        currentCooldown = loadout[currentIndex].rateOfFire;
    }

    [PunRPC]
    private void TakeDamage(int pDamage)
    {
        GetComponent<Player>().TakeDamage(pDamage);
    }

    public void RefreshAmmo(Text pText)
    {
        int clip = loadout[currentIndex].GetClip();
        int ammo = loadout[currentIndex].GetCurrentAmmo();

        pText.text = clip.ToString("D2") + "/" + ammo.ToString("D2");
    }
}
