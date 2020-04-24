using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld.Planet
{
	public class WorldPawns : IExposable
	{
		private HashSet<Pawn> pawnsAlive = new HashSet<Pawn>();

		private HashSet<Pawn> pawnsMothballed = new HashSet<Pawn>();

		private HashSet<Pawn> pawnsDead = new HashSet<Pawn>();

		private HashSet<Pawn> pawnsForcefullyKeptAsWorldPawns = new HashSet<Pawn>();

		public WorldPawnGC gc = new WorldPawnGC();

		private Stack<Pawn> pawnsBeingDiscarded = new Stack<Pawn>();

		private const int TendIntervalTicks = 7500;

		private const int MothballUpdateInterval = 15000;

		private List<Pawn> allPawnsAliveOrDeadResult = new List<Pawn>();

		private List<Pawn> allPawnsAliveResult = new List<Pawn>();

		private static List<Pawn> tmpPawnsToTick = new List<Pawn>();

		private static List<Pawn> tmpPawnsToRemove = new List<Pawn>();

		public List<Pawn> AllPawnsAliveOrDead
		{
			get
			{
				allPawnsAliveOrDeadResult.Clear();
				allPawnsAliveOrDeadResult.AddRange(AllPawnsAlive);
				allPawnsAliveOrDeadResult.AddRange(AllPawnsDead);
				return allPawnsAliveOrDeadResult;
			}
		}

		public List<Pawn> AllPawnsAlive
		{
			get
			{
				allPawnsAliveResult.Clear();
				allPawnsAliveResult.AddRange(pawnsAlive);
				allPawnsAliveResult.AddRange(pawnsMothballed);
				return allPawnsAliveResult;
			}
		}

		public HashSet<Pawn> AllPawnsDead => pawnsDead;

		public HashSet<Pawn> ForcefullyKeptPawns => pawnsForcefullyKeptAsWorldPawns;

		public void WorldPawnsTick()
		{
			tmpPawnsToTick.Clear();
			tmpPawnsToTick.AddRange(pawnsAlive);
			for (int i = 0; i < tmpPawnsToTick.Count; i++)
			{
				try
				{
					tmpPawnsToTick[i].Tick();
				}
				catch (Exception ex)
				{
					Log.ErrorOnce("Exception ticking world pawn " + tmpPawnsToTick[i].ToStringSafe() + ". Suppressing further errors. " + ex, tmpPawnsToTick[i].thingIDNumber ^ 0x4475CF1F);
				}
				try
				{
					if (ShouldAutoTendTo(tmpPawnsToTick[i]))
					{
						TendUtility.DoTend(null, tmpPawnsToTick[i], null);
					}
				}
				catch (Exception ex2)
				{
					Log.ErrorOnce("Exception tending to a world pawn " + tmpPawnsToTick[i].ToStringSafe() + ". Suppressing further errors. " + ex2, tmpPawnsToTick[i].thingIDNumber ^ 0x85C154);
				}
			}
			tmpPawnsToTick.Clear();
			if (Find.TickManager.TicksGame % 15000 == 0)
			{
				DoMothballProcessing();
			}
			tmpPawnsToRemove.Clear();
			foreach (Pawn item in pawnsDead)
			{
				if (item == null)
				{
					Log.ErrorOnce("Dead null world pawn detected, discarding.", 94424128);
					tmpPawnsToRemove.Add(item);
				}
				else if (item.Discarded)
				{
					Log.Error("World pawn " + item + " has been discarded while still being a world pawn. This should never happen, because discard destroy mode means that the pawn is no longer managed by anything. Pawn should have been removed from the world first.");
					tmpPawnsToRemove.Add(item);
				}
			}
			for (int j = 0; j < tmpPawnsToRemove.Count; j++)
			{
				pawnsDead.Remove(tmpPawnsToRemove[j]);
			}
			tmpPawnsToRemove.Clear();
			try
			{
				gc.WorldPawnGCTick();
			}
			catch (Exception arg)
			{
				Log.Error("Error in WorldPawnGCTick(): " + arg);
			}
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref pawnsForcefullyKeptAsWorldPawns, saveDestroyedThings: true, "pawnsForcefullyKeptAsWorldPawns", LookMode.Reference);
			Scribe_Collections.Look(ref pawnsAlive, "pawnsAlive", LookMode.Deep);
			Scribe_Collections.Look(ref pawnsMothballed, "pawnsMothballed", LookMode.Deep);
			Scribe_Collections.Look(ref pawnsDead, saveDestroyedThings: true, "pawnsDead", LookMode.Deep);
			Scribe_Deep.Look(ref gc, "gc");
			BackCompatibility.PostExposeData(this);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (pawnsMothballed == null)
				{
					pawnsMothballed = new HashSet<Pawn>();
				}
				if (pawnsForcefullyKeptAsWorldPawns.RemoveWhere((Pawn x) => x == null) != 0)
				{
					Log.Error("Some pawnsForcefullyKeptAsWorldPawns were null after loading.");
				}
				if (pawnsAlive.RemoveWhere((Pawn x) => x == null) != 0)
				{
					Log.Error("Some pawnsAlive were null after loading.");
				}
				if (pawnsMothballed.RemoveWhere((Pawn x) => x == null) != 0)
				{
					Log.Error("Some pawnsMothballed were null after loading.");
				}
				if (pawnsDead.RemoveWhere((Pawn x) => x == null) != 0)
				{
					Log.Error("Some pawnsDead were null after loading.");
				}
				if (pawnsAlive.RemoveWhere((Pawn x) => x.def == null || x.kindDef == null) != 0)
				{
					Log.Error("Some pawnsAlive had null def after loading.");
				}
				if (pawnsMothballed.RemoveWhere((Pawn x) => x.def == null || x.kindDef == null) != 0)
				{
					Log.Error("Some pawnsMothballed had null def after loading.");
				}
				if (pawnsDead.RemoveWhere((Pawn x) => x.def == null || x.kindDef == null) != 0)
				{
					Log.Error("Some pawnsDead had null def after loading.");
				}
			}
		}

		public bool Contains(Pawn p)
		{
			if (!pawnsAlive.Contains(p) && !pawnsMothballed.Contains(p))
			{
				return pawnsDead.Contains(p);
			}
			return true;
		}

		public void PassToWorld(Pawn pawn, PawnDiscardDecideMode discardMode = PawnDiscardDecideMode.Decide)
		{
			if (pawn.Spawned)
			{
				Log.Error("Tried to call PassToWorld with spawned pawn: " + pawn + ". Despawn him first.");
				return;
			}
			if (Contains(pawn))
			{
				Log.Error("Tried to pass pawn " + pawn + " to world, but it's already here.");
				return;
			}
			if (discardMode == PawnDiscardDecideMode.KeepForever && pawn.Discarded)
			{
				Log.Error("Tried to pass a discarded pawn " + pawn + " to world with discardMode=Keep. Discarded pawns should never be stored in WorldPawns.");
				discardMode = PawnDiscardDecideMode.Decide;
			}
			if (PawnComponentsUtility.HasSpawnedComponents(pawn))
			{
				PawnComponentsUtility.RemoveComponentsOnDespawned(pawn);
			}
			switch (discardMode)
			{
			case PawnDiscardDecideMode.Decide:
				AddPawn(pawn);
				break;
			case PawnDiscardDecideMode.KeepForever:
				pawnsForcefullyKeptAsWorldPawns.Add(pawn);
				AddPawn(pawn);
				break;
			case PawnDiscardDecideMode.Discard:
				DiscardPawn(pawn);
				break;
			}
		}

		public void RemovePawn(Pawn p)
		{
			if (!Contains(p))
			{
				Log.Error("Tried to remove pawn " + p + " from " + GetType() + ", but it's not here.");
			}
			gc.CancelGCPass();
			if (pawnsMothballed.Contains(p) && Find.TickManager.TicksGame % 15000 != 0)
			{
				try
				{
					p.TickMothballed(Find.TickManager.TicksGame % 15000);
				}
				catch (Exception arg)
				{
					Log.Error("Exception ticking mothballed world pawn (just before removing): " + arg);
				}
			}
			pawnsAlive.Remove(p);
			pawnsMothballed.Remove(p);
			pawnsDead.Remove(p);
			pawnsForcefullyKeptAsWorldPawns.Remove(p);
			p.becameWorldPawnTickAbs = -1;
		}

		public void RemoveAndDiscardPawnViaGC(Pawn p)
		{
			RemovePawn(p);
			DiscardPawn(p, silentlyRemoveReferences: true);
		}

		public WorldPawnSituation GetSituation(Pawn p)
		{
			if (!Contains(p))
			{
				return WorldPawnSituation.None;
			}
			if (p.Dead || p.Destroyed)
			{
				return WorldPawnSituation.Dead;
			}
			if (PawnUtility.IsFactionLeader(p))
			{
				return WorldPawnSituation.FactionLeader;
			}
			if (PawnUtility.IsKidnappedPawn(p))
			{
				return WorldPawnSituation.Kidnapped;
			}
			if (p.IsBorrowedByAnyFaction())
			{
				return WorldPawnSituation.Borrowed;
			}
			if (p.IsCaravanMember())
			{
				return WorldPawnSituation.CaravanMember;
			}
			if (PawnUtility.IsTravelingInTransportPodWorldObject(p))
			{
				return WorldPawnSituation.InTravelingTransportPod;
			}
			if (PawnUtility.ForSaleBySettlement(p))
			{
				return WorldPawnSituation.ForSaleBySettlement;
			}
			if (QuestUtility.IsReservedByQuestOrQuestBeingGenerated(p))
			{
				return WorldPawnSituation.ReservedByQuest;
			}
			return WorldPawnSituation.Free;
		}

		public IEnumerable<Pawn> GetPawnsBySituation(WorldPawnSituation situation)
		{
			return AllPawnsAliveOrDead.Where((Pawn x) => GetSituation(x) == situation);
		}

		public int GetPawnsBySituationCount(WorldPawnSituation situation)
		{
			int num = 0;
			foreach (Pawn item in pawnsAlive)
			{
				if (GetSituation(item) == situation)
				{
					num++;
				}
			}
			foreach (Pawn item2 in pawnsDead)
			{
				if (GetSituation(item2) == situation)
				{
					num++;
				}
			}
			return num;
		}

		private bool ShouldAutoTendTo(Pawn pawn)
		{
			if (!pawn.Dead && !pawn.Destroyed && pawn.IsHashIntervalTick(7500) && !pawn.IsCaravanMember())
			{
				return !PawnUtility.IsTravelingInTransportPodWorldObject(pawn);
			}
			return false;
		}

		public bool IsBeingDiscarded(Pawn p)
		{
			return pawnsBeingDiscarded.Contains(p);
		}

		public void Notify_PawnDestroyed(Pawn p)
		{
			if (pawnsAlive.Contains(p) || pawnsMothballed.Contains(p))
			{
				pawnsAlive.Remove(p);
				pawnsMothballed.Remove(p);
				pawnsDead.Add(p);
			}
		}

		private bool ShouldMothball(Pawn p)
		{
			if (DefPreventingMothball(p) == null && !p.IsCaravanMember())
			{
				return !PawnUtility.IsTravelingInTransportPodWorldObject(p);
			}
			return false;
		}

		private HediffDef DefPreventingMothball(Pawn p)
		{
			List<Hediff> hediffs = p.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (!hediffs[i].def.AlwaysAllowMothball && !hediffs[i].IsPermanent())
				{
					return hediffs[i].def;
				}
			}
			return null;
		}

		private void AddPawn(Pawn p)
		{
			gc.CancelGCPass();
			if (p.Dead || p.Destroyed)
			{
				pawnsDead.Add(p);
			}
			else
			{
				pawnsAlive.Add(p);
			}
			p.Notify_PassedToWorld();
		}

		private void DiscardPawn(Pawn p, bool silentlyRemoveReferences = false)
		{
			pawnsBeingDiscarded.Push(p);
			try
			{
				if (!p.Destroyed)
				{
					p.Destroy();
				}
				if (!p.Discarded)
				{
					p.Discard(silentlyRemoveReferences);
				}
			}
			finally
			{
				pawnsBeingDiscarded.Pop();
			}
		}

		private void DoMothballProcessing()
		{
			tmpPawnsToTick.AddRange(pawnsMothballed);
			for (int i = 0; i < tmpPawnsToTick.Count; i++)
			{
				try
				{
					tmpPawnsToTick[i].TickMothballed(15000);
				}
				catch (Exception arg)
				{
					Log.ErrorOnce("Exception ticking mothballed world pawn. Suppressing further errors. " + arg, tmpPawnsToTick[i].thingIDNumber ^ 0x5B84EC45);
				}
			}
			tmpPawnsToTick.Clear();
			tmpPawnsToTick.AddRange(pawnsAlive);
			for (int j = 0; j < tmpPawnsToTick.Count; j++)
			{
				Pawn pawn = tmpPawnsToTick[j];
				if (ShouldMothball(pawn))
				{
					pawnsAlive.Remove(pawn);
					pawnsMothballed.Add(pawn);
				}
			}
			tmpPawnsToTick.Clear();
		}

		public void DebugRunMothballProcessing()
		{
			DoMothballProcessing();
			Log.Message($"World pawn mothball run complete");
		}

		public void UnpinAllForcefullyKeptPawns()
		{
			pawnsForcefullyKeptAsWorldPawns.Clear();
		}

		public void LogWorldPawns()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("======= World Pawns =======");
			stringBuilder.AppendLine("Count: " + AllPawnsAliveOrDead.Count());
			stringBuilder.AppendLine($"(Live: {pawnsAlive.Count} - Mothballed: {pawnsMothballed.Count} - Dead: {pawnsDead.Count}; {pawnsForcefullyKeptAsWorldPawns.Count} forcefully kept)");
			WorldPawnSituation[] array = (WorldPawnSituation[])Enum.GetValues(typeof(WorldPawnSituation));
			foreach (WorldPawnSituation worldPawnSituation in array)
			{
				if (worldPawnSituation != 0)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("== " + worldPawnSituation + " ==");
					foreach (Pawn item in from x in GetPawnsBySituation(worldPawnSituation)
						orderby (x.Faction != null) ? x.Faction.loadID : (-1)
						select x)
					{
						string str = (item.Name != null) ? item.Name.ToStringFull : item.LabelCap;
						str = str + ", " + item.KindLabel;
						if (item.royalty != null && item.royalty.AllTitlesForReading.Count > 0)
						{
							foreach (RoyalTitle item2 in item.royalty.AllTitlesForReading)
							{
								str = str + ", " + item2.def.GetLabelFor(item);
							}
						}
						str = str + ", " + item.Faction;
						stringBuilder.AppendLine(str);
					}
				}
			}
			stringBuilder.AppendLine("===========================");
			Log.Message(stringBuilder.ToString());
		}

		public void LogWorldPawnMothballPrevention()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("======= World Pawns Mothball Prevention =======");
			stringBuilder.AppendLine($"Count: {pawnsAlive.Count()}");
			int num = 0;
			Dictionary<HediffDef, int> dictionary = new Dictionary<HediffDef, int>();
			foreach (Pawn item in pawnsAlive)
			{
				HediffDef hediffDef = DefPreventingMothball(item);
				if (hediffDef == null)
				{
					num++;
				}
				else
				{
					if (!dictionary.ContainsKey(hediffDef))
					{
						dictionary[hediffDef] = 0;
					}
					int num2 = ++dictionary[hediffDef];
				}
			}
			stringBuilder.AppendLine($"Will be mothballed: {num}");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Reasons to avoid mothballing:");
			foreach (KeyValuePair<HediffDef, int> item2 in dictionary.OrderByDescending((KeyValuePair<HediffDef, int> kvp) => kvp.Value))
			{
				stringBuilder.AppendLine($"{item2.Value}: {item2.Key}");
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
