﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class Player : MonoBehaviourPunCallbacks
{

    

    public enum PlayerState {
        Idle,
        Walking,
        Sprinting,
        Jumping,
        Shooting,
        Reloading,
        Dead,
        InMenu,
        Collecting
    };

    [HideInInspector] public bool mIsDead = false;                    
    public float mMaxHealth = 100f;
    public float mCurrentHealth = 100f;
    public Weapon mPlayerWeapon;
    public Text ammoText;
    public Text healthText;
    public Text collectText;
    public Text[] resourceTexts;
    public PlayerState state;
    public GameObject mMenu;
    public GameObject mPlayerUIPrefab;
    public GameObject mFullBodyMesh;
    public Dictionary<string, int> mResourceDict;

    public static GameObject LocalPlayerInstance;

    void Start(){
        Camera camera = GetComponentInChildren<Camera>();
        if (!photonView.IsMine && PhotonNetwork.IsConnected == true) {
            camera.enabled = false;
            return;
        }
        mPlayerWeapon = GetComponentInChildren<Weapon>();
        mPlayerWeapon.mCam = GetComponentInChildren<Camera>();
        mPlayerWeapon.mOwner = this;
        state = PlayerState.Idle;
        mFullBodyMesh = Get_Player_Mesh();
        this.name = PhotonNetwork.NickName;
        mMenu = GameObject.Find("MenuPanel");
        mResourceDict = new Dictionary<string, int>();
        mResourceDict.Add("Iron", 0);
        mResourceDict.Add("Stone", 0);
        mResourceDict.Add("Wood", 0);

        mPlayerUIPrefab = Instantiate(mPlayerUIPrefab);
        mPlayerUIPrefab.GetComponent<PlayerUI>().SetTarget(this);

        if (photonView.IsMine && PhotonNetwork.IsConnected == true) {
            mFullBodyMesh.SetActive(false);
        }
        
    }

    // Update is called once per frame
    void Update(){


        
        if (!photonView.IsMine && PhotonNetwork.IsConnected) {
            return;
        }
        

        Player_Inputs();

        //Player_Inputs();

        if (state == PlayerState.Collecting)
            Debug.Log("Player is currently collecting");


    }


    void Player_Inputs() {

        Debug.DrawRay(mPlayerWeapon.mCam.transform.position, mPlayerWeapon.mCam.transform.TransformDirection(Vector3.forward) * 3, Color.cyan);

        if(Player_Near_Resource() && Input.GetButtonDown("Interact"))
        {
            state = PlayerState.Collecting;
        }

        //Menu
        if (Input.GetButtonDown("Cancel")) {

            if (state != PlayerState.InMenu) {
                Debug.Log("Entering menu");
                state = PlayerState.InMenu;
                mMenu.SetActive(true);
            } else {
                Debug.Log("Leaving menu");
                state = PlayerState.Idle;
                mMenu.SetActive(false);
            }

        }

        if (Input.GetKeyDown(KeyCode.Q)){
            if (!mFullBodyMesh.activeInHierarchy)
            {
                mFullBodyMesh.SetActive(true);
            }
            else
            {
                mFullBodyMesh.SetActive(false);
            }
        }

        //Single fire
        if (Input.GetButtonDown("Fire1") && mPlayerWeapon.mFire_Type == Weapon.Fire_Type.single && state != PlayerState.InMenu) {
            mPlayerWeapon.Fire_Weapon();
        }

        //Automatic fire
        else if (Input.GetButton("Fire1") && (mPlayerWeapon.mFire_Type == Weapon.Fire_Type.fully_Auto || mPlayerWeapon.mFire_Type == Weapon.Fire_Type.Beam) && state != PlayerState.InMenu) {
            //Debug.Log("Fire fully auto");
            mPlayerWeapon.Fire_Weapon();
        }

        //Reload
        if (Input.GetButtonDown("Reload") || mPlayerWeapon.mAmmoLoaded == 0 && state != PlayerState.InMenu) {
            mPlayerWeapon.Reload_Weapon();
        }

        if(mPlayerWeapon.mIsParticle && !Input.GetButton("Fire1")) {
            mPlayerWeapon.mParticleProjectile.mParticles.Stop();
        }

    }

    public bool Player_Near_Resource()
    {

        Vector3 rayOrigin = mPlayerWeapon.mCam.transform.position;
        RaycastHit[] hits;

        hits = Physics.RaycastAll(rayOrigin, mPlayerWeapon.mCam.transform.TransformDirection(Vector3.forward), 3.0f);

        foreach(RaycastHit hit in hits){

            if (hit.collider.CompareTag("Resource") && state != PlayerState.Collecting){
                mPlayerUIPrefab.GetComponent<PlayerUI>().Player_Near_Resource_Text(("Press F To Collect " + hit.collider.GetComponent<ResourceNode>().Get_Name()));
                //collectText.text = ("Press F To Collect " + hit.collider.GetComponent<ResourceNode>().Get_Name());
                return true;
            }
        }
        mPlayerUIPrefab.GetComponent<PlayerUI>().Player_Near_Resource_Text("");
        /*
        if (Physics.Raycast(rayOrigin, mPlayerWeapon.mCam.transform.TransformDirection(Vector3.forward), out hit, 3.0f))
        { 

            if (hit.collider.CompareTag("Resource"))
            {
                collectText.gameObject.SetActive(true);
                collectText.text = ("Press F To Collect " + hit.collider.GetComponent<ResourceNode>().Get_Name());
                return true;
            }
        }
        */
        //collectText.gameObject.SetActive(false);
        return false;
    }

    GameObject Get_Player_Mesh(){

        GameObject mesh = null;

        for(int i = 0; i < transform.childCount; i++){
            if (transform.GetChild(i).CompareTag("Mesh")){
                mesh = transform.GetChild(i).gameObject;
            }
        }

        if(mesh == null){
            Debug.LogError("Mesh is null");
        }
        return mesh;

    }

    [PunRPC]
    public void Take_Damage(float damage) {

        mCurrentHealth -= damage;

        if(mCurrentHealth < 0) {
            mCurrentHealth = 0;
        }
    }

    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {

    //    if (stream.IsWriting) {
    //        stream.SendNext(this.mCurrentHealth);
    //    } else {
    //        this.mCurrentHealth = (float)stream.ReceiveNext();
    //    }
    //}

    

}
