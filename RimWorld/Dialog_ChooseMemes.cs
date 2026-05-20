using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class Dialog_ChooseMemes : Window
{
	private Ideo ideo;

	private Vector2 scrollPos;

	private float viewHeight;

	private List<MemeDef> newMemes = new List<MemeDef>();

	private bool initialSelection;

	private Action done;

	private MemeCategory memeCategory;

	private bool reformingIdeo;

	private static readonly Vector2 ButSize = new Vector2(150f, 38f);

	private const float Width = 980f;

	private const float Height = 724f;

	private const int MaxMemesToRemoveForFluidIdeo = 1;

	private List<Precept> tmpPreceptsToRemove = new List<Precept>();

	private List<MemeDef> tmpPreviousMemes = new List<MemeDef>();

	private static List<MemeDef> memesInCategory = new List<MemeDef>();

	private static Dictionary<MemeGroupDef, List<MemeDef>> memeGroups = new Dictionary<MemeGroupDef, List<MemeDef>>();

	private static readonly List<MemeDef> memesCurrentRow = new List<MemeDef>();

	private static readonly List<MemeDef> memesInGroup = new List<MemeDef>();

	private static readonly IComparer<MemeDef> NormalMemeSorter = GenCollection.CompareBy((MemeDef x) => x.groupDef?.renderOrder ?? int.MaxValue).ThenBy(GenCollection.CompareBy((MemeDef x) => x.renderOrder));

	public override Vector2 InitialSize => new Vector2(980f, 724f);

	private bool ConfiguringNewFluidIdeo
	{
		get
		{
			if (ideo.Fluid)
			{
				return !reformingIdeo;
			}
			return false;
		}
	}

	private bool ReformingFluidIdeo
	{
		get
		{
			if (ideo.Fluid)
			{
				return reformingIdeo;
			}
			return false;
		}
	}

	private IntRange MemeCountRangeAbsolute
	{
		get
		{
			if (ConfiguringNewFluidIdeo)
			{
				return IdeoFoundation.MemeCountRangeFluidAbsolute;
			}
			if (ReformingFluidIdeo)
			{
				int num = ideo.memes.Where((MemeDef m) => m.category == MemeCategory.Normal).Count();
				return new IntRange(num - 1, Mathf.Min(num + 1, IdeoFoundation.MemeCountRangeAbsolute.max));
			}
			return IdeoFoundation.MemeCountRangeAbsolute;
		}
	}

	private int NormalMemesRemoveCount
	{
		get
		{
			int num = 0;
			for (int i = 0; i < ideo.memes.Count; i++)
			{
				if (ideo.memes[i].category == MemeCategory.Normal && !newMemes.Contains(ideo.memes[i]))
				{
					num++;
				}
			}
			return num;
		}
	}

	public Dialog_ChooseMemes(Ideo ideo, MemeCategory memeCategory, bool initialSelection = false, Action done = null, List<MemeDef> preSelectedMemes = null, bool reformingIdeo = false)
	{
		this.ideo = ideo;
		this.memeCategory = memeCategory;
		this.initialSelection = initialSelection;
		this.done = done;
		closeOnCancel = ideo.memes.Any((MemeDef m) => m.category == MemeCategory.Normal);
		this.reformingIdeo = reformingIdeo;
		absorbInputAroundWindow = true;
		newMemes.Clear();
		newMemes.AddRange(ideo.memes);
		if (preSelectedMemes == null)
		{
			return;
		}
		foreach (MemeDef preSelectedMeme in preSelectedMemes)
		{
			if (!newMemes.Contains(preSelectedMeme))
			{
				newMemes.Add(preSelectedMeme);
			}
		}
	}

	private int RandomGeneratedMemeCount()
	{
		return GenMath.RoundRandom(MemeCountRangeAbsolute.Average);
	}

	public override void DoWindowContents(Rect rect)
	{
		Rect outRect = rect;
		outRect.height -= ButSize.y;
		string label = ((memeCategory == MemeCategory.Structure) ? ((string)"ChooseStructure".Translate()) : ((!ConfiguringNewFluidIdeo) ? ((string)"ChooseMemes".Translate()) : ((string)"ChooseStartingMeme".Translate())));
		Text.Font = GameFont.Medium;
		Widgets.Label(new Rect(outRect.x, outRect.y, rect.width, 30f), label);
		Text.Font = GameFont.Small;
		outRect.yMin += 30f;
		Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);
		Widgets.BeginScrollView(outRect, ref scrollPos, viewRect);
		float curY = 0f;
		if (memeCategory == MemeCategory.Structure)
		{
			DoMemeSelector(viewRect, MemeCategory.Structure, ref curY);
		}
		else if (memeCategory == MemeCategory.Normal)
		{
			DoMemeSelector(viewRect, MemeCategory.Normal, ref curY);
		}
		viewHeight = Mathf.Max(viewHeight, curY);
		Widgets.EndScrollView();
		if (Widgets.ButtonText(new Rect(0f, rect.height - ButSize.y, ButSize.x, ButSize.y), "Back".Translate()))
		{
			Close();
			if (memeCategory == MemeCategory.Structure)
			{
				NotifyConfigureIdeoPage();
			}
			else if (memeCategory == MemeCategory.Normal)
			{
				if (initialSelection)
				{
					Find.WindowStack.Add(new Dialog_ChooseMemes(ideo, MemeCategory.Structure, initialSelection));
				}
				else
				{
					NotifyConfigureIdeoPage();
				}
			}
		}
		if (Widgets.ButtonText(new Rect((int)(rect.width - ButSize.x) / 2, rect.height - ButSize.y, ButSize.x, ButSize.y), "Randomize".Translate()))
		{
			SoundDefOf.Tick_High.PlayOneShotOnCamera();
			FactionDef forFaction = IdeoUIUtility.FactionForRandomization(ideo);
			if (memeCategory == MemeCategory.Normal)
			{
				if (ReformingFluidIdeo)
				{
					newMemes = IdeoUtility.RandomizeNormalMemesForReforming(MemeCountRangeAbsolute.max, ideo.memes, forFaction);
				}
				else
				{
					newMemes = IdeoUtility.RandomizeNormalMemes(RandomGeneratedMemeCount(), newMemes, forFaction, ConfiguringNewFluidIdeo);
				}
			}
			else if (memeCategory == MemeCategory.Structure)
			{
				newMemes = IdeoUtility.RandomizeStructureMeme(newMemes, forFaction);
			}
		}
		Rect rect2 = new Rect(rect.width - ButSize.x, rect.height - ButSize.y, ButSize.x, ButSize.y);
		if (Widgets.ButtonText(rect2, "DoneButton".Translate()))
		{
			TryAccept();
		}
		string text = null;
		Pair<MemeDef, MemeDef> firstIncompatibleMemePair = GetFirstIncompatibleMemePair();
		if (GetMemeCount(MemeCategory.Structure) < 1 || (memeCategory == MemeCategory.Normal && (GetMemeCount(MemeCategory.Normal) < MemeCountRangeAbsolute.min || GetMemeCount(MemeCategory.Normal) > MemeCountRangeAbsolute.max)) || firstIncompatibleMemePair != default(Pair<MemeDef, MemeDef>))
		{
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.MiddleRight;
			GUI.color = Color.red;
			text = ((firstIncompatibleMemePair != default(Pair<MemeDef, MemeDef>)) ? ((string)"IncompatibleMemes".Translate(firstIncompatibleMemePair.First, firstIncompatibleMemePair.Second).CapitalizeFirst()) : ((memeCategory != MemeCategory.Normal) ? ((string)"ChooseStructureMeme".Translate()) : ((GetMemeCount(MemeCategory.Normal) >= MemeCountRangeAbsolute.min) ? ((string)"TooManyMemes".Translate(MemeCountRangeAbsolute.max)) : ((string)(ConfiguringNewFluidIdeo ? "NotEnoughMemesFluidIdeo".Translate(MemeCountRangeAbsolute.min) : "NotEnoughMemes".Translate(MemeCountRangeAbsolute.min))))));
		}
		Rect rect3 = new Rect(rect.xMax - ButSize.x - 240f - 6f, rect2.y, 240f, ButSize.y);
		if (text != null)
		{
			Widgets.Label(rect3, text);
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
		}
		else if (memeCategory == MemeCategory.Normal)
		{
			IdeoUIUtility.DrawImpactInfo(rect3, newMemes);
		}
	}

	private void NotifyConfigureIdeoPage()
	{
		Find.WindowStack.WindowOfType<Page_ConfigureIdeo>()?.Notify_ClosedChooseMemesDialog();
	}

	private void TryAccept()
	{
		Pair<MemeDef, MemeDef> firstIncompatibleMemePair = GetFirstIncompatibleMemePair();
		if (firstIncompatibleMemePair != default(Pair<MemeDef, MemeDef>))
		{
			Messages.Message("MessageIncompatibleMemes".Translate(firstIncompatibleMemePair.First, firstIncompatibleMemePair.Second), MessageTypeDefOf.RejectInput, historical: false);
			return;
		}
		if (memeCategory == MemeCategory.Normal)
		{
			if (GetMemeCount(MemeCategory.Normal) < MemeCountRangeAbsolute.min)
			{
				Messages.Message("MessageNotEnoughMemes".Translate(MemeCountRangeAbsolute.min), MessageTypeDefOf.RejectInput, historical: false);
				return;
			}
			if (GetMemeCount(MemeCategory.Normal) > MemeCountRangeAbsolute.max)
			{
				Messages.Message("MessageTooManyMemes".Translate(MemeCountRangeAbsolute.max), MessageTypeDefOf.RejectInput, historical: false);
				return;
			}
		}
		else if (memeCategory == MemeCategory.Structure && GetMemeCount(MemeCategory.Structure) < 1)
		{
			Messages.Message("MessageNotEnoughStructureMemes".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			return;
		}
		if (!initialSelection && ideo.PreceptsListForReading.Count > 0 && !newMemes.SetsEqual(ideo.memes))
		{
			if (ReformingFluidIdeo)
			{
				IdeoUIUtility.FactionForRandomization(ideo);
				tmpPreceptsToRemove.Clear();
				tmpPreceptsToRemove.AddRange(ideo.foundation.GetPreceptsToRemoveFromMemeChanges(ideo.memes, newMemes));
				if (tmpPreceptsToRemove.Count > 0)
				{
					TaggedString text = "ChangesRemovePrecepts".Translate((from p in tmpPreceptsToRemove
						where p.def.visible
						select p.def.issue.LabelCap.Resolve() + ": " + p.def.LabelCap.Resolve()).ToLineList("  - "));
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, DoAcceptChanges));
				}
				else
				{
					DoAcceptChanges();
				}
				tmpPreceptsToRemove.Clear();
			}
			else
			{
				TaggedString taggedString = "Changing".Translate((memeCategory == MemeCategory.Normal) ? "MemesLower".Translate() : "StructuresLower".Translate());
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ChangesRandomizePrecepts".Translate(taggedString), DoAcceptChanges));
			}
		}
		else
		{
			DoAcceptChanges();
		}
	}

	private void DoAcceptChanges()
	{
		if (!newMemes.SetsEqual(ideo.memes))
		{
			FactionDef forFaction = IdeoUIUtility.FactionForRandomization(ideo);
			if (ReformingFluidIdeo)
			{
				tmpPreviousMemes.Clear();
				tmpPreviousMemes.AddRange(ideo.memes);
				ideo.memes.Clear();
				ideo.memes.AddRange(newMemes);
				ideo.SortMemesInDisplayOrder();
				ideo.foundation.EnsurePreceptsCompatibleWithMemes(tmpPreviousMemes, ideo.memes, new IdeoGenerationParms(forFaction));
				ideo.RecachePrecepts();
				tmpPreviousMemes.Clear();
			}
			else
			{
				ideo.memes.Clear();
				ideo.memes.AddRange(newMemes);
				ideo.SortMemesInDisplayOrder();
				if (ideo.PreceptsListForReading.Any())
				{
					ideo.foundation.RandomizePrecepts(init: true, new IdeoGenerationParms(forFaction));
					if (ideo.foundation is IdeoFoundation_Deity ideoFoundation_Deity && memeCategory == MemeCategory.Structure)
					{
						ideoFoundation_Deity.GenerateDeities();
					}
				}
				else
				{
					ideo.foundation.RandomizeCulture(new IdeoGenerationParms(forFaction));
					ideo.foundation.RandomizePlace();
					if (ideo.foundation is IdeoFoundation_Deity ideoFoundation_Deity2)
					{
						ideoFoundation_Deity2.GenerateDeities();
					}
					ideo.foundation.GenerateTextSymbols();
					ideo.foundation.RandomizePrecepts(init: false, new IdeoGenerationParms(forFaction));
					ideo.foundation.GenerateLeaderTitle();
					ideo.foundation.RandomizeIcon();
					ideo.foundation.InitPrecepts(new IdeoGenerationParms(forFaction));
					ideo.RecachePrecepts();
				}
				ideo.RegenerateDescription(force: true);
				ideo.foundation.RandomizeStyles();
				ideo.style.RecalculateAvailableStyleItems();
			}
		}
		if (memeCategory == MemeCategory.Structure && !ideo.memes.Any((MemeDef x) => x.category == MemeCategory.Normal))
		{
			Find.WindowStack.Add(new Dialog_ChooseMemes(ideo, MemeCategory.Normal, initialSelection: true, done));
		}
		else if (done != null)
		{
			done();
		}
		Close();
	}

	public override void OnAcceptKeyPressed()
	{
		TryAccept();
	}

	private Pair<MemeDef, MemeDef> GetFirstIncompatibleMemePair()
	{
		for (int i = 0; i < newMemes.Count; i++)
		{
			for (int j = i + 1; j < newMemes.Count; j++)
			{
				for (int k = 0; k < newMemes[i].exclusionTags.Count; k++)
				{
					if (newMemes[j].exclusionTags.Contains(newMemes[i].exclusionTags[k]))
					{
						return new Pair<MemeDef, MemeDef>(newMemes[i], newMemes[j]);
					}
				}
			}
		}
		return default(Pair<MemeDef, MemeDef>);
	}

	private void DoMemeSelector(Rect viewRect, MemeCategory category, ref float curY)
	{
		curY += 17f;
		string text;
		if (category == MemeCategory.Structure)
		{
			text = "ChooseStructureMemesInfo".Translate();
		}
		else if (ConfiguringNewFluidIdeo)
		{
			text = "ChooseNormalMemesFluidIdeoInfo".Translate(MemeCountRangeAbsolute.min);
		}
		else
		{
			text = ((!ReformingFluidIdeo) ? ((string)"ChooseNormalMemesInfo".Translate(MemeCountRangeAbsolute.min, MemeCountRangeAbsolute.max)) : ((string)"ChooseOrRemoveMeme".Translate()));
			text += " " + "SomeMemesHaveMoreImpact".Translate();
		}
		Widgets.Label(viewRect.x, ref curY, viewRect.width, text);
		curY += 27f;
		memesInCategory.Clear();
		foreach (MemeDef item in DefDatabase<MemeDef>.AllDefsListForReading)
		{
			if (item.category == category && CanUseMeme(item))
			{
				memesInCategory.Add(item);
			}
		}
		if (category == MemeCategory.Structure)
		{
			DoStructureMemeSelector(viewRect, ref curY, memesInCategory);
		}
		else
		{
			DoNormalMemeSelector(viewRect, ref curY, memesInCategory);
		}
	}

	private void DoStructureMemeSelector(Rect viewRect, ref float curY, List<MemeDef> memes)
	{
		memes.SortBy((MemeDef x) => x.groupDef != null, (MemeDef x) => x.renderOrder);
		float num = curY;
		int num2 = Mathf.FloorToInt(viewRect.width / (IdeoUIUtility.MemeBoxSize.x + 8f));
		float num3 = 8f;
		int num4 = memes.Count((MemeDef x) => x.groupDef == null);
		if (num4 < num2)
		{
			num3 = (viewRect.width - IdeoUIUtility.MemeBoxSize.x * (float)(num4 + 1)) / (float)num4;
		}
		memeGroups.Clear();
		for (int num5 = 0; num5 < memes.Count; num5++)
		{
			MemeDef memeDef = memes[num5];
			if (memeDef.groupDef != null)
			{
				if (memeGroups.ContainsKey(memeDef.groupDef))
				{
					memeGroups[memeDef.groupDef].Add(memeDef);
					continue;
				}
				memeGroups.Add(memeDef.groupDef, new List<MemeDef> { memeDef });
			}
			else
			{
				int num6 = num5 / num2;
				int num7 = num5 % num2;
				int num8 = ((num5 >= num4 - num4 % num2) ? (num4 % num2) : num2);
				float num9 = (viewRect.width - (float)num8 * IdeoUIUtility.MemeBoxSize.x - (float)(num8 - 1) * num3) / 2f;
				Rect memeBox = new Rect(num9 + (float)num7 * IdeoUIUtility.MemeBoxSize.x + (float)num7 * num3, curY + (float)num6 * IdeoUIUtility.MemeBoxSize.y + (float)num6 * num3, IdeoUIUtility.MemeBoxSize.x, IdeoUIUtility.MemeBoxSize.y);
				DrawMeme(memeDef, memeBox, drawHighlight: false);
				num = Mathf.Max(num, memeBox.yMax);
			}
		}
		if (memeGroups.Any())
		{
			if (num4 > 0)
			{
				num += num3;
			}
			float num10 = num;
			foreach (KeyValuePair<MemeGroupDef, List<MemeDef>> memeGroup in memeGroups)
			{
				num = Mathf.Max(num, DrawMemeGroup(memeGroup.Value, new Vector2(viewRect.x + viewRect.width * memeGroup.Key.drawOffset.x, viewRect.y + num10 + viewRect.height * memeGroup.Key.drawOffset.y), memeGroup.Key.maxRows).yMax);
			}
		}
		memeGroups.Clear();
		GUI.color = Color.white;
		curY = num;
	}

	private void DoNormalMemeSelector(Rect viewRect, ref float curY, List<MemeDef> memes)
	{
		memes.Sort(NormalMemeSorter);
		float gapBetweenBoxes = 8f;
		int num = Mathf.FloorToInt(viewRect.width / (IdeoUIUtility.MemeBoxSize.x + gapBetweenBoxes));
		int impact;
		for (impact = 1; impact <= 3; impact++)
		{
			int num2 = 0;
			foreach (MemeDef meme in memes)
			{
				if (meme.impact == impact)
				{
					num2++;
				}
			}
			if (num2 == 0)
			{
				continue;
			}
			int num3 = ((num2 <= num) ? num : Mathf.Min(Mathf.CeilToInt((float)num2 / 2f), num));
			Rect rect = new Rect(viewRect.x, curY, viewRect.width, 30f);
			Widgets.Label(rect, "IdeoImpact".Translate() + ": " + IdeoImpactUtility.MemeImpactLabel(impact).CapitalizeFirst());
			curY = rect.yMax;
			GUI.color = Color.gray;
			Widgets.DrawLineHorizontal(rect.x, curY - 7f, rect.width);
			GUI.color = Color.white;
			memesCurrentRow.Clear();
			foreach (IGrouping<MemeGroupDef, MemeDef> item in from x in memes
				where x.impact == impact
				group x by x.groupDef)
			{
				memesInGroup.Clear();
				memesInGroup.AddRange(item);
				if (memesCurrentRow.Count + memesInGroup.Count > num3)
				{
					DrawRow(ref curY);
				}
				foreach (MemeDef item2 in memesInGroup)
				{
					if (memesCurrentRow.Count >= num3)
					{
						DrawRow(ref curY);
					}
					memesCurrentRow.Add(item2);
				}
			}
			DrawRow(ref curY);
		}
		void DrawRow(ref float y)
		{
			if (memesCurrentRow.Count != 0)
			{
				float num4 = (viewRect.width - (float)memesCurrentRow.Count * (IdeoUIUtility.MemeBoxSize.x + gapBetweenBoxes)) / 2f;
				MemeGroupDef memeGroupDef = null;
				for (int i = 0; i < memesCurrentRow.Count; i++)
				{
					MemeDef memeDef = memesCurrentRow[i];
					if (i == 0)
					{
						memeGroupDef = memeDef.groupDef;
					}
					else if (memeGroupDef != memeDef.groupDef || (memeDef.groupDef != null && memeDef.groupDef.renderWithGap))
					{
						memeGroupDef = memeDef.groupDef;
						num4 += gapBetweenBoxes;
					}
					Rect memeBox = new Rect(viewRect.x + num4, y, IdeoUIUtility.MemeBoxSize.x, IdeoUIUtility.MemeBoxSize.y).Rounded();
					DrawMeme(memeDef, memeBox, drawHighlight: false);
					num4 = memeBox.xMax;
				}
				memesCurrentRow.Clear();
				y += gapBetweenBoxes + IdeoUIUtility.MemeBoxSize.y;
			}
		}
	}

	private Rect DrawMemeGroup(List<MemeDef> memes, Vector2 offset, int maxRows)
	{
		int num = Mathf.CeilToInt((float)memes.Count / (float)maxRows);
		int num2 = Mathf.CeilToInt((float)memes.Count / (float)num);
		Rect rect = new Rect(offset.x, offset.y, (float)num * IdeoUIUtility.MemeBoxSize.x, (float)num2 * IdeoUIUtility.MemeBoxSize.y);
		GUI.color = new Color(1f, 1f, 1f, 0.5f);
		Widgets.DrawHighlight(rect);
		GUI.color = Color.white;
		bool flag = memes.Count % 2 != 0 && maxRows > 1;
		for (int i = 0; i < memes.Count; i++)
		{
			int num3 = i;
			if (flag && (float)i / (float)num > 0f)
			{
				num3++;
			}
			float num4 = offset.x + (float)(num3 % num) * IdeoUIUtility.MemeBoxSize.x;
			if (num3 / num == 0 && flag)
			{
				num4 += IdeoUIUtility.MemeBoxSize.x / 2f;
			}
			DrawMeme(memes[i], new Rect(num4, offset.y + (float)(num3 / num) * IdeoUIUtility.MemeBoxSize.y, IdeoUIUtility.MemeBoxSize.x, IdeoUIUtility.MemeBoxSize.y), drawHighlight: false);
		}
		return rect;
	}

	private void DrawMeme(MemeDef meme, Rect memeBox, bool drawHighlight = true)
	{
		if (newMemes.Contains(meme))
		{
			if (!CanRemoveMeme(meme).Accepted)
			{
				GUI.color = (ReformingFluidIdeo ? Color.gray : Color.red);
			}
			Widgets.DrawBox(memeBox);
			GUI.color = Color.white;
		}
		if (!drawHighlight)
		{
			Widgets.DrawLightHighlight(memeBox);
		}
		IdeoUIUtility.DoMeme(memeBox, meme, null, IdeoEditMode.None, drawHighlight);
		if (!Widgets.ButtonInvisible(memeBox))
		{
			return;
		}
		if (newMemes.Contains(meme))
		{
			if (meme.category != MemeCategory.Structure)
			{
				AcceptanceReport acceptanceReport = CanRemoveMeme(meme);
				if (acceptanceReport.Accepted)
				{
					newMemes.Remove(meme);
					SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
				}
				else
				{
					Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.RejectInput, historical: false);
				}
			}
			return;
		}
		if (meme.category == MemeCategory.Structure)
		{
			for (int num = newMemes.Count - 1; num >= 0; num--)
			{
				if (newMemes[num].category == MemeCategory.Structure)
				{
					newMemes.RemoveAt(num);
				}
			}
		}
		else if (ConfiguringNewFluidIdeo)
		{
			newMemes.RemoveAll((MemeDef m) => m.category == MemeCategory.Normal);
		}
		else if (ReformingFluidIdeo)
		{
			if (NormalMemesRemoveCount >= 1 && !ideo.memes.Contains(meme))
			{
				return;
			}
			newMemes.RemoveAll((MemeDef m) => !ideo.memes.Contains(m));
		}
		newMemes.Add(meme);
		SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
	}

	private bool CanUseMeme(MemeDef meme)
	{
		if (meme.hiddenInChooseMemes)
		{
			return false;
		}
		if (IdeoUIUtility.DevEditMode)
		{
			return true;
		}
		if (ConfiguringNewFluidIdeo && memeCategory == MemeCategory.Normal && !IdeoUtility.IsMemeAllowedForInitialFluidIdeo(meme))
		{
			return false;
		}
		if (Current.Game.World == null)
		{
			return IdeoUtility.IsMemeAllowedFor(meme, Find.Scenario.playerFaction.factionDef);
		}
		foreach (Faction allFaction in Find.FactionManager.AllFactions)
		{
			if (!allFaction.def.hidden && !allFaction.def.isPlayer && allFaction.ideos != null && (allFaction.ideos.IsPrimary(ideo) || allFaction.ideos.IsMinor(ideo)) && !IdeoUtility.IsMemeAllowedFor(meme, allFaction.def))
			{
				return false;
			}
		}
		return true;
	}

	private AcceptanceReport CanRemoveMeme(MemeDef meme)
	{
		if (IdeoUIUtility.DevEditMode)
		{
			return true;
		}
		if (ReformingFluidIdeo)
		{
			if (NormalMemesRemoveCount >= 1)
			{
				return false;
			}
			if (newMemes.Count > ideo.memes.Count && ideo.memes.Contains(meme))
			{
				return false;
			}
			if (ideo.memes.Count((MemeDef m) => m.category == MemeCategory.Normal) <= 1 && ideo.memes.Contains(meme))
			{
				return false;
			}
		}
		if (Current.Game.World == null)
		{
			FactionDef factionDef = Find.Scenario.playerFaction.factionDef;
			if (factionDef.requiredMemes != null && factionDef.requiredMemes.Contains(meme))
			{
				return "CannotRemoveMemeRequiredPlayer".Translate(meme.label.Named("MEME"));
			}
		}
		else
		{
			foreach (Faction allFaction in Find.FactionManager.AllFactions)
			{
				if (!allFaction.def.hidden && !allFaction.def.isPlayer && allFaction.ideos != null && (allFaction.ideos.IsPrimary(ideo) || allFaction.ideos.IsMinor(ideo)) && allFaction.def.requiredMemes != null && allFaction.def.requiredMemes.Contains(meme))
				{
					return "CannotRemoveMemeRequired".Translate(meme.label.Named("MEME")) + ": " + "RequiredByFaction".Translate(allFaction.Named("FACTION"));
				}
			}
		}
		return true;
	}

	private int GetMemeCount(MemeCategory category)
	{
		int num = 0;
		for (int i = 0; i < newMemes.Count; i++)
		{
			if (newMemes[i].category == category)
			{
				num++;
			}
		}
		return num;
	}
}
