using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Player : MonoBehaviourPunCallbacks
{

    #region Variables
    public static bool cursorLocked;

    public float speed;
    public float sprintModifier;
    public float jumpForce;
    public float legnthOfSlide;
    public float slideModifier;
    public float crouchModifier;
    public float slideAmount;
    public float crouchAmount;

    public int maxHealth;

    public Camera normalCam;
    public GameObject cameraParent;
    public GameObject standingCollider;
    public GameObject crouchingCollider;
    public LayerMask ground;
    public Transform groundDetector;
    public Transform weaponParent;

    private Vector3 weaponParentOrigin;
    private Vector3 targetWeaponBobPosition;
    private Vector3 slideDirection;
    private Vector3 camOrigin;
    private Vector3 weaponParentCurrentPos;

    private float baseFOV;
    private float sprintFOVModifier = 1.5f;
    private float movementCounter;
    private float idleCounter;
    private float slideTime;

    private int currentHealth;

    private bool sliding;
    private bool crouched;

    private Transform uiHealthBar;
    private Text uiAmmo;

    private Rigidbody rig;
    private Manager manager;
    private Weapon weapon;
    #endregion

    #region MonoBehaviour Callbacks
    private void Start()
    {
        currentHealth = maxHealth;

        manager = GameObject.Find("Manager").GetComponent<Manager>();
        weapon = GetComponent<Weapon>();

        cameraParent.SetActive(photonView.IsMine);
        //Can shoot players who arent themselves
        if(!photonView.IsMine) gameObject.layer = 11;

        baseFOV = normalCam.fieldOfView;
        camOrigin = normalCam.transform.localPosition;

        if(Camera.main) Camera.main.enabled = false;
        rig = GetComponent<Rigidbody>();
        weaponParentOrigin = weaponParent.localPosition;
        weaponParentCurrentPos = weaponParentOrigin;

        if (photonView.IsMine)
        {
            uiHealthBar = GameObject.Find("HUD/Health/Bar").transform;
            uiAmmo = GameObject.Find("HUD/Ammo/Text").GetComponent<Text>();
            RefreshHealthBar();
        }
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        //Axles
        float t_hmove = Input.GetAxisRaw("Horizontal");
        float t_vmove = Input.GetAxisRaw("Vertical");


        //Controls
        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jump = Input.GetKeyDown(KeyCode.Space);
        bool crouch = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl); //!


        //States
        bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.15f, ground); //!
        bool isJumping = jump && isGrounded;
        bool isSprinting = sprint && t_vmove > 0 && !isJumping && isGrounded;
        bool isCrouching = crouch && !isSprinting && !isJumping && isGrounded; //!


        //Crouching //!
        if (isCrouching)
        {
            photonView.RPC("SetCrouch", RpcTarget.All, !crouched);
        }


        //Jumping
        if (isJumping)
        {
            if (crouched) photonView.RPC("SetCrouch", RpcTarget.All, false); //!
            rig.AddForce(Vector3.up * jumpForce);
        }

        if (Input.GetKeyDown(KeyCode.U)) TakeDamage(10);


        //Head Bob //!
        if (sliding)
        {
            //sliding
            HeadBob(movementCounter, 0.15f, 0.075f);
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);
        }
        else if (t_hmove == 0 && t_vmove == 0)
        {
            //idling
            HeadBob(idleCounter, 0.01f, 0.01f);
            idleCounter += Time.deltaTime;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
        }
        else if (!isSprinting && !crouched)
        {
            //walking
            HeadBob(movementCounter, 0.035f, 0.035f);
            movementCounter += Time.deltaTime * 3f;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
        }
        else if (crouched)
        {
            //crouching
            HeadBob(movementCounter, 0.02f, 0.02f);
            movementCounter += Time.deltaTime * 1.75f;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
        }
        else
        {
            //sprinting
            HeadBob(movementCounter, 0.15f, 0.075f);
            movementCounter += Time.deltaTime * 7f;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);
        }

        //UI Refreshes
        RefreshHealthBar();
        weapon.RefreshAmmo(uiAmmo);
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        //Axles
        float t_hmove = Input.GetAxisRaw("Horizontal");
        float t_vmove = Input.GetAxisRaw("Vertical");


        //Controls
        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jump = Input.GetKeyDown(KeyCode.Space);
        bool slide = Input.GetKey(KeyCode.LeftControl);


        //States
        bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
        bool isJumping = jump && isGrounded;
        bool isSprinting = sprint && t_vmove > 0 && !isJumping && isGrounded;
        bool isSliding = isSprinting && slide && !sliding;


        //Movement
        Vector3 t_direction = Vector3.zero;
        float t_adjustedSpeed = speed;

        if (!sliding)
        {
            t_direction = new Vector3(t_hmove, 0, t_vmove);
            t_direction.Normalize();
            t_direction = transform.TransformDirection(t_direction);

            if (isSprinting)
            {
                if (crouched) photonView.RPC("SetCrouch", RpcTarget.All, false); //!
                t_adjustedSpeed *= sprintModifier;
            }
            else if (crouched)
            {
                t_adjustedSpeed *= crouchModifier; //!
            }
        }
        else
        {
            t_direction = slideDirection;
            t_adjustedSpeed *= slideModifier;
            slideTime -= Time.deltaTime;
            if (slideTime <= 0)
            {
                sliding = false;
                weaponParentCurrentPos -= Vector3.down * (slideAmount - crouchAmount); //
            }
        }

        Vector3 t_targetVelocity = t_direction * t_adjustedSpeed * Time.deltaTime;
        t_targetVelocity.y = rig.velocity.y;
        rig.velocity = t_targetVelocity;


        //Sliding
        if (isSliding)
        {
            sliding = true;
            slideDirection = t_direction;
            slideTime = legnthOfSlide;
            weaponParentCurrentPos += Vector3.down * (slideAmount - crouchAmount); //!
            if (!crouched) photonView.RPC("SetCrouch", RpcTarget.All, true); //!
        }

        //Camera Stuff
        if (sliding)
        {
            normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier * 1.15f, Time.deltaTime * 8f);
            normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, camOrigin + Vector3.down * slideAmount, Time.deltaTime * 6f); //!
        }
        else
        {
            if (isSprinting) { normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier, Time.deltaTime * 8f); }
            else { normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV, Time.deltaTime * 8f); ; }

            if (crouched) normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, camOrigin + Vector3.down * crouchAmount, Time.deltaTime * 6f); //!
            else normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, camOrigin, Time.deltaTime * 6f); //!
        }
    }
    #endregion

    #region Public Methods
    [PunRPC]
    void SetCrouch(bool pState)
    {
        if (crouched == pState) return;

        crouched = pState;

        if (crouched)
        {
            standingCollider.SetActive(false);
            crouchingCollider.SetActive(true);
            weaponParentCurrentPos += Vector3.down * crouchAmount;
        }
        else
        {
            standingCollider.SetActive(true);
            crouchingCollider.SetActive(false);
            weaponParentCurrentPos -= Vector3.down * crouchAmount;
        }
    }

    
    public void TakeDamage(int pDamage)
    {
        if (photonView.IsMine)
        {
            currentHealth -= pDamage;
            RefreshHealthBar();

            if (currentHealth <= 0)
            {
                //Death
                manager.Spawn();
                PhotonNetwork.Destroy(gameObject);
            }
        }

    }
    #endregion

    void HeadBob(float pZ,float pXIntensity,float pYIntensity)
    {
        float aimAdjust = 1f;
        if (weapon.isAiming) aimAdjust = 0.1f;
        targetWeaponBobPosition = weaponParentCurrentPos + new Vector3(Mathf.Cos(pZ) * pXIntensity * aimAdjust, Mathf.Sin(pZ*2) * pYIntensity * aimAdjust, 0);
    }

    void RefreshHealthBar()
    {
        float healthRatio = (float)currentHealth / (float)maxHealth;
        uiHealthBar.localScale = Vector3.Lerp(uiHealthBar.localScale, new Vector3(healthRatio, 1, 1), Time.deltaTime * 8f);
    }
}