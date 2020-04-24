using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_TakeDrug : JoyGiver_Ingest
	{
		private static List<ThingDef> takeableDrugs = new List<ThingDef>();

		protected override Thing BestIngestItem(Pawn pawn, Predicate<Thing> extraValidator)
		{
			if (pawn.drugs == null)
			{
				return null;
			}
			Predicate<Thing> predicate = delegate(Thing t)
			{
				if (!CanIngestForJoy(pawn, t))
				{
					return false;
				}
				return (extraValidator == null || extraValidator(t)) ? true : false;
			};
			ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
			for (int i = 0; i < innerContainer.Count; i++)
			{
				if (predicate(innerContainer[i]))
				{
					return innerContainer[i];
				}
			}
			takeableDrugs.Clear();
			DrugPolicy currentPolicy = pawn.drugs.CurrentPolicy;
			for (int j = 0; j < currentPolicy.Count; j++)
			{
				if (currentPolicy[j].allowedForJoy)
				{
					takeableDrugs.Add(currentPolicy[j].drug);
				}
			}
			takeableDrugs.Shuffle();
			for (int k = 0; k < takeableDrugs.Count; k++)
			{
				List<Thing> list = pawn.Map.listerThings.ThingsOfDef(takeableDrugs[k]);
				if (list.Count > 0)
				{
					Thing thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, list, PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, predicate);
					if (thing != null)
					{
						return thing;
					}
				}
			}
			return null;
		}

		public override float GetChance(Pawn pawn)
		{
			int num = 0;
			if (pawn.story != null)
			{
				num = pawn.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire);
			}
			if (num < 0)
			{
				return 0f;
			}
			float num2 = def.baseChance;
			if (num == 1)
			{
				num2 *= 2f;
			}
			if (num == 2)
			{
				num2 *= 5f;
			}
			return num2;
		}

		protected override Job CreateIngestJob(Thing ingestible, Pawn pawn)
		{
			return DrugAIUtility.IngestAndTakeToInventoryJob(ingestible, pawn);
		}
	}
}
