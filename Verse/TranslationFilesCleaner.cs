using RimWorld;
using RimWorld.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace Verse
{
	public static class TranslationFilesCleaner
	{
		private class PossibleDefInjection
		{
			public string suggestedPath;

			public string normalizedPath;

			public bool isCollection;

			public bool fullListTranslationAllowed;

			public string curValue;

			public IEnumerable<string> curValueCollection;

			public FieldInfo fieldInfo;

			public Def def;
		}

		private const string NewlineTag = "NEWLINE";

		private const string NewlineTagFull = "<!--NEWLINE-->";

		public static void CleanupTranslationFiles()
		{
			LoadedLanguage curLang = LanguageDatabase.activeLanguage;
			LoadedLanguage english = LanguageDatabase.defaultLanguage;
			if (curLang == english)
			{
				return;
			}
			IEnumerable<ModMetaData> activeModsInLoadOrder = ModsConfig.ActiveModsInLoadOrder;
			if (!activeModsInLoadOrder.Any((ModMetaData x) => x.IsCoreMod) || activeModsInLoadOrder.Any((ModMetaData x) => !x.Official))
			{
				Messages.Message("MessageDisableModsBeforeCleaningTranslationFiles".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				return;
			}
			if (LanguageDatabase.activeLanguage.AllDirectories.Any((Tuple<VirtualDirectory, ModContentPack, string> x) => x.Item1 is TarDirectory))
			{
				Messages.Message("MessageUnpackBeforeCleaningTranslationFiles".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				return;
			}
			LongEventHandler.QueueLongEvent(delegate
			{
				if (curLang.anyKeyedReplacementsXmlParseError || curLang.anyDefInjectionsXmlParseError)
				{
					string value = curLang.lastKeyedReplacementsXmlParseErrorInFile ?? curLang.lastDefInjectionsXmlParseErrorInFile;
					Messages.Message("MessageCantCleanupTranslationFilesBeucaseOfXmlError".Translate(value), MessageTypeDefOf.RejectInput, historical: false);
				}
				else
				{
					english.LoadData();
					curLang.LoadData();
					Dialog_MessageBox dialog_MessageBox = Dialog_MessageBox.CreateConfirmation("ConfirmCleanupTranslationFiles".Translate(curLang.FriendlyNameNative), delegate
					{
						LongEventHandler.QueueLongEvent(DoCleanupTranslationFiles, "CleaningTranslationFiles".Translate(), doAsynchronously: true, null);
					}, destructive: true);
					dialog_MessageBox.buttonAText = "ConfirmCleanupTranslationFiles_Confirm".Translate();
					Find.WindowStack.Add(dialog_MessageBox);
				}
			}, null, doAsynchronously: false, null);
		}

		private static void DoCleanupTranslationFiles()
		{
			if (LanguageDatabase.activeLanguage == LanguageDatabase.defaultLanguage)
			{
				return;
			}
			try
			{
				try
				{
					CleanupKeyedTranslations();
				}
				catch (Exception arg)
				{
					Log.Error("Could not cleanup keyed translations: " + arg);
				}
				try
				{
					CleanupDefInjections();
				}
				catch (Exception arg2)
				{
					Log.Error("Could not cleanup def-injections: " + arg2);
				}
				try
				{
					CleanupBackstories();
				}
				catch (Exception arg3)
				{
					Log.Error("Could not cleanup backstories: " + arg3);
				}
				string value = string.Join("\n", ModsConfig.ActiveModsInLoadOrder.Select((ModMetaData x) => GetLanguageFolderPath(LanguageDatabase.activeLanguage, x.RootDir.FullName)).ToArray());
				Messages.Message("MessageTranslationFilesCleanupDone".Translate(value), MessageTypeDefOf.TaskCompletion, historical: false);
			}
			catch (Exception arg4)
			{
				Log.Error("Could not cleanup translation files: " + arg4);
			}
		}

		private static void CleanupKeyedTranslations()
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			LoadedLanguage english = LanguageDatabase.defaultLanguage;
			List<LoadedLanguage.KeyedReplacement> list = (from x in activeLanguage.keyedReplacements
				where !x.Value.isPlaceholder && !english.HaveTextForKey(x.Key)
				select x.Value).ToList();
			HashSet<LoadedLanguage.KeyedReplacement> writtenUnusedKeyedTranslations = new HashSet<LoadedLanguage.KeyedReplacement>();
			foreach (ModMetaData item in ModsConfig.ActiveModsInLoadOrder)
			{
				string languageFolderPath = GetLanguageFolderPath(activeLanguage, item.RootDir.FullName);
				string text = Path.Combine(languageFolderPath, "CodeLinked");
				string text2 = Path.Combine(languageFolderPath, "Keyed");
				DirectoryInfo directoryInfo = new DirectoryInfo(text);
				if (directoryInfo.Exists)
				{
					if (!Directory.Exists(text2))
					{
						Directory.Move(text, text2);
						Thread.Sleep(1000);
						directoryInfo = new DirectoryInfo(text2);
					}
				}
				else
				{
					directoryInfo = new DirectoryInfo(text2);
				}
				DirectoryInfo directoryInfo2 = new DirectoryInfo(Path.Combine(GetLanguageFolderPath(english, item.RootDir.FullName), "Keyed"));
				if (!directoryInfo2.Exists)
				{
					if (item.IsCoreMod)
					{
						Log.Error("English keyed translations folder doesn't exist.");
					}
					if (!directoryInfo.Exists)
					{
						continue;
					}
				}
				if (!directoryInfo.Exists)
				{
					directoryInfo.Create();
				}
				FileInfo[] files = directoryInfo.GetFiles("*.xml", SearchOption.AllDirectories);
				foreach (FileInfo fileInfo in files)
				{
					try
					{
						fileInfo.Delete();
					}
					catch (Exception ex)
					{
						Log.Error("Could not delete " + fileInfo.Name + ": " + ex);
					}
				}
				files = directoryInfo2.GetFiles("*.xml", SearchOption.AllDirectories);
				foreach (FileInfo fileInfo2 in files)
				{
					try
					{
						string path = new Uri(directoryInfo2.FullName + Path.DirectorySeparatorChar).MakeRelativeUri(new Uri(fileInfo2.FullName)).ToString();
						string text3 = Path.Combine(directoryInfo.FullName, path);
						Directory.CreateDirectory(Path.GetDirectoryName(text3));
						fileInfo2.CopyTo(text3);
					}
					catch (Exception ex2)
					{
						Log.Error("Could not copy " + fileInfo2.Name + ": " + ex2);
					}
				}
				files = directoryInfo.GetFiles("*.xml", SearchOption.AllDirectories);
				foreach (FileInfo fileInfo3 in files)
				{
					try
					{
						XDocument xDocument = XDocument.Load(fileInfo3.FullName, LoadOptions.PreserveWhitespace);
						XElement xElement = xDocument.DescendantNodes().OfType<XElement>().FirstOrDefault();
						if (xElement == null)
						{
							continue;
						}
						try
						{
							XNode[] array = xElement.DescendantNodes().ToArray();
							foreach (XNode xNode in array)
							{
								XElement xElement2 = xNode as XElement;
								if (xElement2 == null)
								{
									continue;
								}
								XNode[] array2 = xElement2.DescendantNodes().ToArray();
								foreach (XNode xNode2 in array2)
								{
									try
									{
										XText xText = xNode2 as XText;
										if (xText != null && !xText.Value.NullOrEmpty())
										{
											string comment = " EN: " + xText.Value + " ";
											xNode.AddBeforeSelf(new XComment(SanitizeXComment(comment)));
											xNode.AddBeforeSelf(Environment.NewLine);
											xNode.AddBeforeSelf("  ");
										}
									}
									catch (Exception ex3)
									{
										Log.Error("Could not add comment node in " + fileInfo3.Name + ": " + ex3);
									}
									xNode2.Remove();
								}
								try
								{
									if (activeLanguage.TryGetTextFromKey(xElement2.Name.ToString(), out TaggedString translated))
									{
										if (!translated.NullOrEmpty())
										{
											xElement2.Add(new XText(translated.Replace("\n", "\\n").RawText));
										}
									}
									else
									{
										xElement2.Add(new XText("TODO"));
									}
								}
								catch (Exception ex4)
								{
									Log.Error("Could not add existing translation or placeholder in " + fileInfo3.Name + ": " + ex4);
								}
							}
							bool flag = false;
							foreach (LoadedLanguage.KeyedReplacement item2 in list)
							{
								if (new Uri(fileInfo3.FullName).Equals(new Uri(item2.fileSourceFullPath)))
								{
									if (!flag)
									{
										xElement.Add("  ");
										xElement.Add(new XComment(" UNUSED "));
										xElement.Add(Environment.NewLine);
										flag = true;
									}
									XElement xElement3 = new XElement(item2.key);
									if (item2.isPlaceholder)
									{
										xElement3.Add(new XText("TODO"));
									}
									else if (!item2.value.NullOrEmpty())
									{
										xElement3.Add(new XText(item2.value.Replace("\n", "\\n")));
									}
									xElement.Add("  ");
									xElement.Add(xElement3);
									xElement.Add(Environment.NewLine);
									writtenUnusedKeyedTranslations.Add(item2);
								}
							}
							if (flag)
							{
								xElement.Add(Environment.NewLine);
							}
						}
						finally
						{
							SaveXMLDocumentWithProcessedNewlineTags(xDocument.Root, fileInfo3.FullName);
						}
					}
					catch (Exception ex5)
					{
						Log.Error("Could not process " + fileInfo3.Name + ": " + ex5);
					}
				}
			}
			foreach (IGrouping<string, LoadedLanguage.KeyedReplacement> item3 in from x in list
				where !writtenUnusedKeyedTranslations.Contains(x)
				group x by x.fileSourceFullPath)
			{
				try
				{
					if (File.Exists(item3.Key))
					{
						Log.Error("Could not save unused keyed translations to " + item3.Key + " because this file already exists.");
						continue;
					}
					SaveXMLDocumentWithProcessedNewlineTags(new XDocument(new XElement("LanguageData", new XComment("NEWLINE"), new XComment(" UNUSED "), item3.Select(delegate(LoadedLanguage.KeyedReplacement x)
					{
						string text4 = x.isPlaceholder ? "TODO" : x.value;
						return new XElement(x.key, new XText(text4.NullOrEmpty() ? "" : text4.Replace("\n", "\\n")));
					}), new XComment("NEWLINE"))), item3.Key);
				}
				catch (Exception ex6)
				{
					Log.Error("Could not save unused keyed translations to " + item3.Key + ": " + ex6);
				}
			}
		}

		private static void CleanupDefInjections()
		{
			foreach (ModMetaData item in ModsConfig.ActiveModsInLoadOrder)
			{
				string languageFolderPath = GetLanguageFolderPath(LanguageDatabase.activeLanguage, item.RootDir.FullName);
				string text = Path.Combine(languageFolderPath, "DefLinked");
				string text2 = Path.Combine(languageFolderPath, "DefInjected");
				DirectoryInfo directoryInfo = new DirectoryInfo(text);
				if (directoryInfo.Exists)
				{
					if (!Directory.Exists(text2))
					{
						Directory.Move(text, text2);
						Thread.Sleep(1000);
						directoryInfo = new DirectoryInfo(text2);
					}
				}
				else
				{
					directoryInfo = new DirectoryInfo(text2);
				}
				if (!directoryInfo.Exists)
				{
					directoryInfo.Create();
				}
				FileInfo[] files = directoryInfo.GetFiles("*.xml", SearchOption.AllDirectories);
				foreach (FileInfo fileInfo in files)
				{
					try
					{
						fileInfo.Delete();
					}
					catch (Exception ex)
					{
						Log.Error("Could not delete " + fileInfo.Name + ": " + ex);
					}
				}
				foreach (Type item2 in GenDefDatabase.AllDefTypesWithDatabases())
				{
					try
					{
						CleanupDefInjectionsForDefType(item2, directoryInfo.FullName, item);
					}
					catch (Exception ex2)
					{
						Log.Error("Could not process def-injections for type " + item2.Name + ": " + ex2);
					}
				}
			}
		}

		private static void CleanupDefInjectionsForDefType(Type defType, string defInjectionsFolderPath, ModMetaData mod)
		{
			List<KeyValuePair<string, DefInjectionPackage.DefInjection>> list = (from x in LanguageDatabase.activeLanguage.defInjections.Where((DefInjectionPackage x) => x.defType == defType).SelectMany((DefInjectionPackage x) => x.injections)
				where !x.Value.isPlaceholder && x.Value.ModifiesDefFromModOrNullCore(mod, defType)
				select x).ToList();
			Dictionary<string, DefInjectionPackage.DefInjection> dictionary = new Dictionary<string, DefInjectionPackage.DefInjection>();
			foreach (KeyValuePair<string, DefInjectionPackage.DefInjection> item2 in list)
			{
				if (!dictionary.ContainsKey(item2.Value.normalizedPath))
				{
					dictionary.Add(item2.Value.normalizedPath, item2.Value);
				}
			}
			List<PossibleDefInjection> possibleDefInjections = new List<PossibleDefInjection>();
			DefInjectionUtility.ForEachPossibleDefInjection(defType, delegate(string suggestedPath, string normalizedPath, bool isCollection, string str, IEnumerable<string> collection, bool translationAllowed, bool fullListTranslationAllowed, FieldInfo fieldInfo, Def def)
			{
				if (translationAllowed)
				{
					PossibleDefInjection item = new PossibleDefInjection
					{
						suggestedPath = suggestedPath,
						normalizedPath = normalizedPath,
						isCollection = isCollection,
						fullListTranslationAllowed = fullListTranslationAllowed,
						curValue = str,
						curValueCollection = collection,
						fieldInfo = fieldInfo,
						def = def
					};
					possibleDefInjections.Add(item);
				}
			}, mod);
			if (!possibleDefInjections.Any() && !list.Any())
			{
				return;
			}
			List<KeyValuePair<string, DefInjectionPackage.DefInjection>> source = list.Where((KeyValuePair<string, DefInjectionPackage.DefInjection> x) => !x.Value.injected).ToList();
			foreach (string fileName in possibleDefInjections.Select((PossibleDefInjection x) => GetSourceFile(x.def)).Concat(source.Select((KeyValuePair<string, DefInjectionPackage.DefInjection> x) => x.Value.fileSource)).Distinct())
			{
				try
				{
					XDocument xDocument = new XDocument();
					bool flag = false;
					try
					{
						XElement xElement = new XElement("LanguageData");
						xDocument.Add(xElement);
						xElement.Add(new XComment("NEWLINE"));
						List<PossibleDefInjection> source2 = possibleDefInjections.Where((PossibleDefInjection x) => GetSourceFile(x.def) == fileName).ToList();
						List<KeyValuePair<string, DefInjectionPackage.DefInjection>> source3 = source.Where((KeyValuePair<string, DefInjectionPackage.DefInjection> x) => x.Value.fileSource == fileName).ToList();
						foreach (string defName in from x in source2.Select((PossibleDefInjection x) => x.def.defName).Concat(source3.Select((KeyValuePair<string, DefInjectionPackage.DefInjection> x) => x.Value.DefName)).Distinct()
							orderby x
							select x)
						{
							try
							{
								IEnumerable<PossibleDefInjection> enumerable = source2.Where((PossibleDefInjection x) => x.def.defName == defName);
								IEnumerable<KeyValuePair<string, DefInjectionPackage.DefInjection>> enumerable2 = source3.Where((KeyValuePair<string, DefInjectionPackage.DefInjection> x) => x.Value.DefName == defName);
								if (enumerable.Any())
								{
									bool flag2 = false;
									foreach (PossibleDefInjection item3 in enumerable)
									{
										if (item3.isCollection)
										{
											IEnumerable<string> englishList = GetEnglishList(item3.normalizedPath, item3.curValueCollection, dictionary);
											bool flag3 = false;
											if (englishList != null)
											{
												int num = 0;
												foreach (string item4 in englishList)
												{
													_ = item4;
													if (dictionary.ContainsKey(item3.normalizedPath + "." + num))
													{
														flag3 = true;
														break;
													}
													num++;
												}
											}
											if (flag3 || !item3.fullListTranslationAllowed)
											{
												if (englishList == null)
												{
													continue;
												}
												int num2 = -1;
												foreach (string item5 in englishList)
												{
													num2++;
													string text = item3.normalizedPath + "." + num2;
													string suggestedPath2 = item3.suggestedPath + "." + num2;
													if (TKeySystem.TrySuggestTKeyPath(text, out string tKeyPath))
													{
														suggestedPath2 = tKeyPath;
													}
													if (!dictionary.TryGetValue(text, out DefInjectionPackage.DefInjection value))
													{
														value = null;
													}
													if (value == null && !DefInjectionUtility.ShouldCheckMissingInjection(item5, item3.fieldInfo, item3.def))
													{
														continue;
													}
													flag2 = true;
													flag = true;
													try
													{
														if (!item5.NullOrEmpty())
														{
															xElement.Add(new XComment(SanitizeXComment(" EN: " + item5.Replace("\n", "\\n") + " ")));
														}
													}
													catch (Exception ex)
													{
														Log.Error("Could not add comment node in " + fileName + ": " + ex);
													}
													xElement.Add(GetDefInjectableFieldNode(suggestedPath2, value));
												}
												continue;
											}
											bool flag4 = false;
											if (englishList != null)
											{
												foreach (string item6 in englishList)
												{
													if (DefInjectionUtility.ShouldCheckMissingInjection(item6, item3.fieldInfo, item3.def))
													{
														flag4 = true;
														break;
													}
												}
											}
											if (!dictionary.TryGetValue(item3.normalizedPath, out DefInjectionPackage.DefInjection value2))
											{
												value2 = null;
											}
											if (value2 == null && !flag4)
											{
												continue;
											}
											flag2 = true;
											flag = true;
											try
											{
												string text2 = ListToLiNodesString(englishList);
												if (!text2.NullOrEmpty())
												{
													xElement.Add(new XComment(SanitizeXComment(" EN:\n" + text2.Indented() + "\n  ")));
												}
											}
											catch (Exception ex2)
											{
												Log.Error("Could not add comment node in " + fileName + ": " + ex2);
											}
											xElement.Add(GetDefInjectableFieldNode(item3.suggestedPath, value2));
											continue;
										}
										if (!dictionary.TryGetValue(item3.normalizedPath, out DefInjectionPackage.DefInjection value3))
										{
											value3 = null;
										}
										string text3 = (value3 != null && value3.injected) ? value3.replacedString : item3.curValue;
										if (value3 == null && !DefInjectionUtility.ShouldCheckMissingInjection(text3, item3.fieldInfo, item3.def))
										{
											continue;
										}
										flag2 = true;
										flag = true;
										try
										{
											if (!text3.NullOrEmpty())
											{
												xElement.Add(new XComment(SanitizeXComment(" EN: " + text3.Replace("\n", "\\n") + " ")));
											}
										}
										catch (Exception ex3)
										{
											Log.Error("Could not add comment node in " + fileName + ": " + ex3);
										}
										xElement.Add(GetDefInjectableFieldNode(item3.suggestedPath, value3));
									}
									if (flag2)
									{
										xElement.Add(new XComment("NEWLINE"));
									}
								}
								if (!enumerable2.Any())
								{
									continue;
								}
								flag = true;
								xElement.Add(new XComment(" UNUSED "));
								foreach (KeyValuePair<string, DefInjectionPackage.DefInjection> item7 in enumerable2)
								{
									xElement.Add(GetDefInjectableFieldNode(item7.Value.path, item7.Value));
								}
								xElement.Add(new XComment("NEWLINE"));
							}
							catch (Exception ex4)
							{
								Log.Error("Could not process def-injections for def " + defName + ": " + ex4);
							}
						}
					}
					finally
					{
						if (flag)
						{
							string text4 = Path.Combine(defInjectionsFolderPath, defType.Name);
							Directory.CreateDirectory(text4);
							SaveXMLDocumentWithProcessedNewlineTags(xDocument, Path.Combine(text4, fileName));
						}
					}
				}
				catch (Exception ex5)
				{
					Log.Error("Could not process def-injections for file " + fileName + ": " + ex5);
				}
			}
		}

		private static void CleanupBackstories()
		{
			string text = Path.Combine(GetActiveLanguageCoreModFolderPath(), "Backstories");
			Directory.CreateDirectory(text);
			string path = Path.Combine(text, "Backstories.xml");
			File.Delete(path);
			XDocument xDocument = new XDocument();
			try
			{
				XElement xElement = new XElement("BackstoryTranslations");
				xDocument.Add(xElement);
				xElement.Add(new XComment("NEWLINE"));
				foreach (KeyValuePair<string, Backstory> item in BackstoryDatabase.allBackstories.OrderBy((KeyValuePair<string, Backstory> x) => x.Key))
				{
					try
					{
						XElement xElement2 = new XElement(item.Key);
						AddBackstoryFieldElement(xElement2, "title", item.Value.title, item.Value.untranslatedTitle, item.Value.titleTranslated);
						AddBackstoryFieldElement(xElement2, "titleFemale", item.Value.titleFemale, item.Value.untranslatedTitleFemale, item.Value.titleFemaleTranslated);
						AddBackstoryFieldElement(xElement2, "titleShort", item.Value.titleShort, item.Value.untranslatedTitleShort, item.Value.titleShortTranslated);
						AddBackstoryFieldElement(xElement2, "titleShortFemale", item.Value.titleShortFemale, item.Value.untranslatedTitleShortFemale, item.Value.titleShortFemaleTranslated);
						AddBackstoryFieldElement(xElement2, "desc", item.Value.baseDesc, item.Value.untranslatedDesc, item.Value.descTranslated);
						xElement.Add(xElement2);
						xElement.Add(new XComment("NEWLINE"));
					}
					catch (Exception ex)
					{
						Log.Error("Could not process backstory " + item.Key + ": " + ex);
					}
				}
			}
			finally
			{
				SaveXMLDocumentWithProcessedNewlineTags(xDocument, path);
			}
		}

		private static void AddBackstoryFieldElement(XElement addTo, string fieldName, string currentValue, string untranslatedValue, bool wasTranslated)
		{
			if (wasTranslated || !untranslatedValue.NullOrEmpty())
			{
				if (!untranslatedValue.NullOrEmpty())
				{
					addTo.Add(new XComment(SanitizeXComment(" EN: " + untranslatedValue.Replace("\n", "\\n") + " ")));
				}
				string text = wasTranslated ? currentValue : "TODO";
				addTo.Add(new XElement(fieldName, text.NullOrEmpty() ? "" : text.Replace("\n", "\\n")));
			}
		}

		private static string GetActiveLanguageCoreModFolderPath()
		{
			ModContentPack modContentPack = LoadedModManager.RunningMods.FirstOrDefault((ModContentPack x) => x.IsCoreMod);
			return GetLanguageFolderPath(LanguageDatabase.activeLanguage, modContentPack.RootDir);
		}

		public static string GetLanguageFolderPath(LoadedLanguage language, string modRootDir)
		{
			return Path.Combine(Path.Combine(modRootDir, "Languages"), language.folderName);
		}

		private static void SaveXMLDocumentWithProcessedNewlineTags(XNode doc, string path)
		{
			File.WriteAllText(path, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" + doc.ToString().Replace("<!--NEWLINE-->", "").Replace("&gt;", ">"), Encoding.UTF8);
		}

		private static string ListToLiNodesString(IEnumerable<string> list)
		{
			if (list == null)
			{
				return "";
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string item in list)
			{
				stringBuilder.Append("<li>");
				if (!item.NullOrEmpty())
				{
					stringBuilder.Append(item.Replace("\n", "\\n"));
				}
				stringBuilder.Append("</li>");
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}

		private static XElement ListToXElement(IEnumerable<string> list, string name, List<Pair<int, string>> comments)
		{
			XElement xElement = new XElement(name);
			if (list != null)
			{
				int num = 0;
				foreach (string item in list)
				{
					if (comments != null)
					{
						for (int i = 0; i < comments.Count; i++)
						{
							if (comments[i].First == num)
							{
								xElement.Add(new XComment(comments[i].Second));
							}
						}
					}
					XElement xElement2 = new XElement("li");
					if (!item.NullOrEmpty())
					{
						xElement2.Add(new XText(item.Replace("\n", "\\n")));
					}
					xElement.Add(xElement2);
					num++;
				}
				if (comments != null)
				{
					for (int j = 0; j < comments.Count; j++)
					{
						if (comments[j].First == num)
						{
							xElement.Add(new XComment(comments[j].Second));
						}
					}
				}
			}
			return xElement;
		}

		private static string AppendXmlExtensionIfNotAlready(string fileName)
		{
			if (!fileName.ToLower().EndsWith(".xml"))
			{
				return fileName + ".xml";
			}
			return fileName;
		}

		private static string GetSourceFile(Def def)
		{
			if (!def.fileName.NullOrEmpty())
			{
				return AppendXmlExtensionIfNotAlready(def.fileName);
			}
			return "Unknown.xml";
		}

		private static string TryRemoveLastIndexSymbol(string str)
		{
			int num = str.LastIndexOf('.');
			if (num >= 0 && str.Substring(num + 1).All((char x) => char.IsNumber(x)))
			{
				return str.Substring(0, num);
			}
			return str;
		}

		private static IEnumerable<string> GetEnglishList(string normalizedPath, IEnumerable<string> curValue, Dictionary<string, DefInjectionPackage.DefInjection> injectionsByNormalizedPath)
		{
			if (injectionsByNormalizedPath.TryGetValue(normalizedPath, out DefInjectionPackage.DefInjection value) && value.injected)
			{
				return value.replacedList;
			}
			if (curValue == null)
			{
				return null;
			}
			List<string> list = curValue.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				string key = normalizedPath + "." + i;
				if (injectionsByNormalizedPath.TryGetValue(key, out DefInjectionPackage.DefInjection value2) && value2.injected)
				{
					list[i] = value2.replacedString;
				}
			}
			return list;
		}

		private static XElement GetDefInjectableFieldNode(string suggestedPath, DefInjectionPackage.DefInjection existingInjection)
		{
			if (existingInjection == null || existingInjection.isPlaceholder)
			{
				return new XElement(suggestedPath, new XText("TODO"));
			}
			if (existingInjection.IsFullListInjection)
			{
				return ListToXElement(existingInjection.fullListInjection, suggestedPath, existingInjection.fullListInjectionComments);
			}
			XElement xElement;
			if (!existingInjection.injection.NullOrEmpty())
			{
				if (existingInjection.suggestedPath.EndsWith(".slateRef") && ConvertHelper.IsXml(existingInjection.injection))
				{
					try
					{
						return XElement.Parse("<" + suggestedPath + ">" + existingInjection.injection + "</" + suggestedPath + ">");
					}
					catch (Exception ex)
					{
						Log.Warning("Could not parse XML: " + existingInjection.injection + ". Exception: " + ex);
						xElement = new XElement(suggestedPath);
						xElement.Add(existingInjection.injection);
						return xElement;
					}
				}
				xElement = new XElement(suggestedPath);
				xElement.Add(new XText(existingInjection.injection.Replace("\n", "\\n")));
			}
			else
			{
				xElement = new XElement(suggestedPath);
			}
			return xElement;
		}

		private static string SanitizeXComment(string comment)
		{
			while (comment.Contains("-----"))
			{
				comment = comment.Replace("-----", "- - -");
			}
			while (comment.Contains("--"))
			{
				comment = comment.Replace("--", "- -");
			}
			return comment;
		}
	}
}
