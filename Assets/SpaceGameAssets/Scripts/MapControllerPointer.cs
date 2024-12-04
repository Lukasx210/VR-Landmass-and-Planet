using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapControllerPointer : MonoBehaviour
{
    public MapControllerHandle XHandle;
    public MapControllerHandle YHandle;
    public float middleZ;
    public Transform pointer;
    public Transform parent;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float xPos = XHandle.value;
        float yPos = YHandle.value;
        pointer.position = new Vector3(xPos, yPos, parent.position.z);
    }
}
