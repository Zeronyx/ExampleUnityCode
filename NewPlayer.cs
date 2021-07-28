using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewPlayer : PhysicsObject {

	public AudioSource audioSource;
	private AnimatorFunctions animatorFunctions;
	private Vector3 origLocalScale;
	public bool frozen = false;
	private float launch = 1;
	public int health;
	public int maxHealth;
	[SerializeField] private ParticleSystem deathParticles;
	[SerializeField] Vector2 launchPower;
	public GameObject attackHit;
	public float maxSpeed = 7;
    public float jumpTakeOffSpeed = 7;
	public bool recovering;
	public float recoveryCounter;
	public float recoveryTime = 2;
	public CameraEffects cameraEffect;
	public string groundType = "grass";
	public AudioClip stepSound;
	public AudioClip jumpSound;
	public AudioClip grassSound;
	public AudioClip stoneSound;
	[SerializeField] private float launchRecovery;
	[SerializeField] private GameObject graphic;
	[SerializeField] private Animator animator;
	[SerializeField] private bool jumping;

	public RaycastHit2D ground;

	private static NewPlayer instance;
	public static NewPlayer Instance{
		get 
		{ 
			if (instance == null) instance = GameObject.FindObjectOfType<NewPlayer>(); 
			return instance;
		}
	}

	void Start(){
		health = maxHealth;
		audioSource = GetComponent<AudioSource>();
		animatorFunctions = GetComponent<AnimatorFunctions>();
		origLocalScale = transform.localScale;
		SetGroundType ();
	}


    protected override void ComputeVelocity()
    {
		//Player movement && attack
		Vector2 move = Vector2.zero;

		ground = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), -Vector2.up);
		launch += (0 - launch) * Time.deltaTime*launchRecovery;

		if (!frozen) {
			move.x = Input.GetAxis ("Horizontal") + launch;

			//Attack

			if (Input.GetButtonDown("Attack")) {
				animator.SetTrigger ("attack");
			}
		} else {
			launch = 0;
		}

		if(recovering){
			recoveryCounter += Time.deltaTime;
			if (recoveryCounter >= recoveryTime) {
				recoveryCounter = 0;
				recovering = false;
			}
		}
			
        //animator.SetFloat ("velocityX", Mathf.Abs (velocity.x) / maxSpeed);
        //targetVelocity = move * maxSpeed;
    }

	public void SetGroundType(){
		switch (groundType) {
		case "Grass":
			stepSound = grassSound;
			break;
		case "Stone":
			stepSound = stoneSound;
			break;
		}
	}

	public void Freeze(bool freeze){
		frozen = freeze;
		launch = 0;
	}

	public void PlayStepSound(){
		audioSource.pitch = (Random.Range(0.6f, 1f));
		audioSource.PlayOneShot(NewPlayer.Instance.stepSound);
	}

	public void PlayJumpSound(){
		audioSource.pitch = (Random.Range(0.6f, 1f));
		audioSource.PlayOneShot(NewPlayer.Instance.jumpSound);
	}

	public void Hit(int launchDirection){
		if (!recovering) {
			//cameraEffect.Shake (100, 1);
			animator.SetTrigger ("hurt");
			velocity.y = launchPower.y;
			launch = launchDirection * (launchPower.x/5);
            recoveryCounter = 0;
			recovering = true;
            if (health > 1) {
                health -= 1;
            } else {
                Die();
            }
			GameManager.Instance.playerUI.HealthBarHurt();
		}
	}

	public void Die(){
        deathParticles.gameObject.SetActive (true);
        deathParticles.Emit (10);
        deathParticles.transform.parent = transform.parent;
        GameManager.Instance.playerUI.resetPlayer = true;
        GameManager.Instance.playerUI.animator.SetTrigger ("coverScreen");
		GameManager.Instance.playerUI.loadSceneName = SceneManager.GetActiveScene().name;
		GameManager.Instance.playerUI.spawnToObject = "SpawnStart";
		GameManager.Instance.playerUI.resetPlayer = true;
        GameManager.Instance.playerUI.playerDied = true;
		//GetComponent<MeshRenderer> ().enabled = false;
		Freeze (true);
	}

	public void ResetLevel(){
        Freeze (false);
		health = maxHealth;
		//deathParticles.gameObject.SetActive (false);
		GetComponent<MeshRenderer> ().enabled = true;
	}
}