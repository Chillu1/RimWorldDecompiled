using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class KidnappedPawnsTracker : IExposable
{
	private Faction faction;

	private List<Pawn> kidnappedPawns = new List<Pawn>();

	private const int TryRecruitInterval = 15051;

	private const float RecruitMTBDays = 30f;

	public List<Pawn> KidnappedPawnsListForReading => kidnappedPawns;

	public KidnappedPawnsTracker(Faction faction)
	{
		this.faction = faction;
	}

	public void ExposeData()
	{
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			kidnappedPawns.RemoveAll((Pawn x) => x.Destroyed);
		}
		Scribe_Collections.Look(ref kidnappedPawns, "kidnappedPawns", LookMode.Reference);
	}

	public void Kidnap(Pawn pawn, Pawn kidnapper)
	{
		if (kidnappedPawns.Contains(pawn))
		{
			Log.Error("Tried to kidnap already kidnapped pawn " + pawn);
			return;
		}
		if (pawn.Faction == faction)
		{
			Log.Error("Tried to kidnap pawn with the same faction: " + pawn);
			return;
		}
		pawn.PreKidnapped(kidnapper);
		pawn.DeSpawnOrDeselect();
		kidnappedPawns.Add(pawn);
		if (!Find.WorldPawns.Contains(pawn))
		{
			Find.WorldPawns.PassToWorld(pawn);
			if (!Find.WorldPawns.Contains(pawn))
			{
				Log.Error("WorldPawns discarded kidnapped pawn.");
				kidnappedPawns.Remove(pawn);
			}
		}
		if (pawn.Faction == Faction.OfPlayer)
		{
			PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(pawn, null, PawnDiedOrDownedThoughtsKind.Lost);
			BillUtility.Notify_ColonistUnavailable(pawn);
			if (kidnapper != null)
			{
				Find.LetterStack.ReceiveLetter("LetterLabelPawnsKidnapped".Translate(pawn.Named("PAWN")), "LetterPawnsKidnapped".Translate(pawn.Named("PAWN"), kidnapper.Faction.Named("FACTION")), LetterDefOf.NegativeEvent);
			}
		}
		QuestUtility.SendQuestTargetSignals(pawn.questTags, "Kidnapped", pawn.Named("SUBJECT"), kidnapper.Named("KIDNAPPER"));
		Find.GameEnder.CheckOrUpdateGameOver();
	}

	public void RemoveKidnappedPawn(Pawn pawn)
	{
		if (kidnappedPawns.Remove(pawn))
		{
			if (pawn.Faction == Faction.OfPlayer)
			{
				PawnDiedOrDownedThoughtsUtility.RemoveLostThoughts(pawn);
			}
		}
		else
		{
			Log.Warning("Tried to remove kidnapped pawn " + pawn?.ToString() + " but he's not here.");
		}
	}

	public void LogKidnappedPawns()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(faction.Name + ":");
		for (int i = 0; i < kidnappedPawns.Count; i++)
		{
			stringBuilder.AppendLine(kidnappedPawns[i].Name.ToStringFull);
		}
		Log.Message(stringBuilder.ToString());
	}

	public void KidnappedPawnsTrackerTick()
	{
		for (int num = kidnappedPawns.Count - 1; num >= 0; num--)
		{
			if (kidnappedPawns[num].DestroyedOrNull())
			{
				kidnappedPawns.RemoveAt(num);
			}
		}
		if (Find.TickManager.TicksGame % 15051 != 0)
		{
			return;
		}
		for (int num2 = kidnappedPawns.Count - 1; num2 >= 0; num2--)
		{
			if (Rand.MTBEventOccurs(30f, 60000f, 15051f))
			{
				kidnappedPawns[num2].SetFaction(faction);
				kidnappedPawns.RemoveAt(num2);
			}
		}
	}
}
