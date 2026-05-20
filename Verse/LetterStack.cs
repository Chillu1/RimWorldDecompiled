using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

public sealed class LetterStack : IExposable
{
	private List<Letter> letters = new List<Letter>();

	private List<Letter> letterQueue = new List<Letter>();

	private int mouseoverLetterIndex = -1;

	private float lastTopYInt;

	private BundleLetter bundleLetterCache;

	private const float LettersBottomY = 350f;

	public const float LetterSpacing = 12f;

	private List<Letter> tmpBundledLetters = new List<Letter>();

	public List<Letter> LettersListForReading => letters;

	public float LastTopY => lastTopYInt;

	public BundleLetter BundleLetter
	{
		get
		{
			if (bundleLetterCache == null)
			{
				bundleLetterCache = (BundleLetter)LetterMaker.MakeLetter(LetterDefOf.BundleLetter);
			}
			return bundleLetterCache;
		}
	}

	public void ReceiveLetter(TaggedString label, TaggedString text, LetterDef textLetterDef, LookTargets lookTargets, Faction relatedFaction = null, Quest quest = null, List<ThingDef> hyperlinkThingDefs = null, string debugInfo = null, int delayTicks = 0, bool playSound = true)
	{
		ChoiceLetter choiceLetter = LetterMaker.MakeLetter(label, text, textLetterDef, lookTargets, relatedFaction, quest, hyperlinkThingDefs);
		ReceiveLetter(choiceLetter, debugInfo, delayTicks, playSound);
	}

	public void ReceiveLetter(TaggedString label, TaggedString text, LetterDef textLetterDef, string debugInfo = null, int delayTicks = 0, bool playSound = true)
	{
		ChoiceLetter choiceLetter = LetterMaker.MakeLetter(label, text, textLetterDef);
		ReceiveLetter(choiceLetter, debugInfo, delayTicks, playSound);
	}

	public void ReceiveLetter(Letter let, string debugInfo = null, int delayTicks = 0, bool playSound = true)
	{
		let.debugInfo = debugInfo;
		if (delayTicks > 0)
		{
			let.arrivalTick = Find.TickManager.TicksGame + delayTicks;
			letterQueue.Add(let);
		}
		else if (let.CanShowInLetterStack)
		{
			if (playSound)
			{
				let.def.arriveSound.PlayOneShotOnCamera();
			}
			if ((int)Prefs.AutomaticPauseMode >= (int)let.def.pauseMode)
			{
				Find.TickManager.Pause();
			}
			else if (let.def.forcedSlowdown)
			{
				Find.TickManager.slower.SignalForceNormalSpeedShort();
			}
			let.arrivalTime = Time.time;
			let.arrivalTick = Find.TickManager.TicksGame;
			letters.Add(let);
			Find.Archive.Add(let);
			let.Received();
		}
	}

	public void RemoveLetter(Letter let)
	{
		letters.Remove(let);
		let.Removed();
	}

	public void LettersOnGUI(float baseY)
	{
		float num = baseY;
		float num2 = baseY - Find.Alerts.AlertsHeight;
		float num3 = 42f;
		int num4 = Mathf.FloorToInt(num2 / num3);
		int num5 = Math.Max(letters.Count - num4, 0);
		if (num5 > 0)
		{
			num5++;
		}
		for (int num6 = letters.Count - 1; num6 >= num5; num6--)
		{
			num -= 30f;
			letters[num6].DrawButtonAt(num);
			num -= 12f;
		}
		if (num5 > 0)
		{
			tmpBundledLetters.Clear();
			tmpBundledLetters.AddRange(letters.Take(num5));
			num -= 30f;
			BundleLetter.SetLetters(tmpBundledLetters);
			BundleLetter.DrawButtonAt(num);
			num -= 12f;
			tmpBundledLetters.Clear();
		}
		lastTopYInt = num;
		if (Event.current.type == EventType.Repaint)
		{
			num = baseY;
			for (int num7 = letters.Count - 1; num7 >= num5; num7--)
			{
				num -= 30f;
				letters[num7].CheckForMouseOverTextAt(num);
				num -= 12f;
			}
			if (num5 > 0)
			{
				num -= 30f;
				BundleLetter.CheckForMouseOverTextAt(num);
				num -= 12f;
			}
		}
	}

	public void OpenAutomaticLetters()
	{
		if (Find.WindowStack.WindowsForcePause)
		{
			return;
		}
		foreach (Letter letter in letters)
		{
			if (letter.ShouldAutomaticallyOpenLetter)
			{
				letter.OpenLetter();
				break;
			}
		}
	}

	public void LetterStackTick()
	{
		OpenAutomaticLetters();
		for (int num = letterQueue.Count - 1; num >= 0; num--)
		{
			if (Find.TickManager.TicksGame >= letterQueue[num].arrivalTick)
			{
				ReceiveLetter(letterQueue[num]);
				letterQueue.RemoveAt(num);
			}
		}
	}

	public void LetterStackUpdate()
	{
		if (mouseoverLetterIndex >= 0 && mouseoverLetterIndex < letters.Count)
		{
			letters[mouseoverLetterIndex].lookTargets.TryHighlight();
		}
		mouseoverLetterIndex = -1;
		for (int num = letters.Count - 1; num >= 0; num--)
		{
			if (!letters[num].CanShowInLetterStack)
			{
				RemoveLetter(letters[num]);
			}
		}
	}

	public void Notify_LetterMouseover(Letter let)
	{
		mouseoverLetterIndex = letters.IndexOf(let);
	}

	public void Notify_FactionRemoved(Faction faction)
	{
		for (int i = 0; i < letters.Count; i++)
		{
			if (letters[i].relatedFaction == faction)
			{
				letters[i].relatedFaction = null;
			}
		}
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref letters, "letters", LookMode.Reference);
		Scribe_Collections.Look(ref letterQueue, "letterQueue", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (letterQueue == null)
			{
				letterQueue = new List<Letter>();
			}
			letters.RemoveAll((Letter x) => x == null);
			letterQueue.RemoveAll((Letter x) => x == null);
		}
	}
}
