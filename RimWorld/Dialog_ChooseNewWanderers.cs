using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class Dialog_ChooseNewWanderers : Window
{
	private int curPawnIndex;

	private int generationIndex;

	private const float TitleAreaHeight = 45f;

	private const float PawnEntryHeight = 60f;

	private const float SkillSummaryHeight = 127f;

	private const float CrossIconSize = 15f;

	private const float TabAreaWidth = 140f;

	private static readonly Vector2 PawnSelectorPortraitSize = new Vector2(70f, 110f);

	private static readonly Vector2 ConfirmButtonSize = new Vector2(150f, 38f);

	private const int DefaultPawnCount = 3;

	private const int MinPawnCount = 1;

	private const int MaxPawnCount = 6;

	private static readonly FloatRange ExcludeBiologicalAgeRange = new FloatRange(12.1f, 13f);

	public override Vector2 InitialSize => new Vector2(1020f, 764f);

	private static List<Pawn> StartingAndOptionalPawns => Find.GameInitData.startingAndOptionalPawns;

	private static PawnGenerationRequest DefaultStartingPawnRequest
	{
		get
		{
			PawnKindDef basicMemberKind = Faction.OfPlayer.def.basicMemberKind;
			Faction ofPlayer = Faction.OfPlayer;
			FloatRange? excludeBiologicalAgeRange = (ModsConfig.BiotechActive ? new FloatRange?(ExcludeBiologicalAgeRange) : ((FloatRange?)null));
			return new PawnGenerationRequest(basicMemberKind, ofPlayer, PawnGenerationContext.PlayerStarter, null, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 50f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: true, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, excludeBiologicalAgeRange);
		}
	}

	public Dialog_ChooseNewWanderers()
	{
		doCloseX = true;
		forcePause = true;
		absorbInputAroundWindow = true;
	}

	public override void PreOpen()
	{
		base.PreOpen();
		Current.Game.InitData = new GameInitData();
		Find.GameInitData.startingPawnCount = 3;
		StartingPawnUtility.ClearAllStartingPawns();
		generationIndex = 0;
		while (StartingAndOptionalPawns.Count < 3)
		{
			StartingPawnUtility.SetGenerationRequest(generationIndex, DefaultStartingPawnRequest);
			StartingPawnUtility.AddNewPawn(generationIndex);
			generationIndex++;
		}
		curPawnIndex = 0;
	}

	public override void DoWindowContents(Rect rect)
	{
		if (!base.IsOpen)
		{
			return;
		}
		Rect rect2 = rect;
		Text.Font = GameFont.Medium;
		Widgets.Label(new Rect(0f, 0f, rect2.width, 45f), "ChooseNewWanderers".Translate());
		Text.Font = GameFont.Small;
		rect2.yMin += 45f;
		Rect rect3 = new Rect(rect.width - ConfirmButtonSize.x, rect.height - ConfirmButtonSize.y, ConfirmButtonSize.x, ConfirmButtonSize.y);
		if (Widgets.ButtonText(rect3, "Confirm".Translate()))
		{
			IncidentParms incidentParms = new IncidentParms();
			incidentParms.target = Find.AnyPlayerHomeMap;
			incidentParms.forced = true;
			Find.Storyteller.TryFire(new FiringIncident(IncidentDefOf.GameEndedWanderersJoin, null, incidentParms));
			Find.LetterStack.RemoveLetter(Find.LetterStack.LettersListForReading.Find((Letter letter) => letter.def == LetterDefOf.GameEnded));
			Find.GameEnder.gameEnding = false;
			Find.GameEnder.newWanderersCreatedTick = Find.TickManager.TicksGame;
			Close();
			Current.Game.InitData = null;
		}
		else
		{
			rect2.yMax -= rect3.height + 10f;
			Rect rect4 = rect2;
			rect4.width = 140f;
			DrawPawnList(rect4);
			Rect rect5 = rect2;
			rect5.xMin += 150f;
			Rect rect6 = rect5.BottomPartPixels(127f);
			rect5.yMax = rect6.yMin - 10f;
			StartingPawnUtility.DrawPortraitArea(rect5, curPawnIndex, renderClothes: true, renderHeadgear: true);
			StartingPawnUtility.DrawSkillSummaries(rect6);
		}
	}

	private void DrawPawnList(Rect rect)
	{
		Rect rect2 = rect;
		rect2.height = 60f;
		rect2 = rect2.ContractedBy(4f);
		for (int i = 0; i < StartingAndOptionalPawns.Count; i++)
		{
			DoPawnRow(rect2, i);
			rect2.y += 60f;
		}
		rect2.y += 4f;
		if (StartingAndOptionalPawns.Count < 6 && Widgets.ButtonText(new Rect(rect2.x, rect2.y, rect2.width, 25f), "+"))
		{
			Find.GameInitData.startingPawnCount++;
			StartingPawnUtility.AddNewPawn(generationIndex);
			generationIndex++;
		}
	}

	private void DoPawnRow(Rect rect, int index)
	{
		Pawn pawn = StartingAndOptionalPawns[index];
		Widgets.DrawOptionBackground(rect, curPawnIndex == index);
		MouseoverSounds.DoRegion(rect);
		Widgets.BeginGroup(rect);
		Rect rect2 = rect.AtZero().ContractedBy(4f);
		GUI.color = new Color(1f, 1f, 1f, 0.2f);
		GUI.DrawTexture(new Rect(110f - PawnSelectorPortraitSize.x / 2f, 40f - PawnSelectorPortraitSize.y / 2f, PawnSelectorPortraitSize.x, PawnSelectorPortraitSize.y), PortraitsCache.Get(pawn, PawnSelectorPortraitSize, Rot4.South));
		GUI.color = Color.white;
		Widgets.Label(label: (!(pawn.Name is NameTriple nameTriple)) ? pawn.LabelShort : (string.IsNullOrEmpty(nameTriple.Nick) ? nameTriple.First : nameTriple.Nick), rect: rect2.TopPart(0.5f).Rounded());
		if (Text.CalcSize(pawn.story.TitleCap).x > rect2.width)
		{
			Widgets.Label(rect2.BottomPart(0.5f).Rounded(), pawn.story.TitleShortCap);
		}
		else
		{
			Widgets.Label(rect2.BottomPart(0.5f).Rounded(), pawn.story.TitleCap);
		}
		if (Mouse.IsOver(rect2) && StartingAndOptionalPawns.Count > 1 && Widgets.ButtonImage(new Rect(rect2.xMax - 15f, rect2.y, 15f, 15f), TexButton.Delete))
		{
			Find.GameInitData.startingPawnCount--;
			StartingAndOptionalPawns.Remove(pawn);
			curPawnIndex = Math.Min(StartingAndOptionalPawns.Count - 1, curPawnIndex);
		}
		if (Widgets.ButtonInvisible(rect2))
		{
			curPawnIndex = index;
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
		}
		Widgets.EndGroup();
	}

	public override void PostClose()
	{
		base.PostClose();
		if (Find.GameEnder.gameEnding)
		{
			StartingPawnUtility.ClearAllStartingPawns();
		}
	}
}
