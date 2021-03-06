using UnityEngine;

using System.Linq;
using System.Collections.Generic;

public delegate void Action();

[RequireComponent(typeof(CharacterMotor))]
[RequireComponent(typeof(CharacterStatus))]
[RequireComponent(typeof(CharacterAttack))]
[RequireComponent(typeof(CharacterInventory))]

// 1.完善frozetime与被击动作 2.添加倒地判定，倒地拥有倒地连击，倒地保护，起身受击三个阶段

// Start()                  初始化相关对象
// Update()                 更新相关状态(伤害判定阶段，攻击动作阶段等)
// GotHit()                 public 更改受击状态和停滞时间
// resetCombo()             重置攻击状态
// fightAnimation()         播放攻击动画，并重置diddamaged攻击伤害判定
// Attack()                 public 攻击指令
// Move()                   public 向目标方向移动，设置moveDirection时转向，根据转向调整移动距离，修改跑步动画速度，管理站立动画和跑步动画

public class CharacterSystem : MonoBehaviour
{
    public float MoveSpeed = 0.2f; // Move speed
	public float TurnSpeed = 5; // turning speed

    public int actionIndex;
    public float attackSpeed;
    public Action doSomething;
    public Action doWhenStop;

    public string[] PoseHitNames;// pose animation when character got hit
	public string PoseIdle = "Idle";
	public string PoseRun = "Run";
	public bool IsHero;

    //private variable
    public float frozetime;
    public enum STATUS { NORMAL, PREPARE, ATTACK, HITED };
    public STATUS status;
    public bool notMove;

    CharacterMotor motor;
    CharacterAttack attack;
    ActionManager actionManager;

    void Start()
	{
		motor = gameObject.GetComponent<CharacterMotor>();
        attack = gameObject.GetComponent<CharacterAttack>();
        // Play pose Idle first
        gameObject.GetComponent<Animation>().CrossFade(PoseIdle);
        actionManager = (ActionManager)FindObjectOfType(typeof(ActionManager));

        status = STATUS.NORMAL;
        notMove = false;
    }

    void Update()
    {
        // Animation combo system
        if (status == STATUS.PREPARE || status == STATUS.ATTACK)
        {
            // checking index of PoseAttackNames list
            AnimationState attackState = gameObject.GetComponent<Animation>()[
                ((BaseAction)actionManager.actionHash[actionIndex]).animationName]; // get animation PoseAttackNames[poseIndex]
            attackState.layer = 2;
            attackState.blendMode = AnimationBlendMode.Blend;
            attackState.speed = attackSpeed;

            // if the time of attack animation is running to 80% of animation. It's should be Finish this pose.
            if (attackState.time >= ((BaseAction)actionManager.actionHash[actionIndex]).finishTime ||
                attackState.time >= attackState.length)
            {
                attackState.normalizedTime = attackState.length;
                status = STATUS.NORMAL;
                doWhenStop();
            }
            // if the time of attack animation is running to marking point (PoseAttackTime[poseIndex])
            // calling CharacterAttack.cs to push a damage out
            else if (attackState.time >= ((BaseAction)actionManager.actionHash[actionIndex]).prepareTime)
            {
                status = STATUS.ATTACK;
                doSomething();
            }
        }

        // Freeze when got hit
        if (status == STATUS.HITED){
			if(frozetime > 0){
				frozetime--;
			}else{
                status = STATUS.NORMAL;
				this.gameObject.GetComponent<Animation>().Play(PoseIdle);
			}
		}
	}
	
	public void GotHit(float time){
		if(!IsHero){
			if(PoseHitNames.Length > 0){
				// play random Hit animation
				this.gameObject.GetComponent<Animation>().Play(PoseHitNames[Random.Range(0,PoseHitNames.Length)], PlayMode.StopAll);
			}
			frozetime = time * Time.deltaTime;// froze time when got hit
            status = STATUS.NORMAL;
		}
	}
	
	public void Attack()
	{
		if (frozetime <= 0){
            attack.attackStackTimeTemp = Time.time;
            attack.fightAnimation();
            attack.attackStack += 1;
		}
	}
    
	public void Move(Vector3 dir){
        if (!notMove )
        {
            if (status == STATUS.PREPARE)
                moveDirection = dir * ((BaseAction)actionManager.actionHash[actionIndex]).reducedMoveSpeed[0];
            else if (status == STATUS.ATTACK)
                moveDirection = dir * ((BaseAction)actionManager.actionHash[actionIndex]).reducedMoveSpeed[1];
            else if (status == STATUS.HITED)
                moveDirection = dir * 0.2f;
            else
                moveDirection = dir;
        }
	}
	
	Vector3 direction;
	
	private Vector3 moveDirection
	{
		get { return direction; }
		set
		{
			direction = value;
			if(direction.magnitude > 0.1f)
	    	{
	    		var newRotation	= Quaternion.LookRotation(direction);
				transform.rotation	= Quaternion.Slerp(transform.rotation,newRotation,Time.deltaTime * TurnSpeed);
			}
			direction *= MoveSpeed * 0.5f * (Vector3.Dot(gameObject.transform.forward,direction) + 1);
			
			if(direction.magnitude > 0.001f)
			{
				// Play Runing Animation when moving
				float speedaimation = direction.magnitude * 3;
				gameObject.GetComponent<Animation>().CrossFade(PoseRun);
				if(speedaimation < 1){
					speedaimation = 1;
				}
				// Speed animation sync to Move Speed
				gameObject.GetComponent<Animation>()[PoseRun].speed	= speedaimation;
			}
			else{
				// Play Idle Animation when stoped
				gameObject.GetComponent<Animation>().CrossFade(PoseIdle);
			}
			if(motor){
				motor.inputMoveDirection = direction;
			}
		}
	}
	
	float pushPower = 2.0f;

	void OnControllerColliderHit(ControllerColliderHit hit)// Character can push an object.
	{
	    var body = hit.collider.attachedRigidbody;
	    if(body == null || body.isKinematic){
			return;
		}
	    if(hit.moveDirection.y < -0.3){
			return;
		}
		
	    var pushDir = Vector3.Scale(hit.moveDirection,new Vector3(1,0,1));
	    body.velocity = pushDir * pushPower;
	}
}
