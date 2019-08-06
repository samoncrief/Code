using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    //basic variables
    public bool freePlay; //is the game in Free Play mode?
    public bool inMenu; //is the player in a menu?
    public bool waveEnd; //is the game done spawning enemies for this wave?
    public float spawnTimer = -1;
    public int killCount = 0;
    public int wave = 0;
    public int waveProgress = 0;
    public int playerClass;
    public int enemyCount;
    Vector3 spawnPos;

    //objects
    public GameObject enemySPrefab;
    public GameObject enemyMPrefab;
    public GameObject enemyLPrefab;
    public GameObject enemyIPrefab;
    public GameObject enemyPrefab;
    public GameObject player;
    public GameObject spawning;

    //enemy spawn points
    public Vector3[] spawnPoints =
    {
        new Vector3(70,5.5f,-50),//0
        new Vector3(76.5f,5.5f,-50),//1
        new Vector3(83,5.5f,-50),//2
        new Vector3(89.5f,5.5f,-50),//3
        new Vector3(55,5.5f,-70),//4
        new Vector3(55,5.5f,-76.5f),//5
        new Vector3(55,5.5f,-83),//6
        new Vector3(55,5.5f,-89.5f),//7
        new Vector3(-89.5f,5.5f,50),//8
        new Vector3(-83,5.5f,50),//9
        new Vector3(-76.5f,5.5f,50),//10
        new Vector3(-70,5.5f,50),//11
        new Vector3(-55,5.5f,70),//12
        new Vector3(-55,5.5f,76.5f),//13
        new Vector3(-55,5.5f,83),//14
        new Vector3(-55,5.5f,89.5f)//15
    };

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        freePlay = false;
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        enemyCount = 0;
        inMenu = true;
        Time.timeScale = 0;
    
    }

    // Update is called once per frame
    void Update()
    {
        if (!inMenu) //when the player doesn't have the game paused
        {
            if (freePlay && Time.time > spawnTimer)
            {
                //free play
                switch (Random.Range(1, 8))
                {
                    case 1:
                        enemyPrefab = enemyMPrefab;
                        spawnPos = spawnPoints[Random.Range(0, 16)];
                        spawning = Instantiate(enemyPrefab, spawnPos, new Quaternion());
                        spawning.GetComponent<EnemyController>().Spawn(spawnPos, player);
                        enemyCount++;
                        break;
                    case 2:
                        enemyPrefab = enemyLPrefab;
                        spawnPos = spawnPoints[Random.Range(0, 16)];
                        spawning = Instantiate(enemyPrefab, spawnPos, new Quaternion());
                        spawning.GetComponent<EnemyController>().Spawn(spawnPos, player);
                        enemyCount++;
                        break;
                    case 3:
                        enemyPrefab = enemyIPrefab;
                        spawnPos = spawnPoints[Random.Range(0, 16)];
                        spawning = Instantiate(enemyPrefab, spawnPos, new Quaternion());
                        spawning.GetComponent<EnemyController>().Spawn(spawnPos, player);
                        enemyCount++;
                        break;
                    default:
                        enemyPrefab = enemySPrefab;
                        spawnPos = spawnPoints[Random.Range(0, 16)];
                        spawning = Instantiate(enemyPrefab, spawnPos, new Quaternion());
                        spawning.GetComponent<EnemyController>().Spawn(spawnPos, player);
                        spawnPos = spawnPoints[Random.Range(0, 16)];
                        spawning = Instantiate(enemyPrefab, spawnPos, new Quaternion());
                        spawning.GetComponent<EnemyController>().Spawn(spawnPos, player);
                        enemyCount += 2;
                        break;
                }/*
            { //use this block to test specific enemies
                enemyPrefab = enemyMPrefab;
                spawnPos = spawnPoints[Random.Range(0, 16)];
                spawning = Instantiate(enemyPrefab, spawnPos, new Quaternion());
                spawning.GetComponent<EnemyController>().Spawn(spawnPos, player);
            }*/
                spawnTimer = Time.time + 2f;
            }
            else if (!freePlay && wave < 15)
            {//GetComponent<Waves>().wave.Length){
             //horde
                if (Time.time > spawnTimer && waveEnd == false)
                {
                    switch (GetComponent<Waves>().wave[wave][waveProgress].enemy) // assign enemy type
                    {
                        case 1:
                            enemyPrefab = enemyMPrefab;
                            break;
                        case 2:
                            enemyPrefab = enemyLPrefab;
                            break;
                        case 3:
                            enemyPrefab = enemyIPrefab;
                            break;
                        default:
                            enemyPrefab = enemySPrefab;
                            break;
                    }
                    for (int i = 0; i < GetComponent<Waves>().wave[wave][waveProgress].point.Length; i++) //spawn enemy at point
                    {
                        spawnPos = spawnPoints[GetComponent<Waves>().wave[wave][waveProgress].point[i]];
                        spawning = Instantiate(enemyPrefab, spawnPos, new Quaternion());
                        spawning.GetComponent<EnemyController>().Spawn(spawnPos, player);
                        spawnTimer += GetComponent<Waves>().wave[wave][waveProgress].timeUntil;
                        enemyCount++;
                    }
                    waveProgress++;
                    if (waveProgress >= GetComponent<Waves>().wave[wave].Length)
                    {
                        waveProgress = 0;
                        waveEnd = true;
                    }
                }
                if(waveEnd && enemyCount <= 0)
                {

                    wave++;
                    waveEnd = false;
                }
            }
            

            if (Input.GetButtonDown("Cancel"))
            {
                pause();
            }
        }
        else
        {
            if (Input.GetButtonDown("Cancel"))
            {
                unpause();
                if(spawnTimer < 0)
                {
                    spawnTimer = Time.time;
                }
            }
        }
    }

    public void resetAll() //put game manager variables back to original values
    {
        waveEnd = false; ;
        spawnTimer = -1;
        killCount = 0;
        wave = 0;
        waveProgress = 0;
        enemyCount = 0;
    }
    public void pause()
    {
        Time.timeScale = 0;
        inMenu = true;
    }
    public void unpause()
    {
        inMenu = false;
        Time.timeScale = 1;
    }
    public string waveText(int i) //get text from waves for other scripts
    {
        return GetComponent<Waves>().messages[i];
    }
}
