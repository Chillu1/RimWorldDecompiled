using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

public class Dialog_NamePawn : Window
{
	private class NameContext
	{
		public string current;

		public TaggedString label;

		public float labelWidth;

		public int maximumNameLength;

		public float textboxWidth;

		public string textboxName;

		public bool editable;

		public int nameIndex;

		public List<string> suggestedNames;

		private List<FloatMenuOption> suggestedOptions;

		public NameContext(string label, int nameIndex, string currentName, int maximumNameLength, bool editable, List<string> suggestedNames)
		{
			current = currentName;
			this.nameIndex = nameIndex;
			this.label = label.Translate().CapitalizeFirst() + ":";
			labelWidth = Mathf.Ceil(this.label.GetWidthCached());
			this.maximumNameLength = maximumNameLength;
			textboxWidth = Mathf.Ceil(Text.CalcSize(new string('W', maximumNameLength + 2)).x);
			textboxName = label;
			this.editable = editable;
			this.suggestedNames = suggestedNames;
			if (suggestedNames == null)
			{
				return;
			}
			suggestedOptions = new List<FloatMenuOption>(suggestedNames.Count);
			foreach (string suggestedName in suggestedNames)
			{
				suggestedOptions.Add(new FloatMenuOption(suggestedName, delegate
				{
					current = suggestedName;
				}));
			}
		}

		public void MakeRow(Pawn pawn, float randomizeButtonWidth, TaggedString randomizeText, TaggedString suggestedText, ref RectDivider divider, ref string focusControlOverride)
		{
			Widgets.Label(divider.NewCol(labelWidth), label);
			RectDivider rectDivider = divider.NewCol(textboxWidth);
			if (editable)
			{
				GUI.SetNextControlName(textboxName);
				CharacterCardUtility.DoNameInputRect(rectDivider, ref current, maximumNameLength);
			}
			else
			{
				Widgets.Label(rectDivider, current);
			}
			if (!editable || nameIndex < 0)
			{
				return;
			}
			Rect rect = divider.NewCol(randomizeButtonWidth);
			if (suggestedNames != null)
			{
				List<string> list = suggestedNames;
				if (list != null && list.Count > 0 && Widgets.ButtonText(rect, suggestedText))
				{
					Find.WindowStack.Add(new FloatMenu(suggestedOptions));
				}
			}
			else if (Widgets.ButtonText(rect, randomizeText))
			{
				SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
				Name name = PawnBioAndNameGenerator.GeneratePawnName(pawn, NameStyle.Full, null, forceNoNick: false, pawn.genes?.Xenotype);
				if (name is NameTriple nameTriple)
				{
					current = nameTriple[nameIndex];
				}
				else if (name is NameSingle nameSingle)
				{
					current = nameSingle.Name;
				}
			}
		}
	}

	private Pawn pawn;

	private List<NameContext> names = new List<NameContext>(4);

	private bool firstCall = true;

	private string focusControlOverride;

	private string currentControl;

	private TaggedString descriptionText;

	private float? descriptionHeight;

	private float randomizeButtonWidth;

	private Vector2 size = new Vector2(800f, 800f);

	private float? renameHeight;

	private Rot4 portraitDirection;

	private float cameraZoom = 1f;

	private float portraitSize = 128f;

	private float humanPortraitVerticalOffset = -18f;

	private TaggedString cancelText = "Cancel".Translate().CapitalizeFirst();

	private TaggedString acceptText = "Accept".Translate().CapitalizeFirst();

	private TaggedString randomizeText;

	private TaggedString suggestedText;

	private TaggedString renameText;

	private string genderText;

	private const float ButtonHeight = 35f;

	private const float NameFieldsHeight = 30f;

	private const int MaximumNumberOfNames = 4;

	private const float VerticalMargin = 4f;

	private const float HorizontalMargin = 17f;

	private const float PortraitSize = 128f;

	private const int ContextHash = 136098329;

	private string CurPawnNick
	{
		get
		{
			if (pawn.Name is NameTriple)
			{
				return names[1].current;
			}
			if (pawn.Name is NameSingle)
			{
				return names[0].current;
			}
			throw new InvalidOperationException();
		}
		set
		{
			int num = -1;
			if (pawn.Name is NameTriple)
			{
				num = 1;
			}
			else
			{
				if (!(pawn.Name is NameSingle))
				{
					throw new InvalidOperationException();
				}
				num = 0;
			}
			names[num].current = value;
		}
	}

	private string CurPawnTitle
	{
		get
		{
			if (!(names.Last().textboxName == "BackstoryTitle"))
			{
				return null;
			}
			return names.Last().current;
		}
	}

	public override Vector2 InitialSize => size;

	private Name BuildName()
	{
		if (pawn.Name is NameTriple)
		{
			return new NameTriple(names[0].current?.Trim(), names[1].current?.Trim(), names[2].current?.Trim());
		}
		if (pawn.Name is NameSingle)
		{
			return new NameSingle(names[0].current?.Trim());
		}
		throw new InvalidOperationException();
	}

	public Dialog_NamePawn(Pawn pawn, NameFilter visibleNames, NameFilter editableNames, Dictionary<NameFilter, List<string>> suggestedNames, string initialFirstNameOverride = null, string initialNickNameOverride = null, string initialLastNameOverride = null, string initialTitleOverride = null)
	{
		this.pawn = pawn;
		if (pawn.RaceProps.Humanlike)
		{
			descriptionText = "{0}: {1}\n{2}: {3}".Formatted("Mother".Translate().CapitalizeFirst(), DescribePawn(pawn.GetMother()), "Father".Translate().CapitalizeFirst(), DescribePawn(pawn.GetFather()));
			renameText = "RenamePerson".Translate().CapitalizeFirst();
			portraitDirection = Rot4.South;
			if (pawn.story?.bodyType == BodyTypeDefOf.Hulk)
			{
				cameraZoom = 0.9f;
				humanPortraitVerticalOffset -= 8f;
			}
		}
		else
		{
			descriptionText = pawn.KindLabelIndefinite().CapitalizeFirst();
			renameText = ((ModsConfig.BiotechActive && pawn.RaceProps.IsMechanoid) ? "RenameMech" : "RenameAnimal").Translate().CapitalizeFirst();
			portraitDirection = Rot4.East;
			Vector3 extents = pawn.Drawer.renderer.BodyGraphic.MeshAt(portraitDirection).bounds.extents;
			float num = Math.Max(extents.x, extents.z);
			cameraZoom = 1f / num;
			portraitSize = Mathf.Min(128f, Mathf.Ceil(128f * num));
		}
		NameTriple nameTriple = pawn.Name as NameTriple;
		if (nameTriple != null && (visibleNames & NameFilter.First) > NameFilter.None)
		{
			names.Add(new NameContext("FirstName", 0, initialFirstNameOverride ?? nameTriple.First, 12, (editableNames & NameFilter.First) > NameFilter.None, suggestedNames?.GetWithFallback(NameFilter.First)));
		}
		if ((visibleNames & NameFilter.Nick) > NameFilter.None)
		{
			string text = ((nameTriple == null || nameTriple.NickSet || (editableNames & NameFilter.Nick) <= NameFilter.None) ? pawn.Name.ToStringShort : "");
			names.Add(new NameContext("NickName", 1, initialNickNameOverride ?? text, 16, (editableNames & NameFilter.Nick) > NameFilter.None, suggestedNames?.GetWithFallback(NameFilter.Nick)));
		}
		if (nameTriple != null && (visibleNames & NameFilter.Last) > NameFilter.None)
		{
			names.Add(new NameContext("LastName", 2, initialLastNameOverride ?? nameTriple.Last, 12, (editableNames & NameFilter.Last) > NameFilter.None, suggestedNames?.GetWithFallback(NameFilter.Last)));
		}
		if (pawn.story != null && (visibleNames & NameFilter.Title) > NameFilter.None)
		{
			names.Add(new NameContext("BackstoryTitle", -1, initialTitleOverride ?? pawn.story.title ?? "", 16, (editableNames & NameFilter.Title) > NameFilter.None, suggestedNames?.GetWithFallback(NameFilter.Title)));
		}
		float num2 = names.Max((NameContext name) => name.labelWidth);
		float num3 = names.Max((NameContext name) => name.textboxWidth);
		foreach (NameContext name in names)
		{
			name.labelWidth = num2;
			name.textboxWidth = num3;
		}
		randomizeText = "Randomize".Translate().CapitalizeFirst();
		suggestedText = "Suggested".Translate().CapitalizeFirst() + "...";
		randomizeButtonWidth = ButtonWidth(randomizeText.GetWidthCached());
		genderText = string.Format("{0}: {1}", "Gender".Translate().CapitalizeFirst(), pawn.GetGenderLabel().CapitalizeFirst());
		float x = 2f * Margin + num2 + num3 + randomizeButtonWidth + 34f;
		size = new Vector2(x, size.y);
		forcePause = true;
		absorbInputAroundWindow = true;
		closeOnClickedOutside = true;
		closeOnAccept = false;
	}

	public override void DoWindowContents(Rect inRect)
	{
		bool flag = false;
		if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
		{
			flag = true;
			Event.current.Use();
		}
		bool flag2 = false;
		bool forward = true;
		if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Tab)
		{
			flag2 = true;
			forward = !Event.current.shift;
			Event.current.Use();
		}
		if (!firstCall && Event.current.type == EventType.Layout)
		{
			currentControl = GUI.GetNameOfFocusedControl();
		}
		RectAggregator rectAggregator = new RectAggregator(new Rect(inRect.x, inRect.y, inRect.width, 0f), 136098329, new Vector2(17f, 4f));
		if (!renameHeight.HasValue)
		{
			Text.Font = GameFont.Medium;
			renameHeight = Mathf.Ceil(renameText.RawText.GetHeightCached());
			Text.Font = GameFont.Small;
		}
		descriptionHeight = descriptionHeight ?? Mathf.Ceil(Text.CalcHeight(descriptionText, rectAggregator.Rect.width - portraitSize - 17f));
		float num = renameHeight.Value + 4f + descriptionHeight.Value;
		if (!pawn.RaceProps.Humanlike && portraitSize > num)
		{
			num = portraitSize;
		}
		RectDivider rectDivider = rectAggregator.NewRow(num);
		Text.Font = GameFont.Medium;
		Pawn obj = pawn;
		Vector2 vector = new Vector2(portraitSize, portraitSize);
		Rot4 rotation = portraitDirection;
		PawnHealthState? healthStateOverride = PawnHealthState.Mobile;
		float num2 = cameraZoom;
		RenderTexture image = PortraitsCache.Get(obj, vector, rotation, default(Vector3), num2, supersample: true, compensateForUIScale: true, renderHeadgear: true, renderClothes: true, null, null, stylingStation: false, healthStateOverride);
		Rect position = rectDivider.NewCol(portraitSize);
		if (pawn.RaceProps.Humanlike)
		{
			position.y += humanPortraitVerticalOffset;
		}
		position.height = portraitSize;
		GUI.DrawTexture(position, image);
		RectDivider rectDivider2 = rectDivider.NewRow(renameHeight.Value);
		Rect rect = rectDivider2.NewCol(renameHeight.Value, HorizontalJustification.Right);
		GUI.DrawTexture(rect, pawn.gender.GetIcon());
		TooltipHandler.TipRegion(rect, genderText);
		Widgets.Label(rectDivider2, renameText);
		Text.Font = GameFont.Small;
		Widgets.Label(rectDivider.NewRow(descriptionHeight.Value), descriptionText);
		Text.Anchor = TextAnchor.MiddleLeft;
		foreach (NameContext name2 in names)
		{
			RectDivider divider = rectAggregator.NewRow(30f);
			name2.MakeRow(pawn, randomizeButtonWidth, randomizeText, suggestedText, ref divider, ref focusControlOverride);
		}
		Text.Anchor = TextAnchor.UpperLeft;
		rectAggregator.NewRow(17.5f);
		RectDivider rectDivider3 = rectAggregator.NewRow(35f);
		float width = Mathf.Floor((rectDivider3.Rect.width - 17f) / 2f);
		if (Widgets.ButtonText(rectDivider3.NewCol(width), cancelText))
		{
			Close();
		}
		if (Widgets.ButtonText(rectDivider3.NewCol(width), acceptText) || flag)
		{
			Name name = BuildName();
			if (!name.IsValid)
			{
				Messages.Message("NameInvalid".Translate(), pawn, MessageTypeDefOf.NeutralEvent, historical: false);
			}
			else
			{
				pawn.Name = name;
				if (pawn.story != null)
				{
					pawn.story.Title = CurPawnTitle;
				}
				Find.WindowStack.TryRemove(this);
				string text = (pawn.def.race.Animal ? ((string)"AnimalGainsName".Translate(CurPawnNick)) : (pawn.def.race.IsMechanoid ? ((string)"MechGainsName".Translate(CurPawnNick)) : ((!(name is NameTriple nameTriple)) ? ((string)"PawnGainsName".Translate(CurPawnNick, pawn.story.Title, pawn.Named("PAWN")).AdjustedFor(pawn)) : ((string)"PawnGainsName".Translate(nameTriple.Nick, pawn.story.Title, pawn.Named("PAWN")).AdjustedFor(pawn)))));
				Messages.Message(text, pawn, MessageTypeDefOf.PositiveEvent, historical: false);
				pawn.babyNamingDeadline = -1;
			}
		}
		size = new Vector2(size.x, Mathf.Ceil(size.y + (rectAggregator.Rect.height - inRect.height)));
		SetInitialSizeAndPosition();
		if (flag2 || firstCall)
		{
			FocusNextControl(currentControl, forward);
			firstCall = false;
		}
		if (Event.current.type == EventType.Layout && !string.IsNullOrEmpty(focusControlOverride))
		{
			GUI.FocusControl(focusControlOverride);
			focusControlOverride = null;
		}
	}

	private void FocusNextControl(string currentControl, bool forward)
	{
		int num = names.FindIndex((NameContext name) => name.textboxName == currentControl);
		int num2 = -1;
		if (forward)
		{
			for (int num3 = 1; num3 <= names.Count; num3++)
			{
				int num4 = (num + num3) % names.Count;
				if (names[num4].editable)
				{
					num2 = num4;
					break;
				}
			}
		}
		else
		{
			for (int num5 = 1; num5 <= names.Count; num5++)
			{
				int num6 = (names.Count + num - num5) % names.Count;
				if (names[num6].editable)
				{
					num2 = num6;
					break;
				}
			}
		}
		if (num2 >= 0)
		{
			focusControlOverride = names[num2].textboxName;
		}
	}

	private TaggedString DescribePawn(Pawn pawn)
	{
		if (pawn != null)
		{
			return pawn.FactionDesc(pawn.NameFullColored, extraFactionsInfo: false, pawn.NameFullColored, pawn.gender.GetLabel(pawn.RaceProps.Animal)).Resolve();
		}
		return "Unknown".Translate().Colorize(Color.gray);
	}

	private static float ButtonWidth(float textWidth)
	{
		return Math.Max(114f, textWidth + 35f);
	}
}
