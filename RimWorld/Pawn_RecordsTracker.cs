using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Pawn_RecordsTracker : IExposable
	{
		public Pawn pawn;

		private DefMap<RecordDef, float> records = new DefMap<RecordDef, float>();

		private double storyRelevance;

		private Battle battleActive;

		private int battleExitTick;

		private float storyRelevanceBonus;

		private const int UpdateTimeRecordsIntervalTicks = 80;

		private const float StoryRelevanceBonusRange = 100f;

		private const float StoryRelevanceMultiplierPerYear = 0.2f;

		public float StoryRelevance => (float)storyRelevance + storyRelevanceBonus;

		public Battle BattleActive
		{
			get
			{
				if (battleExitTick < Find.TickManager.TicksGame)
				{
					return null;
				}
				if (battleActive == null)
				{
					return null;
				}
				while (battleActive.AbsorbedBy != null)
				{
					battleActive = battleActive.AbsorbedBy;
				}
				return battleActive;
			}
		}

		public int LastBattleTick => battleExitTick;

		public Pawn_RecordsTracker(Pawn pawn)
		{
			this.pawn = pawn;
			Rand.PushState();
			Rand.Seed = pawn.thingIDNumber * 681;
			storyRelevanceBonus = Rand.Range(0f, 100f);
			Rand.PopState();
		}

		public void RecordsTick()
		{
			if (!pawn.Dead && pawn.IsHashIntervalTick(80))
			{
				RecordsTickUpdate(80);
				battleActive = BattleActive;
			}
		}

		public void RecordsTickMothballed(int interval)
		{
			RecordsTickUpdate(interval);
		}

		private void RecordsTickUpdate(int interval)
		{
			List<RecordDef> allDefsListForReading = DefDatabase<RecordDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (allDefsListForReading[i].type == RecordType.Time && allDefsListForReading[i].Worker.ShouldMeasureTimeNow(pawn))
				{
					records[allDefsListForReading[i]] += interval;
				}
			}
			storyRelevance *= Math.Pow(0.20000000298023224, 0.0);
		}

		public void Increment(RecordDef def)
		{
			if (def.type != RecordType.Int)
			{
				Log.Error(string.Concat("Tried to increment record \"", def.defName, "\" whose record type is \"", def.type, "\"."));
			}
			else
			{
				records[def] = Mathf.Round(records[def] + 1f);
			}
		}

		public void AddTo(RecordDef def, float value)
		{
			if (def.type == RecordType.Int)
			{
				records[def] = Mathf.Round(records[def] + Mathf.Round(value));
				return;
			}
			if (def.type == RecordType.Float)
			{
				records[def] += value;
				return;
			}
			Log.Error(string.Concat("Tried to add value to record \"", def.defName, "\" whose record type is \"", def.type, "\"."));
		}

		public float GetValue(RecordDef def)
		{
			float num = records[def];
			if (def.type == RecordType.Int || def.type == RecordType.Time)
			{
				return Mathf.Round(num);
			}
			return num;
		}

		public int GetAsInt(RecordDef def)
		{
			return Mathf.RoundToInt(records[def]);
		}

		public void AccumulateStoryEvent(StoryEventDef def)
		{
			storyRelevance += def.importance;
		}

		public void EnterBattle(Battle battle)
		{
			battleActive = battle;
			battleExitTick = Find.TickManager.TicksGame + 5000;
		}

		public void ExposeData()
		{
			battleActive = BattleActive;
			Scribe_Deep.Look(ref records, "records");
			Scribe_Values.Look(ref storyRelevance, "storyRelevance", 0.0);
			Scribe_References.Look(ref battleActive, "battleActive");
			Scribe_Values.Look(ref battleExitTick, "battleExitTick", 0);
			BackCompatibility.PostExposeData(this);
		}
	}
}
