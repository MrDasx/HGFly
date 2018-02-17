﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//控制人物移动
public class HGCharacterController : MonoBehaviour {
	//数值配置----------
	[SerializeField] private float JumpSpeedLimit;
	[SerializeField] private float JumpForce;
	[SerializeField] private float Gravity;
	[SerializeField] private float DropForce;
	[SerializeField] private float HeightInitialize;
	[SerializeField] private float MoveSpeedAddition;
	[SerializeField] private float MoveSpeedUpInterval;
	[SerializeField] private float MoveSpeedInitialize;
	[SerializeField] private float RotateSpeed;
	[SerializeField] private HGBlockType MODEINIT = HGBlockType.Mode_Flypee;
	private HGOpinion OP;
    //--------------------

    HGCharacter Charc;
	GameObject Environ;
	string[] ModeOp = new string[11];
	// Use this for initialization
	void LoadOpinion() {
		OP = HGOpinionLoader.OPtemp;
		print(OP.GodMode);
	}
	 void Awake() {
		HGOpinionLoader.Init();
	}
	void Start() {
		LoadOpinion();
		Charc = this.gameObject.GetComponent<HGCharacter>();
		Environ = GameObject.FindWithTag("Environment_");
		Charc.transform.position = new Vector2(0f, HeightInitialize);
		ModeOp[0] = "HGi_flypee";
		ModeOp[1] = "HGi_Sky";
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown("Cancel")) {
			HGm_back();
			return;
		}
        switch (Charc.GetMode()) {
            case HGBlockType.Mode_Pause:
				HGc_mode_pause();
                break;
            case HGBlockType.Mode_Flypee:
				HGc_mode_flypee();
                break;
            case HGBlockType.Mode_Start:
                Charc.UpdateMode(Charc.GetMode());
                break;
            case HGBlockType.Mode_End:
				HGc_mode_end();
                break;
			case HGBlockType.Mode_SkyBattle:
				HGc_mode_sky();
				break;
            default:
                break;
        }
	}//待完善

    void OnCollisionEnter2D(Collision2D collision) {
		if (collision.gameObject.name == "Ground")
			GetComponent<AudioSource>().clip = HGAudioLoader.Load("fall");
		else GetComponent<AudioSource>().clip = HGAudioLoader.Load("hit");
		GetComponent<AudioSource>().Play();

		if (!OP.GodMode) {
			Charc.UpdateMode(HGBlockType.Mode_End);
			StopCoroutine("AutoAddSpeed");
			GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
			GetComponent<ConstantForce2D>().force = new Vector2(0f, DropForce);
			UITimer.StopTiming();
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
		HGBlock.Clear();
		HGObjectPool.GetIns().Clear();
		SceneManager.LoadScene(0);
	}
	void HGc_mode_end() {
		if (Input.GetButtonDown("Restart")) {
			print("Reseted\n");
			transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			transform.position = new Vector2(0f, HeightInitialize);
			GetComponent<ConstantForce2D>().force = new Vector2(0f, 0f);
			GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
			GetComponent<Rigidbody2D>().angularVelocity = 0f;
			Environ.GetComponent<HGEnvironment>().Environ_Init();
			UITimer.ResetTiming();
			Charc.UpdateMode(HGBlockType.Mode_Pause);
		}
	}
	void HGc_mode_pause() {
		if (Input.GetButtonDown("Jump")) {
			Time.timeScale = 1;
			print("Started\n");
			StartCoroutine("AutoAddSpeed");
			Charc.UpdateMode(MODEINIT);
			UITimer.StartTiming();
		}
	}
	void HGc_mode_flypee() {
		if (OP.GodMode && GetComponent<Rigidbody2D>().velocity.x < MoveSpeedInitialize)
			GetComponent<Rigidbody2D>().velocity = new Vector2(MoveSpeedInitialize, GetComponent<Rigidbody2D>().velocity.y);
		if (Input.GetButtonDown("Jump")) {
			GetComponent<AudioSource>().clip = HGAudioLoader.Load("jump");
			GetComponent<AudioSource>().Play();
			Vector2 VecTemp = new Vector2(0f, (GetComponent<Rigidbody2D>().velocity.y < JumpSpeedLimit ? JumpForce : 0f));
			GetComponent<Rigidbody2D>().AddForce(VecTemp);
		}
		if (Input.GetButtonDown("Pause")) {
			print("Paused\n");
			Time.timeScale = 0;
			UITimer.StopTiming();
			Charc.UpdateMode(HGBlockType.Mode_Pause);
		}
	}
	void HGc_mode_sky() {
		HGc_mode_flypee();
	}

	public void UpdateModeOp(HGBlockType mode) {
		print((int)mode);
		if ((int)mode<8) Invoke(ModeOp[(int)mode], 0f);
	}
	void HGi_flypee() {
		GetComponent<ConstantForce2D>().force = new Vector2(0f, Gravity);
		if (GetComponent<Rigidbody2D>().velocity==Vector2.zero) GetComponent<Rigidbody2D>().velocity = new Vector2(MoveSpeedInitialize, 0f);
	}
	void HGi_Sky() {

	}
}
