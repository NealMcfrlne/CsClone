using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Gun",menuName ="Gun")]
public class Gun : ScriptableObject
{
    public string gunName;
    public GameObject prefab;
    public float rateOfFire;
    public float aimSpeed;
    public float bloom;
    public float recoil;
    public float kickback;
    public float reloadTime;
    public int damage;
    public int ammo;
    public int clipSize;
    public int burst;//0 semi | 1 auto | 2+ burst

    private int currentAmmo;//stash
    private int clip;

    public void Initialize()
    {
        currentAmmo = ammo;
        clip = clipSize;
    }

    public bool FireBullet()
    {
        if (clip > 0)
        {
            clip -= 1;
            return true;
        }
        else return false;
    }

    public void Reload()
    {
        currentAmmo += clip;
        clip = Mathf.Min(clipSize, currentAmmo);
        currentAmmo -= clip;
    }

    public int GetCurrentAmmo() { return currentAmmo; }
    public int GetClip() { return clip; }
}
