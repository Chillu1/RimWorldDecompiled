using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class Dialog_ReformIdeo : Window
{
	public const float HeaderHeight = 40f;

	public const float DescriptionHeight = 50f;

	private static readonly Vector2 MemeBoxSize = IdeoUIUtility.MemeBoxSize;

	private const int StyleBoxSize = 50;

	private const int MemeBoxGap = 10;

	private const int ClearChangesBtnHeight = 30;

	private const int Width = 750;

	private static readonly Color DisabledColor = new Color32(55, 55, 55, 200);

	private const int MaxMemesPerRow = 5;

	private Ideo newIdeo;

	private Ideo ideo;

	private Vector2 scrollPosition;

	private float scrollViewHeight;

	private IdeoReformStage stage;

	private List<MemeDef> tmpNormalMemes = new List<MemeDef>();

	private List<MemeDef> tmpPreSelectedMemes = new List<MemeDef>();

	public override Vector2 InitialSize
	{
		get
		{
			if (stage == IdeoReformStage.MemesAndStyles)
			{
				return new Vector2(750f, 700f);
			}
			return new Vector2(750f, Mathf.Min(1000f, UI.screenHeight));
		}
	}

	public bool StructureMemeChanged => newIdeo.StructureMeme != ideo.StructureMeme;

	public bool NormalMemesChanged
	{
		get
		{
			int num = 0;
			for (int i = 0; i < ideo.memes.Count; i++)
			{
				if (ideo.memes[i].category == MemeCategory.Normal)
				{
					if (!newIdeo.memes.Contains(ideo.memes[i]))
					{
						return true;
					}
					num++;
				}
			}
			int num2 = 0;
			for (int j = 0; j < newIdeo.memes.Count; j++)
			{
				if (newIdeo.memes[j].category == MemeCategory.Normal)
				{
					num2++;
				}
			}
			return num != num2;
		}
	}

	public bool StylesChanged
	{
		get
		{
			if (!newIdeo.thingStyleCategories.SetsEqual(ideo.thingStyleCategories) && !StructureMemeChanged)
			{
				return !NormalMemesChanged;
			}
			return false;
		}
	}

	public bool AnyChooseOneChanges
	{
		get
		{
			if (!StructureMemeChanged && !NormalMemesChanged)
			{
				return StylesChanged;
			}
			return true;
		}
	}

	public Dialog_ReformIdeo(Ideo ideo)
	{
		if (ModLister.CheckIdeology("Reform ideo dialog"))
		{
			forcePause = true;
			doCloseX = false;
			doCloseButton = false;
			absorbInputAroundWindow = true;
			forceCatchAcceptAndCancelEventEvenIfUnfocused = true;
			this.ideo = ideo;
			newIdeo = IdeoGenerator.MakeIdeo(ideo.foundation.def);
			ideo.CopyTo(newIdeo);
		}
	}

	public override void DoWindowContents(Rect inRect)
	{
		Text.Font = GameFont.Medium;
		Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 40f), "ReformIdeoligion".Translate());
		Text.Font = GameFont.Small;
		Widgets.Label(new Rect(inRect.x, inRect.y + 40f, inRect.width, 50f), "ReformIdeoligionDesc".Translate());
		Rect outRect = new Rect(inRect.x, inRect.y + 40f + 50f, inRect.width, inRect.height - 55f - 40f - 50f);
		Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, scrollViewHeight);
		Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
		float num = 0f;
		if (stage == IdeoReformStage.MemesAndStyles)
		{
			Rect rect = new Rect(0f, num, viewRect.width - 16f, Text.LineHeight);
			Widgets.Label(rect, "ReformIdeoChooseOneChange".Translate());
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			Widgets.DrawLineHorizontal(0f, rect.y + Text.LineHeight, viewRect.width - 16f);
			GUI.color = Color.white;
			num += 2f * Text.LineHeight;
			Widgets.Label(new Rect(viewRect.x, num, viewRect.width, Text.LineHeight), "ReformIdeoChangeStructure".Translate());
			num += Text.LineHeight + 10f;
			bool flag = AnyChooseOneChanges && !StructureMemeChanged;
			Rect rect2 = new Rect(viewRect.x + viewRect.width / 2f - MemeBoxSize.x / 2f, num, MemeBoxSize.x, MemeBoxSize.y);
			IdeoUIUtility.DoMeme(rect2, newIdeo.StructureMeme, newIdeo, (!flag) ? IdeoEditMode.Reform : IdeoEditMode.None);
			if (flag)
			{
				Widgets.DrawRectFast(rect2, DisabledColor);
				if (Widgets.ButtonInvisible(rect2))
				{
					Messages.Message("MessageFluidIdeoOneChangeAllowed".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				}
			}
			num += MemeBoxSize.y + 17f;
			tmpNormalMemes.Clear();
			for (int i = 0; i < newIdeo.memes.Count; i++)
			{
				if (newIdeo.memes[i].category == MemeCategory.Normal)
				{
					tmpNormalMemes.Add(newIdeo.memes[i]);
				}
			}
			Widgets.Label(new Rect(viewRect.x, num, viewRect.width, Text.LineHeight), (ideo.memes.Count((MemeDef m) => m.category == MemeCategory.Normal) <= 1) ? "ReformIdeoAddMeme".Translate() : "ReformIdeoAddOrRemoveMeme".Translate());
			num += Text.LineHeight + 10f;
			int num2 = Mathf.CeilToInt((float)tmpNormalMemes.Count / 5f);
			bool flag2 = AnyChooseOneChanges && !NormalMemesChanged;
			for (int num3 = 0; num3 < num2; num3++)
			{
				num += (float)num3 * MemeBoxSize.y + (float)((num3 > 0) ? 10 : 0);
				int num4 = num3 * 5;
				int num5 = Mathf.Min(5, tmpNormalMemes.Count - num3 * 5);
				float num6 = (float)num5 * (MemeBoxSize.x + 10f);
				float num7 = (viewRect.width - num6) / 2f;
				for (int num8 = num4; num8 < num4 + num5; num8++)
				{
					Rect rect3 = new Rect(num7, num, MemeBoxSize.x, MemeBoxSize.y);
					IdeoUIUtility.DoMeme(rect3, tmpNormalMemes[num8], newIdeo, (!flag2) ? IdeoEditMode.Reform : IdeoEditMode.None, drawHighlight: true, delegate
					{
						tmpPreSelectedMemes.Clear();
						tmpPreSelectedMemes.AddRange(newIdeo.memes.Where((MemeDef m) => !ideo.memes.Contains(m)));
						ideo.CopyTo(newIdeo);
						Find.WindowStack.Add(new Dialog_ChooseMemes(newIdeo, MemeCategory.Normal, initialSelection: false, null, tmpPreSelectedMemes, reformingIdeo: true));
					});
					if (flag2)
					{
						Widgets.DrawRectFast(rect3, DisabledColor);
						if (Widgets.ButtonInvisible(rect3))
						{
							Messages.Message("MessageFluidIdeoOneChangeAllowed".Translate(), MessageTypeDefOf.RejectInput, historical: false);
						}
					}
					num7 += MemeBoxSize.x + 10f;
				}
			}
			tmpNormalMemes.Clear();
			num += MemeBoxSize.y + 17f;
			Widgets.Label(new Rect(viewRect.x, num, viewRect.width, Text.LineHeight), "ReformIdeoChangeStyles".Translate());
			num += Text.LineHeight + 10f;
			float curX = viewRect.x;
			Rect position = new Rect(curX, num, 0f, 0f);
			bool flag3 = AnyChooseOneChanges && !StylesChanged;
			IdeoUIUtility.DoStyles(ref num, ref curX, viewRect.width, newIdeo, (!flag3) ? IdeoEditMode.Reform : IdeoEditMode.None, 50);
			if (flag3)
			{
				position.width = curX - position.x;
				position.height = num - position.y;
				Widgets.DrawRectFast(position, DisabledColor);
			}
			num += 67f;
		}
		else
		{
			Rect rect4 = new Rect(0f, num, viewRect.width - 16f, Text.LineHeight);
			Widgets.Label(rect4, "ReformIdeoChangeAny".Translate());
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			Widgets.DrawLineHorizontal(0f, rect4.y + Text.LineHeight, viewRect.width - 16f);
			GUI.color = Color.white;
			num += 2f * Text.LineHeight;
			float width = viewRect.width - 16f;
			IdeoUIUtility.DoNameAndSymbol(ref num, width, newIdeo, IdeoEditMode.Reform);
			num += 10f;
			IdeoUIUtility.DoDescription(ref num, width, newIdeo, IdeoEditMode.Reform);
			num += 10f;
			if (newIdeo.foundation != null)
			{
				IdeoUIUtility.DoFoundationInfo(ref num, width, newIdeo, IdeoEditMode.Reform);
				num += 10f;
			}
			IdeoUIUtility.DoPrecepts(ref num, width, newIdeo, IdeoEditMode.Reform);
			num += 10f;
			IdeoUIUtility.DoAppearanceItems(newIdeo, IdeoEditMode.Reform, ref num, width);
		}
		if (Event.current.type == EventType.Layout)
		{
			scrollViewHeight = num;
		}
		Widgets.EndScrollView();
		Rect rect5 = new Rect(inRect.xMax - Window.CloseButSize.x, inRect.height - Window.CloseButSize.y, Window.CloseButSize.x, Window.CloseButSize.y);
		if (stage == IdeoReformStage.MemesAndStyles)
		{
			Rect rect6 = rect5;
			rect6.x = inRect.x;
			if (Widgets.ButtonText(rect6, "Cancel".Translate()))
			{
				Close();
			}
			if (AnyChooseOneChanges)
			{
				if (Widgets.ButtonText(new Rect(inRect.x + (inRect.width - Window.CloseButSize.x) / 2f, inRect.height - Window.CloseButSize.y, Window.CloseButSize.x, Window.CloseButSize.y), "ReformIdeoResetChanges".Translate()))
				{
					SoundDefOf.Tick_Low.PlayOneShotOnCamera();
					ResetAllChooseOneChanges();
				}
				num += 47f;
			}
			if (Widgets.ButtonText(rect5, "Next".Translate()))
			{
				stage = IdeoReformStage.PreceptsNarrativeAndDeities;
			}
			return;
		}
		Rect rect7 = rect5;
		rect7.x = inRect.x;
		Rect rect8 = inRect;
		rect8.xMin = rect8.xMax - rect5.width * 3.1f;
		rect8.width = rect5.width * 2f;
		rect8.yMin = rect8.yMax - rect5.height;
		Pair<Precept, Precept> pair = newIdeo.FirstIncompatiblePreceptPair();
		if (pair != default(Pair<Precept, Precept>))
		{
			GUI.color = Color.red;
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.UpperRight;
			string text = pair.First.TipLabel;
			string text2 = pair.Second.TipLabel;
			if (text == text2)
			{
				text = pair.First.UIInfoSecondLine;
				text2 = pair.Second.UIInfoSecondLine;
			}
			Widgets.Label(rect8, "MessageIdeoIncompatiblePrecepts".Translate(text.Named("PRECEPT1"), text2.Named("PRECEPT2")).CapitalizeFirst());
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
		}
		if (Widgets.ButtonText(rect7, "Back".Translate()))
		{
			stage = IdeoReformStage.MemesAndStyles;
		}
		Rect rect9 = rect5;
		rect9.x = inRect.x + (inRect.width - rect9.width) / 2f - ((pair != default(Pair<Precept, Precept>)) ? 65f : 0f);
		if (Widgets.ButtonText(rect9, "Randomize".Translate()))
		{
			RandomizeNewIdeo();
		}
		if (Widgets.ButtonText(rect5, "DoneButton".Translate()))
		{
			IdeoDevelopmentUtility.ConfirmChangesToIdeo(ideo, newIdeo, delegate
			{
				IdeoDevelopmentUtility.ApplyChangesToIdeo(ideo, newIdeo);
				Close();
			});
		}
	}

	private void RandomizeNewIdeo()
	{
		IdeoGenerationParms parms = new IdeoGenerationParms(Faction.OfPlayer.def);
		newIdeo.foundation.RandomizeCulture(parms);
		newIdeo.foundation.RandomizePlace();
		if (newIdeo.foundation is IdeoFoundation_Deity ideoFoundation_Deity)
		{
			ideoFoundation_Deity.GenerateDeities();
		}
		newIdeo.foundation.GenerateTextSymbols();
		newIdeo.foundation.GenerateLeaderTitle();
		newIdeo.foundation.RandomizeIcon();
		newIdeo.foundation.RandomizePrecepts(init: true, parms);
		newIdeo.RegenerateDescription(force: true);
	}

	private void ResetAllChooseOneChanges()
	{
		newIdeo.memes.Clear();
		newIdeo.memes.AddRange(ideo.memes);
		newIdeo.thingStyleCategories.Clear();
		newIdeo.thingStyleCategories.AddRange(ideo.thingStyleCategories);
	}
}
