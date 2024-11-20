using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityApplier : MonoBehaviour
{

    public Rigidbody rb;
    public List<GameObject> gravityTargets;
    public float gravityModifier;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach(GameObject target in gravityTargets)
        {
            Vector3 positionDiff = target.transform.position - rb.transform.position;
            rb.AddForce(positionDiff * gravityModifier);
        }
    }
}
