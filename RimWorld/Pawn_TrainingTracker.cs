using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Pawn_TrainingTracker : IExposable
	{
		public Pawn pawn;

		private DefMap<TrainableDef, bool> wantedTrainables = new DefMap<TrainableDef, bool>();

		private DefMap<TrainableDef, int> steps = new DefMap<TrainableDef, int>();

		private DefMap<TrainableDef, bool> learned = new DefMap<TrainableDef, bool>();

		public Thing attackTarget;

		private int countDecayFrom;

		public const float AttackTargetRange = 25.9f;

		public static readonly Texture2D AttackTargetTexture = ContentFinder<Texture2D>.Get("UI/Commands/AnimalAttack");

		private static readonly Texture2D CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

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
			Scribe_References.Look(ref attackTarget, "attackTarget");
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
			TrainabilityDef trainability = TrainableUtility.GetTrainability(pawn);
			if (td.requiredTrainability != null && trainability != null && trainability.intelligenceOrder < td.requiredTrainability?.intelligenceOrder)
			{
				return false;
			}
			return learned[td];
		}

		public AcceptanceReport CanAssignToTrain(TrainableDef td)
		{
			bool visible;
			return CanAssignToTrain(td, out visible);
		}

		public AcceptanceReport CanAssignToTrain(TrainableDef td, out bool visible)
		{
			return CanAssignToTrain(td, pawn.def, out visible, pawn);
		}

		public static AcceptanceReport CanAssignToTrain(TrainableDef td, ThingDef pawnDef, out bool visible, Pawn pawn = null)
		{
			if (pawnDef.race.untrainableTags != null)
			{
				for (int i = 0; i < pawnDef.race.untrainableTags.Count; i++)
				{
					if (td.MatchesTag(pawnDef.race.untrainableTags[i]))
					{
						visible = false;
						return false;
					}
				}
			}
			if (ModsConfig.OdysseyActive && td.specialTrainable)
			{
				List<TrainableDef> specialTrainables = pawnDef.race.specialTrainables;
				if (specialTrainables == null || !specialTrainables.Contains(td))
				{
					visible = false;
					return false;
				}
			}
			float num = pawn?.BodySize ?? pawnDef.race.baseBodySize;
			string text = pawn?.LabelShort ?? pawnDef.label;
			TrainabilityDef trainabilityDef = TrainableUtility.GetTrainability(pawn) ?? pawnDef.race.trainability;
			if (pawnDef.race.trainableTags != null)
			{
				for (int j = 0; j < pawnDef.race.trainableTags.Count; j++)
				{
					if (td.MatchesTag(pawnDef.race.trainableTags[j]))
					{
						if (num < td.minBodySize)
						{
							visible = true;
							return new AcceptanceReport("CannotTrainTooSmall".Translate(text).Resolve());
						}
						visible = true;
						return true;
					}
				}
			}
			if (!td.defaultTrainable && !td.specialTrainable)
			{
				visible = false;
				return false;
			}
			if (num < td.minBodySize)
			{
				visible = true;
				return new AcceptanceReport("CannotTrainTooSmall".Translate(text).Resolve());
			}
			if (td.requiredTrainability != null && trainabilityDef.intelligenceOrder < td.requiredTrainability.intelligenceOrder)
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
				if (Find.TickManager.TicksGame < countDecayFrom + TrainableUtility.DegradationPeriodTicks(pawn) || pawn.RaceProps.animalType == AnimalType.Dryad)
				{
					return;
				}
				TrainableDef trainableDef = (from kvp in steps
					where kvp.Value > 0
					select kvp.Key).Except(steps.Where((KeyValuePair<TrainableDef, int> kvp) => kvp.Value > 0 && kvp.Key.prerequisites != null).SelectMany((KeyValuePair<TrainableDef, int> kvp) => kvp.Key.prerequisites)).RandomElement();
				if (trainableDef == TrainableDefOf.Tameness && !TrainableUtility.TamenessCanDecay(pawn))
				{
					countDecayFrom = Find.TickManager.TicksGame;
					return;
				}
				countDecayFrom = Find.TickManager.TicksGame;
				DefMap<TrainableDef, int> defMap = steps;
				TrainableDef def = trainableDef;
				int value = defMap[def] - 1;
				defMap[def] = value;
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

		private static bool CanEverDoAttackTarget(Pawn pawn)
		{
			if (!ModsConfig.OdysseyActive)
			{
				return false;
			}
			return pawn.training?.HasLearned(TrainableDefOf.AttackTarget) ?? false;
		}

		private static AcceptanceReport CanDoAttackTarget(Pawn pawn)
		{
			if (!CanEverDoAttackTarget(pawn))
			{
				return false;
			}
			if (pawn.playerSettings.Master == null)
			{
				return "NoMaster".Translate();
			}
			if (!pawn.playerSettings.Master.Spawned || pawn.playerSettings.Master.Map != pawn.Map)
			{
				return "MasterNotSpawned".Translate();
			}
			return true;
		}

		public IEnumerable<Gizmo> GetGizmos()
		{
			if (!CanEverDoAttackTarget(pawn))
			{
				yield break;
			}
			Pawn master = pawn.playerSettings.Master;
			AcceptanceReport report = CanDoAttackTarget(pawn);
			yield return new Command_Target
			{
				defaultLabel = "AnimalAttackTarget".Translate(),
				defaultDesc = "AnimalAttackTargetDesc".Translate(),
				targetingParams = TargetingParameters.ForAttackAny(),
				hotKey = KeyBindingDefOf.Misc2,
				icon = AttackTargetTexture,
				groupKey = (report.Accepted ? 23452345 : (-1)),
				action = delegate(LocalTargetInfo target)
				{
					IEnumerable<Pawn> enumerable = Find.Selector.SelectedObjects.Where((object x) => x is Pawn pawn && (bool)CanDoAttackTarget(pawn)).Cast<Pawn>();
					if (!target.Cell.InHorDistOf(master.Position, 25.9f))
					{
						Messages.Message("MessageNotInRangeOfMaster".Translate(), MessageTypeDefOf.RejectInput, historical: false);
					}
					else
					{
						string failStr = null;
						foreach (Pawn item in enumerable)
						{
							FloatMenuUtility.GetMeleeAttackAction(item, target, out failStr, ignoreControlled: true)?.Invoke();
							item.training.attackTarget = target.Thing;
							item.mindState.enemyTarget = target.Thing;
						}
						if (!failStr.NullOrEmpty())
						{
							Messages.Message(failStr, MessageTypeDefOf.RejectInput, historical: false);
						}
					}
				},
				Disabled = !report.Accepted,
				disabledReason = report.Reason,
				onUpdate = delegate
				{
					if (report.Accepted && pawn.playerSettings?.Master != null)
					{
						GenDraw.DrawRadiusRing(pawn.playerSettings.Master.Position, 25.9f);
					}
				}
			};
			if (attackTarget == null || pawn.jobs.curJob.def != JobDefOf.AttackMelee || !(pawn.jobs.curJob.targetA == attackTarget))
			{
				yield break;
			}
			yield return new Command_Action
			{
				defaultLabel = "CancelAttack".Translate(),
				defaultDesc = "CancelAttackDesc".Translate(),
				icon = CancelIcon,
				groupKey = 76547456,
				action = delegate
				{
					foreach (Pawn item2 in Find.Selector.SelectedObjects.Where(delegate(object x)
					{
						Pawn pawn = x as Pawn;
						return pawn?.training?.attackTarget != null && pawn.mindState.enemyTarget == pawn.training.attackTarget;
					}).Cast<Pawn>())
					{
						item2.training.attackTarget = null;
						item2.jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
					SoundDefOf.Tick_Low.PlayOneShotOnCamera();
				}
			};
		}

		public void Debug_MakeDegradeHappenSoon()
		{
			countDecayFrom = Find.TickManager.TicksGame - TrainableUtility.DegradationPeriodTicks(pawn) - 500;
		}
	}
}
