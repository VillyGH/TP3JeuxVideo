using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotsController : MonoBehaviour
{
    public GameObject gun;
    public GameObject bullet;
    private List<GameObject> bullets = new List<GameObject>();
    private GameObject player;
    public float bulletSpeed = 75f;
    private float bulletLifeTime = 2f;
    private float timeBetweenShots = 0.5f;
    private float timeSinceLastShot = 0f;
    private float bulletDamage = 1f;
    private float bulletSpread = 0.1f;
    private float bulletSize = 0.1f;
    private float bulletSpeedMultiplier = 1f;
    private float bulletDamageMultiplier = 1f;
    private float bulletSpreadMultiplier = 1f;
    private float bulletSizeMultiplier = 1f;
    private float bulletLifeTimeMultiplier = 1f;
    private float bulletSpeedMultiplierDuration = 0f;
    private float bulletDamageMultiplierDuration = 0f;
    private float shotcooldown = 0f;
    private float shootingCooldown = 2f;
    [SerializeField] private Finder finder;
    public int maxBulletsEachTeam = 40;
    AudioSource soundSource;
    private int homingBulletExtraCount = 0;

    void Start()
    {
        //gun = GetChildWithTag(gameObject, "Bullets");
        //bullets = gun.transform.GetChild(0).gameObject;//GetChildsSpawnerActive(gun.transform.GetChild(0).gameObject);
        gun = GetChildWithTag(gameObject, "Gun");
        soundSource = gameObject.GetComponent<AudioSource>();
    }

    void Awake()
    {
        InitBullets();
    }

    private void InitBullets()
    {
        for (int i = 0; i < maxBulletsEachTeam; i++)
        {
            GameObject b = Instantiate(bullet);
            b.gameObject.SetActive(false);
            bullets.Add(b);
        }
    }

    public GameObject[] GetChildsSpawnerActive(GameObject parent)
    {
        GameObject[] array = new GameObject[parent.transform.childCount];
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            //if (parent.transform.GetChild(i).gameObject.activeSelf)
            //{
            array[i] = parent.transform.GetChild(i).gameObject;
            //}
        }
        return array;
    }


    public GameObject GetChildWithTag(GameObject parent, string tag)
    {
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            if (parent.transform.GetChild(i).gameObject.tag == tag)
            {
                return parent.transform.GetChild(i).gameObject;
            }
            else
            {
                GameObject child = GetChildWithTag(parent.transform.GetChild(i).gameObject, tag);
                if (child != null)
                {
                    return child;
                }
            }
        }
        return null;
    }


    void Update()
    {
        if (!PauseManager.GameIsPaused || !GameSceneManager.GameIsEnded)
        {
            shotcooldown = DecreaseCooldown(shotcooldown);
            if (Input.GetButtonDown("Fire1"))
            {
                Debug.Log("Fire1");

                if (shotcooldown <= 0)
                {
                    Debug.Log("Fire1 AGAIN");
                    soundSource.PlayOneShot(SoundManager.Instance.FireBulletSound);
                    GameObject bullet = finder.GetFirstAvailableObject(bullets);
                    SpawnBullet(bullet);
                }
            }
            if (Input.GetButtonDown("Fire2"))
            {
                //if player has homming bullets
                if (homingBulletExtraCount > 0)
                {
                    if (shotcooldown <= 0)
                    {
                        homingBulletExtraCount--;
                        GameObject bullet = finder.GetFirstAvailableObject(bullets);
                        SpawnBullet(bullet);
                    }
                }
            }
        }
    }

    private void SpawnBullet(GameObject bullet)
    {
        if (bullet != null)
        {
            bullet.GetComponent<BulletsManager>().SetHoming(false);
            soundSource.PlayOneShot(SoundManager.Instance.FireBulletSound);
            shotcooldown = shootingCooldown;
            bullet.GetComponent<Rigidbody2D>().transform.position = gun.transform.position;
            bullet.GetComponent<Rigidbody2D>().transform.forward = gun.transform.forward;
            bullet.GetComponent<Rigidbody2D>().velocity = new Vector2(bullet.transform.forward.x, bullet.transform.forward.y) * bulletSpeed;
        }
    }


    public void addHommingBullet()
    {
        homingBulletExtraCount += 3;
    }


    private float DecreaseCooldown(float cooldown)
    {
        if (cooldown > 0)
            return cooldown -= Time.deltaTime;
        else
            return 0;
    }

}
