using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class PlayerDialogueScript : MonoBehaviour
{
    public bool nearElias = false;
    public int buttonClickCounter = 0;
    public string[] dialogueLines;
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText;
    public GameObject dialogueButtons;
    public string[] responseOption1;
    public string responseOption2;
    private bool afterOption = false;
    private bool afterOption2 = false;

    // Start is called before the first frame update
    void Start()
    {
        dialogueBox.SetActive(false);
        dialogueButtons.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire2") && nearElias)
        {
            ShowDialogueLines();
        }

    }

    private void ShowDialogueLines()
    {
        if (!afterOption&&!afterOption2)
        {


            //Inserting text
            /*if (buttonClickCounter == 0 )
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
             } */
            dialogueBox.SetActive(true);
            buttonClickCounter = buttonClickCounter + 1;
            /*if (buttonClickCounter == dialogueLines.Length + 1)
            {
                dialogueBox.SetActive(false);
                buttonClickCounter = 0;
            }*/
            if (buttonClickCounter <= dialogueLines.Length)
            {
                dialogueText.text = dialogueLines[buttonClickCounter - 1];
            }
            //buttonClickCounter = Mathf.Clamp(buttonClickCounter , 0, dialogueLines.Length - 1);
            if (buttonClickCounter == dialogueLines.Length)
            {
                dialogueButtons.SetActive(true);

            }
        }
        else if(afterOption&&!afterOption2)
        {
            buttonClickCounter = buttonClickCounter + 1;
            if (buttonClickCounter == responseOption1.Length + 1)
            {
                afterOption = false;
                dialogueBox.SetActive(false);
                buttonClickCounter = 0;
            }
            else
            {
                dialogueText.text = responseOption1[buttonClickCounter - 1];
            }
        }
        else if (afterOption2 && !afterOption)
        {
            afterOption2 = false;
            dialogueBox.SetActive(false);
            buttonClickCounter = 0;
        }
    }

    public void ButtonOptionClick()
    {
        buttonClickCounter = 0;
    }

    public void ButtonOptionOne()
    {
        afterOption = true;
        dialogueText.text = responseOption1[0];
        buttonClickCounter = 1;
    }

    public void ButtonOptionTwo()
    {
        dialogueText.text = responseOption2;
        afterOption2 = true;
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        nearElias = true;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        nearElias = false;
        dialogueBox.SetActive(false);
        buttonClickCounter = 0;
        dialogueButtons.SetActive(false);
    }
}
