using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TestCharacterController : MonoBehaviour
{
    private CharacterController characterController;
    [SerializeField]
    private float speed = 2f;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    [SerializeField] private float playerSpeed = 2.0f;
    private float jumpHeight = 1.0f;
    [SerializeField]private float gravityValue = -9.81f;
    Vector3 move;
    [SerializeField]
    private float rotationSpeed = 1f;
    public GameObject vCam;

    public float timeBetweenFire =2f;
    float fireTimer = 1f;

    public int damage;
    public LayerMask playerLayer;
    private void Start()
    {
        characterController = gameObject.GetComponent<CharacterController>();
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

    }

    void Update()
    {
        groundedPlayer = characterController.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
        gameObject.transform.forward = new Vector3(vCam.transform.forward.x,0, vCam.transform.forward.z);
        move = characterController.transform.forward * Input.GetAxis("Vertical");

        if (move != Vector3.zero)
        {
            //gameObject.transform.forward = move;
        }

        // Changes the height position of the player..
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            //save initial position?
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }
        if (Input.GetButton("Fire1"))
        {
            Shoot();

            //Debug.Log("Fire");
            //if (fireTimer <= 0)
            //{
            //    fireTimer = timeBetweenFire;
            //}
        }
        //controller.Move(playerVelocity * Time.deltaTime);
    }
    void OnEnemyDown()
    {
        Debug.Log("enemyDown test");
    }
    private void Shoot()
    {
        if (Physics.Raycast(vCam.transform.position, vCam.transform.forward, out RaycastHit hit) && hit.transform.TryGetComponent(out Health health))
        {
            health.OnDamage(1, OnEnemyDown);
            //enemyHealth.health -= damageToGive;
        }
        //ShootServer(damage, Camera.main.transform.position, Camera.main.transform.forward);
    }
    private void FixedUpdate()
    {
        playerVelocity.y += gravityValue * Time.deltaTime;
        characterController.Move(move * Time.deltaTime * speed);
        //if (!base.IsOwner) return;
        characterController.Move(playerVelocity * Time.deltaTime);
    }
}