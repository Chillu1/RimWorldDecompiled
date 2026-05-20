using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public static class MedicalCareUtility
{
	private static Texture2D[] careTextures;

	public const float CareSetterHeight = 28f;

	public const float CareSetterWidth = 140f;

	private static bool medicalCarePainting;

	public static void Reset()
	{
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			careTextures = new Texture2D[5];
			careTextures[0] = ContentFinder<Texture2D>.Get("UI/Icons/Medical/NoCare");
			careTextures[1] = ContentFinder<Texture2D>.Get("UI/Icons/Medical/NoMeds");
			careTextures[2] = ThingDefOf.MedicineHerbal.uiIcon;
			careTextures[3] = ThingDefOf.MedicineIndustrial.uiIcon;
			careTextures[4] = ThingDefOf.MedicineUltratech.uiIcon;
		});
	}

	public static void MedicalCareSetter(Rect rect, ref MedicalCareCategory medCare)
	{
		Rect rect2 = new Rect(rect.x, rect.y, rect.width / 5f, rect.height);
		for (int i = 0; i < 5; i++)
		{
			MedicalCareCategory mc = (MedicalCareCategory)i;
			Widgets.DrawHighlightIfMouseover(rect2);
			MouseoverSounds.DoRegion(rect2);
			GUI.DrawTexture(rect2, careTextures[i]);
			Widgets.DraggableResult draggableResult = Widgets.ButtonInvisibleDraggable(rect2);
			if (draggableResult == Widgets.DraggableResult.Dragged)
			{
				medicalCarePainting = true;
			}
			if ((medicalCarePainting && Mouse.IsOver(rect2) && medCare != mc) || draggableResult.AnyPressed())
			{
				medCare = mc;
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
			}
			if (medCare == mc)
			{
				Widgets.DrawBox(rect2, 2);
			}
			if (Mouse.IsOver(rect2))
			{
				TooltipHandler.TipRegion(rect2, () => mc.GetLabel().CapitalizeFirst(), 632165 + i * 17);
			}
			rect2.x += rect2.width;
		}
		if (!Input.GetMouseButton(0))
		{
			medicalCarePainting = false;
		}
	}

	public static string GetLabel(this MedicalCareCategory cat)
	{
		return ("MedicalCareCategory_" + cat).Translate();
	}

	public static bool AllowsMedicine(this MedicalCareCategory cat, ThingDef meds)
	{
		return cat switch
		{
			MedicalCareCategory.NoCare => false, 
			MedicalCareCategory.NoMeds => false, 
			MedicalCareCategory.HerbalOrWorse => meds.GetStatValueAbstract(StatDefOf.MedicalPotency) <= ThingDefOf.MedicineHerbal.GetStatValueAbstract(StatDefOf.MedicalPotency), 
			MedicalCareCategory.NormalOrWorse => meds.GetStatValueAbstract(StatDefOf.MedicalPotency) <= ThingDefOf.MedicineIndustrial.GetStatValueAbstract(StatDefOf.MedicalPotency), 
			MedicalCareCategory.Best => true, 
			_ => throw new InvalidOperationException(), 
		};
	}

	public static void MedicalCareSelectButton(Rect rect, Pawn pawn)
	{
		Widgets.Dropdown(rect, pawn, MedicalCareSelectButton_GetMedicalCare, MedicalCareSelectButton_GenerateMenu, null, careTextures[(uint)pawn.playerSettings.medCare], null, null, null, paintable: true);
	}

	private static MedicalCareCategory MedicalCareSelectButton_GetMedicalCare(Pawn pawn)
	{
		return pawn.playerSettings.medCare;
	}

	private static IEnumerable<Widgets.DropdownMenuElement<MedicalCareCategory>> MedicalCareSelectButton_GenerateMenu(Pawn p)
	{
		for (int i = 0; i < 5; i++)
		{
			MedicalCareCategory mc = (MedicalCareCategory)i;
			yield return new Widgets.DropdownMenuElement<MedicalCareCategory>
			{
				option = new FloatMenuOption(mc.GetLabel().CapitalizeFirst(), delegate
				{
					p.playerSettings.medCare = mc;
				}, MedicalCareIcon(mc), Color.white),
				payload = mc
			};
		}
		yield return new Widgets.DropdownMenuElement<MedicalCareCategory>
		{
			option = new FloatMenuOption("ChangeDefaults".Translate(), delegate
			{
				Find.WindowStack.Add(new Dialog_MedicalDefaults());
			})
		};
	}

	private static Texture2D MedicalCareIcon(MedicalCareCategory category)
	{
		return category switch
		{
			MedicalCareCategory.NoCare => careTextures[0], 
			MedicalCareCategory.NoMeds => careTextures[1], 
			MedicalCareCategory.HerbalOrWorse => careTextures[2], 
			MedicalCareCategory.NormalOrWorse => careTextures[3], 
			MedicalCareCategory.Best => careTextures[4], 
			_ => null, 
		};
	}
}
