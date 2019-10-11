﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;



public abstract class Weapon : MonoBehaviourPunCallbacks
{

    public enum Fire_Type{
        single,
        Semi_Auto,
        fully_Auto,
        Beam
    }

    public string mName;                    /*<mName Name of weapon*/
    public float mDamage;                   /*<Amount of damage*/
    public float mROF;                      /*<Rate of fire*/
    public float mRange;                    /*<Max range*/
    public float mReloadSpeed;              /*<Reload speed*/
    public float mSpread;                   /*<Weapon spread*/
    public int mAmmoLoaded;                 /*<Current loaded ammo*/
    public int mMaxAmmoLoaded;              /*<Max Ammo allowed to be loaded*/
    public int mMaxAmmo;                    /*<Maximum carried ammo*/
    public int mAmmoHeld;                   /*<Current ammo being held*/
    public Fire_Type mFire_Type;            /*<Fire mode for weapon*/
    public bool mIsProjectile;              /*<Does weapon shoot projectiles*/
    public bool mIsParticle;                /*<Does the weapon shoot particles*/
    public ParticleSystem mParticleSystem;  /*<Particle system that the weapon will use for visual effects*/
    public ParticleProjectile mParticleProjectile;  /*<Particle system that the weapon will be firing*/
    public Projectile bullet;               /*<What projectile the weapon will be firing*/
    public Player mOwner;                   /*<The owner of the weapon*/

    private float mTimeToNextFire;
    private bool mIsReloading = false;
    private ObjectPool objectPool;

    public Camera mCam;

    private PhotonView PV;

    public void Start() {
        objectPool = ObjectPool.Instance;
        mOwner = GetComponentInParent<Player>();
        PV = mOwner.GetComponent<PhotonView>();
    }

    public void Fire_Weapon() {

        if (mIsProjectile || mIsParticle) {
            if (mIsProjectile)
                Fire_Projectile();
            else
                Fire_Particles();
            
        } 
        else {
            Fire_Hitscan();
        }

    }
    
    [PunRPC]
    public void Fire_Hitscan() {

        if(mAmmoLoaded <= 0){
            mAmmoLoaded = 0;
            return;
        }

        if (mIsReloading)
            return;

        if (Time.time > mTimeToNextFire){

            mOwner.state = Player.PlayerState.Shooting;
            mTimeToNextFire = Time.time + mROF;

            Vector3 rayOrigin = mCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;


            //Debug.DrawRay(rayOrigin, mCam.transform.forward * mRange, Color.cyan);

            if (Physics.Raycast(rayOrigin, mCam.transform.forward, out hit, mRange)){
                if (hit.collider.CompareTag("Player") && hit.collider.gameObject != mOwner.gameObject) {
                    
                    Debug.Log("You hit " + hit.collider.name + "!");
                    hit.collider.GetComponent<PhotonView>().RPC("Take_Damage", RpcTarget.All, mDamage);
                }
                    

                if (hit.collider.CompareTag("Environment")) {
                    hit.collider.GetComponent<Rigidbody>().AddForce(mCam.transform.forward * 100.0f);
                    Debug.Log("You hit an environmental piece named " + hit.collider.name);
                }
            }

            mAmmoLoaded--;
        }

    }

    public void Fire_Projectile() {

        if (mAmmoLoaded <= 0) {
            mAmmoLoaded = 0;
            return;
        }

        if (mIsReloading)
            return;

        if (Time.time > mTimeToNextFire) {

            mOwner.state = Player.PlayerState.Shooting;
            mTimeToNextFire = Time.time + mROF;

            Projectile projectile = objectPool.SpawnFromPool(bullet.mName).GetComponent<Projectile>();
            projectile.mOwner = mOwner.gameObject;
            projectile.Shoot_Projectile(projectile, mCam.transform, mCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 1.5f)));

            mAmmoLoaded--;
        }
        

    }

    public void Fire_Particles() {

        if (mAmmoLoaded <= 0) {
            mAmmoLoaded = 0;
            if (mParticleProjectile.mParticles.isPlaying) {
                mParticleProjectile.mParticles.Stop();
            }
            return;
        }

        if (mIsReloading) {
            mParticleProjectile.mParticles.Stop();
            return;
        }
            

        if (Time.time > mTimeToNextFire) {
            mParticleProjectile.mOwner = mOwner.gameObject;
            mOwner.state = Player.PlayerState.Shooting;
            mTimeToNextFire = Time.time + mROF;

            Debug.Log(mParticleProjectile.mParticles.name);
            mParticleProjectile.Fire_Particles(mParticleProjectile.mParticles);

            mAmmoLoaded--;
        }


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
        } else if (mAmmoHeld < mMaxAmmoLoaded && mAmmoLoaded > 0) {

            int difference;
            int ammoNeeded = mMaxAmmoLoaded - mAmmoLoaded;
            Debug.Log("Ammo needed = " + ammoNeeded);
            if (ammoNeeded < mAmmoHeld)
                difference = mAmmoHeld - ammoNeeded;
            else
                difference = mAmmoHeld;
            

            mAmmoLoaded += difference;
            mAmmoHeld -= difference;
        } else {
            mAmmoLoaded = mAmmoHeld;
            mAmmoHeld = 0;
        }

        mOwner.state = Player.PlayerState.Idle;
        mIsReloading = false;
    }
}


