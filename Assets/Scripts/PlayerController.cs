using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerTeam
    {
        Team1,
        Team2
    }
    private int isGrounded;
    //private AudioSource audioSource;
    [SerializeField] PlayerTeam playerTeam;
    private Rigidbody2D rigidBody;
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

    private float playerLives = 3;

    public PlayerController()
    {
        canPlayerMove = true;
        maxThreshold = 0.2f;
        acceleration = 0.0f;
    }
    private void Awake()
    {
        //audioSource = GetComponent<AudioSource>();
        rigidBody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (canPlayerMove)
        {
            //Debug.Log("isgrounded: " + IsGrounded());


            rigidBody.velocity = new Vector2(horizontal + acceleration, rigidBody.velocity.y + jumpPower + jumpIntensity);
            jumpPower = 0.0f;
            Debug.Log("Velocity: " + rigidBody.velocity);
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
        if(hasDied)
        {
            StartCoroutine(respawnSequence());
        }
    }


    private void Update()
    {
        //if player is this team
        if(playerTeam == PlayerTeam.Team1)
        {
            Debug.Log("isgrounded: " + isGrounded);
        }
        
        if (canPlayerMove)
        {
            ManageMovement();
            transform.rotation = Quaternion.Euler(0, transform.rotation.y, transform.rotation.z);
        }
    }

    private void ManageMovement()
    {
        if(isRespawning)
        {
            horizontal = Input.GetAxis("Horizontal") * 1.5f;
            //fix later
            //if (Input.GetButtonDown("Jump"))
            //{
            //    isRespawning = false;
            //    StopCoroutine(respawnSequence());
            //    
            //}
        }
        else if (playerTeam == PlayerTeam.Team1)
        {
            horizontal = Input.GetAxis("Horizontal");

            if(horizontal > 0)
            {
                if(acceleration < 4.0f)
                {
                    acceleration += 0.1f;
                }
            }
            else if(horizontal < 0)
            {
                if(acceleration > -4.0f)
                {
                    acceleration -= 0.1f;
                }
            }
            else
            {
                if(acceleration > 0)
                {
                    acceleration -= 0.2f;
                }
                else if(acceleration < 0)
                {
                    acceleration += 0.2f;
                }
            }

            if (Input.GetButtonDown("Jump") && IsGrounded())
            {
                Debug.Log("Jump");
                jumpPower = 3.0f;
                //jumpIntensity = 10.0f;


                if(System.Math.Abs(acceleration) > 1.0f && System.Math.Abs(acceleration) < 2.0f)
                {
                    jumpIntensity = 10.5f;
                }
                else if(System.Math.Abs(acceleration) > 2.0f && System.Math.Abs(acceleration) < 3.0f)
                {
                    jumpIntensity = 2.0f;
                }
                else if(System.Math.Abs(acceleration) > 3.0f )
                {
                    jumpIntensity = 5.0f;
                }
            }
        }
    }

    private IEnumerator respawnSequence()
    {
        this.GetComponent<Rigidbody2D>().isKinematic = true;
        canPlayerMove = true;
        this.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionY; 
        hasDied = false;
        isRespawning = true;
        for (int i = 0; i < 3; i++)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            yield return new WaitForSeconds(0.5f);
            GetComponent<SpriteRenderer>().enabled = true;
            yield return new WaitForSeconds(0.5f);
        }
        //respawn();
        isRespawning = false;
        canPlayerMove = true;
        this.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        this.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        this.GetComponent<Rigidbody2D>().isKinematic = false;
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
                playerLives += 1;
            }

            if ( Collider2D.gameObject.tag == "wall")
            {
                Debug.Log("Hit wall");

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
                //1
                Debug.Log("player died");
                //StartCoroutine(ManageDeath());
                gameObject.SetActive(false);

            }
            if (collision.gameObject.tag == "Bullet")
            {
                if (collision.gameObject.GetComponent<BulletsManager>().GetBulletTeam().ToString() == playerTeam.ToString())
                {
                    return;
                }
                else
                {
                    StartCoroutine(Blink());
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
            if ( collision.gameObject.tag == "Block")
            {
                isGrounded++;
                collisionThreshold = 0;
                maxThreshold = 0.2f;
            }
        }

    }

    private IEnumerator Blink()
    {
        for (int i = 0; i < 3; i++)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            yield return new WaitForSeconds(0.1f);
            GetComponent<SpriteRenderer>().enabled = true;
            yield return new WaitForSeconds(0.1f);
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
