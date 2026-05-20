using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_JoinPlayer : QuestPart
{
	public string inSignal;

	public string outSignalResult;

	public List<Pawn> pawns = new List<Pawn>();

	public bool joinPlayer;

	public bool makePrisoners;

	public MapParent mapParent;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			if (mapParent != null)
			{
				yield return mapParent;
			}
			foreach (Pawn questLookTarget2 in PawnsArriveQuestPartUtility.GetQuestLookTargets(pawns))
			{
				yield return questLookTarget2;
			}
		}
	}

	public override bool IncreasesPopulation => PawnsArriveQuestPartUtility.IncreasesPopulation(pawns, joinPlayer, makePrisoners);

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!(signal.tag == inSignal))
		{
			return;
		}
		pawns.RemoveAll((Pawn x) => x.Destroyed);
		if (joinPlayer)
		{
			foreach (Pawn pawn in pawns)
			{
				if (pawn.IsColonist || pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony)
				{
					RecruitUtility.Recruit(pawn, Faction.OfPlayer);
				}
				else if (pawn.Faction != Faction.OfPlayer)
				{
					pawn.SetFaction(Faction.OfPlayer);
				}
			}
			return;
		}
		if (!makePrisoners)
		{
			return;
		}
		for (int num = 0; num < pawns.Count; num++)
		{
			if (pawns[num].RaceProps.Humanlike)
			{
				if (!pawns[num].IsPrisonerOfColony)
				{
					pawns[num].guest.SetGuestStatus(Faction.OfPlayer, GuestStatus.Prisoner);
				}
				HealthUtility.TryAnesthetize(pawns[num]);
			}
		}
	}

	public override bool QuestPartReserves(Pawn p)
	{
		return pawns.Contains(p);
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		pawns.Replace(replace, with);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Values.Look(ref outSignalResult, "outSignalResult");
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		Scribe_Values.Look(ref joinPlayer, "joinPlayer", defaultValue: false);
		Scribe_Values.Look(ref makePrisoners, "makePrisoners", defaultValue: false);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			pawns.RemoveAll((Pawn x) => x == null);
		}
	}
}
