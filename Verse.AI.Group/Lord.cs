using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Verse.AI.Group
{
	[StaticConstructorOnStartup]
	public class Lord : IExposable, ILoadReferenceable, ISignalReceiver
	{
		public LordManager lordManager;

		private LordToil curLordToil;

		private StateGraph graph;

		public int loadID = -1;

		private LordJob curJob;

		public Faction faction;

		public List<Pawn> ownedPawns = new List<Pawn>();

		public List<Building> ownedBuildings = new List<Building>();

		public List<Thing> extraForbiddenThings = new List<Thing>();

		public List<string> questTags;

		public string inSignalLeave;

		private bool initialized;

		public int ticksInToil;

		public int numPawnsLostViolently;

		public int numPawnsEverGained;

		public int initialColonyHealthTotal;

		public int lastPawnHarmTick = -99999;

		private const int AttackTargetCacheInterval = 60;

		private static readonly Material FlagTex = MaterialPool.MatFrom("UI/Overlays/SquadFlag");

		private int tmpCurLordToilIdx = -1;

		private Dictionary<int, LordToilData> tmpLordToilData = new Dictionary<int, LordToilData>();

		private Dictionary<int, TriggerData> tmpTriggerData = new Dictionary<int, TriggerData>();

		public Map Map => lordManager.map;

		public StateGraph Graph => graph;

		public LordToil CurLordToil => curLordToil;

		public LordJob LordJob => curJob;

		private bool CanExistWithoutPawns => curJob is LordJob_VoluntarilyJoinable;

		private bool ShouldExist
		{
			get
			{
				if (ownedPawns.Count <= 0 && !CanExistWithoutPawns)
				{
					return ownedBuildings.Count > 0;
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
			Scribe_Values.Look(ref loadID, "loadID", 0);
			Scribe_References.Look(ref faction, "faction");
			Scribe_Collections.Look(ref extraForbiddenThings, "extraForbiddenThings", LookMode.Reference);
			Scribe_Collections.Look(ref ownedPawns, "ownedPawns", LookMode.Reference);
			Scribe_Collections.Look(ref ownedBuildings, "ownedBuildings", LookMode.Reference);
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
			}
			ExposeData_StateGraph();
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
			LordJob job = curJob;
			curJob = null;
			SetJob(job);
			foreach (KeyValuePair<int, LordToilData> tmpLordToilDatum in tmpLordToilData)
			{
				if (tmpLordToilDatum.Key < 0 || tmpLordToilDatum.Key >= graph.lordToils.Count)
				{
					Log.Error(string.Concat("Could not find lord toil for lord toil data of type \"", tmpLordToilDatum.Value.GetType(), "\" (lord job: \"", curJob.GetType(), "\"), because lord toil index is out of bounds: ", tmpLordToilDatum.Key));
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
					Log.Error(string.Concat("Could not find trigger for trigger data of type \"", tmpTriggerDatum.Value.GetType(), "\" (lord job: \"", curJob.GetType(), "\"), because trigger index is out of bounds: ", tmpTriggerDatum.Key));
				}
				else
				{
					triggerByIndex.data = tmpTriggerDatum.Value;
				}
			}
			tmpTriggerData.Clear();
			if (tmpCurLordToilIdx < 0 || tmpCurLordToilIdx >= graph.lordToils.Count)
			{
				Log.Error(string.Concat("Current lord toil index out of bounds (lord job: \"", curJob.GetType(), "\"): ", tmpCurLordToilIdx));
			}
			else
			{
				curLordToil = graph.lordToils[tmpCurLordToilIdx];
			}
		}

		public void SetJob(LordJob lordJob)
		{
			if (curJob != null)
			{
				curJob.Cleanup();
			}
			curJob = lordJob;
			curLordToil = null;
			lordJob.lord = this;
			Rand.PushState();
			Rand.Seed = loadID * 193;
			graph = lordJob.CreateGraph();
			Rand.PopState();
			graph.ErrorCheck();
			if (faction != null && !faction.IsPlayer && faction.def.autoFlee && lordJob.AddFleeToil)
			{
				LordToil_PanicFlee lordToil_PanicFlee = new LordToil_PanicFlee();
				lordToil_PanicFlee.useAvoidGrid = true;
				for (int i = 0; i < graph.lordToils.Count; i++)
				{
					Transition transition = new Transition(graph.lordToils[i], lordToil_PanicFlee);
					transition.AddPreAction(new TransitionAction_Message("MessageFightersFleeing".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
					transition.AddTrigger(new Trigger_FractionPawnsLost(faction.def.attackersDownPercentageRangeForAutoFlee.RandomInRangeSeeded(loadID)));
					graph.AddTransition(transition, highPriority: true);
				}
				graph.AddToil(lordToil_PanicFlee);
			}
			for (int j = 0; j < graph.lordToils.Count; j++)
			{
				graph.lordToils[j].lord = this;
			}
			for (int k = 0; k < ownedPawns.Count; k++)
			{
				Map.attackTargetsCache.UpdateTarget(ownedPawns[k]);
			}
		}

		public void Cleanup()
		{
			curJob.Cleanup();
			if (curLordToil != null)
			{
				curLordToil.Cleanup();
			}
			for (int i = 0; i < ownedPawns.Count; i++)
			{
				if (ownedPawns[i].mindState != null)
				{
					ownedPawns[i].mindState.duty = null;
				}
				Map.attackTargetsCache.UpdateTarget(ownedPawns[i]);
				if (ownedPawns[i].Spawned && ownedPawns[i].CurJob != null)
				{
					ownedPawns[i].jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
			}
		}

		public void AddPawn(Pawn p)
		{
			if (ownedPawns.Contains(p))
			{
				Log.Error(string.Concat("Lord for ", faction.ToStringSafe(), " tried to add ", p, " whom it already controls."));
			}
			else if (p.GetLord() != null)
			{
				Log.Error(string.Concat("Tried to add pawn ", p, " to lord ", this, " but this pawn is already a member of lord ", p.GetLord(), ". Pawns can't be members of more than one lord at the same time."));
			}
			else
			{
				ownedPawns.Add(p);
				numPawnsEverGained++;
				Map.attackTargetsCache.UpdateTarget(p);
				curLordToil.UpdateAllDuties();
				curJob.Notify_PawnAdded(p);
			}
		}

		public void AddBuilding(Building b)
		{
			if (ownedBuildings.Contains(b))
			{
				Log.Error(string.Concat("Lord for ", faction.ToStringSafe(), " tried to add ", b, " which it already controls."));
			}
			else
			{
				ownedBuildings.Add(b);
				curLordToil.UpdateAllDuties();
				curJob.Notify_BuildingAdded(b);
			}
		}

		private void RemovePawn(Pawn p)
		{
			ownedPawns.Remove(p);
			if (p.mindState != null)
			{
				p.mindState.duty = null;
			}
			Map.attackTargetsCache.UpdateTarget(p);
		}

		public void GotoToil(LordToil newLordToil)
		{
			LordToil previousToil = curLordToil;
			if (curLordToil != null)
			{
				curLordToil.Cleanup();
			}
			curLordToil = newLordToil;
			ticksInToil = 0;
			if (curLordToil.lord != this)
			{
				Log.Error("curLordToil lord is " + ((curLordToil.lord == null) ? "null (forgot to add toil to graph?)" : curLordToil.lord.ToString()));
				curLordToil.lord = this;
			}
			curLordToil.Init();
			for (int i = 0; i < graph.transitions.Count; i++)
			{
				if (graph.transitions[i].sources.Contains(curLordToil))
				{
					graph.transitions[i].SourceToilBecameActive(graph.transitions[i], previousToil);
				}
			}
			curLordToil.UpdateAllDuties();
		}

		public void LordTick()
		{
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
			TriggerSignal signal = default(TriggerSignal);
			signal.type = TriggerSignalType.FactionRelationsChanged;
			signal.faction = otherFaction;
			signal.previousRelationKind = previousRelationKind;
			CheckTransitionOnSignal(signal);
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

		public void Notify_PawnLost(Pawn pawn, PawnLostCondition cond, DamageInfo? dinfo = null)
		{
			if (ownedPawns.Contains(pawn))
			{
				RemovePawn(pawn);
				if (cond == PawnLostCondition.IncappedOrKilled || cond == PawnLostCondition.MadePrisoner)
				{
					numPawnsLostViolently++;
				}
				curJob.Notify_PawnLost(pawn, cond);
				if (!lordManager.lords.Contains(this))
				{
					return;
				}
				if (!ShouldExist)
				{
					Destroy();
					return;
				}
				curLordToil.Notify_PawnLost(pawn, cond);
				TriggerSignal signal = default(TriggerSignal);
				signal.type = TriggerSignalType.PawnLost;
				signal.thing = pawn;
				signal.condition = cond;
				if (dinfo.HasValue)
				{
					signal.dinfo = dinfo.Value;
				}
				CheckTransitionOnSignal(signal);
			}
			else
			{
				Log.Error(string.Concat("Lord lost pawn ", pawn, " it didn't have. Condition=", cond));
			}
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
				TriggerSignal signal = default(TriggerSignal);
				signal.type = TriggerSignalType.BuildingLost;
				signal.thing = building;
				if (dinfo.HasValue)
				{
					signal.dinfo = dinfo.Value;
				}
				CheckTransitionOnSignal(signal);
			}
			else
			{
				Log.Error(string.Concat("Lord lost building ", building, " it didn't have."));
			}
		}

		public void Notify_BuildingDamaged(Building building, DamageInfo dinfo)
		{
			TriggerSignal signal = default(TriggerSignal);
			signal.type = TriggerSignalType.BuildingDamaged;
			signal.thing = building;
			signal.dinfo = dinfo;
			CheckTransitionOnSignal(signal);
		}

		public void Notify_PawnDamaged(Pawn victim, DamageInfo dinfo)
		{
			TriggerSignal signal = default(TriggerSignal);
			signal.type = TriggerSignalType.PawnDamaged;
			signal.thing = victim;
			signal.dinfo = dinfo;
			CheckTransitionOnSignal(signal);
		}

		public void Notify_PawnAttemptArrested(Pawn victim)
		{
			TriggerSignal signal = default(TriggerSignal);
			signal.type = TriggerSignalType.PawnArrestAttempted;
			signal.thing = victim;
			CheckTransitionOnSignal(signal);
		}

		public void Notify_Clamor(Thing source, ClamorDef clamorType)
		{
			TriggerSignal signal = default(TriggerSignal);
			signal.type = TriggerSignalType.Clamor;
			signal.thing = source;
			signal.clamorType = clamorType;
			CheckTransitionOnSignal(signal);
		}

		public void Notify_PawnAcquiredTarget(Pawn detector, Thing newTarg)
		{
		}

		public void Notify_ReachedDutyLocation(Pawn pawn)
		{
			curLordToil.Notify_ReachedDutyLocation(pawn);
		}

		public void Notify_ConstructionFailed(Pawn pawn, Frame frame, Blueprint_Build newBlueprint)
		{
			curLordToil.Notify_ConstructionFailed(pawn, frame, newBlueprint);
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
		}

		public void Notify_DormancyWakeup()
		{
			TriggerSignal signal = default(TriggerSignal);
			signal.type = TriggerSignalType.DormancyWakeup;
			CheckTransitionOnSignal(signal);
		}

		public void Notify_MechClusterDefeated()
		{
			TriggerSignal signal = default(TriggerSignal);
			signal.type = TriggerSignalType.MechClusterDefeated;
			CheckTransitionOnSignal(signal);
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
			if (ownedPawns.Where((Pawn p) => p.Spawned).Any())
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
				SimpleColor color = ownedPawn.InMentalState ? SimpleColor.Yellow : SimpleColor.White;
				GenDraw.DrawLineBetween(a, ownedPawn.DrawPos, color);
			}
		}

		public void DebugOnGUI()
		{
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Tiny;
			string label = (CurLordToil == null) ? "toil=NULL" : ("toil " + graph.lordToils.IndexOf(CurLordToil) + "\n" + CurLordToil.ToString());
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
			IntVec3 a = UI.MouseCell();
			IntVec3 flagLoc = curLordToil.FlagLoc;
			if (flagLoc.IsValid && a == flagLoc)
			{
				return true;
			}
			for (int i = 0; i < ownedPawns.Count; i++)
			{
				if (a == ownedPawns[i].Position)
				{
					return true;
				}
			}
			return false;
		}
	}
}
