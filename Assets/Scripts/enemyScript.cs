using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyScript : MonoBehaviour
{    
    //object references

    public GameObject[] wayPoints;
    public GameObject player;

    //component references
    Rigidbody2D enemyRB;
    public Animator myAnimator; //enemy's animator component
    public Animator myChildAnimator; //enemy childs' collidor animation component
   
    //states
    public bool hasSpotted = false;
    public bool inRange;
    public bool inAttackRange = false;
    public bool nextToPlayer = false;
    public bool isIdling;
    public bool knockedBack = false;
    bool canTakeDamage = true;
    public bool canAttack = true;

    //stats
    public float knockBack; //knockback force
    public float health = 20;
    public float damage = 5;
    public float maxSightDistance;
    public float attackRange;
    public float enemySpeed;
    public float stopMoveRange;

    //cached/stored information
    Vector2 currentPosition;
    Vector2 nextPosition;
    int placeInWayPoints = 0;
    Vector2 towardsPlayer;
    Vector2 FacingDirection;
    PlayerMovement playerScript;

    // Start is called before the first frame update
    void Start()
    {
        isIdling = true;
        currentPosition = wayPoints[0].transform.position;
        nextPosition = wayPoints[1].transform.position;
        enemyRB = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<PlayerMovement>();
    }

    private void FixedUpdate()
    {
        towardsPlayer = (player.transform.position - transform.position).normalized;
        if (!hasSpotted)
        {
            
            int layerMask = LayerMask.GetMask("Player", "Map");
            Debug.DrawRay(transform.position, towardsPlayer, Color.red, 1f, false);
            RaycastHit2D hit = Physics2D.Raycast(transform.position,towardsPlayer, maxSightDistance, layerMask);
            if (hit)
            {
                if (hit.collider.name == "Player")
                {
                    hasSpotted = true;
                    isIdling = false;
                    inRange = true;
                }
                else
                {
                    inRange = false;
                }
            }
            else
            {
                inRange = false;
            }
        }
        else
        {
            int layerMask = LayerMask.GetMask("Player", "Map");
            Debug.DrawRay(transform.position, towardsPlayer, Color.red, 1f, false);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, towardsPlayer, attackRange, layerMask);
            if (hit)
            {
                if (hit.collider.name == "Player")
                {
                    inAttackRange = true;
                }
                else
                {
                    inAttackRange = false;
                }
            }
            else
            {
                inAttackRange = false;
            }

            RaycastHit2D hit2 = Physics2D.Raycast(transform.position, towardsPlayer, stopMoveRange, layerMask);
            if (hit2)
            {
                if (hit.collider.name == "Player")
                {
                    nextToPlayer = true;
                }
                else
                {
                    nextToPlayer = false;
                }
            }
            else
            {
                nextToPlayer = false;
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (hasSpotted && !knockedBack && !nextToPlayer)
        {
            //move towards the player
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, enemySpeed *Time.deltaTime);
            myAnimator.SetBool("Walking", true);
            myChildAnimator.SetBool("Walking", true);
            myAnimator.SetBool("Idling", false);
            myChildAnimator.SetBool("Idling", false);
            FacingDirection = towardsPlayer;
            AnimBlendSetFloats();
        }
        else
        {
            if (isIdling)
            {
                //move between waypoints
                transform.position = Vector2.MoveTowards(transform.position, nextPosition, enemySpeed * Time.deltaTime);
                //enemyRB.MovePosition(enemyRB.position + (nextPosition * enemySpeed * Time.deltaTime));
                if (new Vector2(transform.position.x, transform.position.y) == nextPosition) //if we have reached the next position
                {
                    isIdling = false;
                    StartCoroutine(IdleWait());
                }
            }
        }

        if (inAttackRange && canAttack)
        {
            StartCoroutine(BlockHealTrigger());
            StartCoroutine(Attack());
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (canTakeDamage && collision.name == "WeaponCollider") 
        {
            PlayerMovement playerInfo = collision.GetComponentInParent<PlayerMovement>();
            if (playerInfo != null)
            {
                health -= playerInfo.damage;
            }
            canTakeDamage = false;
            StartCoroutine(NoDoubleHit());

            if(health <= 0)
            {
                //die
                Destroy(transform.parent.gameObject);
            }

            //knock back
            knockedBack = true;
            StartCoroutine(KnockBackTimer());
            enemyRB.AddForce(-towardsPlayer.normalized *knockBack);
        }

    }

    IEnumerator KnockBackTimer()
    {
        yield return new WaitForSeconds(.2f);
        knockedBack = false;
        enemyRB.velocity = Vector2.zero;
    }

    IEnumerator IdleWait()
    {
        myAnimator.SetBool("Walking", false);
        myChildAnimator.SetBool("Walking", false);
        myAnimator.SetBool("Idling", true);
        myChildAnimator.SetBool("Idling", true);
        yield return new WaitForSeconds(Random.Range(0, 5));
        isIdling = true;
        placeInWayPoints = Random.Range(0, wayPoints.Length);
        nextPosition = wayPoints[placeInWayPoints].transform.position;
    }
    
    //prevents weapon collider attack animation 
    //from dealing damage more than once an attack
    IEnumerator NoDoubleHit()
    {
        for (int i = 0; i < 10; i++)  //wait 3 frames
        {
            yield return null;
        } 
        canTakeDamage = true; //then can take damage again
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, maxSightDistance);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    private void AnimBlendSetFloats()
    {
        myAnimator.SetFloat("FaceX", FacingDirection.x);
        myAnimator.SetFloat("FaceY", FacingDirection.y);
    }

    private void PlayAttackAnimations()
    {
        //checks direction enemy is facing, then triggers animations for that direction.
        if (FacingDirection.x > 0 &&
            Mathf.Abs(FacingDirection.x) > Mathf.Abs(FacingDirection.y))
        {
            myChildAnimator.SetTrigger("AttackRight");
            myAnimator.SetTrigger("AttackingRight");
        }
        if (FacingDirection.x < 0 &&
            Mathf.Abs(FacingDirection.x) > Mathf.Abs(FacingDirection.y))
        {
            myChildAnimator.SetTrigger("AttackLeft");
            myAnimator.SetTrigger("AttackingLeft");
        }
        if (FacingDirection.y > 0 &&
            Mathf.Abs(FacingDirection.y) > Mathf.Abs(FacingDirection.x))
        {
            myChildAnimator.SetTrigger("AttackUp");
            myAnimator.SetTrigger("AttackingUp");
        }
        if (FacingDirection.y < 0 &&
            Mathf.Abs(FacingDirection.y) > Mathf.Abs(FacingDirection.x))
        {
            myChildAnimator.SetTrigger("AttackDown");
            myAnimator.SetTrigger("AttackingDown");
        }

    }
    
    IEnumerator Attack()
    {
        canAttack = false;
        yield return new WaitForSeconds(.7f);
        PlayAttackAnimations();
        yield return new WaitForSeconds(2);
        canAttack = true;
    }

    IEnumerator BlockHealTrigger()
    {
        playerScript.canBlockHeal = true;
        //should eventually be the wound up time plus attack time
        yield return new WaitForSeconds(.7f + .2f);
        playerScript.canBlockHeal = false;
    }
}
