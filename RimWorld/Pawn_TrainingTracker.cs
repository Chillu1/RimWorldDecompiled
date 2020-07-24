using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Pawn_TrainingTracker : IExposable
	{
		private Pawn pawn;

		private DefMap<TrainableDef, bool> wantedTrainables = new DefMap<TrainableDef, bool>();

		private DefMap<TrainableDef, int> steps = new DefMap<TrainableDef, int>();

		private DefMap<TrainableDef, bool> learned = new DefMap<TrainableDef, bool>();

		private int countDecayFrom;

		public Pawn_TrainingTracker(Pawn pawn)
		{
			this.pawn = pawn;
			countDecayFrom = Find.TickManager.TicksGame;
		}

		public void ExposeData()
		{
			Scribe_Deep.Look(ref wantedTrainables, "wantedTrainables");
			Scribe_Deep.Look(ref steps, "steps");
			Scribe_Deep.Look(ref learned, "learned");
			Scribe_Values.Look(ref countDecayFrom, "countDecayFrom", 0);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.PawnTrainingTrackerPostLoadInit(this, ref wantedTrainables, ref steps, ref learned);
			}
		}

		public bool GetWanted(TrainableDef td)
		{
			return wantedTrainables[td];
		}

		private void SetWanted(TrainableDef td, bool wanted)
		{
			wantedTrainables[td] = wanted;
		}

		internal int GetSteps(TrainableDef td)
		{
			return steps[td];
		}

		public bool CanBeTrained(TrainableDef td)
		{
			if (steps[td] >= td.steps)
			{
				return false;
			}
			List<TrainableDef> prerequisites = td.prerequisites;
			if (!prerequisites.NullOrEmpty())
			{
				for (int i = 0; i < prerequisites.Count; i++)
				{
					if (!HasLearned(prerequisites[i]) || CanBeTrained(prerequisites[i]))
					{
						return false;
					}
				}
			}
			return true;
		}

		public bool HasLearned(TrainableDef td)
		{
			return learned[td];
		}

		public AcceptanceReport CanAssignToTrain(TrainableDef td)
		{
			bool visible;
			return CanAssignToTrain(td, out visible);
		}

		public AcceptanceReport CanAssignToTrain(TrainableDef td, out bool visible)
		{
			if (pawn.RaceProps.untrainableTags != null)
			{
				for (int i = 0; i < pawn.RaceProps.untrainableTags.Count; i++)
				{
					if (td.MatchesTag(pawn.RaceProps.untrainableTags[i]))
					{
						visible = false;
						return false;
					}
				}
			}
			if (pawn.RaceProps.trainableTags != null)
			{
				for (int j = 0; j < pawn.RaceProps.trainableTags.Count; j++)
				{
					if (td.MatchesTag(pawn.RaceProps.trainableTags[j]))
					{
						if (pawn.BodySize < td.minBodySize)
						{
							visible = true;
							return new AcceptanceReport("CannotTrainTooSmall".Translate(pawn.LabelCapNoCount, pawn).Resolve());
						}
						visible = true;
						return true;
					}
				}
			}
			if (!td.defaultTrainable)
			{
				visible = false;
				return false;
			}
			if (pawn.BodySize < td.minBodySize)
			{
				visible = true;
				return new AcceptanceReport("CannotTrainTooSmall".Translate(pawn.LabelCapNoCount, pawn).Resolve());
			}
			if (pawn.RaceProps.trainability.intelligenceOrder < td.requiredTrainability.intelligenceOrder)
			{
				visible = true;
				return new AcceptanceReport("CannotTrainNotSmartEnough".Translate(td.requiredTrainability.label).Resolve());
			}
			visible = true;
			return true;
		}

		public TrainableDef NextTrainableToTrain()
		{
			List<TrainableDef> trainableDefsInListOrder = TrainableUtility.TrainableDefsInListOrder;
			for (int i = 0; i < trainableDefsInListOrder.Count; i++)
			{
				if (GetWanted(trainableDefsInListOrder[i]) && CanBeTrained(trainableDefsInListOrder[i]))
				{
					return trainableDefsInListOrder[i];
				}
			}
			return null;
		}

		public void Train(TrainableDef td, Pawn trainer, bool complete = false)
		{
			if (complete)
			{
				steps[td] = td.steps;
			}
			else
			{
				steps[td]++;
			}
			if (steps[td] >= td.steps)
			{
				learned[td] = true;
				if (td == TrainableDefOf.Obedience && trainer != null && pawn.playerSettings != null && pawn.playerSettings.Master == null)
				{
					pawn.playerSettings.Master = trainer;
				}
			}
		}

		public void SetWantedRecursive(TrainableDef td, bool checkOn)
		{
			SetWanted(td, checkOn);
			if (checkOn)
			{
				if (td.prerequisites != null)
				{
					for (int i = 0; i < td.prerequisites.Count; i++)
					{
						SetWantedRecursive(td.prerequisites[i], checkOn: true);
					}
				}
				return;
			}
			foreach (TrainableDef item in DefDatabase<TrainableDef>.AllDefsListForReading.Where((TrainableDef t) => t.prerequisites != null && t.prerequisites.Contains(td)))
			{
				SetWantedRecursive(item, checkOn: false);
			}
		}

		public void TrainingTrackerTickRare()
		{
			if (pawn.Suspended)
			{
				countDecayFrom += 250;
			}
			else if (!pawn.Spawned)
			{
				countDecayFrom += 250;
			}
			else if (steps[TrainableDefOf.Tameness] == 0)
			{
				countDecayFrom = Find.TickManager.TicksGame;
			}
			else
			{
				if (Find.TickManager.TicksGame < countDecayFrom + TrainableUtility.DegradationPeriodTicks(pawn.def))
				{
					return;
				}
				TrainableDef trainableDef = (from kvp in steps
					where kvp.Value > 0
					select kvp.Key).Except(steps.Where((KeyValuePair<TrainableDef, int> kvp) => kvp.Value > 0 && kvp.Key.prerequisites != null).SelectMany((KeyValuePair<TrainableDef, int> kvp) => kvp.Key.prerequisites)).RandomElement();
				if (trainableDef == TrainableDefOf.Tameness && !TrainableUtility.TamenessCanDecay(pawn.def))
				{
					countDecayFrom = Find.TickManager.TicksGame;
					return;
				}
				countDecayFrom = Find.TickManager.TicksGame;
				steps[trainableDef]--;
				if (steps[trainableDef] > 0 || !learned[trainableDef])
				{
					return;
				}
				learned[trainableDef] = false;
				if (pawn.Faction == Faction.OfPlayer)
				{
					if (trainableDef == TrainableDefOf.Tameness)
					{
						pawn.SetFaction(null);
						Messages.Message("MessageAnimalReturnedWild".Translate(pawn.LabelShort, pawn), pawn, MessageTypeDefOf.NegativeEvent);
					}
					else
					{
						Messages.Message("MessageAnimalLostSkill".Translate(pawn.LabelShort, trainableDef.LabelCap, pawn.Named("ANIMAL")), pawn, MessageTypeDefOf.NegativeEvent);
					}
				}
			}
		}

		public void Debug_MakeDegradeHappenSoon()
		{
			countDecayFrom = Find.TickManager.TicksGame - TrainableUtility.DegradationPeriodTicks(pawn.def) - 500;
		}
	}
}
