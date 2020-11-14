using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDialogueScript : MonoBehaviour
{
    bool nearElias = false;
    public int buttonClickCounter = 0; 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire2")&&nearElias)
        {
            //Inserting text
            if (buttonClickCounter == 0 )
            {
                Debug.Log("Dialogue 0");
            }
            if (buttonClickCounter == 1 )
            {
                Debug.Log("Dialogue 1");
            }
            if (buttonClickCounter == 2 )
            {
                Debug.Log("Dialogue 2");
            }
            //buttonClickCounter = buttonClickCounter + 1;
            Debug.Log("button Clicks");
            buttonClickCounter += 1;
            Mathf.Clamp(buttonClickCounter, 0, 3);
            Debug.Log(buttonClickCounter);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        nearElias = true;
        Debug.Log("aaaaaaaaaaaaaaaaaaaaaaaaaaaa");
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        nearElias = false;
        Debug.Log("no");
        
    }
}
