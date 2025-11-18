using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticipantController : MonoBehaviour
{
    public bool Player = true;
    public bool Active = true;

    // Personaje
    private Transform playerTr;
    Rigidbody playerRb;
    Animator playerAnim;
    RagdollController playerRagdoll;

    public float maxHealth = 100f;
    public float currentHealth;

    public float playerSpeed = 0f;

    public bool hasPistol = false;
    public bool hasRiffle = false;

    private Vector2 newDirection;

    // Cámara
    public Transform cameraAxis;
    public Transform cameraTrack;
    public Transform cameraWeaponTrack;
    private Transform theCamera;

    private float rotY = 0f;
    private float rotX = 0f;

    public float camRotSpeed = 200f;
    public float minAngle = -45f;
    public float maxAngle = 45f;
    public float cameraSpeed = 200f;

    // Items
    public GameObject nearItem;
    public GameObject[] itemPrefabs;
    public Transform itemSlot;
    public GameObject crosshair;

    // armas
    public int weapons;
    public GameObject primaryWeapon;
    public GameObject secondaryWeapon;

    void Start()
    {
        playerTr = this.transform;
        playerRb = GetComponent<Rigidbody>();
        playerAnim = GetComponentInChildren<Animator>();
        playerRagdoll = GetComponentInChildren<RagdollController>();

        theCamera = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;

        currentHealth = maxHealth;
        Active = true;
    }

    void Update()
    {
        if (Player)
        {
            MoveLogic();
            CameraLogic();
        }

        if (!Active)
        {
            return;
        }

        ItemLogic();
        AnimLogic();

        if (hasPistol || hasRiffle)
        {
            if (Input.GetAxis("Mouse ScrollWheel") != 0f)
            {
                SwitchWeapon();
            }
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            TakeDamage(10f);
        }
    }

    public void MoveLogic()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float theTime = Time.deltaTime;

        newDirection = new Vector2(moveX, moveZ);

        Vector3 side = playerSpeed * moveX * theTime * playerTr.right;
        Vector3 forward = playerSpeed * moveZ * theTime * playerTr.forward;

        Vector3 endDirection = side + forward;

        playerRb.velocity = endDirection;
    }

    public void CameraLogic()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        float theTime = Time.deltaTime;

        rotY += mouseY * theTime * camRotSpeed;
        rotX = mouseX * theTime * camRotSpeed;

        playerTr.Rotate(0, rotX, 0);

        rotY = Mathf.Clamp(rotY, minAngle, maxAngle);

        Quaternion localRotation = Quaternion.Euler(-rotY, 0, 0);
        cameraAxis.localRotation = localRotation;

        if (hasPistol || hasRiffle)
        {
            cameraTrack.gameObject.SetActive(false);
            cameraWeaponTrack.gameObject.SetActive(true);

            crosshair.gameObject.SetActive(true);

            theCamera.position = Vector3.Lerp(theCamera.position, cameraWeaponTrack.position, cameraSpeed * theTime);
            theCamera.rotation = Quaternion.Lerp(theCamera.rotation, cameraWeaponTrack.rotation, cameraSpeed * theTime);
        }
        else
        {
            cameraTrack.gameObject.SetActive(true);
            cameraWeaponTrack.gameObject.SetActive(false);

            theCamera.position = Vector3.Lerp(theCamera.position, cameraTrack.position, cameraSpeed * theTime);
            theCamera.rotation = Quaternion.Lerp(theCamera.rotation, cameraTrack.rotation, cameraSpeed * theTime);
        }
    }

    public void ItemLogic()
    {
        if (nearItem != null && Input.GetKeyDown(KeyCode.E))
        {
            GameObject instantiatedItem = null;

            bool haveWeapon = false;
            int countWeapons = 0;

            foreach (GameObject itemPrefab in itemPrefabs)
            {
                if (itemPrefab.CompareTag("PW") && nearItem.CompareTag("PW"))
                {
                    instantiatedItem = Instantiate(itemPrefab, itemSlot.position, itemSlot.rotation);
                    primaryWeapon = instantiatedItem;

                    haveWeapon = true;
                    countWeapons++;
                    weapons++;

                    Destroy(nearItem.gameObject);
                    instantiatedItem.transform.parent = itemSlot;
                    nearItem = null;
                    break;
                }
                else if (itemPrefab.CompareTag("SW") && nearItem.CompareTag("SW"))
                {
                    instantiatedItem = Instantiate(itemPrefab, itemSlot.position, itemSlot.rotation);
                    secondaryWeapon = instantiatedItem;

                    haveWeapon = true;
                    countWeapons++;
                    weapons++;

                    Destroy(nearItem.gameObject);
                    instantiatedItem.transform.parent = itemSlot;
                    nearItem = null;
                    break;
                }
            }

            if (haveWeapon && hasPistol && countWeapons > 1)
                hasPistol = false;
            else if (haveWeapon && hasRiffle && countWeapons > 1)
                hasRiffle = false;

            if (instantiatedItem != null && instantiatedItem.CompareTag("PW"))
            {
                hasPistol = true;
                hasRiffle = false;

                if (primaryWeapon != null)
                    primaryWeapon.SetActive(true);
                if (secondaryWeapon != null)
                    secondaryWeapon.SetActive(false);
            }
            else if (instantiatedItem != null && instantiatedItem.CompareTag("SW"))
            {
                hasRiffle = true;
                hasPistol = false;

                if (primaryWeapon != null)
                    primaryWeapon.SetActive(false);
                if (secondaryWeapon != null)
                    secondaryWeapon.SetActive(true);
            }
        }
    }

    public void SwitchWeapon()
    {
        if (primaryWeapon == null || secondaryWeapon == null)
            return;

        bool primaryActive = primaryWeapon.activeSelf;
        bool secondaryActive = secondaryWeapon.activeSelf;

        if (primaryActive)
        {
            primaryWeapon.SetActive(false);
            secondaryWeapon.SetActive(true);

            hasPistol = false;
            hasRiffle = true;
        }
        else
        {
            primaryWeapon.SetActive(true);
            secondaryWeapon.SetActive(false);

            hasPistol = true;
            hasRiffle = false;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            Debug.Log("Moriste Perro!");
            playerRagdoll.Active(true);
            Active = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            nearItem = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            nearItem = null;
        }
    }

    public void AnimLogic()
    {
        playerAnim.SetFloat("X", newDirection.x);
        playerAnim.SetFloat("Y", newDirection.y);

        playerAnim.SetBool("holdPistol", hasPistol);
        playerAnim.SetBool("holdRiffle", hasRiffle);

        if (hasPistol || hasRiffle)
        {
            playerAnim.SetLayerWeight(1, 1);
        }
    }
}
