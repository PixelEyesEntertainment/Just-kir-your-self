using UnityEngine;
using EvolveGames;

public class AnimationBlend : MonoBehaviour
{
	[Header("References")]
	public Animator animator;
	public PlayerController Player;
	
	[Header("Settings")]
	public float smoothSpeed = 10f;

	private float xVel;
	private float yVel;
	private Vector3 lastPos;

	void Start()
	{
		lastPos = transform.position;
	}

	void Update()
	{
		Vector3 velocity = (transform.position - lastPos) / Time.deltaTime;
		lastPos = transform.position;

		Vector3 localVel = transform.InverseTransformDirection(velocity);

		float targetX = localVel.x;
		float targetY = localVel.z;

		xVel = Mathf.Lerp(xVel, targetX, Time.deltaTime * smoothSpeed);
		yVel = Mathf.Lerp(yVel, targetY, Time.deltaTime * smoothSpeed);

		animator.SetFloat("X", xVel);
		animator.SetFloat("Y", yVel);
		
		if(Player.isCroughing){
			
			animator.SetBool("Sit",true);
			
		}else{
			
			animator.SetBool("Sit",false);
			
		}
		
		if(Player.isJumping){
			
			animator.SetBool("Jump",true);
			
		}else{
			
			animator.SetBool("Jump",false);
			
		}
		
	}
}
