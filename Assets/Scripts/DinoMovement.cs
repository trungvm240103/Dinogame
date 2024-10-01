using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DinoMovement : MonoBehaviour
{
    public static DinoMovement Instance;

    [SerializeField] private Animator dinoAnimator;
    [SerializeField] float jumpForce;
    [SerializeField] Rigidbody2D rb;
    private bool isGameStarted = false;
    private bool isTouchingGround = false;
    private bool isInvincible = false; 
    private bool isDead = false;


    private void Awake()
    {
        Instance = this;
    }


        // Update is called once per frame
        void Update()
    {
        bool isJumpButtonPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow);
        bool isCrouchButtonPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.DownArrow);
        if (isJumpButtonPressed)
        {
            if(isGameStarted == true && isTouchingGround == true)
            {
                Jump();
            }
            else 
            {
                isGameStarted = true;
                GameManager.Instance.gameStarted = true;
            }

        }
        else if (isCrouchButtonPressed && isTouchingGround == true)
        {
        }
        dinoAnimator.SetBool("StartedGame", isGameStarted);
        dinoAnimator.SetBool("IsCrouching", isCrouchButtonPressed && isTouchingGround && !isJumpButtonPressed);
        dinoAnimator.SetBool("IsDead", isDead);


    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Floor"))
        {
            isTouchingGround = true;
        }
        else if (other.gameObject.CompareTag("Obstacles"))
        {
            if (!isInvincible)
            {
                isDead = true;
                GameManager.Instance.gameEnded = true;
                GameManager.Instance.backgroundMusic.Stop();

                GameManager.Instance.ShowGameEndScreen();

            }
            else
            {
                Destroy(other.gameObject);

 
            }
        }
    }


    void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce);
        isTouchingGround = false;
    }

    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
    }


}

