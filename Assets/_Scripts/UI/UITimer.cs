﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITimer : MonoBehaviour {
	public Text Textui;
	public int min, sec;
	private static UITimer self;
	public void Tstart() {
		StartCoroutine("AutoTiming");
	}
	public void Tstop() {
		StopCoroutine("AutoTiming");
	}
	public static void StartTiming() {
		self.Tstart();
	}
	public static void StopTiming() {
		self.Tstop();
	}
	public static CharacterStat GetStat() {
		CharacterStat statt = new CharacterStat();
		statt.TimeMin = self.min;
		statt.TimeSec = self.sec;
		statt.Score = GameObject.FindGameObjectWithTag("Character_").GetComponent<HGCharacter>().GetFinalScore();
		return statt;
	}
	public static int GetTime() {
		return (self.min * 60 + self.sec);
	}
	public static void ResetTiming() {
		self.min = 0; self.sec = 0;
		self.Textui.text = string.Format("时间: {0:D2} : {1:D2}", self.min, self.sec);
	}
	public static void InitTiming() {
		self = GameObject.FindGameObjectWithTag("Timer_").GetComponent<UITimer>();
		self.min = 0; self.sec = 0;
		self.Textui.text = string.Format("时间: {0:D2} : {1:D2}", self.min, self.sec);
	}
	IEnumerator AutoTiming() {
		HGCharacter c = GameObject.FindGameObjectWithTag("Character_").GetComponent<HGCharacter>();
		while (true) {
			for (float t = 1f; t > 0; t -= Time.deltaTime)
				yield return null;
			self.sec++;
			if (self.sec >= 60) {
				self.min++;
				self.sec = 0;
			}
			print(self.sec);
			self.Textui.text = string.Format("时间: {0:D2} : {1:D2}", self.min,self.sec);
			c.UpdateScorePRSEC();
		}
	}
}
