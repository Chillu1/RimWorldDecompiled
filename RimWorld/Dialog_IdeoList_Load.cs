using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Dialog_IdeoList_Load : Dialog_IdeoList
	{
		private Action<Ideo> ideoReturner;

		public List<Faction> npcFactions;

		public bool devEditMode;

		public Dialog_IdeoList_Load(Action<Ideo> ideoReturner)
		{
			interactButLabel = "LoadGameButton".Translate();
			this.ideoReturner = ideoReturner;
		}

		public Dialog_IdeoList_Load(Action<Ideo> ideoReturner, List<Faction> npcFactions, bool devEditMode)
			: this(ideoReturner)
		{
			this.npcFactions = npcFactions;
			this.devEditMode = devEditMode;
		}

		protected override void DoFileInteraction(string fileName)
		{
			string filePath = GenFilePaths.AbsPathForIdeo(fileName);
			PreLoadUtility.CheckVersionAndLoad(filePath, ScribeMetaHeaderUtility.ScribeHeaderMode.Ideo, delegate
			{
				HashSet<string> hashSet = new HashSet<string>();
				if (GameDataSaveLoader.TryLoadIdeo(filePath, out var ideo))
				{
					if (!devEditMode && !npcFactions.NullOrEmpty())
					{
						foreach (Faction npcFaction in npcFactions)
						{
							foreach (MemeDef meme in ideo.memes)
							{
								if (!IdeoUtility.IsMemeAllowedFor(meme, npcFaction.def))
								{
									hashSet.Add("UnableToLoadIdeoMemeNotAllowed".Translate(meme.label.Named("MEME"), npcFaction.def.label.Named("FACTIONTYPE")));
								}
							}
							if (npcFaction.def.requiredMemes != null)
							{
								foreach (MemeDef requiredMeme in npcFaction.def.requiredMemes)
								{
									if (!ideo.HasMeme(requiredMeme))
									{
										hashSet.Add("UnableToLoadIdeoMemeRequired".Translate(requiredMeme.label.Named("MEME"), npcFaction.def.label.Named("FACTIONTYPE")));
									}
								}
							}
						}
					}
					if (hashSet.Count == 0)
					{
						ideoReturner(IdeoGenerator.InitLoadedIdeo(ideo));
						Close();
					}
					else
					{
						TaggedString text = "UnableToLoadIdeoDescription".Translate() + "\n\n" + (from txt in hashSet.AsEnumerable()
							orderby txt
							select txt).ToLineList(" - ");
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, delegate
						{
							Close();
						}, destructive: false, "UnableToLoadIdeoTitle".Translate()));
					}
				}
				else
				{
					Close();
				}
			});
		}
	}
}
