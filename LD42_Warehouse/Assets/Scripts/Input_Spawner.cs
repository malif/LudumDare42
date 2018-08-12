using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Input_Spawner : MonoBehaviour
{
    public List<Transform> ContentPrefabs = new List<Transform>();
    public Transform PalletPrefab;

    public List<GameObject> Spawners = new List<GameObject>();
    public List<GameObject> FreeSpawners = new List<GameObject>();

    public Text TimerText = null;
    public Text FreeBays = null;

    private float Timer = 0.0f;

    private float alpha = 1.0f;
    private float alphaDir = -1.0f;

    public int StartingAmount = 1;

    void Start ()
    {
        for (int i = 0; i < StartingAmount; ++i)
        {
            for (int j = 1; j <= (int)ContentType.Brick; ++j)
            {
                CheckBays();
                if (FreeSpawners.Count > 0)
                {
                    SpawnPallet((ContentType) j);
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("ENTER");
    }

    // Update is called once per frame
    void Update ()
    {
        Timer += GameLogic.Instance.DeltaTime();

        TimerText.text = (Mathf.Max(0.0f, GameLogic.Instance.ArrivalInterval - Timer)).ToString("F1");

        CheckBays();

        if (Timer > GameLogic.Instance.ArrivalInterval)
        {
            if (FreeSpawners.Count > 0)
            {
                SpawnPallet();
                Timer = 0.0f;
            }
            else
            {
                GameLogic.Instance.AddMissedArrival();
                Timer = 0.0f;
            }            
        }
    }

    void CheckBays()
    {
        FreeSpawners.Clear();
        foreach (GameObject s in Spawners)
        {
            if (CheckAllCollisions(s.transform.position))
            {
                FreeSpawners.Add(s);
            }
        }
        if (FreeSpawners.Count == 0)
        {
            FreeBays.text = "FULL";
            Color c = Color.red;
            alpha += alphaDir * GameLogic.Instance.DeltaTime();
            if(alpha <= 0.0f)
            {
                alphaDir = 1.0f;
                alpha = 0.0f;
            }
            else if (alpha >= 1.0f)
            {
                alphaDir = -1.0f;
                alpha = 1.0f;
            }
            c.a = alpha;
            FreeBays.color = c;
        }
        else
        {
            alpha = 1.0f;
            alphaDir = -1.0f;
            FreeBays.text = FreeSpawners.Count.ToString();
            FreeBays.color = Color.white;
        }
    }

    private bool CheckAllCollisions(Vector3 Position)
    {
        Collider[] collisions = Physics.OverlapBox(Position, new Vector3(0.25f, 0.25f, 0.25f));
        foreach (Collider col in collisions)
        {
            if (col.gameObject.GetComponent<Pallet>() != null)
                return false;

            if (col.gameObject.GetComponent<Blocker>() != null)
                return false;

            if (col.gameObject.GetComponent<Player_Forklift>() != null)
                return false;
        }
        return true;
    }


    bool SpawnPallet()
    {
        int spawnerIndex = Random.Range(0, FreeSpawners.Count);
        int contentIndex = Random.Range(0, ContentPrefabs.Count);
        if (CheckAllCollisions(FreeSpawners[spawnerIndex].transform.position))
        {
            Transform palletObj = Instantiate(PalletPrefab, FreeSpawners[spawnerIndex].transform.position - new Vector3(0.0f, 0.5f, 0.0f), Quaternion.identity);
            Transform contentObj = Instantiate(ContentPrefabs[contentIndex], FreeSpawners[spawnerIndex].transform.position - new Vector3(0.0f, 0.5f, 0.0f) + new Vector3(0.0f, 0.3f, 0.0f), Quaternion.identity);
            contentObj.SetParent(palletObj);
            palletObj.GetComponentInChildren<Pallet>().Contents = contentObj.gameObject;
            return true;
        }
        return false;
    }

    bool SpawnPallet(ContentType type)
    {
        int spawnerIndex = Random.Range(0, FreeSpawners.Count);
        if (CheckAllCollisions(FreeSpawners[spawnerIndex].transform.position))
        {
            Transform palletObj = Instantiate(PalletPrefab, FreeSpawners[spawnerIndex].transform.position - new Vector3(0.0f, 0.5f, 0.0f), Quaternion.identity);
            Transform contentObj = Instantiate(ContentPrefabs[(int)type - 1], FreeSpawners[spawnerIndex].transform.position - new Vector3(0.0f, 0.5f, 0.0f) + new Vector3(0.0f, 0.3f, 0.0f), Quaternion.identity);
            contentObj.SetParent(palletObj);
            palletObj.GetComponentInChildren<Pallet>().Contents = contentObj.gameObject;
            return true;
        }
        return false;
    }
}
