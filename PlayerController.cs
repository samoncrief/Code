using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //basic variables
    public bool isDead = false;
    bool environmentTag; //used to tell if raycast hit the environment or not
    int shotSpread = 30; //factor used in shotgun spread
    public int playerHP;
    public int playerClass;
    public float speed;
    public float canFire;
    public float abilityTimer;
    public float mercyTimer;
    Ray cursorRay;
    Ray rayGun;
    Vector3 moveDir;
    public Vector3 cursorWorldPos;
    Vector3 dir;
    Vector3 dir2;
    Vector3 weaponHitPos;
    Vector3[] linePoints;
    RaycastHit hit;
    RaycastHit weaponHit;
    RaycastHit[] weaponHits;
    WaitForSeconds shotDuration = new WaitForSeconds(0.1f);
    public AudioClip[] soundArray = new AudioClip[9];

    //components
    CharacterController controller;
    LineRenderer line;
    public AudioSource playerSounds;

    //game objects
    public Camera playerCam;
    public Animator anim;
    public GameObject model;
    public GameObject modelMask;
    public GameObject weaponMag;
    public GameObject shotPrefab;
    public GameObject powerShotPrefab;
    public GameObject cursorPoint;
    public GameObject fWallPrefab;
    public GameObject DFMPrefab;
    public GameObject grenadePrefab;
    public GameObject minePrefab;


    // Start is called before the first frame update
    void Start()
    {
        Physics.IgnoreLayerCollision(gameObject.layer, gameObject.layer); //ignore player-player collisions
        GameManager.Instance.player = gameObject; //pass player to game manager
        playerClass = GameManager.Instance.playerClass;
        canFire = 0;
        abilityTimer = 0;
        speed = 30;
        controller = GetComponent<CharacterController>();
        playerHP = 10;
        line = gameObject.AddComponent<LineRenderer>();
        anim = model.GetComponent<Animator>();
        line.positionCount = 2;
        line.widthMultiplier = 0.2f;
        line.material = modelMask.GetComponent<SkinnedMeshRenderer>().material;
        line.enabled = false;
        mercyTimer = Time.time;
        playerSounds = GetComponent<AudioSource>();

        switch (playerClass)
        {
            case 1://hunter
                modelMask.GetComponent<SkinnedMeshRenderer>().material.color = Color.green;
                weaponMag.GetComponent<SkinnedMeshRenderer>().material.color = Color.green;
                break;
            case 2://bomber
                modelMask.GetComponent<SkinnedMeshRenderer>().material.color = Color.magenta;
                weaponMag.GetComponent<SkinnedMeshRenderer>().material.color = Color.magenta;
                break;
            default://soldier
                //modelMask.GetComponent<MeshRenderer>().material.color = Color.orange;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //character movement
        if (!isDead)
        {
            moveDir = (Vector3.forward * speed * Input.GetAxis("Vertical")) + (Vector3.right * speed * Input.GetAxis("Horizontal"));
            controller.SimpleMove(moveDir);

            if(moveDir == Vector3.zero)
            {
                anim.SetBool("moving", false);
            }
            else if (moveDir != Vector3.zero)
            {
                anim.SetBool("moving", true);
            }

            if (anim.GetBool("moving"))
            {
                anim.SetFloat("dotProd", Mathf.Abs(Vector3.Dot(Vector3.Normalize(moveDir), Vector3.Normalize(dir - Vector3.up * dir.y))));
            }
        }

        if (isDead == false && playerHP < 1)
        {
            isDead = true;
        }

        //firing weapons based on class
        switch (playerClass)
        {
            case (1):
                HunterWeapon();
                break;
            case (2):
                BomberWeapon();
                break;
            default:
                SoldierWeapon();
                break;

        }
        cursorPoint.transform.position = new Vector3(cursorWorldPos.x, 1, cursorWorldPos.z);
    }
        

    private void FixedUpdate()
    {
        //character direction
        if (!isDead)
        {
            cursorRay = playerCam.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(cursorRay, out hit, 1000, ~(LayerMask.GetMask("Entity") | LayerMask.GetMask("Player")));
            cursorWorldPos = hit.point;
            dir = model.transform.position - cursorWorldPos;
            float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            model.transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Enemy" && playerHP > 0 && Time.time > mercyTimer) //take damage from touching enemies
        {
            playerHP--;
            mercyTimer = Time.time + 1;
        }
    }

    private void SoldierWeapon() //called during update while playing soldier
    {
        if (Input.GetMouseButton(0) && Time.time > canFire && !isDead)
        {
            dir2 = new Vector3(dir.x, 0, dir.z);
            Instantiate(shotPrefab, this.transform.position, this.transform.rotation).GetComponent<ShotScript>().angle = -Vector3.Normalize(dir2);
            canFire = Time.time + 0.2f;
            playerSounds.clip = soundArray[0];
            playerSounds.Play();
        }

        if (Input.GetMouseButton(1) && Time.time > canFire && !isDead)
        {
            dir2 = new Vector3(dir.x, 0, dir.z);
            dir2 = -Vector3.Normalize(dir2);
            for (int i = -3; i < 4; i++)
            {
                Instantiate(shotPrefab, this.transform.position, this.transform.rotation).GetComponent<ShotScript>().angle = new Vector3(dir2.x * Mathf.Cos(Mathf.PI * i / shotSpread) - dir2.z * Mathf.Sin(Mathf.PI * i / shotSpread), 0, dir2.x * Mathf.Sin(Mathf.PI * i / shotSpread) + dir2.z * Mathf.Cos(Mathf.PI * i / shotSpread));
            }
            canFire = Time.time + 0.8f;
            playerSounds.clip = soundArray[1];
            playerSounds.Play();
        }

        if (Input.GetButtonDown("Jump") && Time.time > abilityTimer && !isDead)
        {
            Instantiate(fWallPrefab, cursorPoint.transform.position + Vector3.up * 5, model.transform.rotation);
            playerSounds.clip = soundArray[3];
            playerSounds.Play();
            abilityTimer = Time.time + 5;
        }
    }

    private void HunterWeapon() //called during update while playing hunter
    {
        if (Input.GetMouseButton(0) && Time.time > canFire && !isDead)
        {
            dir2 = new Vector3(dir.x, 0, dir.z);
            Instantiate(shotPrefab, this.transform.position, this.transform.rotation).GetComponent<ShotScript>().DAM(4, -Vector3.Normalize(dir2));
            canFire = Time.time + 0.8f;
            playerSounds.clip = soundArray[1];
            playerSounds.Play();
        }

        if (Input.GetMouseButton(1) && Time.time > canFire && !isDead) //raycast through enemies, dealing damage to everything in a line
        {
            dir2 = new Vector3(dir.x, 0, dir.z);
            rayGun = new Ray(this.transform.position, -Vector3.Normalize(dir2));
            environmentTag = Physics.Raycast(rayGun, out weaponHit, 1000, ~LayerMask.GetMask("Entity"));
            if (environmentTag)
            {
                weaponHits = Physics.RaycastAll(this.transform.position, -Vector3.Normalize(dir2), weaponHit.distance);
            }
            else
            {
                weaponHits = Physics.RaycastAll(this.transform.position, -model.transform.forward, 1000);
            }
            
            for(int i = 0; i < weaponHits.Length; i++)
            {
                if(weaponHits[i].collider.gameObject.tag == "Enemy")
                {
                    weaponHits[i].collider.gameObject.GetComponent<EnemyController>().enemyHP -= 4;
                }
            }
            if (environmentTag) { linePoints = new Vector3[2] { this.transform.position, weaponHit.point }; }
            else { linePoints = new Vector3[2] { this.transform.position, this.transform.position - 1000 * model.transform.forward}; }
            line.SetPositions(linePoints);
            StartCoroutine(shotEffect());
            canFire = Time.time + 1.2f;
            
        }

        if (Input.GetButtonDown("Jump") && Time.time > abilityTimer && !isDead)
        {
            dir2 = new Vector3(dir.x, 0, dir.z);
            Instantiate(powerShotPrefab, this.transform.position, this.transform.rotation).GetComponent<ShotScript>().DAM(10, -Vector3.Normalize(dir2));
            canFire = Time.time + 0.3f;
            playerSounds.clip = soundArray[1];
            playerSounds.Play();
            abilityTimer = Time.time + 5;
        }
    }

    private void BomberWeapon() //called during update while playing bomber
    {
        if (Input.GetMouseButton(0) && Time.time > canFire && !isDead)
        {
            dir2 = new Vector3(dir.x, 0, dir.z);
            Instantiate(grenadePrefab, this.transform.position, this.transform.rotation).GetComponent<BombScript>().angle = -Vector3.Normalize(dir2);
            canFire = Time.time + 1.2f;
            playerSounds.clip = soundArray[1];
            playerSounds.Play();
        }

        if (Input.GetMouseButton(1) && Time.time > canFire && !isDead)
        {
            dir2 = new Vector3(dir.x, 0, dir.z);
            Instantiate(minePrefab, this.transform.position, this.transform.rotation).GetComponent<BombScript>().angle = -Vector3.Normalize(dir2);
            canFire = Time.time + 1.2f;
            playerSounds.clip = soundArray[1];
            playerSounds.Play();
        }

        if (Input.GetButtonDown("Jump") && Time.time > abilityTimer && !isDead)
        {
            Instantiate(DFMPrefab, cursorPoint.transform.position + Vector3.up * 5, new Quaternion(0, 0, 0, 0));
            playerSounds.clip = soundArray[3];
            playerSounds.Play();
            abilityTimer = Time.time + 5;
        }
    }

    IEnumerator shotEffect() //coroutine for aftereffect on hunter's pierce shot
    {
        playerSounds.clip = soundArray[2];
        playerSounds.Play();
        line.enabled = true;
        yield return shotDuration;
        line.enabled = false;
    }
}
