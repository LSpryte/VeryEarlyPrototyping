using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDialogueScript : MonoBehaviour
{
    bool nearElias = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire2")&&nearElias)
        {
            
            Debug.Log("FUCK");
            
            
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        nearElias = true;
        Debug.Log("aaaaaaaaaaaaaaaaaaaaaaaaaaaa");
    }
}
