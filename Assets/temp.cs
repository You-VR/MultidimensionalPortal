using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class temp : MonoBehaviour {
	// Update is called once per frame
	void Update () {
        this.gameObject.transform.Translate(new Vector3( 0,  Mathf.Sin(Time.time) / 100, 0));

    }
}
