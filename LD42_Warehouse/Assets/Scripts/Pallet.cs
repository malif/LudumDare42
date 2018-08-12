using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pallet : MonoBehaviour {

    public GameObject Contents = null;
    public GameObject StackedPallet = null;

    public bool AttachedToForklift = false;

    public Player_Forklift forklift = null;

	// Use this for initialization
	void Start () {
		if(Contents != null)
        {
            Contents.transform.parent = transform;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnDetatched()
    {
        Collider[] collisions = Physics.OverlapBox(transform.position - new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.4f, 0.4f, 0.4f));
        foreach (Collider col in collisions)
        {
            Pallet p = col.gameObject.GetComponent<Pallet>();
            if (p != null)
            {
                PalletContent pc = p.Contents.GetComponentInChildren<PalletContent>();
                if (pc != null && pc.Stackable)
                {
                    p.AttachPallet(gameObject);
                    return;
                }
            }
        }
    }

    public void OnPalletPositionUpdated(Vector3 deltaPos, bool updateChildren)
    {
        if (StackedPallet == null)
            return;

        StackedPallet.transform.position += deltaPos;
        if(updateChildren)
            StackedPallet.GetComponent<Pallet>().OnPalletPositionUpdated(deltaPos, false);
    }

    public void OnPalletRotationUpdated(float degrees, Vector3 pivot, bool updateChildren)
    {
        if (StackedPallet == null)
            return;

        StackedPallet.transform.RotateAround(pivot, new Vector3(0.0f, 1.0f, 0.0f), degrees);

        if (updateChildren)
            StackedPallet.GetComponent<Pallet>().OnPalletRotationUpdated(degrees, pivot, false);
    }

    public void OnPalletLiftUpdated(Vector3 deltaPos, bool updateChildren)
    {
        if (StackedPallet == null)
            return;

        StackedPallet.transform.position += deltaPos;
        if(updateChildren)
            StackedPallet.GetComponent<Pallet>().OnPalletPositionUpdated(deltaPos, false);
    }

    public void AttachPallet(GameObject _Pallet)
    {
        Pallet pallet = _Pallet.GetComponent<Pallet>();
        if (pallet == null)
            return;

        if (Contents == null)
            return;

        PalletContent content = Contents.GetComponentInChildren<PalletContent>();
        if (content == null)
            return;

        if(content.Stackable)
        {
            StackedPallet = _Pallet;
        }
    }

    public void DetachPallet()
    {
        if (StackedPallet == null)
            return;
        StackedPallet.GetComponent<Pallet>().OnDetatched();
        StackedPallet = null;
    }
}
