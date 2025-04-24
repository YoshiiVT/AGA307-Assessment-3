using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : Singleton<CharacterManager>
{
    public CharacterController controller;

    [Header("Variables")]
    public float speed = 12f;
    public float gravity = -9.81f;

    [Header("References")]
    public Transform groundCheck;
    private float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Misc")]
    private Vector3 velocity;
    public bool isGrounded;
    private float sprintSpeed;
    private float currentSpeed;

    [Header("Stamina")]
    public float stamina, maxStamina;
    public Image staminaBar;

    [Header("Living")]
    public bool isAlive = true;

    // For sprinting state
    private bool isSprinting = false;

    // Stamina regeneration speed (can be adjusted in Unity Inspector)
    public float regenSpeed = 5f;

    private void Start()
    {
        sprintSpeed = speed * 2;
        currentSpeed = speed;  // Start with normal speed
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Get movement input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Determine direction of movement
        Vector3 move = transform.right * x + transform.forward * z;

        // Sprint only if moving forward
        if (z > 0 && isGrounded && Input.GetButton("Sprint"))
        {
            StartCoroutine(SprintUp());
        }
        else if (stamina <= 0)  // Stop sprinting when stamina reaches 0
        {
            StopSprinting();
        }
        else  // Reset to normal speed if not sprinting
        {
            StopSprinting();
        }

        // Move the character
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Move with gravity applied
        controller.Move(velocity * Time.deltaTime);

        // Update stamina bar and deplete stamina over time while sprinting
        if (isSprinting && stamina > 0)
        {
            stamina -= Time.deltaTime * 10f; // Deplete stamina while sprinting
        }

        // Regenerate stamina when not sprinting, clamp it to maxStamina
        if (!isSprinting && stamina < maxStamina)
        {
            stamina += Time.deltaTime * regenSpeed;  // Regenerate stamina
            stamina = Mathf.Clamp(stamina, 0, maxStamina);  // Ensure stamina doesn't exceed maxStamina
        }

        // Update the stamina bar UI
        staminaBar.fillAmount = stamina / maxStamina;
    }

    // Smoothly increase speed to sprint speed
    public IEnumerator SprintUp()
    {
        isSprinting = true;

        float sprintDuration = 0.5f;  // Duration to reach sprint speed
        float targetSpeed = sprintSpeed;
        float startSpeed = currentSpeed;
        float timeElapsed = 0f;

        while (timeElapsed < sprintDuration && stamina > 0)
        {
            currentSpeed = Mathf.Lerp(startSpeed, targetSpeed, timeElapsed / sprintDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        currentSpeed = targetSpeed;

        // Stop sprinting if stamina runs out
        if (stamina <= 0)
        {
            StopSprinting();
        }
    }

    // Stops sprinting when stamina runs out
    private void StopSprinting()
    {
        isSprinting = false;
        currentSpeed = speed;  // Reset speed to normal
    }
    public void CaughtPlayer()
    {
        Debug.Log("You Have Beem Caught");
        isAlive = false;
    }
}
