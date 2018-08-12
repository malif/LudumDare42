using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ContentType
{
    None,
    Wood,
    Iron,
    Brick
}

public class PalletContent : MonoBehaviour
{
    public bool Stackable = false;
    public ContentType Type = ContentType.Wood;

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
