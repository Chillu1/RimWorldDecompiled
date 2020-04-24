using RimWorld;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class Command_SetPlantToGrow : Command
	{
		public IPlantToGrowSettable settable;

		private List<IPlantToGrowSettable> settables;

		private static List<ThingDef> tmpAvailablePlants = new List<ThingDef>();

		private static readonly Texture2D SetPlantToGrowTex = ContentFinder<Texture2D>.Get("UI/Commands/SetPlantToGrow");

		public Command_SetPlantToGrow()
		{
			tutorTag = "GrowingZoneSetPlant";
			ThingDef thingDef = null;
			bool flag = false;
			foreach (object selectedObject in Find.Selector.SelectedObjects)
			{
				IPlantToGrowSettable plantToGrowSettable = selectedObject as IPlantToGrowSettable;
				if (plantToGrowSettable != null)
				{
					if (thingDef != null && thingDef != plantToGrowSettable.GetPlantDefToGrow())
					{
						flag = true;
						break;
					}
					thingDef = plantToGrowSettable.GetPlantDefToGrow();
				}
			}
			if (flag)
			{
				icon = SetPlantToGrowTex;
				defaultLabel = "CommandSelectPlantToGrowMulti".Translate();
				return;
			}
			icon = thingDef.uiIcon;
			iconAngle = thingDef.uiIconAngle;
			iconOffset = thingDef.uiIconOffset;
			defaultLabel = "CommandSelectPlantToGrow".Translate(thingDef.LabelCap);
		}

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			if (settables == null)
			{
				settables = new List<IPlantToGrowSettable>();
			}
			if (!settables.Contains(settable))
			{
				settables.Add(settable);
			}
			tmpAvailablePlants.Clear();
			foreach (ThingDef item in PlantUtility.ValidPlantTypesForGrowers(settables))
			{
				if (IsPlantAvailable(item, settable.Map))
				{
					tmpAvailablePlants.Add(item);
				}
			}
			tmpAvailablePlants.SortBy((ThingDef x) => 0f - GetPlantListPriority(x), (ThingDef x) => x.label);
			for (int i = 0; i < tmpAvailablePlants.Count; i++)
			{
				ThingDef plantDef = tmpAvailablePlants[i];
				string text = plantDef.LabelCap;
				if (plantDef.plant.sowMinSkill > 0)
				{
					text = text + (string)(" (" + "MinSkill".Translate() + ": ") + plantDef.plant.sowMinSkill + ")";
				}
				list.Add(new FloatMenuOption(text, delegate
				{
					string s = tutorTag + "-" + plantDef.defName;
					if (TutorSystem.AllowAction(s))
					{
						bool flag = true;
						for (int j = 0; j < settables.Count; j++)
						{
							settables[j].SetPlantDefToGrow(plantDef);
							if (flag && plantDef.plant.interferesWithRoof)
							{
								foreach (IntVec3 cell in settables[j].Cells)
								{
									if (cell.Roofed(settables[j].Map))
									{
										Messages.Message("MessagePlantIncompatibleWithRoof".Translate(Find.ActiveLanguageWorker.Pluralize(plantDef.LabelCap)), MessageTypeDefOf.CautionInput, historical: false);
										flag = false;
										break;
									}
								}
							}
						}
						PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.SetGrowingZonePlant, KnowledgeAmount.Total);
						WarnAsAppropriate(plantDef);
						TutorSystem.Notify_Event(s);
					}
				}, plantDef, MenuOptionPriority.Default, null, null, 29f, (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, plantDef)));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		public override bool InheritInteractionsFrom(Gizmo other)
		{
			if (settables == null)
			{
				settables = new List<IPlantToGrowSettable>();
			}
			settables.Add(((Command_SetPlantToGrow)other).settable);
			return false;
		}

		private void WarnAsAppropriate(ThingDef plantDef)
		{
			if (plantDef.plant.sowMinSkill > 0)
			{
				foreach (Pawn item in settable.Map.mapPawns.FreeColonistsSpawned)
				{
					if (item.skills.GetSkill(SkillDefOf.Plants).Level >= plantDef.plant.sowMinSkill && !item.Downed && item.workSettings.WorkIsActive(WorkTypeDefOf.Growing))
					{
						return;
					}
				}
				Find.WindowStack.Add(new Dialog_MessageBox("NoGrowerCanPlant".Translate(plantDef.label, plantDef.plant.sowMinSkill).CapitalizeFirst()));
			}
			if (!plantDef.plant.cavePlant)
			{
				return;
			}
			IntVec3 cell = IntVec3.Invalid;
			for (int i = 0; i < settables.Count; i++)
			{
				foreach (IntVec3 cell2 in settables[i].Cells)
				{
					if (!cell2.Roofed(settables[i].Map) || settables[i].Map.glowGrid.GameGlowAt(cell2, ignoreCavePlants: true) > 0f)
					{
						cell = cell2;
						break;
					}
				}
				if (cell.IsValid)
				{
					break;
				}
			}
			if (cell.IsValid)
			{
				Messages.Message("MessageWarningCavePlantsExposedToLight".Translate(plantDef.LabelCap), new TargetInfo(cell, settable.Map), MessageTypeDefOf.RejectInput);
			}
		}

		public static bool IsPlantAvailable(ThingDef plantDef, Map map)
		{
			List<ResearchProjectDef> sowResearchPrerequisites = plantDef.plant.sowResearchPrerequisites;
			if (sowResearchPrerequisites == null)
			{
				return true;
			}
			for (int i = 0; i < sowResearchPrerequisites.Count; i++)
			{
				if (!sowResearchPrerequisites[i].IsFinished)
				{
					return false;
				}
			}
			if (plantDef.plant.mustBeWildToSow && !map.Biome.AllWildPlants.Contains(plantDef))
			{
				return false;
			}
			return true;
		}

		private float GetPlantListPriority(ThingDef plantDef)
		{
			if (plantDef.plant.IsTree)
			{
				return 1f;
			}
			switch (plantDef.plant.purpose)
			{
			case PlantPurpose.Food:
				return 4f;
			case PlantPurpose.Health:
				return 3f;
			case PlantPurpose.Beauty:
				return 2f;
			case PlantPurpose.Misc:
				return 0f;
			default:
				return 0f;
			}
		}
	}
}
