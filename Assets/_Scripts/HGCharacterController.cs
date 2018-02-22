﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//控制人物移动
public class HGCharacterController : MonoBehaviour {
	//数值配置----------
	[SerializeField] private float JumpSpeedLimit;
	[SerializeField] private float JumpForce;
	[SerializeField] private float JumpSpeed;
	[SerializeField] private float Gravity;
	[SerializeField] private float DropForce;
	[SerializeField] private float HeightInitialize;
	[SerializeField] private float MoveSpeedAddition;
	[SerializeField] private float MoveSpeedUpInterval;
	[SerializeField] private float MoveSpeedInitialize;
	[SerializeField] private float RotateSpeed;

	public GameObject PauseUI;
	public Text SpeedUI;

	private HGOpinion OP;
	private HGCharacter Character;
	private GameObject Environment;
	private bool landing = false;
	private string[] ModeOp = new string[11];
	private HGBlockType ModeTemp;
	private bool started = false;


	private float jumpstay;
	private int TextureOrd;
	private float jumptime;
	//--------------------

	void ModeOpInit() {
		ModeOp[0] = "HGi_flypee";
		ModeOp[1] = "HGi_Sky";
		ModeOp[2] = "HGi_Runner";
	}
	
	void Awake() {
		HGOpinionLoader.Init();
		ModeOpInit();
		Test_Init();
	}

	void Start() {
		Time.timeScale = 1;
		OP = HGOpinionLoader.OPtemp;
		UIInit();
		Test_SetTexture(2);
		Character = this.gameObject.GetComponent<HGCharacter>();
		Environment = GameObject.FindWithTag("Environment_");
		Character.transform.position = new Vector2(0f, HeightInitialize);
	}

	void Update () {
		SpeedUI.text = string.Format("速度：{0}", GetComponent<Rigidbody2D>().velocity.x);
		if (Input.GetButtonDown("Cancel")) {
			HGm_back();
			return;
		}
        switch (Character.GetMode()) {
            case HGBlockType.Mode_Pause:
				HGc_mode_pause();
                break;
            case HGBlockType.Mode_Flypee:
				HGc_mode_flypee();
                break;
            case HGBlockType.Mode_Start:
				HGc_mode_Start();
                break;
            case HGBlockType.Mode_End:
				HGc_mode_end();
                break;
			case HGBlockType.Mode_SkyBattle:
				HGc_mode_sky();
				break;
			case HGBlockType.Mode_Runner:
				HGc_mode_runner();
				break;
            default:
                break;
        }
	}

    void OnCollisionEnter2D(Collision2D collision) {
		landing = true;
		if (collision.gameObject.name == "Ground")
			PlayAudio("fall");
		else PlayAudio("hit");

		if (OP.GodMode) return;
		switch (Character.GetMode()) {
			case HGBlockType.Mode_Flypee:
			case HGBlockType.Mode_SkyBattle:
				HGcol_mode1();
				break;
			case HGBlockType.Mode_Runner:
				HGcol_mode2();
				break;
			default:
				break;
		}
	}
	void HGcol_mode1() {
		Character.UpdateMode(HGBlockType.Mode_End);
		StopCoroutine("AutoAddSpeed");
		GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
		GetComponent<ConstantForce2D>().force = new Vector2(0f, DropForce);
		CharacterStat statt = UITimer.GetStat();
		HGJsonLoader.BasicWrite<CharacterStat>(statt, ".temp");
		HGJsonLoader.Unload();
		UITimer.StopTiming();
		SceneManager.LoadSceneAsync(4, LoadSceneMode.Additive);
	}
	void HGcol_mode2() {
		StopCoroutine("PlayStep");
		StartCoroutine("PlayStep");
		print("landed\n");
	}

	void OnCollisionStay2D() {
		landing = true;
		switch (Character.GetMode()) {
			case HGBlockType.Mode_Flypee:
			case HGBlockType.Mode_SkyBattle:
				break;
			case HGBlockType.Mode_Runner:

				break;
			default:
				break;
		}
	}
	IEnumerator PlayStep() {
		while (true) {
			yield return new WaitForSeconds(HGAudioLoader.Load("Footstep03_01").length);
			PlayAudio("Footstep03_01");
		}
	}
	void OnCollisionExit2D() {
		landing = false;
		switch (Character.GetMode()) {
			case HGBlockType.Mode_Flypee:
			case HGBlockType.Mode_SkyBattle:
				break;
			case HGBlockType.Mode_Runner:
				
				break;
			default:
				break;
		}
	}
	IEnumerator AutoAddSpeed() {
		while (true) {
			for (float timer = MoveSpeedUpInterval; timer > 0; timer -= Time.deltaTime)
				yield return null;
			GetComponent<Rigidbody2D>().velocity += new Vector2(MoveSpeedAddition, 0f);
		}
	}
	void HGm_back() {
		//HGBlock.Clear();
		//HGObjectPool.GetIns().Clear();
		//HGPreloader.Load(0);
		GamePause();
	}
	void HGc_mode_end() {
		if (Input.GetButtonDown("Restart")) {
			print("Reseted\n");
			transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			transform.position = new Vector2(0f, HeightInitialize);
			GetComponent<ConstantForce2D>().force = new Vector2(0f, 0f);
			GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
			GetComponent<Rigidbody2D>().angularVelocity = 0f;
			Environment.GetComponent<HGEnvironment>().Environ_Init();
			UITimer.ResetTiming();
			SceneManager.UnloadSceneAsync(4);
			started = false;
			Character.UpdateMode(HGBlockType.Mode_Start);
		}
	}
	void HGc_mode_pause() {
		if (Input.GetButtonDown("Jump")) {
//			SceneManager.UnloadSceneAsync(4);
			GameContinue();
		}
	}
	void HGc_mode_flypee() {
		if (OP.GodMode && GetComponent<Rigidbody2D>().velocity.x < MoveSpeedInitialize)
			GetComponent<Rigidbody2D>().velocity = new Vector2(MoveSpeedInitialize, GetComponent<Rigidbody2D>().velocity.y);
		if (TextureOrd == 1) jumptime -= Time.deltaTime;
		Test_SetTexture((GetComponent<Rigidbody2D>().velocity.y > 0 ? 3 : 2));
		if (Input.GetButtonDown("Jump")) {
			Test_SetTexture(1);
			PlayAudio("jump");
			GetComponent<Rigidbody2D>().velocity += new Vector2(0f, JumpSpeed > GetComponent<Rigidbody2D>().velocity.y ? JumpSpeed - GetComponent<Rigidbody2D>().velocity.y : JumpSpeed / 2);
		}
		if (Input.GetButtonDown("Pause")) {
			GamePause();
		}
	}
	void HGc_mode_sky() {
		HGc_mode_flypee();
	}
	void HGc_mode_runner() {
		GetComponent<Rigidbody2D>().velocity = new Vector2(MoveSpeedInitialize, GetComponent<Rigidbody2D>().velocity.y);
		
		if (landing)
		if (Input.GetButtonDown("Jump")) {
			StopCoroutine("PlayStep");
			PlayAudio("jump");
			GetComponent<Rigidbody2D>().angularVelocity = 0f;
			if (transform.localRotation.x==0f)
				transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
			else transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			GetComponent<ConstantForce2D>().force = -GetComponent<ConstantForce2D>().force;
		}
		if (Input.GetButtonDown("Pause")) {
			GamePause();
		}
	}

	void HGc_mode_Start() {
		if (started) return;
		started = true;
		Test_SetTexture(3);
		GetComponent<Rigidbody2D>().velocity = new Vector2(0f , 1f);
		StartCoroutine(StartSpeedUp(5f));	
	}

	IEnumerator StartSpeedUp(float time) {
		for(float timer = time; timer > 0; timer -= 0.1f) {
			GetComponent<Rigidbody2D>().velocity += new Vector2(MoveSpeedInitialize / (time*10f)+0.01f,0f);
			yield return null;
		}
		Character.UpdateMode(HGBlockType.Mode_Flypee);
		UITimer.StartTiming();
		StartCoroutine("AutoAddSpeed");
		yield return null;
	}

	public void UpdateModeOp(HGBlockType mode) {
		print((int)mode);
		if ((int)mode<8) Invoke(ModeOp[(int)mode], 0f);
	}
	void HGi_flypee() {
		GetComponent<Rigidbody2D>().angularVelocity = 0f;
		transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		if (GetComponent<BoxCollider2D>().enabled) {
			GetComponent<BoxCollider2D>().enabled = false;
		}
		GetComponent<ConstantForce2D>().force = new Vector2(0f, Gravity);
		if (GetComponent<Rigidbody2D>().velocity==Vector2.zero) GetComponent<Rigidbody2D>().velocity = new Vector2(MoveSpeedInitialize, 0f);
	}
	void HGi_Sky() {
		HGi_flypee();
	}
	void HGi_Runner() {
		GetComponent<Rigidbody2D>().angularVelocity = 0f;
		transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		Test_SetTexture(4);
		if (!GetComponent<BoxCollider2D>().enabled) {
			GetComponent<BoxCollider2D>().enabled = true;
		}
		GetComponent<ConstantForce2D>().force = new Vector2(0f, Gravity);
		if (GetComponent<Rigidbody2D>().velocity == Vector2.zero) GetComponent<Rigidbody2D>().velocity = new Vector2(MoveSpeedInitialize, 0f);
	}

	void PlayAudio(string filename) {
		GetComponent<AudioSource>().clip = HGAudioLoader.Load(filename);
		GetComponent<AudioSource>().Play();
	}

	public void GamePause() {
		PauseUI.SetActive(true);
		print("Paused\n");
		Time.timeScale = 0;
		//UITimer.StopTiming();
		ModeTemp = Character.GetMode();
		Character.UpdateMode(HGBlockType.Mode_Pause);
	}

	public void GameContinue() {
		PauseUI.SetActive(false);
		Time.timeScale = 1;
		print("Started\n");
		Character.UpdateMode(ModeTemp);
		//UITimer.StartTiming();
	}

	void UIInit() {
		PauseUI.SetActive(false);
	}

	void Test_Init() {
		jumpstay = 0.08f;
		TextureOrd = 0;
		jumptime = -1f;
}
	void Test_SetTexture(int ord) {
		if (TextureOrd == ord) return;
		if (ord != 1 && jumptime > 0) return;
		if (ord == 1) jumptime = jumpstay;
		TextureOrd = ord;
		for (int i = 1; i <= 4; i++) {
			transform.Find(string.Format("Action{0}", i)).gameObject.SetActive(ord == i);
		}
	}
}
