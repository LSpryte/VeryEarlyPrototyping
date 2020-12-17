using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    //object references
    [SerializeField] private Slider healthSlider;

    //component references 
    public Rigidbody2D rb; //player's rigidbody
    public Animator myAnimator; //player's animator component
    public Animator myChildAnimator; //player childs' collidor animation component

    //states
    bool canAttack = true; //used for delay between attacks
    bool canCombo = true;
    bool firstFrame = true; //used to set initial facing direction
    bool canStrike1 = true;
    bool canStrike2 = false;
    bool canStrike3 = false;
    bool hit2 = false;
    public bool attack1AnimPlaying = false;
    public float attackAnim1Length = .3f;
    bool canTakeDamage = true;
    bool isAttacking = false;
    public bool isBlocking = false;
    public bool canBlockHeal = false;
    bool isDashing = false;

    //stats
    public float health;
    public float maxHealth = 50;
    public float damage = 5;
    public float moveSpeed; //player movement speed
    Vector3 cameraOffset = new Vector3(0, 0, -10); //distance of camera from ground
    float combo1Timer = 1f; //time allowed to achieve combo
    public float attackSpeedDecay;
    public float damageReduction;
    public float blockHealAmount;
    public float dashSpeed;
    public Vector2 dashDistance;
    public Vector2 targetDashPosition;
    public float dashTime;

    //stored/cached information
    float horizontalInput;
    float verticalInput; //store input
    public Vector2 FacingDirection; //direction the player sprite is facing
    public int attackComboCounter = 0; //used to track spot in combo
    public float lastAttackedTime = 0; //time when button was last pressed
    float storedSpeed;

    // Start is called before the first frame update
    void Start()
    {
        storedSpeed = moveSpeed;
    }

    private void FixedUpdate()
    {
        if (!isDashing)
        {
            Move();
        }
    }

    // Update is called once per frame
    void Update()
    {
        Dash();
        if (!isDashing)
        {
            Attack();
            AnimBlendSetFloats();
            Block();
        }
        MoveCamera();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(canTakeDamage && collision.name == "EnemyWeaponCollider")
        {
            //take damage
            enemyScript enemyInfo = collision.GetComponentInParent<enemyScript>();
            if (enemyInfo != null)
            {
                if (!isBlocking)
                {
                    TakeDamage(enemyInfo.damage);
                }
                else if (isBlocking && !canBlockHeal)
                {
                    float newDamage = Mathf.Clamp(enemyInfo.damage - damageReduction, 0, enemyInfo.damage);
                    TakeDamage(newDamage);
                }
            }
            canTakeDamage = false;
            StartCoroutine(NoDoubleHit());
        }
    }

    //Called in Update()
    private void Dash()
    {
        if (Input.GetButtonDown("Fire3"))
        {
            isDashing = true;
            StartCoroutine(DashTimer(dashTime));
        }
        if (isDashing)
        {
            rb.MovePosition(rb.position + (FacingDirection.normalized * dashSpeed) * Time.deltaTime);
        }
    }

    //called in Update()
    private void Block()
    {
        if (Input.GetButtonDown("Fire2")) //if right click
        {
            isBlocking = true;
            myAnimator.SetBool("Blocking", true);
            if (canBlockHeal == true)
            {
                Heal(blockHealAmount);
            }
        }
        if (Input.GetButtonUp("Fire2"))
        {
            isBlocking = false;
            myAnimator.SetBool("Blocking", false);
        }
    }

    // moves the player sprite, called in fixed update
    private void Move()
    {
        if (!isBlocking)
        {

            if (!isAttacking)
            {
                moveSpeed = storedSpeed;
            }
            else
            {
                moveSpeed -= attackSpeedDecay;
                moveSpeed = Mathf.Clamp(moveSpeed, 0, storedSpeed);
            }
            //store player input data
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");

            //create a vector based off input data
            Vector2 movement = new Vector2(horizontalInput, verticalInput);

            //if the vector does not equal zero (and therefore the player is moving)
            if (movement != Vector2.zero)
            {
                //set facing direction 
                FacingDirection = movement;
                //trigger animations
                myAnimator.SetBool("Walking", true);
                myChildAnimator.SetBool("Walking", true);
                myAnimator.SetBool("Idling", false);
                myChildAnimator.SetBool("Idling", false);
            }
            else //the player is standing still
            {
                if (firstFrame)//if the game has just begun
                {
                    //set the facing direction to up 
                    //otherwise it will be (0,0) and attacks won't trigger
                    FacingDirection = new Vector2(0, 1);
                    firstFrame = false;//no longer first frame, this remains off for rest of the program.
                }
                //trigger animations
                myAnimator.SetBool("Walking", false);
                myChildAnimator.SetBool("Walking", false);
                myAnimator.SetBool("Idling", true);
                myChildAnimator.SetBool("Idling", true);
            }
            movement *= moveSpeed; //multiply movement by speed
            rb.MovePosition(rb.position + movement * Time.fixedDeltaTime); //move game object via rigidbody

        }
    }
   
    //called in update
    private void Attack()
    {
        if (Time.time - lastAttackedTime > combo1Timer)//if a second(combo1Timer time) has passed since the last attack 
        {
            //allow player to perform strike1 again
            if (attackComboCounter != 0)
            {
                attackComboCounter = 0;
            }
        }
        //if player hits the key input manager associates with "Fire1"
        if (Input.GetButtonUp("Fire1"))
        {
            if(attackComboCounter == 0 && !canStrike3) // on the first hit
            {
                lastAttackedTime = Time.time;
                //play attack 1 animations
                StartCoroutine(Attack1Length());
                StartCoroutine(TimeForStrike2()); //start timer for second hit
                attackComboCounter = 1;
                PlayAttackAnimations();
            }
            else if(attackComboCounter == 1 && canStrike2) // if on second hit
            {
                lastAttackedTime = Time.time;
                //play attack 2 animations
                StartCoroutine(TimeForStrike3()); // start timer for third
                attackComboCounter = 2;
                PlayAttackAnimations();
            }
            else if(attackComboCounter == 2 && !attack1AnimPlaying && canStrike3)
            {
                lastAttackedTime = Time.time;
                //play attack 3 animations
                attackComboCounter = 3;
                PlayAttackAnimations();
                attackComboCounter = 0;
            }
            StartCoroutine(IsAttackingTimer());
        }
    }

    //called in attack()
    // plays attack animations for both player and invisible weapon collider
    private void PlayAttackAnimations()
    {
        if (attackComboCounter == 1)
        {
            //if player is holding down two buttons, moving diagonally
            if (Mathf.Abs(FacingDirection.x) == Mathf.Abs(FacingDirection.y))
            {//prioritize vertical animation
                if (FacingDirection.y > 0)
                {
                    myChildAnimator.SetTrigger("AttackUp");
                    myAnimator.SetTrigger("AttackingUp");
                }
                if (FacingDirection.y < 0)
                {
                    myChildAnimator.SetTrigger("AttackDown");
                    myAnimator.SetTrigger("AttackingDown");
                }
                return;
            }
            //checks direction player is facing, then triggers animations for that direction.
            if (FacingDirection.x > 0)
            {
                myChildAnimator.SetTrigger("AttackRight");
                myAnimator.SetTrigger("AttackingRight");
            }
            if (FacingDirection.x < 0)
            {
                myChildAnimator.SetTrigger("AttackLeft");
                myAnimator.SetTrigger("AttackingLeft");
            }
            if (FacingDirection.y > 0)
            {
                myChildAnimator.SetTrigger("AttackUp");
                myAnimator.SetTrigger("AttackingUp");
            }
            if (FacingDirection.y < 0)
            {
                myChildAnimator.SetTrigger("AttackDown");
                myAnimator.SetTrigger("AttackingDown");
            }
        }
        if(attackComboCounter == 2)
        {
            if (Mathf.Abs(FacingDirection.x) == Mathf.Abs(FacingDirection.y))
            {
                if (FacingDirection.y > 0)
                {
                    myChildAnimator.SetTrigger("AttackUp2");
                    myAnimator.SetTrigger("AttackingUp2");
                }
                if (FacingDirection.y < 0)
                {
                    myChildAnimator.SetTrigger("AttackDown2");
                    myAnimator.SetTrigger("AttackingDown2");
                }
                return;
            }
            if (FacingDirection.x > 0)
            {
                myChildAnimator.SetTrigger("AttackRight2");
                myAnimator.SetTrigger("AttackingRight2");
            }
            if (FacingDirection.x < 0)
            {
                myChildAnimator.SetTrigger("AttackLeft2");
                myAnimator.SetTrigger("AttackingLeft2");
            }
            if (FacingDirection.y > 0
                && FacingDirection.x < 0.2 && FacingDirection.x > -0.2)
            {
                myChildAnimator.SetTrigger("AttackUp2");
                myAnimator.SetTrigger("AttackingUp2");
            }
            if (FacingDirection.y < 0 
                && FacingDirection.x < 0.2 && FacingDirection.x > -0.2)
            {
                myChildAnimator.SetTrigger("AttackDown2");
                myAnimator.SetTrigger("AttackingDown2");
            }
            
        }
        if (attackComboCounter == 3)
        {
            if (Mathf.Abs(FacingDirection.x) == Mathf.Abs(FacingDirection.y))
            {
                if (FacingDirection.y > 0)
                {
                    myChildAnimator.SetTrigger("AttackUp3");
                    myAnimator.SetTrigger("AttackingUp3");
                }
                if (FacingDirection.y < 0)
                {
                    myChildAnimator.SetTrigger("AttackDown3");
                    myAnimator.SetTrigger("AttackingDown3");
                }
                return;
            }
            if (FacingDirection.x > 0)
            {
                
                myChildAnimator.SetTrigger("AttackRight3");
                myAnimator.SetTrigger("AttackingRight3");
            }
            if (FacingDirection.x < 0)
            {
                myChildAnimator.SetTrigger("AttackLeft3");
                myAnimator.SetTrigger("AttackingLeft3");
            }
            if (FacingDirection.y > 0
                && FacingDirection.x < 0.2 && FacingDirection.x > -0.2)
            {
                myChildAnimator.SetTrigger("AttackUp3");
                myAnimator.SetTrigger("AttackingUp3");
            }
            if (FacingDirection.y < 0
                && FacingDirection.x < 0.2 && FacingDirection.x > -0.2)
            {
                myChildAnimator.SetTrigger("AttackDown3");
                myAnimator.SetTrigger("AttackingDown3");
            }

        }
    }

    //called in Update
    //sets animation parameters so blend trees for walk and idle animations 
    //know the facing direction. 
    private void AnimBlendSetFloats()
    {
        myAnimator.SetFloat("FaceX", FacingDirection.x);
        myAnimator.SetFloat("FaceY", FacingDirection.y);
    }
    
    private void MoveCamera()
    {
        Camera.main.transform.position = transform.position + cameraOffset; //move camera with player
    }

    //started attack()
    IEnumerator TimeForStrike2()
    {
        canStrike2 = true;
        yield return new WaitForSeconds(1f);
        canStrike2 = false;
    }
    IEnumerator TimeForStrike3()
    {
        canStrike3 = true;
        yield return new WaitForSeconds(1f);
        canStrike3 = false;
    }
    IEnumerator Attack1Length()
    {
        attack1AnimPlaying = true;
        yield return new WaitForSeconds(attackAnim1Length);
        attack1AnimPlaying = false;
    }

    IEnumerator NoDoubleHit()
    {
        for (int i = 0; i < 10; i++)  //wait 3 frames
        {
            yield return null;
        }
        canTakeDamage = true; //then can take damage again
    }

    IEnumerator IsAttackingTimer() // for movement slow during attack
    {
        isAttacking = true;
        yield return new WaitForSeconds(.3f);
        isAttacking = false;
    }


    IEnumerator DashTimer(float time)
    {
        yield return new WaitForSeconds(time);
        if (isAttacking)
        {
            Debug.Log("isAttacking after dash");
        }
        isDashing = false;
    }

    private void Heal(float amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0, maxHealth);
    }

    private void TakeDamage(float amount)
    {
        health -= amount;
        healthSlider.value = Mathf.Clamp(health / maxHealth, 0, 1);
        if (health <= 0)
        {
            //die
            ResetScene();
        }
    }

    private void ResetScene()
    {
        SceneManager.LoadScene(0);
    }
}
