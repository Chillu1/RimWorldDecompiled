using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompCauseGameCondition_PsychicSuppression : CompCauseGameCondition
	{
		public Gender gender;

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			gender = Gender.Male;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref gender, "gender", Gender.None);
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (Prefs.DevMode)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = gender.GetLabel();
				command_Action.action = delegate
				{
					if (gender == Gender.Female)
					{
						gender = Gender.Male;
					}
					else
					{
						gender = Gender.Female;
					}
					ReSetupAllConditions();
				};
				command_Action.hotKey = KeyBindingDefOf.Misc1;
				yield return command_Action;
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			if (base.Active && base.MyTile != -1)
			{
				foreach (Caravan caravan in Find.World.worldObjects.Caravans)
				{
					if (Find.WorldGrid.ApproxDistanceInTiles(caravan.Tile, base.MyTile) < (float)base.Props.worldRange)
					{
						foreach (Pawn pawn in caravan.pawns)
						{
							GameCondition_PsychicSuppression.CheckPawn(pawn, gender);
						}
					}
				}
			}
		}

		protected override void SetupCondition(GameCondition condition, Map map)
		{
			base.SetupCondition(condition, map);
			((GameCondition_PsychicSuppression)condition).gender = gender;
		}

		public override string CompInspectStringExtra()
		{
			string text = base.CompInspectStringExtra();
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			return text + ("AffectedGender".Translate() + ": " + gender.GetLabel().CapitalizeFirst());
		}

		public override void RandomizeSettings()
		{
			gender = (Rand.Bool ? Gender.Male : Gender.Female);
		}
	}
}
