using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_HateChant : LordToil
{
	private readonly int DroneBoostInterval = 15000;

	protected LordToilData_HateChant Data => (LordToilData_HateChant)data;

	public LordToil_HateChant()
	{
		data = new LordToilData_HateChant();
	}

	public LordToil_HateChant(IEnumerable<PsychicRitualParticipant> participants)
	{
		data = new LordToilData_HateChant();
		SetParticipants(participants);
	}

	public override void Init()
	{
		base.Init();
		TryCreatingDrone();
		Data.lastDroneUpdate = Find.TickManager.TicksGame;
	}

	public override void UpdateAllDuties()
	{
		foreach (var (pawn2, intVec2) in Data.chanters)
		{
			if (pawn2?.mindState != null)
			{
				pawn2.mindState.duty = new PawnDuty(DutyDefOf.PerformHateChant, intVec2);
			}
			pawn2?.health.AddHediff(HediffDefOf.PsychicTrance);
		}
	}

	public override void LordToilTick()
	{
		base.LordToilTick();
		if (Data.condition != null && (int)Data.condition.level < 5 && Find.TickManager.TicksGame > Data.lastDroneUpdate + DroneBoostInterval)
		{
			Data.lastDroneUpdate = Find.TickManager.TicksGame;
			Data.condition.level++;
			Messages.Message("MessageHateChantIncreased".Translate(), MessageTypeDefOf.ThreatSmall);
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (Data.condition != null)
		{
			Data.condition.End();
			Data.condition = null;
		}
	}

	public void SetParticipants(IEnumerable<PsychicRitualParticipant> participants)
	{
		PsychicRitualParticipant[] collection = (participants as PsychicRitualParticipant[]) ?? participants.ToArray();
		Data.chanters = new List<PsychicRitualParticipant>(collection);
	}

	private void TryCreatingDrone()
	{
		GameConditionManager gameConditionManager = lord.Map.gameConditionManager;
		if (gameConditionManager == null)
		{
			Log.ErrorOnce($"Couldn't find condition manager for incident target {lord.Map}", 70849667);
		}
		else
		{
			if (gameConditionManager.ConditionIsActive(GameConditionDefOf.HateChantDrone))
			{
				return;
			}
			List<GameCondition> activeConditions = gameConditionManager.ActiveConditions;
			for (int i = 0; i < activeConditions.Count; i++)
			{
				if (!GameConditionDefOf.HateChantDrone.CanCoexistWith(activeConditions[i].def))
				{
					return;
				}
			}
			GameCondition_HateChantDrone gameCondition_HateChantDrone = (GameCondition_HateChantDrone)GameConditionMaker.MakeCondition(GameConditionDefOf.HateChantDrone);
			gameConditionManager.RegisterCondition(gameCondition_HateChantDrone);
			Data.condition = gameCondition_HateChantDrone;
		}
	}
}
