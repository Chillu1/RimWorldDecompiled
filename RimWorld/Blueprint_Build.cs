using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Blueprint_Build : Blueprint
	{
		public ThingDef stuffToUse;

		public override string Label
		{
			get
			{
				string label = def.entityDefToBuild.label;
				if (stuffToUse != null)
				{
					return "ThingMadeOfStuffLabel".Translate(stuffToUse.LabelAsStuff, label) + "BlueprintLabelExtra".Translate();
				}
				return label + "BlueprintLabelExtra".Translate();
			}
		}

		protected override float WorkTotal => def.entityDefToBuild.GetStatValueAbstract(StatDefOf.WorkToBuild, stuffToUse);

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref stuffToUse, "stuffToUse");
		}

		public override ThingDef EntityToBuildStuff()
		{
			return stuffToUse;
		}

		public override List<ThingDefCountClass> MaterialsNeeded()
		{
			return def.entityDefToBuild.CostListAdjusted(stuffToUse);
		}

		protected override Thing MakeSolidThing()
		{
			return ThingMaker.MakeThing(def.entityDefToBuild.frameDef, stuffToUse);
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			Command command = BuildCopyCommandUtility.BuildCopyCommand(def.entityDefToBuild, stuffToUse);
			if (command != null)
			{
				yield return command;
			}
			if (base.Faction == Faction.OfPlayer)
			{
				foreach (Command item in BuildFacilityCommandUtility.BuildFacilityCommands(def.entityDefToBuild))
				{
					yield return item;
				}
			}
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine("ContainedResources".Translate() + ":");
			bool flag = true;
			foreach (ThingDefCountClass item in MaterialsNeeded())
			{
				if (!flag)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append((string)(item.thingDef.LabelCap + ": 0 / ") + item.count);
				flag = false;
			}
			return stringBuilder.ToString().Trim();
		}
	}
}
