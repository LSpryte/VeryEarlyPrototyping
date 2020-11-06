using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyScript : MonoBehaviour
{
    public int health = 10;
    bool canTakeDamage = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (canTakeDamage)
        {
            Debug.Log("hit an enemy!!");
            canTakeDamage = false;
            StartCoroutine(NoDoubleHit());
        }

    }

    //prevents weapon collider attack animation 
    //from dealing damage more than once an attack
    IEnumerator NoDoubleHit()
    {
        for (int i = 0; i < 3; i++)  //wait 3 frames
        {
            yield return null;
        } 
        canTakeDamage = true; //then can take damage again
    }
}
