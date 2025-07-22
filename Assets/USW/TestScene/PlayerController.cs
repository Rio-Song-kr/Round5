using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

    public float groundSpeed = 30f;
    public float airSpeed = 0.2f;
    public float speedDamp = 0.4f;
    public float jumpForce = 1000;
    public bool grounded = false;
    private GameObject groundCheck;
    public Vector2 groundCheckPosition = new Vector2(0,-0.5f);
    public float groundRadius = 0.2f;
    public LayerMask whatIsGround;
    public Vector2 targetVelocity;
    private RopeManager ropeManager;

    private PhysicsMaterial2D physMatBouncy;
    private PhysicsMaterial2D physMatRegular;
    private bool hooked;

    void Awake()
    {
       groundCheck = new GameObject();
       groundCheck.transform.name = "GroundCheck";
       groundCheck.transform.parent = transform;
       groundCheck.transform.localPosition = groundCheckPosition;

       ropeManager = gameObject.GetComponent<RopeManager>();

       physMatBouncy = Resources.Load<PhysicsMaterial2D>("Bounds_Physics Mat");
       physMatRegular = Resources.Load<PhysicsMaterial2D>("Common_Physics Mat");
       
    }
       
    void FixedUpdate()
    {
       grounded = Physics2D.OverlapCircle (groundCheck.transform.position, groundRadius, whatIsGround);

       Vector2 newVelocity;
       newVelocity = new Vector2(Input.GetAxis ("Horizontal"), 0);
       if(newVelocity.magnitude > 1)
          newVelocity.Normalize ();

       if(grounded)
       {
          GetComponent<Rigidbody2D>().velocity += newVelocity * groundSpeed;
          
          float desiredSpeed = GetComponent<Rigidbody2D>().velocity.x;
          
          desiredSpeed = Mathf.Clamp (desiredSpeed, -groundSpeed, groundSpeed);
          GetComponent<Rigidbody2D>().velocity = new Vector2(desiredSpeed, GetComponent<Rigidbody2D>().velocity.y);

          if(Input.GetButtonDown ("Jump"))
          {
             GetComponent<Rigidbody2D>().AddForce (new Vector2(0, jumpForce));
          }
       }
       else if(ropeManager.hook && ropeManager.hookScript.hooked)
       {
          {
             GetComponent<Rigidbody2D>().velocity += newVelocity * airSpeed;
          }
       }

       if(ropeManager.hook && ropeManager.hookScript.hooked)
       {
          ChangeCollMat(physMatBouncy);
       }
       else
       {
          ChangeCollMat(physMatRegular);
       }
    }

    void ChangeCollMat(PhysicsMaterial2D physMat)
    {
       if(gameObject.GetComponent<Collider2D>().sharedMaterial != physMat)
       {
          gameObject.GetComponent<Collider2D>().sharedMaterial = physMat;
          gameObject.GetComponent<Collider2D>().enabled = false;
          gameObject.GetComponent<Collider2D>().enabled = true;
       }
    }
}