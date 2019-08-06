using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    bool freePhysics; //toggle to represent "free" rigidbody physics instead of nav mesh agent physics
    bool see; //store result of the raycast to player
    public float dist; //distance from player
    float enemyShotTime; //timer to set up next shot for ranged enemies
    float physicsTimer; //timer for free physics situations
    public int enemyHP;
    public int enemyClass;
    public GameObject target; //will be the player
    public Vector3 dir2target; //vector to target
    Rigidbody rb;
    GameManager GameManager;
    public NavMeshAgent agent;
    Renderer[] M;
    RaycastHit sight;

    public GameObject enemyShotPrefab;
    public GameObject deathParticles;

    //enemy classes
    //0: Small
    //1: Medium (ranged)
    //2: Large (tank)
    //3: Invisible

    // Start is called before the first frame update
    void Start()
    {
        enemyShotTime = Time.time;
        target = GameManager.Instance.player;
        agent = GetComponent<NavMeshAgent>();
        agent.destination = target.transform.position;
        M = GetComponentsInChildren<Renderer>();
        rb = GetComponent<Rigidbody>();
        freePhysics = false;
        see = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!freePhysics) //using nav agent
        {
            if (target != null)
            {
                agent.destination = target.transform.position;
                dir2target = target.transform.position - transform.position;
                dir2target -= new Vector3(0, dir2target.y, 0);
            }

            if (enemyClass == 1) //ranged controls
            {
                if(sight.collider != null && sight.collider.gameObject.tag == "Player" && Time.time > enemyShotTime)
                {
                    Instantiate(enemyShotPrefab, this.transform.position + 5 * Vector3.up, new Quaternion(0, 0, 0, 0)).GetComponent<ShotScript>().angle = Vector3.Normalize(dir2target);
                    enemyShotTime = Time.time + 3f;
                }

                if (Vector3.Distance(target.transform.position, transform.position) < 50 && sight.collider != null && sight.collider.gameObject.tag == "Player")
                {
                    agent.speed = 1;
                    agent.angularSpeed = 360;
                }
                else
                {
                    agent.speed = 12;
                    agent.angularSpeed = 120;
                }
            }
            if (enemyClass == 3) //invisibility
            {
                if (Vector3.Distance(target.transform.position, transform.position) < 30 && sight.collider != null && sight.collider.gameObject.tag == "Player" && M[0].enabled == false)
                {
                    for (int i = 0; i < M.Length; i++)
                    {
                        M[i].enabled = true;
                    }
                }
                else if (M[0].enabled == true)
                {
                    for (int i = 0; i < M.Length; i++)
                    {
                        M[i].enabled = false;
                    }
                }
            }
        }
        
        dist = Vector3.Distance(transform.position, target.transform.position);
        if (enemyHP < 1)
        {
            Instantiate(deathParticles,gameObject.transform.position, new Quaternion(0,0,0,0));
            Destroy(gameObject);
            GameManager.Instance.enemyCount--;
            GameManager.Instance.killCount++;
        }
        
    }

    private void FixedUpdate()
    {
        if(target != null)
            see = Physics.Raycast(transform.position, dir2target, out sight, 1000f, ~(1<<8));
            
    }

    public void Spawn(Vector3 pos, GameObject targ) //set up the enemy with the nav mesh system
    {
        agent = GetComponent<NavMeshAgent>();
        agent.Warp(pos);
        target = targ;
    }

    public IEnumerator displacementPhysics(Vector3 dir,float T) //coroutine to handle physics interactions
    {
        freePhysics = true;
        agent.enabled = false;
        rb.isKinematic = false;
        physicsTimer = Time.time + T;
        rb.velocity = dir;
        //rb.AddForce(dir,ForceMode.Impulse);

        yield return new WaitForSeconds(T);
        if(Time.time > physicsTimer)
        {
            rb.isKinematic = true;
            agent.enabled = true;
            agent.Warp(new Vector3(transform.position.x, 0, transform.position.z));
            freePhysics = false;
        }
    }
}
