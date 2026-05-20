using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_BetrayalOffer : QuestPartActivable
{
	public ExtraFaction extraFaction;

	public Pawn asker;

	public List<Pawn> pawns = new List<Pawn>();

	public List<string> inSignals = new List<string>();

	public string outSignalEnabled;

	public string outSignalSuccess;

	public string outSignalFailure;

	public string outSignalFailureRecruited;

	public override string ExpiryInfoPart => "QuestBetrayalOffer".Translate(PawnsAliveCount, extraFaction.faction.Name);

	public override string ExpiryInfoPartTip => "QuestBetrayalOfferTip".Translate(asker.NameFullColored, extraFaction.faction.Name);

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			for (int i = 0; i < pawns.Count; i++)
			{
				if (!pawns[i].Destroyed)
				{
					yield return pawns[i];
				}
			}
		}
	}

	private int PawnsAliveCount
	{
		get
		{
			int num = 0;
			for (int i = 0; i < pawns.Count; i++)
			{
				if (!pawns[i].Destroyed)
				{
					num++;
				}
			}
			return num;
		}
	}

	private bool AnyPawnDespawnedButAlive
	{
		get
		{
			for (int i = 0; i < pawns.Count; i++)
			{
				if (!pawns[i].SpawnedOrAnyParentSpawned && !pawns[i].Dead)
				{
					return true;
				}
			}
			return false;
		}
	}

	private bool AllSurvivingPawnsRecruited
	{
		get
		{
			foreach (Pawn pawn in pawns)
			{
				if (!pawn.Destroyed && (!pawn.Faction.IsPlayerSafe() || pawn.GetExtraFaction(extraFaction.factionType, quest) == extraFaction.faction))
				{
					return false;
				}
			}
			return true;
		}
	}

	protected override void Enable(SignalArgs receivedArgs)
	{
		base.Enable(receivedArgs);
		if (!AnyPawnDespawnedButAlive && PawnsAliveCount > 0)
		{
			if (!outSignalEnabled.NullOrEmpty())
			{
				Find.SignalManager.SendSignal(new Signal(outSignalEnabled, receivedArgs));
			}
		}
		else
		{
			Complete();
		}
	}

	protected override void ProcessQuestSignal(Signal signal)
	{
		base.ProcessQuestSignal(signal);
		if (inSignals.Contains(signal.tag))
		{
			if (AnyPawnDespawnedButAlive)
			{
				Find.SignalManager.SendSignal(new Signal(outSignalFailure, signal.args));
				Complete();
			}
			else if (PawnsAliveCount == 0)
			{
				Find.SignalManager.SendSignal(new Signal(outSignalSuccess, signal.args));
				Complete();
			}
			else if (AllSurvivingPawnsRecruited)
			{
				string tag = outSignalFailureRecruited ?? outSignalFailure;
				Find.SignalManager.SendSignal(new Signal(tag, signal.args));
				Complete();
			}
		}
	}

	public override void Notify_FactionRemoved(Faction f)
	{
		if (extraFaction.faction == f)
		{
			extraFaction.faction = null;
		}
	}

	public override void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
	{
		base.Notify_PawnKilled(pawn, dinfo);
		if (pawn == asker && base.State == QuestPartState.Enabled)
		{
			Disable();
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		Scribe_Deep.Look(ref extraFaction, "extraFaction");
		Scribe_References.Look(ref asker, "asker");
		Scribe_Collections.Look(ref inSignals, "inSignals", LookMode.Value);
		Scribe_Values.Look(ref outSignalSuccess, "outSignalSuccess");
		Scribe_Values.Look(ref outSignalFailure, "outSignalFailure");
		Scribe_Values.Look(ref outSignalFailureRecruited, "outSignalFailureRecruited");
		Scribe_Values.Look(ref outSignalEnabled, "outSignalEnabled");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			pawns.RemoveAll((Pawn x) => x == null);
			if (asker == null && base.State == QuestPartState.Enabled)
			{
				Disable();
			}
		}
	}
}
