using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    public MapCreator map_creator = null;
    // Start is called before the first frame update
    void Start()
    {
        map_creator = GameObject.Find("GameRoot").GetComponent<MapCreator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(this.map_creator.isDeleted(this.gameObject)) {   
            GameObject.Destroy(this.gameObject);
        }
    }
}
