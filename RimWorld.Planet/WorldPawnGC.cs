using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldPawnGC : IExposable
	{
		private int lastSuccessfulGCTick;

		private int currentGCRate = 1;

		private const int AdditionalStoryRelevantPawns = 20;

		private const int GCUpdateInterval = 15000;

		private IEnumerator activeGCProcess;

		private StringBuilder logDotgraph;

		private HashSet<string> logDotgraphUniqueLinks;

		public void WorldPawnGCTick()
		{
			if (lastSuccessfulGCTick >= Find.TickManager.TicksGame / 15000 * 15000)
			{
				return;
			}
			if (activeGCProcess == null)
			{
				activeGCProcess = PawnGCPass().GetEnumerator();
				if (DebugViewSettings.logWorldPawnGC)
				{
					Log.Message($"World pawn GC started at rate {currentGCRate}");
				}
			}
			if (activeGCProcess == null)
			{
				return;
			}
			bool flag = false;
			for (int i = 0; i < currentGCRate; i++)
			{
				if (flag)
				{
					break;
				}
				flag = !activeGCProcess.MoveNext();
			}
			if (flag)
			{
				lastSuccessfulGCTick = Find.TickManager.TicksGame;
				currentGCRate = 1;
				activeGCProcess = null;
				if (DebugViewSettings.logWorldPawnGC)
				{
					Log.Message("World pawn GC complete");
				}
			}
		}

		public void CancelGCPass()
		{
			if (activeGCProcess != null)
			{
				activeGCProcess = null;
				currentGCRate = Mathf.Min(currentGCRate * 2, 16777216);
				if (DebugViewSettings.logWorldPawnGC)
				{
					Log.Message("World pawn GC cancelled");
				}
			}
		}

		private IEnumerable AccumulatePawnGCData(Dictionary<Pawn, string> keptPawns)
		{
			foreach (Pawn item in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
			{
				string criticalPawnReason = GetCriticalPawnReason(item);
				if (!criticalPawnReason.NullOrEmpty())
				{
					keptPawns[item] = criticalPawnReason;
					if (logDotgraph != null)
					{
						logDotgraph.AppendLine(string.Format("{0} [label=<{0}<br/><font point-size=\"10\">{1}</font>> color=\"{2}\" shape=\"{3}\"];", DotgraphIdentifier(item), criticalPawnReason, (item.relations != null && item.relations.everSeenByPlayer) ? "black" : "grey", item.RaceProps.Humanlike ? "oval" : "box"));
					}
				}
				else if (logDotgraph != null)
				{
					logDotgraph.AppendLine(string.Format("{0} [color=\"{1}\" shape=\"{2}\"];", DotgraphIdentifier(item), (item.relations != null && item.relations.everSeenByPlayer) ? "black" : "grey", item.RaceProps.Humanlike ? "oval" : "box"));
				}
			}
			foreach (Pawn item2 in (from pawn in PawnsFinder.AllMapsWorldAndTemporary_Alive
				where AllowedAsStoryPawn(pawn) && !keptPawns.ContainsKey(pawn)
				orderby pawn.records.StoryRelevance descending
				select pawn).Take(20))
			{
				keptPawns[item2] = "StoryRelevant";
			}
			Pawn[] criticalPawns = keptPawns.Keys.ToArray();
			Pawn[] array = criticalPawns;
			foreach (Pawn pawn2 in array)
			{
				AddAllRelationships(pawn2, keptPawns);
				yield return null;
			}
			Pawn[] array2 = criticalPawns;
			foreach (Pawn pawn3 in array2)
			{
				AddAllMemories(pawn3, keptPawns);
			}
		}

		private Dictionary<Pawn, string> AccumulatePawnGCDataImmediate()
		{
			Dictionary<Pawn, string> dictionary = new Dictionary<Pawn, string>();
			AccumulatePawnGCData(dictionary).ExecuteEnumerable();
			return dictionary;
		}

		public string PawnGCDebugResults()
		{
			Dictionary<Pawn, string> dictionary = AccumulatePawnGCDataImmediate();
			Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
			foreach (Pawn item in Find.WorldPawns.AllPawnsAlive)
			{
				string key = "Discarded";
				if (dictionary.ContainsKey(item))
				{
					key = dictionary[item];
				}
				if (!dictionary2.ContainsKey(key))
				{
					dictionary2[key] = 0;
				}
				int num = ++dictionary2[key];
			}
			return (from kvp in dictionary2
				orderby kvp.Value descending
				select $"{kvp.Value}: {kvp.Key}").ToLineList();
		}

		public IEnumerable PawnGCPass()
		{
			Dictionary<Pawn, string> keptPawns = new Dictionary<Pawn, string>();
			Pawn[] worldPawnsSnapshot = Find.WorldPawns.AllPawnsAliveOrDead.ToArray();
			foreach (object item in AccumulatePawnGCData(keptPawns))
			{
				_ = item;
				yield return null;
			}
			foreach (Pawn pawn in worldPawnsSnapshot)
			{
				if (pawn.IsWorldPawn() && !keptPawns.ContainsKey(pawn))
				{
					Find.WorldPawns.RemoveAndDiscardPawnViaGC(pawn);
				}
			}
		}

		private string GetCriticalPawnReason(Pawn pawn)
		{
			if (pawn.Discarded)
			{
				return null;
			}
			if (PawnUtility.EverBeenColonistOrTameAnimal(pawn) && pawn.RaceProps.Humanlike)
			{
				return "Colonist";
			}
			if (PawnGenerator.IsBeingGenerated(pawn))
			{
				return "Generating";
			}
			if (PawnUtility.IsFactionLeader(pawn))
			{
				return "FactionLeader";
			}
			if (PawnUtility.IsKidnappedPawn(pawn))
			{
				return "Kidnapped";
			}
			if (pawn.IsCaravanMember())
			{
				return "CaravanMember";
			}
			if (PawnUtility.IsTravelingInTransportPodWorldObject(pawn))
			{
				return "TransportPod";
			}
			if (PawnUtility.ForSaleBySettlement(pawn))
			{
				return "ForSale";
			}
			if (Find.WorldPawns.ForcefullyKeptPawns.Contains(pawn))
			{
				return "ForceKept";
			}
			if (pawn.SpawnedOrAnyParentSpawned)
			{
				return "Spawned";
			}
			if (!pawn.Corpse.DestroyedOrNull())
			{
				return "CorpseExists";
			}
			if (pawn.RaceProps.Humanlike && Current.ProgramState == ProgramState.Playing)
			{
				if (Find.PlayLog.AnyEntryConcerns(pawn))
				{
					return "InPlayLog";
				}
				if (Find.BattleLog.AnyEntryConcerns(pawn))
				{
					return "InBattleLog";
				}
			}
			if (Current.ProgramState == ProgramState.Playing && Find.TaleManager.AnyActiveTaleConcerns(pawn))
			{
				return "InActiveTale";
			}
			if (QuestUtility.IsReservedByQuestOrQuestBeingGenerated(pawn))
			{
				return "ReservedByQuest";
			}
			return null;
		}

		private bool AllowedAsStoryPawn(Pawn pawn)
		{
			if (!pawn.RaceProps.Humanlike)
			{
				return false;
			}
			return true;
		}

		public void AddAllRelationships(Pawn pawn, Dictionary<Pawn, string> keptPawns)
		{
			if (pawn.relations != null)
			{
				foreach (Pawn relatedPawn in pawn.relations.RelatedPawns)
				{
					if (logDotgraph != null)
					{
						string text = $"{DotgraphIdentifier(pawn)}->{DotgraphIdentifier(relatedPawn)} [label=<{pawn.GetRelations(relatedPawn).FirstOrDefault().ToString()}> color=\"purple\"];";
						if (!logDotgraphUniqueLinks.Contains(text))
						{
							logDotgraphUniqueLinks.Add(text);
							logDotgraph.AppendLine(text);
						}
					}
					if (!keptPawns.ContainsKey(relatedPawn))
					{
						keptPawns[relatedPawn] = "Relationship";
					}
				}
			}
		}

		public void AddAllMemories(Pawn pawn, Dictionary<Pawn, string> keptPawns)
		{
			if (pawn.needs != null && pawn.needs.mood != null && pawn.needs.mood.thoughts != null && pawn.needs.mood.thoughts.memories != null)
			{
				foreach (Thought_Memory memory in pawn.needs.mood.thoughts.memories.Memories)
				{
					if (memory.otherPawn != null)
					{
						if (logDotgraph != null)
						{
							string text = $"{DotgraphIdentifier(pawn)}->{DotgraphIdentifier(memory.otherPawn)} [label=<{memory.def}> color=\"orange\"];";
							if (!logDotgraphUniqueLinks.Contains(text))
							{
								logDotgraphUniqueLinks.Add(text);
								logDotgraph.AppendLine(text);
							}
						}
						if (!keptPawns.ContainsKey(memory.otherPawn))
						{
							keptPawns[memory.otherPawn] = "Memory";
						}
					}
				}
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref lastSuccessfulGCTick, "lastSuccessfulGCTick", 0);
			Scribe_Values.Look(ref currentGCRate, "nextGCRate", 1);
		}

		public void LogGC()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("======= GC =======");
			stringBuilder.AppendLine(PawnGCDebugResults());
			Log.Message(stringBuilder.ToString());
		}

		public void RunGC()
		{
			CancelGCPass();
			PerfLogger.Reset();
			foreach (object item in PawnGCPass())
			{
				_ = item;
			}
			float num = PerfLogger.Duration() * 1000f;
			PerfLogger.Flush();
			Log.Message($"World pawn GC run complete in {num} ms");
		}

		public void LogDotgraph()
		{
			logDotgraph = new StringBuilder();
			logDotgraphUniqueLinks = new HashSet<string>();
			logDotgraph.AppendLine("digraph { rankdir=LR;");
			AccumulatePawnGCDataImmediate();
			logDotgraph.AppendLine("}");
			GUIUtility.systemCopyBuffer = logDotgraph.ToString();
			Log.Message("Dotgraph copied to clipboard");
			logDotgraph = null;
			logDotgraphUniqueLinks = null;
		}

		public static string DotgraphIdentifier(Pawn pawn)
		{
			return new string(pawn.LabelShort.Where((char ch) => char.IsLetter(ch)).ToArray()) + "_" + pawn.thingIDNumber.ToString();
		}
	}
}
