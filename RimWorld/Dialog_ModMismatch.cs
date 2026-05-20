using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class Dialog_ModMismatch : Window
{
	private Action loadAction;

	private List<string> loadedModIdsList;

	public List<string> loadedModNamesList;

	private Vector2 addedModListScrollPosition = Vector2.zero;

	private Vector2 missingModListScrollPosition = Vector2.zero;

	private Vector2 sharedModListScrollPosition = Vector2.zero;

	private List<string> runningModIdsList;

	private List<string> runningModNamesList;

	private List<string> addedModsList;

	private List<string> missingModsList;

	private List<string> sharedModsList;

	private static float ButtonWidth = 200f;

	private static float ButtonHeight = 30f;

	private static float ModRowHeight = 24f;

	public override Vector2 InitialSize => new Vector2(900f, 750f);

	public Dialog_ModMismatch(Action loadAction, List<string> loadedModIdsList, List<string> loadedModNamesList)
	{
		this.loadAction = loadAction;
		this.loadedModIdsList = loadedModIdsList.Select((string id) => id.ToLower()).ToList();
		this.loadedModNamesList = loadedModNamesList;
	}

	public override void PreOpen()
	{
		base.PreOpen();
		List<string> source = LoadedModManager.RunningMods.Select((ModContentPack mod) => mod.PackageId).ToList();
		runningModIdsList = source.Select((string id) => ModLister.GetModWithIdentifier(id).PackageId.ToLower()).ToList();
		runningModNamesList = source.Select((string id) => ModLister.GetModWithIdentifier(id).Name).ToList();
		addedModsList = (from modId in runningModIdsList
			where !loadedModIdsList.Contains(modId)
			select runningModNamesList[runningModIdsList.IndexOf(modId)]).ToList();
		missingModsList = (from modId in loadedModIdsList
			where !runningModIdsList.Contains(modId)
			select loadedModNamesList[loadedModIdsList.IndexOf(modId)]).ToList();
		sharedModsList = (from modId in runningModIdsList
			where loadedModIdsList.Contains(modId)
			select runningModNamesList[runningModIdsList.IndexOf(modId)]).ToList();
	}

	public override void DoWindowContents(Rect inRect)
	{
		float num = (inRect.width - 20f) / 3f;
		float num2 = 0f;
		float x = 0f;
		float x2 = num + 10f;
		float x3 = (num + 10f) * 2f;
		Text.Font = GameFont.Medium;
		Widgets.Label(new Rect(0f, num2, inRect.width, Text.LineHeight), "ModsMismatchWarningTitle".Translate());
		num2 += Text.LineHeight + 10f;
		Text.Font = GameFont.Small;
		float height = Text.CalcHeight("ModsMismatchWarningText".Translate(), inRect.width);
		Rect rect = new Rect(0f, num2, inRect.width, height);
		Widgets.Label(rect, "ModsMismatchWarningText".Translate());
		num2 += rect.height + 17f;
		if (!addedModsList.Any() && !missingModsList.Any())
		{
			float height2 = Text.CalcHeight("ModsMismatchOrderChanged".Translate(), inRect.width);
			Widgets.Label(new Rect(0f, num2, inRect.width, height2), "ModsMismatchOrderChanged".Translate());
		}
		else
		{
			float num3 = num2;
			Widgets.Label(new Rect(x, num2, num, Text.LineHeight), "AddedModsList".Translate());
			num2 += Text.LineHeight + 10f;
			float height3 = inRect.height - num2 - ButtonHeight - 10f;
			DoModList(new Rect(x, num2, num, height3), addedModsList, ref addedModListScrollPosition, new Color(0.27f, 0.4f, 0.1f));
			num2 = num3;
			Widgets.Label(new Rect(x2, num2, num, Text.LineHeight), "MissingModsList".Translate());
			num2 += Text.LineHeight + 10f;
			DoModList(new Rect(x2, num2, num, height3), missingModsList, ref missingModListScrollPosition, new Color(0.38f, 0.07f, 0.09f));
			num2 = num3;
			Widgets.Label(new Rect(x3, num2, num, Text.LineHeight), "SharedModsList".Translate());
			num2 += Text.LineHeight + 10f;
			DoModList(new Rect(x3, num2, num, height3), sharedModsList, ref sharedModListScrollPosition);
		}
		float y = inRect.height - ButtonHeight;
		Rect rect2 = new Rect(0f, y, ButtonWidth, ButtonHeight);
		Rect rect3 = new Rect(inRect.width / 2f - ButtonWidth - 4f, y, ButtonWidth, ButtonHeight);
		Rect rect4 = new Rect(inRect.width / 2f + 4f, y, ButtonWidth, ButtonHeight);
		Rect rect5 = new Rect(inRect.width - ButtonWidth, y, ButtonWidth, ButtonHeight);
		if (Widgets.ButtonText(rect2, "GoBack".Translate()))
		{
			HandleGoBackClicked();
		}
		if (Widgets.ButtonText(rect3, "SaveModList".Translate()))
		{
			HandleSaveCurrentModList();
		}
		if (Widgets.ButtonText(rect4, "ChangeLoadedMods".Translate()))
		{
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmLoadSaveList".Translate(), delegate
			{
				HandleChangeLoadedModClicked();
			}, destructive: true));
		}
		if (Widgets.ButtonText(rect5, "LoadAnyway".Translate()))
		{
			HandleLoadAnywayClicked();
		}
	}

	private void DoModList(Rect r, List<string> modList, ref Vector2 scrollPos, Color? rowColor = null)
	{
		int num = 0;
		Widgets.BeginScrollView(viewRect: new Rect(0f, 0f, r.width - 16f, (float)modList.Count * ModRowHeight), outRect: r, scrollPosition: ref scrollPos);
		foreach (string mod in modList)
		{
			Rect rect = new Rect(0f, (float)num * ModRowHeight, r.width, ModRowHeight);
			if (rowColor.HasValue)
			{
				Widgets.DrawBoxSolid(rect, rowColor.Value);
			}
			DoModRow(rect, mod, num);
			num++;
		}
		Widgets.EndScrollView();
	}

	private void DoModRow(Rect r, string modName, int index)
	{
		if (index % 2 == 0)
		{
			Widgets.DrawLightHighlight(r);
		}
		r.xMin += 4f;
		r.xMax -= 4f;
		Widgets.Label(r, modName);
	}

	private void HandleGoBackClicked()
	{
		SoundDefOf.Click.PlayOneShotOnCamera();
		Close();
	}

	private void HandleChangeLoadedModClicked()
	{
		SoundDefOf.Click.PlayOneShotOnCamera();
		if (Current.ProgramState == ProgramState.Entry)
		{
			ModsConfig.SetActiveToList(loadedModIdsList);
		}
		ModsConfig.SaveFromList(loadedModIdsList);
		IEnumerable<string> enumerable = from id in Enumerable.Range(0, loadedModIdsList.Count)
			where ModLister.GetModWithIdentifier(loadedModIdsList[id]) == null
			select loadedModNamesList[id];
		if (enumerable.Any())
		{
			Messages.Message(string.Format("{0}: {1}", "MissingMods".Translate(), enumerable.ToCommaList()), MessageTypeDefOf.RejectInput, historical: false);
		}
		ModsConfig.RestartFromChangedMods();
	}

	private void HandleSaveCurrentModList()
	{
		SoundDefOf.Click.PlayOneShotOnCamera();
		ModList modList = new ModList();
		modList.ids = runningModIdsList;
		modList.names = runningModNamesList;
		Find.WindowStack.Add(new Dialog_ModList_Save(modList));
	}

	private void HandleLoadAnywayClicked()
	{
		SoundDefOf.Click.PlayOneShotOnCamera();
		loadAction();
		Close();
	}

	public override void OnAcceptKeyPressed()
	{
		HandleLoadAnywayClicked();
	}
}
