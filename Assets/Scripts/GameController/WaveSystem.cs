using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

public class WaveSystem : MonoBehaviourPunCallbacks
{
    public PhotonView myPhotonView;
    public GameObject gameBase;

    public GameObject genPar, genEndPar;
    public SoundPlayer genSound, genEndSound;
    public TextMeshProUGUI waveTxt;

    public string[] zombieNames;

    public string[] grassLandscape;
    [Tooltip("" +
        "0 = Min ls Count; " +
        "1 = Max ls Count; " +
        "2 = Offset x; " +
        "3 = Offset y; " +
        "4 = Offset z; " +
        "5 = ls Distance; " +
        "6 = base Distance; " +
        "7 = Max Spawn x; " +
        "8 = Min Spawn x; " +
        "9 = Max Spawn y; " +
        "10 = Min Spawn y; ")]
    public float[] grassSettings;

    public int wave = 0;

    public float zombieSpawnSpeed = 2f;
    public int zombieRemainTotal = 20;
    public int zombieRemain = 0;
    public bool canSpawn = true;

    bool generateDone = false;

    GameObject lastArea;
    int lastSeed;
    string lastBiome;

    List<object> newMapList = new List<object>();


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient) StartCoroutine(NewPlayerMapGen(newPlayer)); 
    }
    IEnumerator NewPlayerMapGen(Player newPlayer)
    {
        myPhotonView.RPC("StartNewWaveRPC", newPlayer, lastSeed, wave - 1, lastBiome);
        yield return new WaitForSecondsRealtime(1f);
        myPhotonView.RPC("GenerateMap", newPlayer, newMapList.ToArray());
    }

    private void Start()
    {
        StartCoroutine(ZombieAliveCheck());
        if (PhotonNetwork.IsMasterClient) StartNewWave();
    }

    float spawnCooldown = 0;
    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && Input.GetKeyDown(KeyCode.O))
        {
            Start();
        }

        // Zombie Spawn
        spawnCooldown -= Time.deltaTime;
        if (spawnCooldown <= 0 && canSpawn && generateDone && zombieRemain > 0)
        {
            spawnCooldown = zombieSpawnSpeed; // Zombie Spawn Speed = Wave Time / Total Zombies
            zombieRemain -= 1;

            zombieSpawnSpeed = Mathf.Clamp(zombieSpawnSpeed, 0.1f, Mathf.Infinity);

            // Spawn new Zombie
            if (PhotonNetwork.IsMasterClient)
            {
                // Find Spawns
                GameObject[] spawns = GameObject.FindGameObjectsWithTag("ZombieSpawn");

                // Spawn Zombie
                var spawnPos = spawns[Random.Range(0, spawns.Length)].transform.position;
                ZombieManager.Instance.SpawnZombie(zombieNames[Random.Range(0, zombieNames.Length)],
                    spawnPos.x, spawnPos.y, spawnPos.z, Random.Range(-2147483648, 2147483647));
            }
        }
    }

    IEnumerator ZombieAliveCheck()
    {
        while (true)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // Check if All Zombies Dead, if it is, start next wave
                if (generateDone && canSpawn && zombieRemain <= 0)
                {
                    GameObject[] allZombies = GameObject.FindGameObjectsWithTag("Zombie");
                    if (allZombies.Length == 0)
                    {
                        StartNewWave();
                    }
                }
            }

            yield return new WaitForSecondsRealtime(3f);
        }
    }

    void StartNewWave()
    {
        // Start New Wave
        myPhotonView.RPC("StartNewWaveRPC", RpcTarget.All, (int)System.DateTime.Now.Ticks, wave, "Grass");
    }

    [PunRPC]
    void StartNewWaveRPC(int seed, int activeWave, string biome)
    {
        // Start Wave
        // Change Wave
        wave = activeWave;
        wave++;

        // Change Spawn Settings
        zombieRemainTotal += 10;
        zombieRemain = zombieRemainTotal;
        zombieSpawnSpeed = 40f / zombieRemainTotal;

        lastSeed = seed;
        lastBiome = biome;

        // Change vars
        GetComponent<BuildingController>().buildMat += 5;

        genPar.SetActive(true);
        genSound.playing = true;
        waveTxt.text = "Wave " + wave;

        waveTxt.GetComponent<Animator>().SetBool("newWave", true);

        StartCoroutine(WaitForSpawnNext(seed, biome)); // its still here but not needed

        generateDone = false;
        if(PhotonNetwork.IsMasterClient) StartCoroutine(BuildIEnum(seed, biome));
    }

    IEnumerator WaitForSpawnNext(int seed, string biome)
    {
        // I made this little delay, becuase if its not, the seed generation will have 2 result instead of one, beacuse the
        // old Area isnt fully destroyed if it generate it instandly       
        yield return null;
        //SpawnNewArea(seed, biome);

        //SpawnNewAreaAsync(seed, biome);
    }

    /*void SpawnNewArea(int seed, string biome)
    {
        Random.InitState(seed);

        // Create New Area        
        GameObject newArea = new GameObject();       
        newArea.name = biome + "-Area " + wave;
        lastArea = newArea;

        // Generate Map
        // Get Coll
        string[] landscapeColl = null;
        float[] settingsColl = null;
        switch (biome)
        {
            case "Grass":
                landscapeColl = grassLandscape;
                settingsColl = grassSettings;
                break;
            default:
                Debug.LogError("Area-Biome: " + biome + " not Found!");
                break;
        }
        
        // Start Generating
        GameObject ground = Instantiate(Resources.Load<GameObject>("Grounds/GrassGround"));
        ground.transform.parent = newArea.transform;
        ground.transform.position = new Vector3(settingsColl[2], settingsColl[3], settingsColl[4]);
        
        int landscapeCount = Random.Range((int)settingsColl[0], (int)settingsColl[1]);

        List<GameObject> generatedObs = new List<GameObject>();
        for (int i = 0; i < landscapeCount; i++)
        {
            // Get all other Landscape (and Buildings)
            List<GameObject> landscapesList = new List<GameObject>(GameObject.FindGameObjectsWithTag("Destructible"));
            landscapesList.AddRange(new List<GameObject>(GameObject.FindGameObjectsWithTag("Landscape")));
            landscapesList.AddRange(new List<GameObject>(GameObject.FindGameObjectsWithTag("Building")));

            // Place Landscape and if it to near to a nother landscpae resplace it
            GameObject s = Instantiate(Resources.Load<GameObject>("Landscape/" + landscapeColl[Random.Range(0, landscapeColl.Length)]),
                Vector3.zero, Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0));
            s.transform.parent = newArea.transform;
            try { s.GetComponent<DestructibleScript>().Id = i; } catch { }

            // Do this while so long, until landscape have no collision
            bool succes = false;
            int tries = 0;
            while (!succes)
            {
                s.transform.position = new Vector3(Random.Range(settingsColl[8], settingsColl[7]), 0, Random.Range(settingsColl[10], settingsColl[9]));

                // Check Coll with all other Landscape
                bool isHit = false;
                for (int i2 = 0; i2 < landscapesList.Count; i2++)
                {
                    if(Vector3.Distance(s.transform.position, landscapesList[i2].transform.position) < settingsColl[5])
                    {
                        isHit = true;
                        break;
                    }
                }

                // Chech Coll with Base
                if (Vector3.Distance(s.transform.position, gameBase.transform.position) < settingsColl[6])
                {
                    isHit = true;
                }

                if (isHit == false)
                {
                    succes = true;
                }
                else if (tries >= 300)
                {
                    Destroy(s);
                    print("To many Tries");
                    break;
                }
                tries++;
            }

            if(succes) generatedObs.Add(s);
        }
    }
    */

    struct TempGameObject
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public int id;
    }

    IEnumerator BuildIEnum(int seed, string biome)
    {
        newMapList = new List<object>();

        yield return new WaitForSecondsRealtime(2f);

        yield return new WaitForEndOfFrame();

        Random.InitState(seed);

        // Create New Area        
        newMapList.Add("Name");
        newMapList.Add(biome + "-Area " + wave); 

        // Generate Map
        // Get Coll
        string[] landscapeColl = null;
        float[] settingsColl = null;
        switch (biome)
        {
            case "Grass":
                landscapeColl = grassLandscape;
                settingsColl = grassSettings;
                break;
            default:
                Debug.LogError("Area-Biome: " + biome + " not Found!");
                break;
        }

        // Start Generating
        newMapList.Add("Ground");
        newMapList.Add("GrassGround");
        newMapList.Add(settingsColl[2]);
        newMapList.Add(settingsColl[3]);
        newMapList.Add(settingsColl[4]);

        int landscapeCount = Random.Range((int)settingsColl[0], (int)settingsColl[1]);       

        List<TempGameObject> generatedObs = new List<TempGameObject>();
        for (int i = 0; i < landscapeCount; i++)
        {
            // Place Landscape and if it to near to a nother landscpae resplace it
            TempGameObject s = new TempGameObject();
            s.name = landscapeColl[Random.Range(0, landscapeColl.Length)];
            s.rotation = Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0);
            s.id = i;

            // Do this while so long, until landscape have no collision
            bool succes = false;
            int tries = 0;
            while (!succes)
            {
                s.position = new Vector3(Random.Range(settingsColl[8], settingsColl[7]), 0, Random.Range(settingsColl[10], settingsColl[9]));

                // Check Coll with all other Landscape
                bool isHit = false;
                for (int i2 = 0; i2 < generatedObs.Count; i2++)
                {
                    if (Vector3.Distance(s.position, generatedObs[i2].position) < settingsColl[5])
                    {
                        isHit = true;
                        break;
                    }
                }

                // Chech Coll with Base
                if (Vector3.Distance(s.position, gameBase.transform.position) < settingsColl[6])
                {
                    isHit = true;
                }

                if (isHit == false)
                {
                    succes = true;
                }
                else if (tries >= 300)
                {
                    print("To many Tries");
                    break;
                }
                tries++;
                //yield return new WaitForSeconds(0.01f);               
            }

            if (succes) generatedObs.Add(s);
            yield return null;
        }

        // Convert Generated Obs Data to newMapList
        for (int i = 0; i < generatedObs.Count; i++)
        {
            newMapList.Add("Landsscape");
            newMapList.Add(generatedObs[i].name);
            newMapList.Add(generatedObs[i].id);
            newMapList.Add(generatedObs[i].position.x);
            newMapList.Add(generatedObs[i].position.y);
            newMapList.Add(generatedObs[i].position.z);
            newMapList.Add(generatedObs[i].rotation.eulerAngles.x);
            newMapList.Add(generatedObs[i].rotation.eulerAngles.y);
            newMapList.Add(generatedObs[i].rotation.eulerAngles.z);
        }
        
        print("New Area Generated");

        myPhotonView.RPC("GenerateMap", RpcTarget.AllViaServer, newMapList.ToArray());
    }
    
    [PunRPC]
    void GenerateMap(object[] newMap)
    {
        StartCoroutine(ConvertNewMapListToMap(newMap));
    }

    IEnumerator ConvertNewMapListToMap(object[] newMap)
    {
        if (lastArea != null) Destroy(lastArea);

        GameObject newArea = Instantiate(new GameObject());

        for (int i = 0; i < newMap.Length; i++)
        {
            if ((string)newMap[i] == "Name")
            {
                newArea.name = newMap[i + 1].ToString();
                i++;
            }
            else if ((string)newMap[i] == "Ground")
            {
                GameObject ground = Instantiate(Resources.Load<GameObject>("Grounds/" + newMap[i + 1]));
                ground.transform.parent = newArea.transform;
                ground.transform.position = new Vector3(float.Parse(newMap[i + 2].ToString()), float.Parse(newMap[i + 3].ToString()), float.Parse(newMap[i + 4].ToString()));
                i += 4;
            }
            else if ((string)newMap[i] == "Landsscape")
            {
                GameObject s = Instantiate(Resources.Load<GameObject>("Landscape/" + (string)newMap[i + 1]),
                new Vector3((float)newMap[i + 3], (float)newMap[i + 4], (float)newMap[i + 5]),
                Quaternion.Euler((float)newMap[i + 6], (float)newMap[i + 7], (float)newMap[i + 8]));
                s.transform.parent = newArea.transform;
                try { s.GetComponent<DestructibleScript>().Id = (int)newMap[i + 2]; } catch { }
                i += 8;
            }

            yield return null;
        }

        lastArea = newArea;

        generateDone = true;

        genPar.SetActive(false);
        genEndPar.SetActive(true);

        genSound.playing = false;
        genSound.Stop();
        genEndSound.PlaySingle();
        waveTxt.text = "";

        print("New Area Loaded");
    }

    /*IEnumerator BuildIEnum(int seed, string biome)
    {
        yield return new WaitForSecondsRealtime(2f);

        if (lastArea != null) Destroy(lastArea);

        yield return new WaitForEndOfFrame();

        Random.InitState(seed);

        // Create New Area        
        GameObject newArea = new GameObject();
        newArea.name = biome + "-Area " + wave;
        lastArea = newArea;

        //newArea.SetActive(false);

        // Generate Map
        // Get Coll
        string[] landscapeColl = null;
        float[] settingsColl = null;
        switch (biome)
        {
            case "Grass":
                landscapeColl = grassLandscape;
                settingsColl = grassSettings;
                break;
            default:
                Debug.LogError("Area-Biome: " + biome + " not Found!");
                break;
        }

        // Start Generating
        GameObject ground = Instantiate(Resources.Load<GameObject>("Grounds/GrassGround"));
        ground.transform.parent = newArea.transform;
        ground.transform.position = new Vector3(settingsColl[2], settingsColl[3], settingsColl[4]);

        int landscapeCount = Random.Range((int)settingsColl[0], (int)settingsColl[1]);

        List<GameObject> generatedObs = new List<GameObject>();
        for (int i = 0; i < landscapeCount; i++)
        {
            // Get all other Landscape (and Buildings)
            List<GameObject> landscapesList = new List<GameObject>(GameObject.FindGameObjectsWithTag("Destructible"));
            landscapesList.AddRange(new List<GameObject>(GameObject.FindGameObjectsWithTag("Landscape")));
            landscapesList.AddRange(new List<GameObject>(GameObject.FindGameObjectsWithTag("Building")));

            // Place Landscape and if it to near to a nother landscpae resplace it
            GameObject s = Instantiate(Resources.Load<GameObject>("Landscape/" + landscapeColl[Random.Range(0, landscapeColl.Length)]),
                Vector3.zero, Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0));
            s.transform.parent = newArea.transform;
            try { s.GetComponent<DestructibleScript>().Id = i; } catch { }

            // Do this while so long, until landscape have no collision
            bool succes = false;
            int tries = 0;
            while (!succes)
            {
                s.transform.position = new Vector3(Random.Range(settingsColl[8], settingsColl[7]), 0, Random.Range(settingsColl[10], settingsColl[9]));

                // Check Coll with all other Landscape
                bool isHit = false;
                for (int i2 = 0; i2 < landscapesList.Count; i2++)
                {
                    if (Vector3.Distance(s.transform.position, landscapesList[i2].transform.position) < settingsColl[5])
                    {
                        isHit = true;
                        break;
                    }
                }

                // Chech Coll with Base
                if (Vector3.Distance(s.transform.position, gameBase.transform.position) < settingsColl[6])
                {
                    isHit = true;
                }

                if (isHit == false)
                {
                    succes = true;
                }
                else if (tries >= 300)
                {
                    Destroy(s);
                    print("To many Tries");
                    break;
                }
                tries++;
                //yield return new WaitForSeconds(0.01f);               
            }

            if (succes) generatedObs.Add(s);
            yield return null;
        }

        //newArea.SetActive(true);
        generateDone = true;
        print("New Area Generated");

        genPar.SetActive(false);
        genEndPar.SetActive(true);

        genSound.playing = false;
        genSound.Stop();
        genEndSound.PlaySingle();
        waveTxt.text = "";
    }
    */
    /*async void SpawnNewAreaAsync(int seed, string biome)
    {
        Random.InitState(seed);

        // Create New Area        
        GameObject newArea = new GameObject();
        newArea.name = biome + "-Area " + wave;
        lastArea = newArea;

        // Generate Map
        // Get Coll
        string[] landscapeColl = null;
        float[] settingsColl = null;
        switch (biome)
        {
            case "Grass":
                landscapeColl = grassLandscape;
                settingsColl = grassSettings;
                break;
            default:
                Debug.LogError("Area-Biome: " + biome + " not Found!");
                break;
        }

        // Start Generating
        GameObject ground = Instantiate(Resources.Load<GameObject>("Grounds/GrassGround"));
        ground.transform.parent = newArea.transform;
        ground.transform.position = new Vector3(settingsColl[2], settingsColl[3], settingsColl[4]);

        int landscapeCount = Random.Range((int)settingsColl[0], (int)settingsColl[1]);

        List<GameObject> generatedObs = new List<GameObject>();
        for (int i = 0; i < landscapeCount; i++)
        {
            // Get all other Landscape (and Buildings)
            List<GameObject> landscapesList = new List<GameObject>(GameObject.FindGameObjectsWithTag("Destructible"));
            landscapesList.AddRange(new List<GameObject>(GameObject.FindGameObjectsWithTag("Landscape")));
            landscapesList.AddRange(new List<GameObject>(GameObject.FindGameObjectsWithTag("Building")));

            // Place Landscape and if it to near to a nother landscpae resplace it
            GameObject s = Instantiate(Resources.Load<GameObject>("Landscape/" + landscapeColl[Random.Range(0, landscapeColl.Length)]),
                Vector3.zero, Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0));
            s.transform.parent = newArea.transform;
            try { s.GetComponent<DestructibleScript>().Id = i; } catch { }

            s.transform.parent = newArea.transform;
            try { s.GetComponent<DestructibleScript>().Id = i; } catch { }

            // Do this while so long, until landscape have no collision
            Vector3 sPos = s.transform.position;

            var result = await Task.Run(() =>
            {
                bool complete = false;
                int tries = 0;
                while (!complete)
                {
                    sPos = new Vector3(Random.Range(settingsColl[8], settingsColl[7]), 0, Random.Range(settingsColl[10], settingsColl[9]));

                    // Check Coll with all other Landscape
                    bool isHit = false;
                    for (int i2 = 0; i2 < landscapesList.Count; i2++)
                    {
                        if (Vector3.Distance(s.transform.position, landscapesList[i2].transform.position) < settingsColl[5])
                        {
                            isHit = true;
                            break;
                        }
                    }

                    // Chech Coll with Base
                    if (Vector3.Distance(s.transform.position, gameBase.transform.position) < settingsColl[6])
                    {
                        isHit = true;
                    }

                    if (isHit == false)
                    {
                        complete = true;
                        return "complete";
                    }
                    else if (tries >= 300)
                    {                        
                        break;
                    }
                    tries++;
                }

                return "fail";
            });

            if (result == "complete") generatedObs.Add(s);
            else if(result == "fail")
            {
                Destroy(s);
                print("To many Tries");
            }
        }
    }
    */
}
