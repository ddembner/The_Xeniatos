﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public abstract class Weapon : MonoBehaviour
{

    public string mName;        /*<Name of weapon*/
    public float mDamage;       /*<Amount of damage*/
    public float mROF;          /*<Rate of fire*/
    public float mRange;        /*<Max range*/
    public float mReloadSpeed;  /*<Reload speed*/
    public float mSpread;       /*<Weapon spread*/
    public int mAmmoLoaded;     /*<Current loaded ammo*/
    public int mMaxAmmoLoaded;  /*<Max Ammo allowed to be loaded*/
    public int mMaxAmmo;        /*<Maximum carried ammo*/
    public int mAmmoHeld;       /*<Current ammo being held*/
    public bool mIsProjectile;  /*<Does weapon shoot projectiles*/
    [HideInInspector]
    public Player mOwner;

    private float mTimeToNextFire;
    private bool mIsReloading = false;

    public Camera mCam;

    public void Fire_Weapon() {

        if (mIsProjectile) {
            Fire_Projectile();
        } 
        else {
            Fire_Hitscan();
        }

    }

    public void Fire_Hitscan() {

        if(mAmmoLoaded <= 0){
            mAmmoLoaded = 0;
            return;
        }

        if (mIsReloading)
            return;

        if (Time.time > mTimeToNextFire){


            mTimeToNextFire = Time.time + mROF;

            Vector3 rayOrigin = mCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;


            //Debug.DrawRay(rayOrigin, mCam.transform.forward * mRange, Color.cyan);

            if (Physics.Raycast(rayOrigin, mCam.transform.forward, out hit, mRange)){
                if (hit.collider.CompareTag("Player"))
                    Debug.Log("You hit " + hit.collider.name);

                if (hit.collider.CompareTag("Environment")) {
                    hit.collider.GetComponent<Rigidbody>().AddForce(mCam.transform.forward * 100.0f);
                    Debug.Log("You hit an environmental piece named " + hit.collider.name);
                }
            }

            mAmmoLoaded--;
        }

    }

    public void Fire_Projectile() {

    }

    public void Reload_Weapon() {

        if (mIsReloading || mAmmoLoaded == mMaxAmmoLoaded || mAmmoHeld == 0)
            return;


        StartCoroutine(Reloading(mReloadSpeed));
        

    }

    IEnumerator Reloading(float time) {

        mIsReloading = true;
        mOwner.state = Player.PlayerState.Reloading;

        yield return new WaitForSeconds(time);

        if (mAmmoHeld >= mMaxAmmoLoaded && mAmmoLoaded == 0) {
            mAmmoHeld -= mMaxAmmoLoaded;
            mAmmoLoaded = mMaxAmmoLoaded;
        } else if (mAmmoHeld >= mMaxAmmoLoaded && mAmmoLoaded > 0) {
            int difference = mMaxAmmoLoaded - mAmmoLoaded;
            mAmmoLoaded += difference;
            mAmmoHeld -= difference;
        } else {
            mAmmoLoaded = mAmmoHeld;
            mAmmoHeld = 0;
        }

        mIsReloading = false;
    }
}
