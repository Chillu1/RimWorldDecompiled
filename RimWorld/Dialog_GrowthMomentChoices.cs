using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Dialog_GrowthMomentChoices : Window
{
	private ChoiceLetter_GrowthMoment letter;

	private List<SkillDef> chosenPassions = new List<SkillDef>();

	private Trait chosenTrait;

	private TaggedString text;

	private bool showBio = true;

	private float scrollHeight;

	private Vector2 scrollPosition;

	private const float WidthWithTabs = 1000f;

	private const float WidthRegular = 480f;

	private const float PassionListingWidth = 230f;

	private const float PassionIconSize = 24f;

	private const float OptionTabIn = 30f;

	private static List<TabRecord> tmpTabs = new List<TabRecord>();

	private SkillDef SinglePassionChoice
	{
		get
		{
			if (chosenPassions.Count == 1)
			{
				return chosenPassions[0];
			}
			return null;
		}
	}

	private float Height => CharacterCardUtility.PawnCardSize(letter.pawn).y + Window.CloseButSize.y + 4f + Margin * 2f;

	public override Vector2 InitialSize
	{
		get
		{
			if (!letter.ShowInfoTabs)
			{
				return new Vector2(480f, Height);
			}
			return new Vector2(1000f, Height);
		}
	}

	public Dialog_GrowthMomentChoices(TaggedString text, ChoiceLetter_GrowthMoment letter)
	{
		this.text = text;
		this.letter = letter;
		forcePause = true;
		absorbInputAroundWindow = true;
		if (!SelectionsMade())
		{
			closeOnAccept = false;
			closeOnCancel = false;
		}
	}

	public override void DoWindowContents(Rect inRect)
	{
		bool showInfoTabs = letter.ShowInfoTabs;
		float width = 446f;
		Rect outRect = (showInfoTabs ? inRect.LeftPartPixels(width) : inRect);
		outRect.yMax -= 4f + Window.CloseButSize.y;
		Text.Font = GameFont.Small;
		Rect viewRect = new Rect(outRect.x, outRect.y, outRect.width - 16f, scrollHeight);
		Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
		float curY = 0f;
		Widgets.Label(0f, ref curY, viewRect.width, text.Resolve());
		curY += 14f;
		DrawPassionChoices(viewRect.width, ref curY);
		DrawTraitChoices(viewRect.width, ref curY);
		DrawBottomText(viewRect.width, ref curY);
		if (Event.current.type == EventType.Layout)
		{
			scrollHeight = Mathf.Max(curY, outRect.height);
		}
		Widgets.EndScrollView();
		Rect rect = new Rect(0f, outRect.yMax + 4f, inRect.width, Window.CloseButSize.y);
		AcceptanceReport acceptanceReport = CanClose();
		Rect rect2 = new Rect(rect.xMax - Window.CloseButSize.x, rect.y, Window.CloseButSize.x, Window.CloseButSize.y);
		if (letter.ArchiveView)
		{
			rect2.x = rect.center.x - Window.CloseButSize.x / 2f;
		}
		else
		{
			if (Widgets.ButtonText(new Rect(rect.x, rect.y, Window.CloseButSize.x, Window.CloseButSize.y), "Later".Translate()))
			{
				if (letter.ShouldAutomaticallyOpenLetter)
				{
					Messages.Message("MessageCannotPostponeGrowthMoment".Translate(letter.pawn.Named("PAWN")), null, MessageTypeDefOf.RejectInput, historical: false);
				}
				else
				{
					Close();
				}
			}
			if (!acceptanceReport.Accepted)
			{
				TextAnchor anchor = Text.Anchor;
				GameFont font = Text.Font;
				Text.Font = GameFont.Tiny;
				Text.Anchor = TextAnchor.MiddleRight;
				Rect rect3 = rect;
				rect3.xMax = rect2.xMin - 4f;
				Widgets.Label(rect3, acceptanceReport.Reason.Colorize(ColoredText.WarningColor));
				Text.Font = font;
				Text.Anchor = anchor;
			}
		}
		if (Widgets.ButtonText(rect2, "OK".Translate()))
		{
			if (acceptanceReport.Accepted)
			{
				letter.MakeChoices(chosenPassions, chosenTrait);
				Close();
				Find.LetterStack.RemoveLetter(letter);
			}
			else
			{
				Messages.Message(acceptanceReport.Reason, null, MessageTypeDefOf.RejectInput, historical: false);
			}
		}
		if (showInfoTabs)
		{
			Rect rect4 = inRect.RightPartPixels(1000f - outRect.width - 34f);
			rect4.xMin += 17f;
			rect4.yMax -= 4f + Window.CloseButSize.y;
			tmpTabs.Clear();
			tmpTabs.Add(new TabRecord("TabCharacter".Translate(), delegate
			{
				showBio = true;
			}, showBio));
			tmpTabs.Add(new TabRecord("TabHealth".Translate(), delegate
			{
				showBio = false;
			}, !showBio));
			rect4.yMin += 32f;
			Rect rect5 = new Rect(rect4.x + (showBio ? 17f : 0f), rect4.y, rect4.width, rect4.height);
			Widgets.DrawMenuSection(rect4);
			if (showBio)
			{
				CharacterCardUtility.DrawCharacterCard(rect5, letter.pawn, null, default(Rect), showName: false);
			}
			else
			{
				HealthCardUtility.DrawHediffListing(rect5, letter.pawn, showBloodLoss: false, 17f);
			}
			TabDrawer.DrawTabs(rect4, tmpTabs);
			tmpTabs.Clear();
		}
	}

	private bool SelectionsMade()
	{
		if (letter.ArchiveView)
		{
			return true;
		}
		if (!letter.passionChoices.NullOrEmpty() && chosenPassions.NullOrEmpty() && letter.passionGainsCount > 0)
		{
			return false;
		}
		if (!letter.traitChoices.NullOrEmpty() && chosenTrait == null)
		{
			return false;
		}
		return true;
	}

	private AcceptanceReport CanClose()
	{
		if (letter.ArchiveView)
		{
			return true;
		}
		if (!letter.passionChoices.NullOrEmpty() && chosenPassions.Count != letter.passionGainsCount)
		{
			if (letter.passionGainsCount == 1)
			{
				return "SelectPassionSingular".Translate();
			}
			return "SelectPassionsPlural".Translate(letter.passionGainsCount);
		}
		if (!letter.traitChoices.NullOrEmpty() && chosenTrait == null)
		{
			return "SelectATrait".Translate();
		}
		if (!SelectionsMade())
		{
			return "BirthdayMakeChoices".Translate();
		}
		return AcceptanceReport.WasAccepted;
	}

	private void DrawTraitChoices(float width, ref float curY)
	{
		if (!letter.ArchiveView && !letter.traitChoices.NullOrEmpty())
		{
			Widgets.Label(0f, ref curY, width, "BirthdayPickTrait".Translate(letter.pawn).Resolve() + ":");
			Listing_Standard listing_Standard = new Listing_Standard();
			Rect rect = new Rect(0f, curY, 230f, 99999f);
			listing_Standard.Begin(rect);
			foreach (Trait traitChoice in letter.traitChoices)
			{
				if (listing_Standard.RadioButton(traitChoice.LabelCap, chosenTrait == traitChoice, 30f, traitChoice.TipString(letter.pawn)))
				{
					chosenTrait = traitChoice;
				}
			}
			if (letter.noTraitOptionShown)
			{
				TaggedString taggedString = "BirthdayNoTraitChoice".Translate();
				TaggedString taggedString2 = "BirthdayNoTraitChoiceTooltip".Translate(letter.pawn);
				if (listing_Standard.RadioButton(taggedString, chosenTrait == ChoiceLetter_GrowthMoment.NoTrait, 30f, taggedString2))
				{
					chosenTrait = ChoiceLetter_GrowthMoment.NoTrait;
				}
			}
			listing_Standard.End();
			curY += listing_Standard.CurHeight + 10f + 4f;
		}
		if (letter.ArchiveView && letter.chosenTrait != null)
		{
			Widgets.Label(0f, ref curY, width, "BirthdayTraitArchive".Translate(letter.chosenTrait.Label.Colorize(ColorLibrary.LightBlue)));
			curY += 14f;
		}
	}

	private void DrawPassionChoices(float width, ref float curY)
	{
		if (!letter.ArchiveView && !letter.passionChoices.NullOrEmpty() && letter.passionGainsCount > 0)
		{
			Widgets.Label(0f, ref curY, width, ((letter.passionGainsCount == 1) ? "BirthdayPickPassion".Translate(letter.pawn) : "BirthdayPickPassions".Translate(letter.pawn, letter.passionGainsCount)).Resolve() + ":");
			Listing_Standard listing_Standard = new Listing_Standard();
			Rect rect = new Rect(0f, curY, 230f, 99999f);
			listing_Standard.Begin(rect);
			foreach (SkillDef passionChoice in letter.passionChoices)
			{
				SkillRecord skill = letter.pawn.skills.GetSkill(passionChoice);
				Passion passion = (chosenPassions.Contains(passionChoice) ? skill.passion.IncrementPassion() : skill.passion);
				if ((int)passion > 0)
				{
					Texture2D image = ((passion == Passion.Major) ? SkillUI.PassionMajorIcon : SkillUI.PassionMinorIcon);
					GUI.DrawTexture(new Rect(rect.xMax - 55f, listing_Standard.CurHeight, 24f, 24f), image);
				}
				if (letter.passionGainsCount > 1)
				{
					bool checkOn = chosenPassions.Contains(passionChoice);
					bool flag = checkOn;
					listing_Standard.CheckboxLabeled(passionChoice.LabelCap, ref checkOn, 30f);
					if (checkOn != flag)
					{
						if (checkOn)
						{
							chosenPassions.Add(passionChoice);
						}
						else
						{
							chosenPassions.Remove(passionChoice);
						}
					}
				}
				else if (listing_Standard.RadioButton(passionChoice.LabelCap, SinglePassionChoice == passionChoice, 30f))
				{
					chosenPassions.Clear();
					chosenPassions.Add(passionChoice);
				}
			}
			listing_Standard.End();
			curY += listing_Standard.CurHeight + 10f + 4f;
		}
		if (letter.ArchiveView && !letter.chosenPassions.NullOrEmpty())
		{
			Widgets.Label(0f, ref curY, width, "BirthdayPassionArchive".Translate(letter.chosenPassions.Select((SkillDef x) => x.label).ToCommaList(useAnd: true).Colorize(ColorLibrary.LightBlue)));
			curY += 14f;
		}
	}

	private void DrawBottomText(float width, ref float curY)
	{
		if (letter.growthTier >= 0 && (!letter.passionChoices.NullOrEmpty() || !letter.traitChoices.NullOrEmpty()))
		{
			string text = "BirthdayGrowthTier".Translate(letter.pawn, letter.growthTier).Colorize(ColoredText.SubtleGrayColor);
			if (letter.pawn.Name != letter.oldName)
			{
				text = "BirthdayNickname".Translate(letter.oldName.ToStringFull.Colorize(ColoredText.NameColor), letter.pawn.LabelShort.Colorize(ColoredText.NameColor)).Resolve() + "\n\n" + text;
			}
			Widgets.Label(0f, ref curY, width, text);
			curY += 10f;
		}
	}
}
