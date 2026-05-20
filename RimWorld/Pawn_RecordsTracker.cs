using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Pawn_RecordsTracker : IExposable
{
	public Pawn pawn;

	private DefMap<RecordDef, float> records = new DefMap<RecordDef, float>();

	private Battle battleActive;

	private int battleExitTick;

	private const int UpdateTimeRecordsIntervalTicks = 80;

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
	}

	public void RecordsTickInterval(int delta)
	{
		if (!pawn.Dead && pawn.IsHashIntervalTick(80, delta))
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
	}

	public void Increment(RecordDef def)
	{
		if (def.type != RecordType.Int)
		{
			Log.Error("Tried to increment record \"" + def.defName + "\" whose record type is \"" + def.type.ToString() + "\".");
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
		Log.Error("Tried to add value to record \"" + def.defName + "\" whose record type is \"" + def.type.ToString() + "\".");
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

	public void EnterBattle(Battle battle)
	{
		battleActive = battle;
		battleExitTick = Find.TickManager.TicksGame + 5000;
	}

	public void ExposeData()
	{
		battleActive = BattleActive;
		Scribe_Deep.Look(ref records, "records");
		Scribe_References.Look(ref battleActive, "battleActive");
		Scribe_Values.Look(ref battleExitTick, "battleExitTick", 0);
		BackCompatibility.PostExposeData(this);
	}
}
