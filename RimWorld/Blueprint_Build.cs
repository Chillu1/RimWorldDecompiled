using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Blueprint_Build : Blueprint, IHaulEnroute, ILoadReferenceable
	{
		public ThingDef stuffToUse;

		public ThingStyleDef selectedStyleDef;

		public bool styleOverridden;

		public ColorInt? glowerColorOverride;

		private static readonly CachedTexture ChangeStyleTex = new CachedTexture("UI/Gizmos/ChangeStyle");

		public override string Label
		{
			get
			{
				string text = def.entityDefToBuild.label;
				if (base.StyleSourcePrecept != null)
				{
					text = base.StyleSourcePrecept.TransformThingLabel(text);
				}
				if (stuffToUse != null)
				{
					return "ThingMadeOfStuffLabel".Translate(stuffToUse.LabelAsStuff, text) + "BlueprintLabelExtra".Translate();
				}
				return text + "BlueprintLabelExtra".Translate();
			}
		}

		protected override float WorkTotal => def.entityDefToBuild.GetStatValueAbstract(StatDefOf.WorkToBuild, stuffToUse);

		public ThingDef BuildDef => def.entityDefToBuild as ThingDef;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref stuffToUse, "stuffToUse");
			Scribe_Defs.Look(ref selectedStyleDef, "selectedStyleDef");
			Scribe_Values.Look(ref styleOverridden, "styleOverridden", defaultValue: false);
			Scribe_Values.Look(ref glowerColorOverride, "glowerColorOverride");
		}

		public override ThingStyleDef EntityToBuildStyle()
		{
			return selectedStyleDef ?? StyleDef;
		}

		public override BuildableDef EntityToBuild()
		{
			return def.entityDefToBuild;
		}

		public override ThingDef EntityToBuildStuff()
		{
			return stuffToUse;
		}

		public override List<ThingDefCountClass> TotalMaterialCost()
		{
			return def.entityDefToBuild.CostListAdjusted(stuffToUse);
		}

		protected override Thing MakeSolidThing(out bool shouldSelect)
		{
			Frame frame = (Frame)ThingMaker.MakeThing(def.entityDefToBuild.frameDef, stuffToUse);
			frame.StyleSourcePrecept = base.StyleSourcePrecept;
			frame.StyleDef = StyleDef;
			frame.glowerColorOverride = glowerColorOverride;
			shouldSelect = false;
			base.Map.enrouteManager.SendReservations(this, frame);
			return frame;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			Command command = BuildCopyCommandUtility.BuildCopyCommand(def.entityDefToBuild, stuffToUse, base.StyleSourcePrecept as Precept_Building, selectedStyleDef ?? StyleDef, styleOverridden, glowerColorOverride);
			if (command != null)
			{
				yield return command;
			}
			if (base.Faction != Faction.OfPlayer)
			{
				yield break;
			}
			if (ModsConfig.IdeologyActive && Find.IdeoManager.classicMode)
			{
				BuildableDef entityDefToBuild = def.entityDefToBuild;
				ThingDef thingDef = entityDefToBuild as ThingDef;
				if (thingDef != null && thingDef.CanBeStyled())
				{
					Color stuffColor = ((stuffToUse == null) ? Color.white : def.entityDefToBuild.GetColorForStuff(stuffToUse));
					Command_Action command_Action = new Command_Action
					{
						defaultLabel = "ChangeStyle".Translate().CapitalizeFirst(),
						defaultDesc = "ChangeStyleDesc".Translate(),
						icon = ChangeStyleTex.Texture,
						Order = 15f,
						action = delegate
						{
							List<FloatMenuOption> list = new List<FloatMenuOption>
							{
								new FloatMenuOption("Basic".Translate().CapitalizeFirst(), delegate
								{
									ChangeStyleOfAllSelected(def.entityDefToBuild, null);
								}, Widgets.GetIconFor(thingDef, base.Stuff), stuffColor)
							};
							foreach (StyleCategoryDef relevantStyleCategory in thingDef.RelevantStyleCategories)
							{
								foreach (ThingDefStyle thingDefStyle in relevantStyleCategory.thingDefStyles)
								{
									if (thingDefStyle.ThingDef == thingDef)
									{
										ThingDefStyle tdStyle = thingDefStyle;
										list.Add(new FloatMenuOption(relevantStyleCategory.LabelCap, delegate
										{
											ChangeStyleOfAllSelected(def.entityDefToBuild, tdStyle.StyleDef);
										}, Widgets.GetIconFor(thingDef, base.Stuff, tdStyle.StyleDef), stuffColor));
										break;
									}
								}
							}
							if (list.Any())
							{
								Find.WindowStack.Add(new FloatMenu(list));
							}
						}
					};
					if (!thingDef.RelevantStyleCategories.Any())
					{
						command_Action.Disable("ChangeStyleDisabledNoCategories".Translate());
					}
					yield return command_Action;
				}
			}
			foreach (Command item in BuildRelatedCommandUtility.RelatedBuildCommands(def.entityDefToBuild))
			{
				yield return item;
			}
		}

		private void ChangeStyleOfAllSelected(BuildableDef buildable, ThingStyleDef styleDef)
		{
			foreach (object selectedObject in Find.Selector.SelectedObjects)
			{
				if (selectedObject is Blueprint_Build blueprint_Build && blueprint_Build.def.entityDefToBuild == buildable)
				{
					blueprint_Build.StyleDef = styleDef;
					blueprint_Build.styleOverridden = true;
					blueprint_Build.DirtyMapMesh(blueprint_Build.Map);
				}
			}
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			if (mode == DestroyMode.Cancel || (mode != DestroyMode.Vanish && !def.IsBlueprint))
			{
				foreach (Thing attachedBuilding in GenConstruct.GetAttachedBuildings(this))
				{
					attachedBuilding.DeSpawn(mode);
				}
			}
			base.DeSpawn(mode);
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
			foreach (ThingDefCountClass item in TotalMaterialCost())
			{
				if (!flag)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(string.Concat(item.thingDef.LabelCap + ": 0 / ", item.count.ToString()));
				flag = false;
			}
			if (StyleDef?.Category != null && base.StyleSourcePrecept == null)
			{
				stringBuilder.AppendInNewLine("Style".Translate() + ": " + StyleDef.Category.LabelCap);
			}
			return stringBuilder.ToString().Trim();
		}
	}
}
