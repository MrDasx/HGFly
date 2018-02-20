﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HGOpinionLoader : MonoBehaviour {
	public static HGOpinion OPtemp;
	public GameObject Seedt;
	public GameObject bgmid;
	// Use this for initialization
	public static void Init() {
		OPtemp = HGJsonLoader.BasicRead<HGOpinion>("config.das");
		HGJsonLoader.Unload();
	}
	void Start () {
		Init();
		Invoke("UpdateOpinion", 0f);
	}

	void UpdateOpinion() {
		transform.Find("GodMode").GetComponent<Toggle>().isOn = OPtemp.GodMode;
		Seedt.GetComponent<InputField>().text = OPtemp.Seed;
		bgmid.GetComponent<InputField>().text = OPtemp.BgmID.ToString();
	}
}
