/// <summary>
/// Character attack.
/// this class contail a DoDamage() function to Push a damage out
/// using for called by <CharacterSystem>
/// </summary>

using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

// ��ɶ����ȴ���
// StartDamage()            ��������б�������Activated
// AddObjHitted()           �������ж���
// AddFloatingText()        ��ʾ�˺�ֵ
// DoDamage()               �˺��ж��������˺�������ʾ�˺�Ч��

public class CharacterAttack : MonoBehaviour
{
    //********** attack animation **********//
    public string[] ComboAttackLists; // list of combo set
    public float SpeedAttack = 1.5f; // Attack speed
    
    public int WeaponType; // type of attacking
    private int attackStep = 0;
    private string[] comboList;
    public int attackStack;//?
    public float attackStackTimeTemp;//?

    CharacterSystem system;
    ActionManager actionManager;
    void Start()
    {
        ComboAttackLists = new string[1];
        ComboAttackLists[0] = "0,1";
        system = gameObject.GetComponent<CharacterSystem>();
        actionManager = (ActionManager)FindObjectOfType(typeof(ActionManager));
    }

    private void resetCombo()
    {
        attackStep = 0;
        attackStack = 0;
    }

    public void fightAnimation()
    {
        system.status = CharacterSystem.STATUS.NORMAL;
        if (attackStep >= comboList.Length)
        {
            resetCombo();
        }

        int poseIndex = int.Parse(comboList[attackStep]);
        if (actionManager.actionHash.Contains(poseIndex))
        {// checking poseIndex is must in the PoseAttackNames list.
            if (this.gameObject.GetComponent<CharacterAttack>())
            {
                // Play Attack Animation
                giveSystemMessage();
                this.gameObject.GetComponent<Animation>().Play(((BaseAction)actionManager.actionHash[poseIndex]).animationName, PlayMode.StopAll);
                system.status = CharacterSystem.STATUS.ATTACKPREPARE;
            }
        }
    }

    void doWhenStop()
    {
        attackStep += 1;
        attackStack -= 1;

        // checking if a calling attacking is stacked
        if (attackStack >= 1)
        {
            fightAnimation();//��������
        }

        // reset character damage system
        this.gameObject.GetComponent<CharacterAttack>().StartDamage();
    }

    void giveSystemMessage()
    {
        int poseIndex = int.Parse(comboList[attackStep]);
        system.speed = SpeedAttack * ((BaseAction)actionManager.actionHash[poseIndex]).speed;
        system.prepareTime = ((BaseAction)actionManager.actionHash[poseIndex]).prepareTime;
        system.animationName = ((BaseAction)actionManager.actionHash[poseIndex]).animationName;
        system.doSomething = DoDamage;
        system.doWhenStop = doWhenStop;
    }

    void Update()
    {
        comboList = ComboAttackLists[WeaponType].Split(',');

        if (Time.time > attackStackTimeTemp + 3 && attackStack < 1)
        {
            resetCombo();
        }
    }

    //********** doDamage **********//
    public float Direction = 0.5f;// Direction of object can hit
	public float Radius	= 1;
	public int Force = 500;
	public AudioClip[] SoundHit;
	public bool Activated;
	public GameObject FloatingText;

	HashSet<GameObject> listObjHitted = new HashSet<GameObject>();// list of hited objects
	public void StartDamage()
	{
		listObjHitted.Clear();// Clear list of hited objects
		Activated = false;
	}
	
	private void AddObjHitted(GameObject obj){
		listObjHitted.Add(obj);
	}

	public void AddFloatingText(Vector3 pos,string text){
		// Adding Floating Text Effect
		if(FloatingText){
			var floattext = (GameObject)Instantiate(FloatingText,pos,transform.rotation);
			if(floattext.GetComponent<FloatingText>()){
				floattext.GetComponent<FloatingText>().Text = text;
			}
			GameObject.Destroy(floattext,1);
		}
	}
	
	public void DoDamage()
	{
		Activated = true;
	    var explosionPos = transform.position;
	    var colliders = Physics.OverlapSphere(explosionPos, Radius);
		foreach(var hit in colliders)
		{
	        if (!hit || hit.gameObject == this.gameObject || hit.gameObject.tag == this.gameObject.tag)
	            continue;

			if(listObjHitted.Contains(hit.gameObject))
				continue;

	        var dir	= (hit.transform.position - transform.position).normalized;
	        var direction = Vector3.Dot(dir,transform.forward);
			if(direction < Direction)// Only hit an object in the direction.
				continue;

			var orbit	= Camera.main.gameObject.GetComponent<OrbitGameObject>();
			if(orbit != null && orbit.Target == hit.gameObject)
				ShakeCamera.Shake(0.5f,0.5f);

			var dirforce = (this.transform.forward + transform.up) * Force;
			if(hit.gameObject.GetComponent<CharacterStatus>())
			{
				if(SoundHit.Length>0){
					int randomindex = Random.Range(0, SoundHit.Length);
					if(SoundHit[randomindex] != null){
						AudioSource.PlayClipAtPoint(SoundHit[randomindex], this.transform.position);
					}
				}
				
				int damage = this.gameObject.GetComponent<CharacterStatus>().Damage;
				int damageCal = (int)Random.Range(damage/2.0f, damage)+1;
				var status	= hit.gameObject.GetComponent<CharacterStatus>();
				int takedamage = status.ApplayDamage(damageCal, dirforce);

				// Add Particle Effect
				AddFloatingText(hit.transform.position + Vector3.up, takedamage.ToString());
				status.AddParticle(hit.transform.position + Vector3.up);
			}
			if(hit.GetComponent<Rigidbody>()){
				// push rigidbody object
				hit.GetComponent<Rigidbody>().AddForce(dirforce);
			}
			// add this object to the list. We wont make a multiple hit in the same object
			AddObjHitted(hit.gameObject);
    	}
	}
}