using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerTeam
    {
        Blue,
        Red
    }
    private int isGrounded;
    [SerializeField] PlayerTeam team;
    private Rigidbody2D rigidBody;
    private Animator animator;
    private float horizontal;
    private int collisionThreshold;
    private float jumpTimer;
    private float jumpIntensity;
    private float jumpPower;
    private float maxThreshold;
    private bool hasDied;
    private bool canPlayerMove;
    private float deathTimer;
    private float acceleration;
    public bool isRespawning = false;
    private bool isInvicible = false;
    [SerializeField] private Finder finders;
    GameSceneManager managerOfTheScene;


    public PlayerController()
    {
        canPlayerMove = true;
        maxThreshold = 0.2f;
        acceleration = 0.0f;
    }
    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        managerOfTheScene = FindObjectOfType<GameSceneManager>();
    }

    private void FixedUpdate()
    {
        if (canPlayerMove)
        {
            rigidBody.velocity = new Vector2(horizontal + acceleration, rigidBody.velocity.y + jumpPower + jumpIntensity);
            jumpPower = 0.0f;
            animator.SetBool("Is Jumping", false);
        }

        if (jumpIntensity <= 10.0f)
        {
            jumpIntensity = 0.0f;
        }
    }

    private void OnDisable()
    {
        rigidBody.velocity = new Vector2(0.0f, 0.0f);
        hasDied = true;
        canPlayerMove = false;
        acceleration = 0.0f;
        jumpIntensity = 0.0f;
        jumpPower = 0.0f;
    }

    private void OnEnable()
    {
        if (hasDied)
        {
            respawnSequence();
        }
    }

    private void Update()
    {
        if (canPlayerMove)
        {
            ManageMovement();
            transform.rotation = Quaternion.Euler(0, transform.rotation.y, transform.rotation.z);
            animator.SetFloat("Speed", horizontal);
        }
    }

    private void ManageMovement()
    {
        if (team == PlayerTeam.Blue)
        {
            horizontal = Input.GetAxis("Horizontal");
        }
        else if (team == PlayerTeam.Red)
        {
            horizontal = Input.GetAxis("HorizontalP2");
        }
        if (isRespawning)
        {
            horizontal *= 1.5f;
            //fix later
            //if (Input.GetButtonDown("Jump"))
            //{
            //    isRespawning = false;
            //    StopCoroutine(respawnSequence());
            //
            //}
        }
        else
        {
            if (horizontal > 0)
            {
                if (acceleration < 4.0f)
                {
                    acceleration += 0.1f;
                }
            }
            else if (horizontal < 0)
            {
                if (acceleration > -4.0f)
                {
                    acceleration -= 0.1f;
                }
            }
            else
            {
                if (acceleration > 0)
                {
                    acceleration -= 0.2f;
                }
                else if (acceleration < 0)
                {
                    acceleration += 0.2f;
                }
            }

            if ((Input.GetButtonDown("Jump") && team == PlayerTeam.Blue) || (Input.GetButtonDown("JumpP2") && team == PlayerTeam.Red) && IsGrounded())
            {
                animator.SetBool("Is Jumping", true);
                jumpPower = 3.0f;
                //jumpIntensity = 10.0f;
                if (System.Math.Abs(acceleration) > 1.0f && System.Math.Abs(acceleration) < 2.0f)
                {
                    jumpIntensity = 10.5f;
                }
                else if (System.Math.Abs(acceleration) > 2.0f && System.Math.Abs(acceleration) < 3.0f)
                {
                    jumpIntensity = 2.0f;
                }
                else if (System.Math.Abs(acceleration) > 3.0f)
                {
                    jumpIntensity = 5.0f;
                }
            }
        }
    }

    private void respawnSequence()
    {
        GetComponent<Rigidbody2D>().isKinematic = true;
        canPlayerMove = true;
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionY;
        hasDied = false;
        isRespawning = true;
        StartCoroutine(Blink(0.5f));
        //respawn();
        isRespawning = false;
        canPlayerMove = true;
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        GetComponent<Rigidbody2D>().isKinematic = false;
    }

    public bool isPlayerDeadOrRespawning()
    {
        return hasDied || isRespawning;
    }

    private void OnCollisionEnter2D(Collision2D Collider2D)
    {
        if (!hasDied && !isRespawning)
        {
            if (Collider2D.gameObject.tag == "GreenMushroom")
            {

                managerOfTheScene.AddLife(team);
               
            }

            if (Collider2D.gameObject.tag == "wall")
            {
                float oldAcceleration = acceleration;
                if (rigidBody.velocity.x > 0)
                {
                    acceleration = 0.0f;
                    acceleration = 5.0f;//oldAcceleration * 0.75f;
                }
                else
                {
                    acceleration = 0.0f;
                    acceleration = -5.0f;//-oldAcceleration * 0.75f;
                }
            }
            if (Collider2D.gameObject.tag == "Player")
            {
                Debug.Log("Hit player");
                //fix that later i guess
                Collider2D.gameObject.GetComponent<PlayerController>().acceleration = acceleration;
            }
        }
    }

    public float GetPlayerAcceleration()
    {
        return acceleration;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasDied && !isRespawning)
        {
            if (collision.gameObject.tag == "Despawner")
            {
                Debug.Log("player died");
                //StartCoroutine(ManageDeath());
                gameObject.SetActive(false);

            }
            if (collision.gameObject.tag == "Bullet")
            {
                if (collision.gameObject.GetComponent<BulletsManager>().GetBulletTeam().ToString() == team.ToString())
                {
                    return;
                }
                else
                {
                    StartCoroutine(Blink(0.1f));
                    if (collision.transform.position.x > transform.position.x)
                    {
                        acceleration = -5.0f;
                    }
                    else if (collision.transform.position.x < (double)transform.position.x)
                    {
                        acceleration = 5.0f;
                    }
                    else
                    {
                        acceleration = -5.0f;
                    }
                    rigidBody.AddForce(new Vector2(0, collision.gameObject.GetComponent<Rigidbody2D>().velocity.y * 0.5f), ForceMode2D.Impulse);
                }
            }
            if (collision.gameObject.tag == "Block")
            {
                isGrounded++;
                collisionThreshold = 0;
                maxThreshold = 0.2f;
            }
        }

    }

    private IEnumerator Blink(float delay)
    {
        for (int i = 0; i < 3; i++)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            yield return new WaitForSeconds(delay);
            GetComponent<SpriteRenderer>().enabled = true;
            yield return new WaitForSeconds(delay);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if ((collision.gameObject.tag == "Block") && !IsGrounded())
        {
            if (collision.transform.position.x > transform.position.x)
            {
                collisionThreshold = 1;
            }
            else if (collision.transform.position.x < (double)transform.position.x)
            {
                collisionThreshold = -1;
            }
            else
            {
                collisionThreshold = 0;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Block" && !IsGrounded() && collisionThreshold != 0)
        {
            collisionThreshold = 0;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Block")
        {
            isGrounded--;
        }
    }

    public bool IsGrounded()
    {
        return isGrounded != 0;
    }
}
