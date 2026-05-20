using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_GetJoy : ThinkNode_JobGiver
{
	[Unsaved(false)]
	private DefMap<JoyGiverDef, float> joyGiverChances;

	private const float JoyBuffer = 0.99f;

	protected virtual bool CanDoDuringMedicalRest => false;

	protected virtual bool JoyGiverAllowed(JoyGiverDef def)
	{
		return true;
	}

	protected virtual Job TryGiveJobFromJoyGiverDefDirect(JoyGiverDef def, Pawn pawn)
	{
		return def.Worker.TryGiveJob(pawn);
	}

	public override void ResolveReferences()
	{
		joyGiverChances = new DefMap<JoyGiverDef, float>();
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!CanDoDuringMedicalRest && pawn.InBed() && HealthAIUtility.ShouldSeekMedicalRest(pawn))
		{
			return null;
		}
		if (pawn.needs.joy.CurLevel >= 0.99f)
		{
			return null;
		}
		List<JoyGiverDef> allDefsListForReading = DefDatabase<JoyGiverDef>.AllDefsListForReading;
		JoyToleranceSet tolerances = pawn.needs.joy.tolerances;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			JoyGiverDef joyGiverDef = allDefsListForReading[i];
			joyGiverChances[joyGiverDef] = 0f;
			if (!JoyGiverAllowed(joyGiverDef) || pawn.needs.joy.tolerances.BoredOf(joyGiverDef.joyKind) || !joyGiverDef.Worker.CanBeGivenTo(pawn))
			{
				continue;
			}
			if (joyGiverDef.pctPawnsEverDo < 1f)
			{
				Rand.PushState(pawn.thingIDNumber ^ 0x3C49C49);
				if (Rand.Value >= joyGiverDef.pctPawnsEverDo)
				{
					Rand.PopState();
					continue;
				}
				Rand.PopState();
			}
			float num = tolerances[joyGiverDef.joyKind];
			float b = Mathf.Pow(1f - num, 5f);
			b = Mathf.Max(0.001f, b);
			joyGiverChances[joyGiverDef] = joyGiverDef.Worker.GetChance(pawn) * b;
		}
		for (int j = 0; j < joyGiverChances.Count; j++)
		{
			if (!allDefsListForReading.TryRandomElementByWeight((JoyGiverDef d) => joyGiverChances[d], out var result))
			{
				break;
			}
			Job job = TryGiveJobFromJoyGiverDefDirect(result, pawn);
			if (job != null)
			{
				return job;
			}
			joyGiverChances[result] = 0f;
		}
		return null;
	}
}
