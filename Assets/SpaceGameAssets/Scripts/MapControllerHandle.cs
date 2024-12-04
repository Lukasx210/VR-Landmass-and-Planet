using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapControllerHandle : MonoBehaviour
{
    public Transform handleTransform;
    public float value; // value representing how far along the map edge this handle is
    public int MaxValueTranslation; // furthest in either direction that the handle can move
    public bool isX; // indicated wether the handle represents the X or the Y
    public float initialTransformValue;
    // Start is called before the first frame update
    void Start()
    {
        if (isX)
        {
            initialTransformValue = handleTransform.position.x;
        }
        else
        {
            initialTransformValue = handleTransform.position.y;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isX)
        {
            if(handleTransform.position.x > initialTransformValue + MaxValueTranslation)
            {
                handleTransform.Translate(new Vector3(handleTransform.position.x - ((initialTransformValue + MaxValueTranslation) - handleTransform.position.x), 0, 0));
            }
            else if(handleTransform.position.x < initialTransformValue - MaxValueTranslation)
            {
                handleTransform.Translate(new Vector3(handleTransform.position.x+ ((initialTransformValue - MaxValueTranslation) - handleTransform.position.x), 0, 0));
            }
            value = handleTransform.position.x;
        }
        else
        {
            if (handleTransform.position.y > initialTransformValue + MaxValueTranslation)
            {
                handleTransform.Translate(new Vector3(0, handleTransform.position.y - ((initialTransformValue + MaxValueTranslation) - handleTransform.position.y), 0));
            }
            else if (handleTransform.position.y < initialTransformValue - MaxValueTranslation)
            {
                handleTransform.Translate(new Vector3(0, handleTransform.position.y + ((initialTransformValue - MaxValueTranslation) - handleTransform.position.y), 0));
            }
            value = handleTransform.position.y;
        }
    }
}
