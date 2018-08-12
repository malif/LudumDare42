using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum TruckState
{
    LOADING_BAY,
    TRANSIT_AWAY,
    AWAY,
    TRANSIT_LOADING_BAY

}
public class LoadingBay : MonoBehaviour {

    public GameObject Truck = null;
    public Transform TruckPos = null;

    private TruckState State = TruckState.AWAY;

    public float TruckDistance = 20.0f;

    private float Timer = 0.0f;

    public float TransitTime = 5.0f;

    public ContentType[] RequiredContents = new ContentType[4];

    public Image[] GoodsSlot = new Image[4];

    public Sprite[] sprites = new Sprite[4];

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < 4; ++i)
        {
            GoodsSlot[i].enabled = false;
        }
    }

    void GenerateRequiredGoods()
    {
        int count = Random.Range(0, 4);

        for(int i = 0; i <= count; ++i)
        {
            int RandomContent = Random.Range(1, 4);
            RequiredContents[i] = (ContentType) RandomContent;
            GoodsSlot[i].sprite = sprites[(int)RequiredContents[i]];
            GoodsSlot[i].enabled = true;
        }
        for(int i = count + 1; i < 4; ++i)
        {
            RequiredContents[i] = ContentType.None;
            GoodsSlot[i].enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (State)
        {
            case TruckState.LOADING_BAY:
                {
                    if (CheckGoods())
                    {
                        State = TruckState.TRANSIT_AWAY;
                        Timer = 0.0f;
                    }
                }
                break;
            case TruckState.TRANSIT_AWAY:
                {
                    Timer += GameLogic.Instance.DeltaTime();

                    Truck.transform.position += new Vector3(TruckDistance / TransitTime * GameLogic.Instance.DeltaTime(), 0.0f, 0.0f);
                    if (Timer >= TransitTime)
                    {
                        Truck.transform.position = TruckPos.position + new Vector3(TruckDistance, 0.0f, 0.0f);
                        State = TruckState.AWAY;
                        Timer = 0.0f;
                    }
                }
                break;
            case TruckState.AWAY:
                {
                    Timer += GameLogic.Instance.DeltaTime();

                    if (Timer >= GameLogic.Instance.TruckWaitTime)
                    {
                        State = TruckState.TRANSIT_LOADING_BAY;
                        Timer = 0.0f;
                    }
                }
                break;
            case TruckState.TRANSIT_LOADING_BAY:
                {
                    Timer += GameLogic.Instance.DeltaTime();
                    Truck.transform.position -= new Vector3(TruckDistance / TransitTime * GameLogic.Instance.DeltaTime(), 0.0f, 0.0f);
                    if (Timer >= TransitTime)
                    {
                        //Generate goals
                        GenerateRequiredGoods();
                        Truck.transform.position = TruckPos.position;
                        State = TruckState.LOADING_BAY;
                        Timer = 0.0f;
                    }
                }
                break;
            default:
                break;
        }
    }

    private bool CheckGoods()
    {
        Vector3 size = GetComponent<BoxCollider>().size;
        List<ContentType> remainingTypes = new List<ContentType>();
        foreach (ContentType c in RequiredContents)
        {
            if(c != ContentType.None)
            {
                remainingTypes.Add(c);
            }
        }

        List<GameObject> foundPallets = new List<GameObject>();

        Collider[] collisions = Physics.OverlapBox(transform.position , new Vector3(0.4f, 0.8f, 0.4f));
        foreach (Collider col in collisions)
        {
            Pallet p = col.gameObject.GetComponent<Pallet>();
            if (p != null)
            {
                PalletContent pc = p.Contents.GetComponentInChildren<PalletContent>();
                if (pc != null)
                {
                    if(remainingTypes.Contains(pc.Type))
                    {
                        remainingTypes.Remove(pc.Type);
                        foundPallets.Add(p.gameObject);
                    }
                    else
                    {
                        return false;
                        //WARNING UI HERE
                    }

                }
            }
        }

        if(remainingTypes.Count == 0)
        {
            GameLogic.Instance.AddDeliveryScore(foundPallets.Count);
            foreach(GameObject go in foundPallets)
            {
                Player_Forklift f = go.GetComponent<Pallet>().forklift;
                if (f != null)
                {
                    f.DetatchPallet();
                }
                if(go.transform.parent.gameObject)
                {
                    Destroy(go.transform.parent.gameObject);
                }
                else
                {
                    Destroy(go);
                }
            }

            for (int i = 0; i < 4; ++i)
            {
                RequiredContents[i] = ContentType.None;
                GoodsSlot[i].enabled = false;
            }
            return true;
        }

        //ADD SCORE HERE
        return false;
    }

    public void AssignGoods(GameObject pallet)
    {

    }

    public void WithdrawGoods(GameObject pallet)
    {

    }
}
