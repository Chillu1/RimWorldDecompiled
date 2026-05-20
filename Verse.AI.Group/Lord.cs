using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse.AI.Group;

[StaticConstructorOnStartup]
public class Lord : IExposable, ILoadReferenceable, ISignalReceiver, IDisposable
{
	public LordManager lordManager;

	private LordToil curLordToil;

	private StateGraph graph;

	public int loadID = -1;

	private LordJob curJob;

	public Faction faction;

	public List<Pawn> ownedPawns = new List<Pawn>();

	public List<Building> ownedBuildings = new List<Building>();

	public List<Corpse> ownedCorpses = new List<Corpse>();

	public List<Thing> extraForbiddenThings = new List<Thing>();

	public List<string> questTags;

	public string inSignalLeave;

	private bool initialized;

	public int ticksInToil;

	public int numPawnsLostViolently;

	public int numPawnsEverGained;

	public int initialColonyHealthTotal;

	public int lastPawnHarmTick = -99999;

	private static readonly Material FlagTex = MaterialPool.MatFrom("UI/Overlays/SquadFlag");

	private int tmpCurLordToilIdx = -1;

	private Dictionary<int, LordToilData> tmpLordToilData = new Dictionary<int, LordToilData>();

	private Dictionary<int, TriggerData> tmpTriggerData = new Dictionary<int, TriggerData>();

	public Map Map => lordManager.map;

	public StateGraph Graph => graph;

	public LordToil CurLordToil => curLordToil;

	public LordJob LordJob => curJob;

	private bool CanExistWithoutPawns
	{
		get
		{
			if (!curJob.ShouldExistWithoutPawns)
			{
				return Map.deferredSpawner.GetRequestByLord(this) != null;
			}
			return true;
		}
	}

	private bool ShouldExist
	{
		get
		{
			if (ownedPawns.Count <= 0 && !CanExistWithoutPawns)
			{
				if (ownedBuildings.Count > 0)
				{
					return curJob.KeepExistingWhileHasAnyBuilding;
				}
				return false;
			}
			return true;
		}
	}

	public bool AnyActivePawn
	{
		get
		{
			for (int i = 0; i < ownedPawns.Count; i++)
			{
				if (ownedPawns[i].mindState != null && ownedPawns[i].mindState.Active)
				{
					return true;
				}
			}
			return false;
		}
	}

	private void Init()
	{
		initialized = true;
		initialColonyHealthTotal = Map.wealthWatcher.HealthTotal;
	}

	public string GetUniqueLoadID()
	{
		return "Lord_" + loadID;
	}

	public void ExposeData()
	{
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			extraForbiddenThings.RemoveAll((Thing x) => x.DestroyedOrNull());
			ownedPawns.RemoveAll((Pawn x) => x.DestroyedOrNull());
			ownedBuildings.RemoveAll((Building x) => x.DestroyedOrNull());
			ownedCorpses.RemoveAll((Corpse x) => x.DestroyedOrNull());
		}
		Scribe_Values.Look(ref loadID, "loadID", 0);
		Scribe_References.Look(ref faction, "faction");
		Scribe_Collections.Look(ref extraForbiddenThings, "extraForbiddenThings", LookMode.Reference);
		Scribe_Collections.Look(ref ownedPawns, "ownedPawns", LookMode.Reference);
		Scribe_Collections.Look(ref ownedBuildings, "ownedBuildings", LookMode.Reference);
		Scribe_Collections.Look(ref ownedCorpses, "ownedCorpses", LookMode.Reference);
		Scribe_Deep.Look(ref curJob, "lordJob");
		Scribe_Values.Look(ref initialized, "initialized", defaultValue: true);
		Scribe_Values.Look(ref ticksInToil, "ticksInToil", 0);
		Scribe_Values.Look(ref numPawnsEverGained, "numPawnsEverGained", 0);
		Scribe_Values.Look(ref numPawnsLostViolently, "numPawnsLostViolently", 0);
		Scribe_Values.Look(ref initialColonyHealthTotal, "initialColonyHealthTotal", 0);
		Scribe_Values.Look(ref lastPawnHarmTick, "lastPawnHarmTick", -99999);
		Scribe_Values.Look(ref inSignalLeave, "inSignalLeave");
		Scribe_Collections.Look(ref questTags, "questTags", LookMode.Value);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			extraForbiddenThings.RemoveAll((Thing x) => x == null);
			ownedPawns.RemoveAll((Pawn x) => x == null);
			ownedBuildings.RemoveAll((Building x) => x == null);
			ownedCorpses.RemoveAll((Corpse x) => x == null);
			ownedPawns.ForEach(delegate(Pawn p)
			{
				p.lord = this;
			});
		}
		ExposeData_StateGraph();
	}

	public AcceptanceReport AllowsDrafting(Pawn pawn)
	{
		if (curJob == null)
		{
			return false;
		}
		return curJob.AllowsDrafting(pawn);
	}

	public AcceptanceReport AllowsFloatMenu(Pawn pawn)
	{
		if (curJob == null)
		{
			return true;
		}
		return curJob.AllowsFloatMenu(pawn);
	}

	public bool BlocksSocialInteraction(Pawn pawn)
	{
		return curJob?.BlocksSocialInteraction(pawn) ?? false;
	}

	public bool PrisonerSecure(Pawn pawn)
	{
		return curJob?.PrisonerSecure(pawn) ?? false;
	}

	public AcceptanceReport AbilityAllowed(Ability ability)
	{
		if (curJob == null)
		{
			return true;
		}
		return curJob.AbilityAllowed(ability);
	}

	public ThinkResult Notify_DutyConstantResult(ThinkResult result, Pawn pawn, JobIssueParams issueParams)
	{
		if (curJob == null)
		{
			return result;
		}
		return curJob.Notify_DutyConstantResult(result, pawn, issueParams);
	}

	public ThinkResult Notify_DutyResult(ThinkResult result, Pawn pawn, JobIssueParams issueParams)
	{
		if (curJob == null)
		{
			return result;
		}
		return curJob.Notify_DutyResult(result, pawn, issueParams);
	}

	private void ExposeData_StateGraph()
	{
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			tmpLordToilData.Clear();
			for (int i = 0; i < graph.lordToils.Count; i++)
			{
				if (graph.lordToils[i].data != null)
				{
					tmpLordToilData.Add(i, graph.lordToils[i].data);
				}
			}
			tmpTriggerData.Clear();
			int num = 0;
			for (int j = 0; j < graph.transitions.Count; j++)
			{
				for (int k = 0; k < graph.transitions[j].triggers.Count; k++)
				{
					if (graph.transitions[j].triggers[k].data != null)
					{
						tmpTriggerData.Add(num, graph.transitions[j].triggers[k].data);
					}
					num++;
				}
			}
			tmpCurLordToilIdx = graph.lordToils.IndexOf(curLordToil);
		}
		Scribe_Collections.Look(ref tmpLordToilData, "lordToilData", LookMode.Value, LookMode.Deep);
		Scribe_Collections.Look(ref tmpTriggerData, "triggerData", LookMode.Value, LookMode.Deep);
		Scribe_Values.Look(ref tmpCurLordToilIdx, "curLordToilIdx", -1);
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		if (curJob.LostImportantReferenceDuringLoading)
		{
			lordManager.RemoveLord(this);
			return;
		}
		LordJob lordJob = curJob;
		curJob = null;
		SetJob(lordJob, loading: true);
		foreach (KeyValuePair<int, LordToilData> tmpLordToilDatum in tmpLordToilData)
		{
			if (tmpLordToilDatum.Key < 0 || tmpLordToilDatum.Key >= graph.lordToils.Count)
			{
				Log.Error("Could not find lord toil for lord toil data of type \"" + tmpLordToilDatum.Value.GetType()?.ToString() + "\" (lord job: \"" + curJob.GetType()?.ToString() + "\"), because lord toil index is out of bounds: " + tmpLordToilDatum.Key);
			}
			else
			{
				graph.lordToils[tmpLordToilDatum.Key].data = tmpLordToilDatum.Value;
			}
		}
		tmpLordToilData.Clear();
		foreach (KeyValuePair<int, TriggerData> tmpTriggerDatum in tmpTriggerData)
		{
			Trigger triggerByIndex = GetTriggerByIndex(tmpTriggerDatum.Key);
			if (triggerByIndex == null)
			{
				Log.Error("Could not find trigger for trigger data of type \"" + tmpTriggerDatum.Value.GetType()?.ToString() + "\" (lord job: \"" + curJob.GetType()?.ToString() + "\"), because trigger index is out of bounds: " + tmpTriggerDatum.Key);
			}
			else
			{
				triggerByIndex.data = tmpTriggerDatum.Value;
			}
		}
		tmpTriggerData.Clear();
		if (tmpCurLordToilIdx < 0 || tmpCurLordToilIdx >= graph.lordToils.Count)
		{
			Log.Error("Current lord toil index out of bounds (lord job: \"" + curJob.GetType()?.ToString() + "\"): " + tmpCurLordToilIdx);
		}
		else
		{
			curLordToil = graph.lordToils[tmpCurLordToilIdx];
		}
	}

	public void SetJob(LordJob lordJob, bool loading = false)
	{
		if (curJob != null)
		{
			curJob.Cleanup();
		}
		curJob = lordJob;
		curLordToil = null;
		lordJob.lord = this;
		if (!loading)
		{
			lordJob.Notify_AddedToLord();
		}
		Rand.PushState();
		Rand.Seed = loadID * 193;
		graph = lordJob.CreateGraph();
		Rand.PopState();
		graph.ErrorCheck();
		if (faction != null && !faction.IsPlayer && faction.def.autoFlee && !faction.neverFlee && lordJob.AddFleeToil && lordJob.lord.Map.CanEverExit)
		{
			LordToil_PanicFlee lordToil_PanicFlee = new LordToil_PanicFlee();
			lordToil_PanicFlee.useAvoidGrid = true;
			for (int i = 0; i < graph.lordToils.Count; i++)
			{
				Transition transition = new Transition(graph.lordToils[i], lordToil_PanicFlee);
				transition.AddPreAction(new TransitionAction_Message("MessageFightersFleeing".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
				transition.AddTrigger(new Trigger_FractionPawnsLost(faction.def.attackersDownPercentageRangeForAutoFlee.RandomInRangeSeeded(loadID)));
				transition.AddPostAction(new TransitionAction_Custom((Action)delegate
				{
					QuestUtility.SendQuestTargetSignals(lordJob.lord.questTags, "Fleeing", lordJob.lord.Named("SUBJECT"));
				}));
				graph.AddTransition(transition, highPriority: true);
			}
			graph.AddToil(lordToil_PanicFlee);
		}
		for (int num = 0; num < graph.lordToils.Count; num++)
		{
			graph.lordToils[num].lord = this;
		}
		for (int num2 = 0; num2 < ownedPawns.Count; num2++)
		{
			Map.attackTargetsCache.UpdateTarget(ownedPawns[num2]);
		}
	}

	public void Cleanup()
	{
		try
		{
			curJob.Cleanup();
		}
		catch (Exception ex)
		{
			Log.Error("Error in LordJob.Cleanup(): " + ex);
		}
		if (curLordToil != null)
		{
			try
			{
				curLordToil.Cleanup();
			}
			catch (Exception ex2)
			{
				Log.Error("Error in LordToil.Cleanup(): " + ex2);
			}
		}
		for (int i = 0; i < ownedPawns.Count; i++)
		{
			if (ownedPawns[i].mindState != null)
			{
				ownedPawns[i].mindState.duty = null;
			}
			if (ownedPawns[i].lord == this)
			{
				ownedPawns[i].lord = null;
			}
			Map.attackTargetsCache.UpdateTarget(ownedPawns[i]);
			if (curJob.EndPawnJobOnCleanup(ownedPawns[i]) && ownedPawns[i].Spawned && ownedPawns[i].CurJob != null && (!curJob.DontInterruptLayingPawnsOnCleanup || !RestUtility.IsLayingForJobCleanup(ownedPawns[i])))
			{
				ownedPawns[i].jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}
		try
		{
			curJob.PostCleanup();
		}
		catch (Exception ex3)
		{
			Log.Error("Error in LordJob.PostCleanup(): " + ex3);
		}
	}

	public bool CanAddPawn(Pawn p)
	{
		return curLordToil.CanAddPawn(p);
	}

	public void AddPawns(IEnumerable<Pawn> pawns, bool updateDuties = true)
	{
		foreach (Pawn pawn in pawns)
		{
			AddPawnInternal(pawn, updateDuties: false);
		}
		if (updateDuties)
		{
			try
			{
				curLordToil.UpdateAllDuties();
			}
			catch (Exception ex)
			{
				Log.Error("Error in LordToil.UpdateAllDuties(): " + ex);
			}
		}
	}

	public void AddPawn(Pawn p)
	{
		if (!CanAddPawn(p))
		{
			Log.Error("Tried to add pawn " + p?.ToString() + " to lord " + this?.ToString() + " but this pawn can't be added to this lord.");
		}
		else
		{
			AddPawnInternal(p, updateDuties: true);
		}
	}

	private void AddPawnInternal(Pawn p, bool updateDuties)
	{
		if (ownedPawns.Contains(p))
		{
			Log.Error("Lord for " + faction.ToStringSafe() + " tried to add " + p?.ToString() + " whom it already controls.");
			return;
		}
		if (p.GetLord() != null)
		{
			Log.Error("Tried to add pawn " + p?.ToString() + " to lord with lord job" + LordJob?.ToString() + " but this pawn is already a member of lord with lord job" + p.GetLord().LordJob?.ToString() + ". Pawns can't be members of more than one lord at the same time.");
			return;
		}
		ownedPawns.Add(p);
		p.lord = this;
		numPawnsEverGained++;
		Map.attackTargetsCache.UpdateTarget(p);
		if (updateDuties)
		{
			try
			{
				curLordToil.UpdateAllDuties();
			}
			catch (Exception ex)
			{
				Log.Error("Error in LordToil.UpdateAllDuties(): " + ex);
			}
		}
		try
		{
			curJob?.Notify_PawnAdded(p);
		}
		catch (Exception ex2)
		{
			Log.Error("Error in LordJob.Notify_PawnAdded(): " + ex2);
		}
	}

	public void AddBuilding(Building b)
	{
		if (ownedBuildings.Contains(b))
		{
			Log.Error("Lord for " + faction.ToStringSafe() + " tried to add " + b?.ToString() + " which it already controls.");
			return;
		}
		ownedBuildings.Add(b);
		try
		{
			curLordToil.UpdateAllDuties();
		}
		catch (Exception ex)
		{
			Log.Error("Error in LordToil.UpdateAllDuties(): " + ex);
		}
		try
		{
			curJob.Notify_BuildingAdded(b);
		}
		catch (Exception ex2)
		{
			Log.Error("Error in LordJob.Notify_BuildingAdded(): " + ex2);
		}
	}

	public void AddCorpse(Corpse c)
	{
		if (ownedCorpses.Contains(c))
		{
			Log.Error("Lord for " + faction.ToStringSafe() + " tried to add " + c?.ToString() + " which it already controls.");
			return;
		}
		ownedCorpses.Add(c);
		try
		{
			curJob.Notify_CorpseAdded(c);
		}
		catch (Exception ex)
		{
			Log.Error("Error in LordJob.Notify_CorpseAdded(): " + ex);
		}
	}

	public void RemovePawn(Pawn p)
	{
		ownedPawns.Remove(p);
		if (p.mindState != null)
		{
			p.mindState.duty = null;
		}
		if (p.lord == this)
		{
			p.lord = null;
		}
		Map.attackTargetsCache.UpdateTarget(p);
	}

	public void RemovePawns(List<Pawn> pawns)
	{
		foreach (Pawn pawn in pawns)
		{
			RemovePawn(pawn);
		}
	}

	public void RemoveAllPawns(bool interruptPawnJobs = true)
	{
		foreach (Pawn ownedPawn in ownedPawns)
		{
			if (ownedPawn.mindState != null)
			{
				ownedPawn.mindState.duty = null;
				if (interruptPawnJobs && ownedPawn.jobs?.curJob?.lord == this)
				{
					ownedPawn.jobs.EndCurrentJob(JobCondition.InterruptForced, startNewJob: false);
				}
			}
			Map.attackTargetsCache.UpdateTarget(ownedPawn);
			if (ownedPawn.lord == this)
			{
				ownedPawn.lord = null;
			}
		}
		ownedPawns.Clear();
	}

	public void RemoveAllBuildings()
	{
		ownedBuildings.Clear();
	}

	public void GotoToil(LordToil newLordToil)
	{
		LordToil previousToil = curLordToil;
		if (curLordToil != null)
		{
			try
			{
				curLordToil.Cleanup();
			}
			catch (Exception ex)
			{
				Log.Error("Error in LordToil.Cleanup(): " + ex);
			}
		}
		curLordToil = newLordToil;
		ticksInToil = 0;
		if (curLordToil.lord != this)
		{
			Log.Error("curLordToil lord is " + ((curLordToil.lord == null) ? "null (forgot to add toil to graph?)" : curLordToil.lord.ToString()));
			curLordToil.lord = this;
		}
		try
		{
			curLordToil.Init();
		}
		catch (Exception ex2)
		{
			Log.Error("Error in LordToil.Init(): " + ex2);
		}
		for (int i = 0; i < graph.transitions.Count; i++)
		{
			if (graph.transitions[i].sources.Contains(curLordToil))
			{
				graph.transitions[i].SourceToilBecameActive(graph.transitions[i], previousToil);
			}
		}
		try
		{
			curLordToil.UpdateAllDuties();
		}
		catch (Exception ex3)
		{
			Log.Error("Error in LordToil.UpdateAllDuties(): " + ex3);
		}
	}

	public void LordTick()
	{
		if (ticksInToil % 60 == 0)
		{
			for (int i = 0; i < ownedPawns.Count; i++)
			{
				Pawn pawn = ownedPawns[i];
				if (Find.WorldPawns.GetSituation(pawn) == WorldPawnSituation.Free)
				{
					Log.ErrorOnce($"Lord {this} ({curJob}) owns a free world pawn {pawn.LabelShort}. Is there WorldPawnSituation to be defined?", loadID ^ 0x50E9382F);
				}
			}
		}
		if (!initialized)
		{
			Init();
		}
		curJob.LordJobTick();
		curLordToil.LordToilTick();
		CheckTransitionOnSignal(TriggerSignal.ForTick);
		ticksInToil++;
	}

	private Trigger GetTriggerByIndex(int index)
	{
		int num = 0;
		for (int i = 0; i < graph.transitions.Count; i++)
		{
			for (int j = 0; j < graph.transitions[i].triggers.Count; j++)
			{
				if (num == index)
				{
					return graph.transitions[i].triggers[j];
				}
				num++;
			}
		}
		return null;
	}

	public void ReceiveMemo(string memo)
	{
		CheckTransitionOnSignal(TriggerSignal.ForMemo(memo));
	}

	public void Notify_FactionRelationsChanged(Faction otherFaction, FactionRelationKind previousRelationKind)
	{
		CheckTransitionOnSignal(new TriggerSignal
		{
			type = TriggerSignalType.FactionRelationsChanged,
			faction = otherFaction,
			previousRelationKind = previousRelationKind
		});
		for (int i = 0; i < ownedPawns.Count; i++)
		{
			if (ownedPawns[i].Spawned)
			{
				ownedPawns[i].jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}
	}

	private void Destroy()
	{
		lordManager.RemoveLord(this);
		curJob.Notify_LordDestroyed();
		if (faction != null)
		{
			QuestUtility.SendQuestTargetSignals(questTags, "AllEnemiesDefeated");
		}
	}

	public void Notify_PawnJobDone(Pawn pawn, JobCondition condition)
	{
		try
		{
			curJob.Notify_PawnJobDone(pawn, condition);
		}
		catch (Exception ex)
		{
			Log.Error("Error in LordJob.Notify_PawnJobDone(): " + ex);
		}
		try
		{
			curLordToil.Notify_PawnJobDone(pawn, condition);
		}
		catch (Exception ex2)
		{
			Log.Error("Error in LordToil.Notify_PawnJobDone(): " + ex2);
		}
	}

	public void Notify_PawnLost(Pawn pawn, PawnLostCondition cond, DamageInfo? dinfo = null)
	{
		if (!curJob.ShouldRemovePawn(pawn, cond))
		{
			return;
		}
		if (ownedPawns.Contains(pawn))
		{
			RemovePawn(pawn);
			if (cond == PawnLostCondition.Incapped || cond == PawnLostCondition.Killed || cond == PawnLostCondition.MadePrisoner)
			{
				numPawnsLostViolently++;
			}
			try
			{
				curJob.Notify_PawnLost(pawn, cond);
			}
			catch (Exception ex)
			{
				Log.Error("Error in LordJob.Notify_PawnLost(): " + ex);
			}
			if (!lordManager.lords.Contains(this))
			{
				return;
			}
			if (!ShouldExist)
			{
				Destroy();
				return;
			}
			try
			{
				curLordToil.Notify_PawnLost(pawn, cond);
			}
			catch (Exception ex2)
			{
				Log.Error("Error in LordToil.Notify_PawnLost(): " + ex2);
			}
			TriggerSignal signal = new TriggerSignal
			{
				type = TriggerSignalType.PawnLost,
				thing = pawn,
				condition = cond
			};
			if (dinfo.HasValue)
			{
				signal.dinfo = dinfo.Value;
			}
			CheckTransitionOnSignal(signal);
		}
		else
		{
			Log.Error("Lord lost pawn " + pawn?.ToString() + " it didn't have. Condition=" + cond);
		}
	}

	public void Notify_InMentalState(Pawn pawn, MentalStateDef def)
	{
		curJob.Notify_InMentalState(pawn, def);
	}

	public void Notify_BuildingLost(Building building, DamageInfo? dinfo = null)
	{
		if (ownedBuildings.Contains(building))
		{
			ownedBuildings.Remove(building);
			curJob.Notify_BuildingLost(building);
			if (!lordManager.lords.Contains(this))
			{
				return;
			}
			if (!ShouldExist)
			{
				Destroy();
				return;
			}
			curLordToil.Notify_BuildingLost(building);
			TriggerSignal signal = new TriggerSignal
			{
				type = TriggerSignalType.BuildingLost,
				thing = building
			};
			if (dinfo.HasValue)
			{
				signal.dinfo = dinfo.Value;
			}
			CheckTransitionOnSignal(signal);
		}
		else
		{
			Log.Error("Lord lost building " + building?.ToString() + " it didn't have.");
		}
	}

	public void Notify_CorpseLost(Corpse corpse, DamageInfo? dinfo = null)
	{
		if (ownedCorpses.Contains(corpse))
		{
			ownedCorpses.Remove(corpse);
			curJob.Notify_CorpseLost(corpse);
			if (!lordManager.lords.Contains(this))
			{
				return;
			}
			if (!ShouldExist)
			{
				Destroy();
				return;
			}
			curLordToil.Notify_CorpseLost(corpse);
			TriggerSignal signal = new TriggerSignal
			{
				type = TriggerSignalType.CorpseLost,
				thing = corpse
			};
			if (dinfo.HasValue)
			{
				signal.dinfo = dinfo.Value;
			}
			CheckTransitionOnSignal(signal);
		}
		else
		{
			Log.Error("Lord lost corpse " + corpse?.ToString() + " it didn't have.");
		}
	}

	public void Notify_BuildingDamaged(Building building, DamageInfo dinfo)
	{
		CheckTransitionOnSignal(new TriggerSignal
		{
			type = TriggerSignalType.BuildingDamaged,
			thing = building,
			dinfo = dinfo
		});
	}

	public void Notify_PawnDamaged(Pawn victim, DamageInfo dinfo)
	{
		curLordToil?.Notify_PawnDamaged(victim, dinfo);
		CheckTransitionOnSignal(new TriggerSignal
		{
			type = TriggerSignalType.PawnDamaged,
			thing = victim,
			dinfo = dinfo
		});
	}

	public void Notify_PawnAttemptArrested(Pawn victim)
	{
		CheckTransitionOnSignal(new TriggerSignal
		{
			type = TriggerSignalType.PawnArrestAttempted,
			thing = victim
		});
	}

	public void Notify_Clamor(Thing source, ClamorDef clamorType)
	{
		CheckTransitionOnSignal(new TriggerSignal
		{
			type = TriggerSignalType.Clamor,
			thing = source,
			clamorType = clamorType
		});
	}

	public void Notify_PawnAcquiredTarget(Pawn detector, Thing newTarg)
	{
		curLordToil?.Notify_PawnAcquiredTarget(detector, newTarg);
		CheckTransitionOnSignal(new TriggerSignal
		{
			type = TriggerSignalType.AcquiredTarget,
			thing = detector,
			otherThing = newTarg
		});
	}

	public void Notify_BuildingSpawnedOnMap(Building b)
	{
		curLordToil?.Notify_BuildingSpawnedOnMap(b);
	}

	public void Notify_BuildingDespawnedOnMap(Building b)
	{
		curLordToil?.Notify_BuildingDespawnedOnMap(b);
	}

	public void Notify_ReachedDutyLocation(Pawn pawn)
	{
		curLordToil.Notify_ReachedDutyLocation(pawn);
	}

	public void Notify_MapRemoved()
	{
		curJob?.Notify_MapRemoved();
	}

	public void Notify_ConstructionFailed(Pawn pawn, Frame frame, Blueprint_Build newBlueprint)
	{
		curLordToil.Notify_ConstructionFailed(pawn, frame, newBlueprint);
	}

	public void Notify_ConstructionCompleted(Pawn pawn, Building building)
	{
		curLordToil.Notify_ConstructionCompleted(pawn, building);
	}

	public void Notify_SignalReceived(Signal signal)
	{
		if (signal.tag == inSignalLeave)
		{
			if (ownedPawns.Any() && faction != null)
			{
				Messages.Message("MessagePawnsLeaving".Translate(faction.def.pawnsPlural), ownedPawns, MessageTypeDefOf.NeutralEvent);
			}
			LordToil lordToil = Graph.lordToils.Find((LordToil st) => st is LordToil_PanicFlee);
			if (lordToil != null)
			{
				GotoToil(lordToil);
			}
			else
			{
				lordManager.RemoveLord(this);
			}
		}
		CheckTransitionOnSignal(TriggerSignal.ForSignal(signal));
	}

	public void Notify_DormancyWakeup()
	{
		CheckTransitionOnSignal(new TriggerSignal
		{
			type = TriggerSignalType.DormancyWakeup
		});
	}

	public void Notify_MechClusterDefeated()
	{
		CheckTransitionOnSignal(new TriggerSignal
		{
			type = TriggerSignalType.MechClusterDefeated
		});
	}

	public void Notify_PawnUndowned(Pawn pawn)
	{
		curJob.Notify_PawnUndowned(pawn);
	}

	public void Notify_PawnDowned(Pawn pawn)
	{
		curJob.Notify_PawnDowned(pawn);
	}

	private bool CheckTransitionOnSignal(TriggerSignal signal)
	{
		if (Trigger_PawnHarmed.SignalIsHarm(signal))
		{
			lastPawnHarmTick = Find.TickManager.TicksGame;
		}
		for (int i = 0; i < graph.transitions.Count; i++)
		{
			if (graph.transitions[i].sources.Contains(curLordToil) && graph.transitions[i].CheckSignal(this, signal))
			{
				return true;
			}
		}
		return false;
	}

	private Vector3 DebugCenter()
	{
		Vector3 result = Map.Center.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
		if (ownedPawns.Any((Pawn p) => p.Spawned))
		{
			result.x = ownedPawns.Where((Pawn p) => p.Spawned).Average((Pawn p) => p.DrawPos.x);
			result.z = ownedPawns.Where((Pawn p) => p.Spawned).Average((Pawn p) => p.DrawPos.z);
		}
		return result;
	}

	public void DebugDraw()
	{
		Vector3 a = DebugCenter();
		IntVec3 flagLoc = curLordToil.FlagLoc;
		if (flagLoc.IsValid)
		{
			Graphics.DrawMesh(MeshPool.plane14, flagLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Building), Quaternion.identity, FlagTex, 0);
		}
		GenDraw.DrawLineBetween(a, flagLoc.ToVector3Shifted(), SimpleColor.Red);
		foreach (Pawn ownedPawn in ownedPawns)
		{
			SimpleColor color = (ownedPawn.InMentalState ? SimpleColor.Yellow : SimpleColor.White);
			GenDraw.DrawLineBetween(a, ownedPawn.DrawPos, color);
		}
	}

	public void DebugOnGUI()
	{
		Text.Anchor = TextAnchor.MiddleCenter;
		Text.Font = GameFont.Tiny;
		string label = ((CurLordToil == null) ? "toil=NULL" : ("toil " + graph.lordToils.IndexOf(CurLordToil) + "\n" + CurLordToil));
		Vector2 vector = DebugCenter().MapToUIPosition();
		Widgets.Label(new Rect(vector.x - 100f, vector.y - 100f, 200f, 200f), label);
		Text.Anchor = TextAnchor.UpperLeft;
	}

	public string DebugString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Start steal threshold: " + StealAIUtility.StartStealingMarketValueThreshold(this).ToString("F0"));
		stringBuilder.AppendLine("Duties:");
		foreach (Pawn ownedPawn in ownedPawns)
		{
			stringBuilder.AppendLine("   " + ownedPawn.LabelCap + " - " + ownedPawn.mindState.duty);
		}
		stringBuilder.AppendLine("Raw save data:");
		stringBuilder.AppendLine(Scribe.saver.DebugOutputFor(this));
		return stringBuilder.ToString();
	}

	private bool ShouldDoDebugOutput()
	{
		IntVec3 intVec = UI.MouseCell();
		IntVec3 flagLoc = curLordToil.FlagLoc;
		if (flagLoc.IsValid && intVec == flagLoc)
		{
			return true;
		}
		for (int i = 0; i < ownedPawns.Count; i++)
		{
			if (intVec == ownedPawns[i].Position)
			{
				return true;
			}
		}
		return false;
	}

	public void Dispose()
	{
		graph?.Dispose();
		curJob?.Dispose();
	}
}
