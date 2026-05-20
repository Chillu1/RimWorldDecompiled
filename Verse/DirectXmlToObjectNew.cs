using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Xml;

namespace Verse;

public static class DirectXmlToObjectNew
{
	public delegate void ParseValueAndSetFieldDelegate(object target, FieldInfo field, XmlNode node, Type typeBeingDeserialized);

	public delegate void ParseValueAndAddListItemDelegate(IList target, int unused, XmlNode node, Type itemType);

	public delegate Def ParseValueAndReturnDefDelegate(int unused, int unused2, XmlNode node, Type defType);

	private class DummyTypeToHoldDynamicMethods
	{
	}

	private static readonly MethodInfo XmlNodeGetChildNodesMethod = typeof(XmlNode).GetProperty("ChildNodes").GetGetMethod();

	private static readonly MethodInfo XmlNodeGetInnerTextMethod = typeof(XmlNode).GetProperty("InnerText").GetGetMethod();

	private static readonly MethodInfo XmlNodeGetFirstChildMethod = typeof(XmlNode).GetProperty("FirstChild").GetGetMethod();

	private static readonly MethodInfo XmlNodeHasChildNodesMethod = typeof(XmlNode).GetProperty("HasChildNodes").GetGetMethod();

	private static readonly MethodInfo XmlNodeGetOuterXmlMethod = typeof(XmlNode).GetProperty("OuterXml").GetGetMethod();

	private static readonly MethodInfo XmlNodeGetNodeTypeMethod = typeof(XmlNode).GetProperty("NodeType").GetGetMethod();

	private static readonly MethodInfo XmlNodeGetValueMethod = typeof(XmlNode).GetProperty("Value").GetGetMethod();

	private static readonly MethodInfo XmlNodeGetNameMethod = typeof(XmlNode).GetProperty("Name").GetGetMethod();

	private static readonly MethodInfo XmlNodeGetAttributesMethod = typeof(XmlNode).GetProperty("Attributes").GetGetMethod();

	private static readonly MethodInfo XmlNodeGetItemByNameMethod = typeof(XmlNode).GetMethod("get_Item", new Type[1] { typeof(string) });

	private static readonly MethodInfo XmlNodeListGetCountMethod = typeof(XmlNodeList).GetProperty("Count").GetGetMethod();

	private static readonly MethodInfo XmlNodeListGetItemMethod = typeof(XmlNodeList).GetMethod("Item", new Type[1] { typeof(int) });

	private static readonly MethodInfo XmlAttributeCollectionGetItemByNameMethod = typeof(XmlAttributeCollection).GetMethod("get_ItemOf", new Type[1] { typeof(string) });

	private static readonly MethodInfo XmlAttributeGetValueMethod = typeof(XmlAttribute).GetProperty("Value").GetGetMethod();

	private static readonly MethodInfo FieldSetValueMethod = typeof(FieldInfo).GetMethod("SetValue", new Type[2]
	{
		typeof(object),
		typeof(object)
	});

	private static readonly MethodInfo IListAddItemMethod = typeof(IList).GetMethod("Add", new Type[1] { typeof(object) });

	private static readonly MethodInfo ValidateListNodeMethod = typeof(DirectXmlToObject).GetMethod("ValidateListNode", BindingFlags.Static | BindingFlags.Public);

	private static readonly MethodInfo ParseHelperFromStringNonGenericMethod = typeof(ParseHelper).GetMethod("FromString", BindingFlags.Static | BindingFlags.Public, null, new Type[2]
	{
		typeof(string),
		typeof(Type)
	}, null);

	private static readonly MethodInfo InnerTextWithReplacedNewlinesOrXmlMethod = typeof(DirectXmlToObject).GetMethod("InnerTextWithReplacedNewlinesOrXML", BindingFlags.Static | BindingFlags.Public);

	private static readonly MethodInfo LogMessageMethod = typeof(Log).GetMethod("Message", new Type[1] { typeof(string) });

	private static readonly MethodInfo LogErrorMethod = typeof(Log).GetMethod("Error", new Type[1] { typeof(string) });

	private static readonly MethodInfo TypeGetTypeFromHandleMethod = typeof(Type).GetMethod("GetTypeFromHandle");

	private static readonly MethodInfo TypeGetFieldMethod = typeof(Type).GetMethod("GetField", new Type[2]
	{
		typeof(string),
		typeof(BindingFlags)
	});

	private static readonly MethodInfo StringConcatMethod = typeof(string).GetMethod("Concat", new Type[2]
	{
		typeof(string),
		typeof(string)
	});

	private static readonly MethodInfo StringConcatObjObjMethod = typeof(string).GetMethod("Concat", new Type[2]
	{
		typeof(object),
		typeof(object)
	});

	private static readonly MethodInfo StringToLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);

	private static readonly MethodInfo StringEqualsWithComparisonModeMethod = typeof(string).GetMethod("Equals", new Type[2]
	{
		typeof(string),
		typeof(StringComparison)
	});

	private static readonly MethodInfo XmlInheritanceGetResolvedNodeForMethod = typeof(XmlInheritance).GetMethod("GetResolvedNodeFor", BindingFlags.Static | BindingFlags.Public);

	private static readonly MethodInfo DictionaryAddMethod = typeof(IDictionary).GetMethod("Add");

	private static readonly MethodInfo RegisterObjectWantsCrossRefMethod = typeof(DirectXmlCrossRefLoader).GetMethod("RegisterObjectWantsCrossRef", BindingFlags.Static | BindingFlags.Public, null, new Type[6]
	{
		typeof(object),
		typeof(FieldInfo),
		typeof(string),
		typeof(string),
		typeof(string),
		typeof(Type)
	}, null);

	private static readonly MethodInfo RegisterDictionaryWantsCrossRefMethod = typeof(DirectXmlCrossRefLoader).GetMethods(BindingFlags.Static | BindingFlags.Public).First((MethodInfo m) => m.Name == "RegisterDictionaryWantsCrossRef");

	private static readonly MethodInfo RegisterListWantsCrossRefMethod = typeof(DirectXmlCrossRefLoader).GetMethods(BindingFlags.Static | BindingFlags.Public).First((MethodInfo m) => m.Name == "RegisterListWantsCrossRef");

	private static readonly MethodInfo GenTypesIsDefMethod = typeof(GenTypes).GetMethod("IsDef", BindingFlags.Static | BindingFlags.Public);

	private static readonly MethodInfo GenTypesGetTypeInAnyAssemblyMethod = typeof(GenTypes).GetMethod("GetTypeInAnyAssembly", BindingFlags.Static | BindingFlags.Public);

	private static readonly MethodInfo GenTextIsNullOrEmptyMethod = typeof(GenText).GetMethod("NullOrEmpty", BindingFlags.Static | BindingFlags.Public);

	private static readonly MethodInfo GetFieldSetterForTypeMethod = typeof(DirectXmlToObjectNew).GetMethod("GetFieldSetterForType", BindingFlags.Static | BindingFlags.Public);

	private static readonly MethodInfo GetListItemAdderForTypeMethod = typeof(DirectXmlToObjectNew).GetMethod("GetListItemAdderForType", BindingFlags.Static | BindingFlags.Public);

	private static readonly MethodInfo ResolveFieldForNodeMethod = typeof(DirectXmlToObjectNew).GetMethod("ResolveFieldForNode", BindingFlags.Static | BindingFlags.Public);

	private static readonly MethodInfo ResolveTypeForNodeFromFieldMethod = typeof(DirectXmlToObjectNew).GetMethod("ResolveTypeForNode", BindingFlags.Static | BindingFlags.Public, null, new Type[2]
	{
		typeof(FieldInfo),
		typeof(XmlNode)
	}, null);

	private static readonly MethodInfo ResolveTypeForNodeFromTypeMethod = typeof(DirectXmlToObjectNew).GetMethod("ResolveTypeForNode", BindingFlags.Static | BindingFlags.Public, null, new Type[2]
	{
		typeof(Type),
		typeof(XmlNode)
	}, null);

	private static readonly MethodInfo MakeInstanceOfTypeForEmptyNodeMethod = typeof(DirectXmlToObjectNew).GetMethod("MakeInstanceOfTypeForEmptyNode", BindingFlags.Static | BindingFlags.Public);

	private static readonly MethodInfo GetNodeOnlyChildMethod = typeof(DirectXmlToObjectNew).GetMethod("GetNodeOnlyChild", BindingFlags.Static | BindingFlags.Public);

	private static readonly MethodInfo ValidateMayRequiresMethod = typeof(DirectXmlToObjectNew).GetMethod("ValidateMayRequires", BindingFlags.Static | BindingFlags.Public);

	private static readonly MethodInfo ParseValueAndSetFieldDelegateInvokeMethod = typeof(ParseValueAndSetFieldDelegate).GetMethod("Invoke");

	private static readonly MethodInfo ParseValueAndAddListItemDelegateInvokeMethod = typeof(ParseValueAndAddListItemDelegate).GetMethod("Invoke");

	private static readonly MethodInfo ParseValueAndReturnDefDelegateInvokeMethod = typeof(ParseValueAndReturnDefDelegate).GetMethod("Invoke");

	private static readonly ConstructorInfo InvalidOperationExceptionStringConstructor = typeof(InvalidOperationException).GetConstructor(new Type[1] { typeof(string) });

	private static readonly Dictionary<Type, ParseValueAndSetFieldDelegate> parseMethods = new Dictionary<Type, ParseValueAndSetFieldDelegate>();

	private static readonly Dictionary<Type, ParseValueAndAddListItemDelegate> parseListItemMethods = new Dictionary<Type, ParseValueAndAddListItemDelegate>();

	private static readonly Dictionary<Type, ParseValueAndReturnDefDelegate> parseDefMethods = new Dictionary<Type, ParseValueAndReturnDefDelegate>();

	public static Def DefFromNodeNew(XmlNode node, LoadableXmlAsset loadingAsset)
	{
		if (node.NodeType != XmlNodeType.Element)
		{
			return null;
		}
		XmlAttribute xmlAttribute = node.Attributes["Abstract"];
		if (xmlAttribute != null && xmlAttribute.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
		{
			return null;
		}
		XmlNode resolvedNodeFor = XmlInheritance.GetResolvedNodeFor(node);
		string text = node.Name;
		XmlAttribute xmlAttribute2 = resolvedNodeFor.Attributes["Class"];
		if (xmlAttribute2 != null)
		{
			text = xmlAttribute2.Value;
		}
		Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly(text);
		if (typeInAnyAssembly == null || !GenTypes.IsDef(typeInAnyAssembly))
		{
			Log.ErrorOnce("Type " + text + " is not a Def type or could not be found, in file " + ((loadingAsset != null) ? loadingAsset.name : "(unknown)") + ". Context: " + node.OuterXml, text.GetHashCode());
			return null;
		}
		Def def = null;
		try
		{
			ParseValueAndReturnDefDelegate defParserForType = GetDefParserForType(typeInAnyAssembly);
			DeepProfiler.Start($"ParseValueAndReturnDef (for {typeInAnyAssembly})");
			def = defParserForType(0, 0, node, typeInAnyAssembly);
			DeepProfiler.End();
			def.ResolveDefNameHash();
		}
		catch (Exception arg)
		{
			Log.Error(string.Format("Exception loading def from file {0}: {1}", (loadingAsset != null) ? loadingAsset.name : "(unknown)", arg));
		}
		return def;
	}

	public static ParseValueAndSetFieldDelegate GetFieldSetterForType(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type", "Cannot get field setter for null type.");
		}
		if (TypeCanBeDeserializedWithSharedBody(type))
		{
			type = typeof(object);
		}
		if (parseMethods.TryGetValue(type, out var value))
		{
			return value;
		}
		DeepProfiler.Start("CreateFieldSetterForType");
		value = CreateFieldSetterForType(type);
		DeepProfiler.End();
		parseMethods[type] = value;
		return value;
	}

	public static ParseValueAndAddListItemDelegate GetListItemAdderForType(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type", "Cannot get list item adder for null type.");
		}
		if (TypeCanBeDeserializedWithSharedBody(type))
		{
			type = typeof(object);
		}
		if (parseListItemMethods.TryGetValue(type, out var value))
		{
			return value;
		}
		DeepProfiler.Start("CreateListItemAdderForType");
		value = CreateListItemAdderForType(type);
		DeepProfiler.End();
		parseListItemMethods[type] = value;
		return value;
	}

	public static ParseValueAndReturnDefDelegate GetDefParserForType(Type type)
	{
		if (TypeCanBeDeserializedWithSharedBody(type))
		{
			type = typeof(Def);
		}
		if (parseDefMethods.TryGetValue(type, out var value))
		{
			return value;
		}
		value = CreateDefParserForType(type);
		parseDefMethods[type] = value;
		return value;
	}

	private static ParseValueAndSetFieldDelegate CreateFieldSetterForType(Type type)
	{
		if (XmlToObjectUtils.CustomDataLoadMethodOf(type) != null)
		{
			return CreateFieldSetterForCustomLoadable(type);
		}
		if (GenTypes.IsSlateRef(type))
		{
			return CreateFieldSetterForSlateRef(type);
		}
		if (type == typeof(string))
		{
			return CreateFieldSetterForString();
		}
		if (GenTypes.HasFlagsAttribute(type))
		{
			return CreateFieldSetterForFlagsEnum(type);
		}
		if (GenTypes.IsList(type))
		{
			return CreateFieldSetterForList(type);
		}
		if (GenTypes.IsDictionary(type))
		{
			return CreateFieldSetterForDict(type);
		}
		return CreateFieldSetterForGeneralType(type);
	}

	private static ParseValueAndAddListItemDelegate CreateListItemAdderForType(Type type)
	{
		if (XmlToObjectUtils.CustomDataLoadMethodOf(type) != null)
		{
			return CreateListItemAdderForCustomLoadable(type);
		}
		if (GenTypes.IsSlateRef(type))
		{
			return CreateListItemAdderForSlateRef(type);
		}
		if (type == typeof(string))
		{
			return CreateListItemAdderForString();
		}
		if (GenTypes.HasFlagsAttribute(type))
		{
			return CreateListItemAdderForFlagsEnum(type);
		}
		if (GenTypes.IsList(type))
		{
			return CreateListItemAdderForList(type);
		}
		if (GenTypes.IsDictionary(type))
		{
			return CreateListItemAdderForDict(type);
		}
		return CreateListItemAdderForGeneralType(type);
	}

	private static ParseValueAndReturnDefDelegate CreateDefParserForType(Type type)
	{
		if (!GenTypes.IsDef(type))
		{
			throw new InvalidOperationException("Cannot create a Def parser for type " + type.FullName + ". Only Def types are supported.");
		}
		string text = ((type == typeof(object)) ? "SharedBody" : GetDynamicMethodNameSuffixForType(type));
		DynamicMethod dynamicMethod = new DynamicMethod("ParseAndReturnDef_" + text, type, new Type[4]
		{
			typeof(int),
			typeof(int),
			typeof(XmlNode),
			typeof(Type)
		}, typeof(DummyTypeToHoldDynamicMethods));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		LocalBuilder localBuilder = iLGenerator.DeclareLocal(type);
		EmitIlToCreateAndPopulateComplexType(iLGenerator, type, localBuilder);
		iLGenerator.Emit(OpCodes.Ldloc, localBuilder);
		iLGenerator.Emit(OpCodes.Ret);
		return (ParseValueAndReturnDefDelegate)dynamicMethod.CreateDelegate(typeof(ParseValueAndReturnDefDelegate));
	}

	private static ParseValueAndSetFieldDelegate CreateFieldSetterForCustomLoadable(Type type)
	{
		DynamicMethod dynamicMethod = new DynamicMethod("ParseAndSetCustomLoadableField_" + GetDynamicMethodNameSuffixForType(type), null, new Type[4]
		{
			typeof(object),
			typeof(FieldInfo),
			typeof(XmlNode),
			typeof(Type)
		}, typeof(DummyTypeToHoldDynamicMethods));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		EmitIlToCreateCustomLoadable(iLGenerator, type);
		iLGenerator.Emit(OpCodes.Callvirt, FieldSetValueMethod);
		iLGenerator.Emit(OpCodes.Ret);
		return (ParseValueAndSetFieldDelegate)dynamicMethod.CreateDelegate(typeof(ParseValueAndSetFieldDelegate));
	}

	private static ParseValueAndSetFieldDelegate CreateFieldSetterForSlateRef(Type slateRefType)
	{
		DynamicMethod dynamicMethod = new DynamicMethod("ParseAndSetSlateRefField_" + GetDynamicMethodNameSuffixForType(slateRefType), null, new Type[4]
		{
			typeof(object),
			typeof(FieldInfo),
			typeof(XmlNode),
			typeof(Type)
		}, typeof(DummyTypeToHoldDynamicMethods));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		EmitIlToCreateSlateRef(iLGenerator, slateRefType);
		iLGenerator.Emit(OpCodes.Callvirt, FieldSetValueMethod);
		iLGenerator.Emit(OpCodes.Ret);
		return (ParseValueAndSetFieldDelegate)dynamicMethod.CreateDelegate(typeof(ParseValueAndSetFieldDelegate));
	}

	private static ParseValueAndSetFieldDelegate CreateFieldSetterForString()
	{
		DynamicMethod dynamicMethod = new DynamicMethod("ParseAndSetStringField", null, new Type[4]
		{
			typeof(object),
			typeof(FieldInfo),
			typeof(XmlNode),
			typeof(Type)
		}, typeof(DummyTypeToHoldDynamicMethods));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		EmitIlToCreateString(iLGenerator);
		iLGenerator.Emit(OpCodes.Callvirt, FieldSetValueMethod);
		iLGenerator.Emit(OpCodes.Ret);
		return (ParseValueAndSetFieldDelegate)dynamicMethod.CreateDelegate(typeof(ParseValueAndSetFieldDelegate));
	}

	private static ParseValueAndSetFieldDelegate CreateFieldSetterForFlagsEnum(Type type)
	{
		DynamicMethod dynamicMethod = new DynamicMethod("ParseAndSetFlagsEnumField_" + GetDynamicMethodNameSuffixForType(type), null, new Type[4]
		{
			typeof(object),
			typeof(FieldInfo),
			typeof(XmlNode),
			typeof(Type)
		}, typeof(DummyTypeToHoldDynamicMethods));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		LocalBuilder localBuilder = iLGenerator.DeclareLocal(type);
		Label label = iLGenerator.DefineLabel();
		Label label2 = iLGenerator.DefineLabel();
		EmitIlToHandleSingleTextNodeViaParseHelper(iLGenerator, type, localBuilder, label2, label);
		iLGenerator.MarkLabel(label2);
		EmitIlToParseFlagsEnum(iLGenerator, type);
		iLGenerator.Emit(OpCodes.Unbox_Any, type);
		iLGenerator.Emit(OpCodes.Stloc, localBuilder);
		iLGenerator.MarkLabel(label);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldloc, localBuilder);
		iLGenerator.Emit(OpCodes.Box, type);
		iLGenerator.Emit(OpCodes.Callvirt, FieldSetValueMethod);
		iLGenerator.Emit(OpCodes.Ret);
		return (ParseValueAndSetFieldDelegate)dynamicMethod.CreateDelegate(typeof(ParseValueAndSetFieldDelegate));
	}

	private static ParseValueAndSetFieldDelegate CreateFieldSetterForList(Type listType)
	{
		Type type = listType.GetGenericArguments()[0];
		DynamicMethod dynamicMethod = new DynamicMethod("ParseAndSetListField_" + GetDynamicMethodNameSuffixForType(type), null, new Type[4]
		{
			typeof(object),
			typeof(FieldInfo),
			typeof(XmlNode),
			typeof(Type)
		}, typeof(DummyTypeToHoldDynamicMethods));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		EmitIlToCreateAndPopulateList(iLGenerator, listType, type);
		iLGenerator.Emit(OpCodes.Callvirt, FieldSetValueMethod);
		iLGenerator.Emit(OpCodes.Ret);
		return (ParseValueAndSetFieldDelegate)dynamicMethod.CreateDelegate(typeof(ParseValueAndSetFieldDelegate));
	}

	private static ParseValueAndSetFieldDelegate CreateFieldSetterForDict(Type dictType)
	{
		Type type = dictType.GetGenericArguments()[0];
		Type type2 = dictType.GetGenericArguments()[1];
		DynamicMethod dynamicMethod = new DynamicMethod("ParseAndSetDictField_" + GetDynamicMethodNameSuffixForType(type) + "_" + GetDynamicMethodNameSuffixForType(type2), null, new Type[4]
		{
			typeof(object),
			typeof(FieldInfo),
			typeof(XmlNode),
			typeof(Type)
		}, typeof(DummyTypeToHoldDynamicMethods));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		EmitILToCreateAndPopulateDictionary(iLGenerator, dictType, type, type2);
		iLGenerator.Emit(OpCodes.Callvirt, FieldSetValueMethod);
		iLGenerator.Emit(OpCodes.Ret);
		return (ParseValueAndSetFieldDelegate)dynamicMethod.CreateDelegate(typeof(ParseValueAndSetFieldDelegate));
	}

	private static ParseValueAndSetFieldDelegate CreateFieldSetterForGeneralType(Type type)
	{
		string text = ((type == typeof(object)) ? "SharedBody" : GetDynamicMethodNameSuffixForType(type));
		DynamicMethod dynamicMethod = new DynamicMethod("ParseAndSetComplexTypeField_" + text, null, new Type[4]
		{
			typeof(object),
			typeof(FieldInfo),
			typeof(XmlNode),
			typeof(Type)
		}, typeof(DummyTypeToHoldDynamicMethods));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		Type innerIfNullable = type.GetInnerIfNullable();
		bool flag = innerIfNullable != type;
		LocalBuilder localBuilder = iLGenerator.DeclareLocal(type);
		LocalBuilder localBuilder2 = (flag ? iLGenerator.DeclareLocal(innerIfNullable) : null);
		Label label = iLGenerator.DefineLabel();
		Label label2 = iLGenerator.DefineLabel();
		Label label3 = iLGenerator.DefineLabel();
		Label label4 = iLGenerator.DefineLabel();
		Label label5 = iLGenerator.DefineLabel();
		EmitIlToHandleEmptyNode(iLGenerator, type, localBuilder, label3, label2);
		iLGenerator.MarkLabel(label3);
		EmitIlToErrorAndMakeDefaultValueForCdata(iLGenerator, type, label4, label, localBuilder);
		iLGenerator.MarkLabel(label4);
		if (ParseHelper.HandlesType(type))
		{
			EmitIlToHandleSingleTextNodeViaParseHelper(iLGenerator, innerIfNullable, flag ? localBuilder2 : localBuilder, label5, label);
		}
		iLGenerator.MarkLabel(label5);
		bool num = !innerIfNullable.IsPrimitive && !innerIfNullable.IsEnum && innerIfNullable != typeof(Type);
		bool flag2 = innerIfNullable.IsValueType || innerIfNullable.GetConstructor(Type.EmptyTypes) != null;
		if (num && flag2)
		{
			EmitIlToCreateAndPopulateComplexType(iLGenerator, innerIfNullable, flag ? localBuilder2 : localBuilder);
		}
		iLGenerator.MarkLabel(label);
		if (flag)
		{
			iLGenerator.Emit(OpCodes.Ldloc, localBuilder2);
			iLGenerator.Emit(OpCodes.Newobj, type.GetConstructor(new Type[1] { innerIfNullable }));
			iLGenerator.Emit(OpCodes.Stloc, localBuilder);
		}
		iLGenerator.MarkLabel(label2);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldloc, localBuilder);
		if (type.IsValueType)
		{
			iLGenerator.Emit(OpCodes.Box, type);
		}
		iLGenerator.Emit(OpCodes.Callvirt, FieldSetValueMethod);
		iLGenerator.Emit(OpCodes.Ret);
		return (ParseValueAndSetFieldDelegate)dynamicMethod.CreateDelegate(typeof(ParseValueAndSetFieldDelegate));
	}

	private static ParseValueAndAddListItemDelegate CreateListItemAdderForCustomLoadable(Type type)
	{
		DynamicMethod dynamicMethod = new DynamicMethod("ParseAndAddCustomLoadableToList_" + GetDynamicMethodNameSuffixForType(type), null, new Type[4]
		{
			typeof(object),
			typeof(int),
			typeof(XmlNode),
			typeof(Type)
		}, typeof(DummyTypeToHoldDynamicMethods));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		EmitIlToCreateCustomLoadable(iLGenerator, type);
		iLGenerator.Emit(OpCodes.Callvirt, IListAddItemMethod);
		iLGenerator.Emit(OpCodes.Pop);
		iLGenerator.Emit(OpCodes.Ret);
		return (ParseValueAndAddListItemDelegate)dynamicMethod.CreateDelegate(typeof(ParseValueAndAddListItemDelegate));
	}

	private static ParseValueAndAddListItemDelegate CreateListItemAdderForSlateRef(Type slateRefType)
	{
		DynamicMethod dynamicMethod = new DynamicMethod("ParseAndAddSlateRefToList_" + GetDynamicMethodNameSuffixForType(slateRefType), null, new Type[4]
		{
			typeof(object),
			typeof(int),
			typeof(XmlNode),
			typeof(Type)
		}, typeof(DummyTypeToHoldDynamicMethods));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		EmitIlToCreateSlateRef(iLGenerator, slateRefType);
		iLGenerator.Emit(OpCodes.Callvirt, IListAddItemMethod);
		iLGenerator.Emit(OpCodes.Pop);
		iLGenerator.Emit(OpCodes.Ret);
		return (ParseValueAndAddListItemDelegate)dynamicMethod.CreateDelegate(typeof(ParseValueAndAddListItemDelegate));
	}

	private static ParseValueAndAddListItemDelegate CreateListItemAdderForString()
	{
		DynamicMethod dynamicMethod = new DynamicMethod("ParseAndAddStringToList", null, new Type[4]
		{
			typeof(object),
			typeof(int),
			typeof(XmlNode),
			typeof(Type)
		}, typeof(DummyTypeToHoldDynamicMethods));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		EmitIlToCreateString(iLGenerator);
		iLGenerator.Emit(OpCodes.Callvirt, IListAddItemMethod);
		iLGenerator.Emit(OpCodes.Pop);
		iLGenerator.Emit(OpCodes.Ret);
		return (ParseValueAndAddListItemDelegate)dynamicMethod.CreateDelegate(typeof(ParseValueAndAddListItemDelegate));
	}

	private static ParseValueAndAddListItemDelegate CreateListItemAdderForFlagsEnum(Type type)
	{
		DynamicMethod dynamicMethod = new DynamicMethod("ParseAndAddFlagsEnumToList_" + GetDynamicMethodNameSuffixForType(type), null, new Type[4]
		{
			typeof(object),
			typeof(int),
			typeof(XmlNode),
			typeof(Type)
		}, typeof(DummyTypeToHoldDynamicMethods));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		EmitIlToParseFlagsEnum(iLGenerator, type);
		iLGenerator.Emit(OpCodes.Callvirt, IListAddItemMethod);
		iLGenerator.Emit(OpCodes.Pop);
		iLGenerator.Emit(OpCodes.Ret);
		return (ParseValueAndAddListItemDelegate)dynamicMethod.CreateDelegate(typeof(ParseValueAndAddListItemDelegate));
	}

	private static ParseValueAndAddListItemDelegate CreateListItemAdderForList(Type listType)
	{
		Type type = listType.GetGenericArguments()[0];
		DynamicMethod dynamicMethod = new DynamicMethod("ParseAndAddListToList_" + GetDynamicMethodNameSuffixForType(type), null, new Type[4]
		{
			typeof(object),
			typeof(int),
			typeof(XmlNode),
			typeof(Type)
		}, typeof(DummyTypeToHoldDynamicMethods));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		EmitIlToCreateAndPopulateList(iLGenerator, listType, type);
		iLGenerator.Emit(OpCodes.Callvirt, IListAddItemMethod);
		iLGenerator.Emit(OpCodes.Pop);
		iLGenerator.Emit(OpCodes.Ret);
		return (ParseValueAndAddListItemDelegate)dynamicMethod.CreateDelegate(typeof(ParseValueAndAddListItemDelegate));
	}

	private static ParseValueAndAddListItemDelegate CreateListItemAdderForDict(Type dictType)
	{
		Type keyType = dictType.GetGenericArguments()[0];
		Type valueType = dictType.GetGenericArguments()[1];
		DynamicMethod dynamicMethod = new DynamicMethod("ParseAndAddDictToList_" + GetDynamicMethodNameSuffixForType(dictType), null, new Type[4]
		{
			typeof(object),
			typeof(int),
			typeof(XmlNode),
			typeof(Type)
		}, typeof(DummyTypeToHoldDynamicMethods));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		EmitILToCreateAndPopulateDictionary(iLGenerator, dictType, keyType, valueType);
		iLGenerator.Emit(OpCodes.Callvirt, IListAddItemMethod);
		iLGenerator.Emit(OpCodes.Pop);
		iLGenerator.Emit(OpCodes.Ret);
		return (ParseValueAndAddListItemDelegate)dynamicMethod.CreateDelegate(typeof(ParseValueAndAddListItemDelegate));
	}

	private static ParseValueAndAddListItemDelegate CreateListItemAdderForGeneralType(Type type)
	{
		string text = ((type == typeof(object)) ? "SharedBody" : GetDynamicMethodNameSuffixForType(type));
		DynamicMethod dynamicMethod = new DynamicMethod("ParseAndAddComplexTypeToList_" + text, null, new Type[4]
		{
			typeof(object),
			typeof(int),
			typeof(XmlNode),
			typeof(Type)
		}, typeof(DummyTypeToHoldDynamicMethods));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		LocalBuilder localBuilder = iLGenerator.DeclareLocal(type);
		Label label = iLGenerator.DefineLabel();
		Label label2 = iLGenerator.DefineLabel();
		Label label3 = iLGenerator.DefineLabel();
		Label label4 = iLGenerator.DefineLabel();
		EmitIlToHandleEmptyNode(iLGenerator, type, localBuilder, label2, label);
		iLGenerator.MarkLabel(label2);
		EmitIlToErrorAndMakeDefaultValueForCdata(iLGenerator, type, label3, label, localBuilder);
		iLGenerator.MarkLabel(label3);
		if (ParseHelper.HandlesType(type))
		{
			EmitIlToHandleSingleTextNodeViaParseHelper(iLGenerator, type, localBuilder, label4, label);
		}
		iLGenerator.MarkLabel(label4);
		EmitIlToCreateAndPopulateComplexType(iLGenerator, type, localBuilder);
		iLGenerator.MarkLabel(label);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldloc, localBuilder);
		if (type.IsValueType)
		{
			iLGenerator.Emit(OpCodes.Box, type);
		}
		iLGenerator.Emit(OpCodes.Callvirt, IListAddItemMethod);
		iLGenerator.Emit(OpCodes.Pop);
		iLGenerator.Emit(OpCodes.Ret);
		return (ParseValueAndAddListItemDelegate)dynamicMethod.CreateDelegate(typeof(ParseValueAndAddListItemDelegate));
	}

	private static void EmitIlToCreateCustomLoadable(ILGenerator il, Type typeToInstantiate)
	{
		ConstructorInfo constructor = typeToInstantiate.GetConstructor(Type.EmptyTypes);
		MethodInfo methodInfo = XmlToObjectUtils.CustomDataLoadMethodOf(typeToInstantiate);
		if (constructor == null)
		{
			throw new InvalidOperationException("Type " + typeToInstantiate.FullName + " does not have a parameterless constructor, but needs one to use LoadDataFromXmlCustom.");
		}
		if (methodInfo == null)
		{
			throw new InvalidOperationException("Type " + typeToInstantiate.FullName + " does not have a method named LoadDataFromXmlCustom, but we are trying to create a parser for it using that method.");
		}
		il.Emit(OpCodes.Newobj, constructor);
		il.Emit(OpCodes.Dup);
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Call, XmlInheritanceGetResolvedNodeForMethod);
		il.Emit(OpCodes.Call, methodInfo);
	}

	private static void EmitIlToCreateSlateRef(ILGenerator il, Type slateRefType)
	{
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Call, InnerTextWithReplacedNewlinesOrXmlMethod);
		il.Emit(OpCodes.Ldtoken, slateRefType);
		il.Emit(OpCodes.Call, TypeGetTypeFromHandleMethod);
		il.Emit(OpCodes.Call, ParseHelperFromStringNonGenericMethod);
	}

	private static void EmitIlToCreateString(ILGenerator il)
	{
		LocalBuilder local = il.DeclareLocal(typeof(XmlNodeType));
		LocalBuilder local2 = il.DeclareLocal(typeof(string));
		Label label = il.DefineLabel();
		Label label2 = il.DefineLabel();
		Label label3 = il.DefineLabel();
		Label label4 = il.DefineLabel();
		Label label5 = il.DefineLabel();
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Callvirt, XmlNodeHasChildNodesMethod);
		il.Emit(OpCodes.Brtrue, label);
		il.Emit(OpCodes.Ldstr, "");
		il.Emit(OpCodes.Stloc, local2);
		il.Emit(OpCodes.Br, label5);
		il.MarkLabel(label);
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Callvirt, XmlNodeGetChildNodesMethod);
		il.Emit(OpCodes.Callvirt, XmlNodeListGetCountMethod);
		il.Emit(OpCodes.Ldc_I4_1);
		il.Emit(OpCodes.Beq, label2);
		il.Emit(OpCodes.Ldstr, "XML node has more than one child node, which is unsupported for string parsing. Context: ");
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Callvirt, XmlNodeGetOuterXmlMethod);
		il.Emit(OpCodes.Call, StringConcatMethod);
		il.Emit(OpCodes.Newobj, InvalidOperationExceptionStringConstructor);
		il.Emit(OpCodes.Throw);
		il.MarkLabel(label2);
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Callvirt, XmlNodeGetFirstChildMethod);
		il.Emit(OpCodes.Callvirt, XmlNodeGetNodeTypeMethod);
		il.Emit(OpCodes.Dup);
		il.Emit(OpCodes.Stloc, local);
		il.Emit(OpCodes.Ldc_I4, 3);
		il.Emit(OpCodes.Beq, label3);
		il.Emit(OpCodes.Ldloc, local);
		il.Emit(OpCodes.Ldc_I4, 4);
		il.Emit(OpCodes.Beq, label4);
		il.Emit(OpCodes.Ldstr, "XML node has an unsupported child node type to parse as string. Expected Text or CDATA. Context: ");
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Callvirt, XmlNodeGetOuterXmlMethod);
		il.Emit(OpCodes.Call, StringConcatMethod);
		il.Emit(OpCodes.Newobj, InvalidOperationExceptionStringConstructor);
		il.Emit(OpCodes.Throw);
		il.MarkLabel(label3);
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Callvirt, XmlNodeGetInnerTextMethod);
		il.Emit(OpCodes.Ldtoken, typeof(string));
		il.Emit(OpCodes.Call, TypeGetTypeFromHandleMethod);
		il.Emit(OpCodes.Call, ParseHelperFromStringNonGenericMethod);
		il.Emit(OpCodes.Castclass, typeof(string));
		il.Emit(OpCodes.Stloc, local2);
		il.Emit(OpCodes.Br, label5);
		il.MarkLabel(label4);
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Callvirt, XmlNodeGetFirstChildMethod);
		il.Emit(OpCodes.Callvirt, XmlNodeGetValueMethod);
		il.Emit(OpCodes.Stloc, local2);
		il.Emit(OpCodes.Br, label5);
		il.MarkLabel(label5);
		il.Emit(OpCodes.Ldloc, local2);
	}

	private static void EmitIlToParseFlagsEnum(ILGenerator il, Type enumType)
	{
		LocalBuilder local = il.DeclareLocal(typeof(XmlNode));
		LocalBuilder local2 = il.DeclareLocal(enumType);
		LocalBuilder local3 = il.DeclareLocal(typeof(XmlNodeList));
		LocalBuilder local4 = il.DeclareLocal(typeof(int));
		LocalBuilder local5 = il.DeclareLocal(typeof(int));
		LocalBuilder local6 = il.DeclareLocal(enumType);
		Label label = il.DefineLabel();
		Label label2 = il.DefineLabel();
		Label label3 = il.DefineLabel();
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Callvirt, XmlNodeGetChildNodesMethod);
		il.Emit(OpCodes.Dup);
		il.Emit(OpCodes.Stloc, local3);
		il.Emit(OpCodes.Callvirt, XmlNodeListGetCountMethod);
		il.Emit(OpCodes.Stloc, local4);
		il.Emit(OpCodes.Ldc_I4_0);
		il.Emit(OpCodes.Stloc, local2);
		il.Emit(OpCodes.Ldc_I4_0);
		il.Emit(OpCodes.Stloc, local6);
		il.Emit(OpCodes.Ldc_I4_0);
		il.Emit(OpCodes.Stloc, local5);
		il.MarkLabel(label);
		il.Emit(OpCodes.Ldloc, local5);
		il.Emit(OpCodes.Ldloc, local4);
		il.Emit(OpCodes.Bge, label3);
		il.Emit(OpCodes.Ldloc, local3);
		il.Emit(OpCodes.Ldloc, local5);
		il.Emit(OpCodes.Callvirt, XmlNodeListGetItemMethod);
		il.Emit(OpCodes.Stloc, local);
		il.Emit(OpCodes.Ldloc, local);
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Ldtoken, enumType);
		il.Emit(OpCodes.Call, TypeGetTypeFromHandleMethod);
		il.Emit(OpCodes.Call, ValidateListNodeMethod);
		il.Emit(OpCodes.Brfalse, label2);
		il.Emit(OpCodes.Ldloc, local);
		il.Emit(OpCodes.Callvirt, XmlNodeGetInnerTextMethod);
		il.Emit(OpCodes.Ldtoken, enumType);
		il.Emit(OpCodes.Call, TypeGetTypeFromHandleMethod);
		il.Emit(OpCodes.Call, ParseHelperFromStringNonGenericMethod);
		il.Emit(OpCodes.Unbox_Any, enumType);
		il.Emit(OpCodes.Stloc, local6);
		il.Emit(OpCodes.Ldloc, local2);
		il.Emit(OpCodes.Ldloc, local6);
		il.Emit(OpCodes.Or);
		il.Emit(OpCodes.Stloc, local2);
		il.MarkLabel(label2);
		il.Emit(OpCodes.Ldloc, local5);
		il.Emit(OpCodes.Ldc_I4_1);
		il.Emit(OpCodes.Add);
		il.Emit(OpCodes.Stloc, local5);
		il.Emit(OpCodes.Br, label);
		il.MarkLabel(label3);
		il.Emit(OpCodes.Ldloc, local2);
		il.Emit(OpCodes.Box, enumType);
	}

	private static void EmitIlToCreateAndPopulateList(ILGenerator il, Type listType, Type itemType)
	{
		ConstructorInfo constructor = listType.GetConstructor(new Type[1] { typeof(int) });
		bool num = GenTypes.IsDef(itemType);
		LocalBuilder local = il.DeclareLocal(listType);
		LocalBuilder local2 = il.DeclareLocal(typeof(XmlNodeList));
		LocalBuilder local3 = il.DeclareLocal(typeof(int));
		LocalBuilder local4 = il.DeclareLocal(typeof(int));
		LocalBuilder local5 = il.DeclareLocal(typeof(XmlNode));
		LocalBuilder local6 = il.DeclareLocal(typeof(XmlAttribute));
		LocalBuilder local7 = il.DeclareLocal(typeof(XmlAttribute));
		LocalBuilder local8 = il.DeclareLocal(typeof(XmlAttribute));
		LocalBuilder local9 = il.DeclareLocal(typeof(string));
		LocalBuilder local10 = il.DeclareLocal(typeof(string));
		il.DeclareLocal(typeof(XmlAttribute));
		il.DeclareLocal(typeof(string));
		Label label = il.DefineLabel();
		Label label2 = il.DefineLabel();
		Label label3 = il.DefineLabel();
		Label label4 = il.DefineLabel();
		Label label5 = il.DefineLabel();
		Label label6 = il.DefineLabel();
		LocalBuilder local11 = il.DeclareLocal(typeof(Type));
		il.DefineLabel();
		Label loc = il.DefineLabel();
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Callvirt, XmlNodeGetAttributesMethod);
		il.Emit(OpCodes.Ldstr, "IsNull");
		il.Emit(OpCodes.Callvirt, XmlAttributeCollectionGetItemByNameMethod);
		il.Emit(OpCodes.Dup);
		il.Emit(OpCodes.Stloc, local6);
		il.Emit(OpCodes.Brfalse, label);
		il.Emit(OpCodes.Ldloc, local6);
		il.Emit(OpCodes.Callvirt, XmlAttributeGetValueMethod);
		il.Emit(OpCodes.Ldstr, "true");
		il.Emit(OpCodes.Ldc_I4, 3);
		il.Emit(OpCodes.Call, StringEqualsWithComparisonModeMethod);
		il.Emit(OpCodes.Brfalse, label);
		il.Emit(OpCodes.Ldnull);
		il.Emit(OpCodes.Stloc, local);
		il.Emit(OpCodes.Br, label4);
		il.MarkLabel(label);
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Callvirt, XmlNodeGetChildNodesMethod);
		il.Emit(OpCodes.Dup);
		il.Emit(OpCodes.Stloc, local2);
		il.Emit(OpCodes.Callvirt, XmlNodeListGetCountMethod);
		il.Emit(OpCodes.Dup);
		il.Emit(OpCodes.Stloc, local3);
		il.Emit(OpCodes.Newobj, constructor);
		il.Emit(OpCodes.Stloc, local);
		il.Emit(OpCodes.Ldc_I4_0);
		il.Emit(OpCodes.Stloc, local4);
		il.MarkLabel(label2);
		il.Emit(OpCodes.Ldloc, local4);
		il.Emit(OpCodes.Ldloc, local3);
		il.Emit(OpCodes.Bge, label4);
		il.Emit(OpCodes.Ldloc, local2);
		il.Emit(OpCodes.Ldloc, local4);
		il.Emit(OpCodes.Callvirt, XmlNodeListGetItemMethod);
		il.Emit(OpCodes.Stloc, local5);
		il.Emit(OpCodes.Ldloc, local5);
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Ldtoken, itemType);
		il.Emit(OpCodes.Call, TypeGetTypeFromHandleMethod);
		il.Emit(OpCodes.Call, ValidateListNodeMethod);
		il.Emit(OpCodes.Brfalse, label3);
		il.Emit(OpCodes.Ldnull);
		il.Emit(OpCodes.Stloc, local9);
		il.Emit(OpCodes.Ldnull);
		il.Emit(OpCodes.Stloc, local10);
		il.Emit(OpCodes.Ldloc, local5);
		il.Emit(OpCodes.Callvirt, XmlNodeGetAttributesMethod);
		il.Emit(OpCodes.Ldstr, "MayRequire");
		il.Emit(OpCodes.Callvirt, XmlAttributeCollectionGetItemByNameMethod);
		il.Emit(OpCodes.Dup);
		il.Emit(OpCodes.Stloc, local7);
		il.Emit(OpCodes.Brfalse, label5);
		il.Emit(OpCodes.Ldloc, local7);
		il.Emit(OpCodes.Callvirt, XmlAttributeGetValueMethod);
		il.Emit(OpCodes.Stloc, local9);
		il.MarkLabel(label5);
		il.Emit(OpCodes.Ldloc, local5);
		il.Emit(OpCodes.Callvirt, XmlNodeGetAttributesMethod);
		il.Emit(OpCodes.Ldstr, "MayRequireAnyOf");
		il.Emit(OpCodes.Callvirt, XmlAttributeCollectionGetItemByNameMethod);
		il.Emit(OpCodes.Dup);
		il.Emit(OpCodes.Stloc, local8);
		il.Emit(OpCodes.Brfalse, label6);
		il.Emit(OpCodes.Ldloc, local8);
		il.Emit(OpCodes.Callvirt, XmlAttributeGetValueMethod);
		il.Emit(OpCodes.Stloc, local10);
		il.MarkLabel(label6);
		if (num)
		{
			il.Emit(OpCodes.Ldloc, local);
			il.Emit(OpCodes.Castclass, listType);
			il.Emit(OpCodes.Ldloc, local5);
			il.Emit(OpCodes.Callvirt, XmlNodeGetInnerTextMethod);
			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Callvirt, XmlNodeGetNameMethod);
			il.Emit(OpCodes.Box, typeof(string));
			il.Emit(OpCodes.Ldloc, local9);
			il.Emit(OpCodes.Ldloc, local10);
			il.Emit(OpCodes.Call, RegisterListWantsCrossRefMethod.MakeGenericMethod(itemType));
		}
		else
		{
			il.Emit(OpCodes.Ldloc, local9);
			il.Emit(OpCodes.Ldloc, local10);
			il.Emit(OpCodes.Call, ValidateMayRequiresMethod);
			il.Emit(OpCodes.Brfalse, label3);
			il.Emit(OpCodes.Ldtoken, itemType);
			il.Emit(OpCodes.Call, TypeGetTypeFromHandleMethod);
			il.Emit(OpCodes.Ldloc, local5);
			il.Emit(OpCodes.Call, ResolveTypeForNodeFromTypeMethod);
			il.Emit(OpCodes.Stloc, local11);
			il.MarkLabel(loc);
			il.Emit(OpCodes.Ldloc, local11);
			il.Emit(OpCodes.Call, GetListItemAdderForTypeMethod);
			il.Emit(OpCodes.Ldloc, local);
			il.Emit(OpCodes.Ldc_I4_0);
			il.Emit(OpCodes.Ldloc, local5);
			il.Emit(OpCodes.Ldloc, local11);
			il.Emit(OpCodes.Call, ParseValueAndAddListItemDelegateInvokeMethod);
		}
		il.MarkLabel(label3);
		il.Emit(OpCodes.Ldloc, local4);
		il.Emit(OpCodes.Ldc_I4_1);
		il.Emit(OpCodes.Add);
		il.Emit(OpCodes.Stloc, local4);
		il.Emit(OpCodes.Br, label2);
		il.MarkLabel(label4);
		il.Emit(OpCodes.Ldloc, local);
	}

	private static void EmitILToCreateAndPopulateDictionary(ILGenerator il, Type dictionaryType, Type keyType, Type valueType)
	{
		bool num = GenTypes.IsDef(keyType);
		bool flag = GenTypes.IsDef(valueType);
		bool flag2 = !num && !flag;
		Type type = typeof(KeyValuePair<, >).MakeGenericType(keyType, valueType);
		MethodInfo getMethod = type.GetProperty("Key").GetGetMethod();
		MethodInfo getMethod2 = type.GetProperty("Value").GetGetMethod();
		ConstructorInfo constructor = dictionaryType.GetConstructor(new Type[1] { typeof(int) });
		LocalBuilder local = il.DeclareLocal(dictionaryType);
		LocalBuilder local2 = il.DeclareLocal(typeof(XmlNodeList));
		LocalBuilder local3 = il.DeclareLocal(typeof(int));
		LocalBuilder local4 = il.DeclareLocal(typeof(int));
		LocalBuilder local5 = il.DeclareLocal(typeof(XmlNode));
		LocalBuilder local6 = il.DeclareLocal(typeof(XmlNode));
		LocalBuilder local7 = il.DeclareLocal(typeof(XmlNode));
		LocalBuilder local8 = il.DeclareLocal(typeof(XmlAttribute));
		LocalBuilder local9 = il.DeclareLocal(typeof(ParseValueAndSetFieldDelegate));
		LocalBuilder local10 = il.DeclareLocal(typeof(ParseValueAndSetFieldDelegate));
		LocalBuilder local11 = il.DeclareLocal(type);
		LocalBuilder local12 = il.DeclareLocal(typeof(object));
		LocalBuilder local13 = il.DeclareLocal(typeof(FieldInfo));
		LocalBuilder local14 = il.DeclareLocal(typeof(FieldInfo));
		LocalBuilder local15 = il.DeclareLocal(typeof(Type));
		LocalBuilder local16 = il.DeclareLocal(typeof(Type));
		Label label = il.DefineLabel();
		Label label2 = il.DefineLabel();
		Label label3 = il.DefineLabel();
		Label label4 = il.DefineLabel();
		if (flag2)
		{
			il.Emit(OpCodes.Ldloca, local11);
			il.Emit(OpCodes.Initobj, type);
			il.Emit(OpCodes.Ldtoken, type);
			il.Emit(OpCodes.Call, TypeGetTypeFromHandleMethod);
			il.Emit(OpCodes.Ldstr, "key");
			il.Emit(OpCodes.Ldc_I4, 36);
			il.Emit(OpCodes.Callvirt, TypeGetFieldMethod);
			il.Emit(OpCodes.Stloc, local13);
			il.Emit(OpCodes.Ldtoken, type);
			il.Emit(OpCodes.Call, TypeGetTypeFromHandleMethod);
			il.Emit(OpCodes.Ldstr, "value");
			il.Emit(OpCodes.Ldc_I4, 36);
			il.Emit(OpCodes.Callvirt, TypeGetFieldMethod);
			il.Emit(OpCodes.Stloc, local14);
		}
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Callvirt, XmlNodeGetAttributesMethod);
		il.Emit(OpCodes.Ldstr, "IsNull");
		il.Emit(OpCodes.Callvirt, XmlAttributeCollectionGetItemByNameMethod);
		il.Emit(OpCodes.Dup);
		il.Emit(OpCodes.Stloc, local8);
		il.Emit(OpCodes.Brfalse, label4);
		il.Emit(OpCodes.Ldloc, local8);
		il.Emit(OpCodes.Callvirt, XmlAttributeGetValueMethod);
		il.Emit(OpCodes.Ldstr, "true");
		il.Emit(OpCodes.Ldc_I4, 3);
		il.Emit(OpCodes.Call, StringEqualsWithComparisonModeMethod);
		il.Emit(OpCodes.Brfalse, label4);
		il.Emit(OpCodes.Ldnull);
		il.Emit(OpCodes.Stloc, local);
		il.Emit(OpCodes.Br, label3);
		il.MarkLabel(label4);
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Callvirt, XmlNodeGetChildNodesMethod);
		il.Emit(OpCodes.Dup);
		il.Emit(OpCodes.Stloc, local2);
		il.Emit(OpCodes.Callvirt, XmlNodeListGetCountMethod);
		il.Emit(OpCodes.Dup);
		il.Emit(OpCodes.Stloc, local3);
		il.Emit(OpCodes.Newobj, constructor);
		il.Emit(OpCodes.Stloc, local);
		il.Emit(OpCodes.Ldc_I4_0);
		il.Emit(OpCodes.Stloc, local4);
		il.MarkLabel(label);
		il.Emit(OpCodes.Ldloc, local4);
		il.Emit(OpCodes.Ldloc, local3);
		il.Emit(OpCodes.Bge, label3);
		il.Emit(OpCodes.Ldloc, local2);
		il.Emit(OpCodes.Ldloc, local4);
		il.Emit(OpCodes.Callvirt, XmlNodeListGetItemMethod);
		il.Emit(OpCodes.Stloc, local5);
		il.Emit(OpCodes.Ldloc, local5);
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Ldtoken, type);
		il.Emit(OpCodes.Call, TypeGetTypeFromHandleMethod);
		il.Emit(OpCodes.Call, ValidateListNodeMethod);
		il.Emit(OpCodes.Brfalse, label2);
		if (flag2)
		{
			il.Emit(OpCodes.Ldloc, local5);
			il.Emit(OpCodes.Ldstr, "key");
			il.Emit(OpCodes.Callvirt, XmlNodeGetItemByNameMethod);
			il.Emit(OpCodes.Stloc, local6);
			il.Emit(OpCodes.Ldloc, local5);
			il.Emit(OpCodes.Ldstr, "value");
			il.Emit(OpCodes.Callvirt, XmlNodeGetItemByNameMethod);
			il.Emit(OpCodes.Stloc, local7);
			il.Emit(OpCodes.Ldloc, local11);
			il.Emit(OpCodes.Box, type);
			il.Emit(OpCodes.Stloc, local12);
			il.Emit(OpCodes.Ldtoken, keyType);
			il.Emit(OpCodes.Call, TypeGetTypeFromHandleMethod);
			il.Emit(OpCodes.Ldloc, local6);
			il.Emit(OpCodes.Callvirt, ResolveTypeForNodeFromTypeMethod);
			il.Emit(OpCodes.Dup);
			il.Emit(OpCodes.Stloc, local15);
			il.Emit(OpCodes.Call, GetFieldSetterForTypeMethod);
			il.Emit(OpCodes.Stloc, local9);
			il.Emit(OpCodes.Ldloc, local9);
			il.Emit(OpCodes.Ldloc, local12);
			il.Emit(OpCodes.Ldloc, local13);
			il.Emit(OpCodes.Ldloc, local6);
			il.Emit(OpCodes.Ldloc, local15);
			il.Emit(OpCodes.Call, ParseValueAndSetFieldDelegateInvokeMethod);
			il.Emit(OpCodes.Ldtoken, valueType);
			il.Emit(OpCodes.Call, TypeGetTypeFromHandleMethod);
			il.Emit(OpCodes.Ldloc, local7);
			il.Emit(OpCodes.Callvirt, ResolveTypeForNodeFromTypeMethod);
			il.Emit(OpCodes.Dup);
			il.Emit(OpCodes.Stloc, local16);
			il.Emit(OpCodes.Call, GetFieldSetterForTypeMethod);
			il.Emit(OpCodes.Stloc, local10);
			il.Emit(OpCodes.Ldloc, local10);
			il.Emit(OpCodes.Ldloc, local12);
			il.Emit(OpCodes.Ldloc, local14);
			il.Emit(OpCodes.Ldloc, local7);
			il.Emit(OpCodes.Ldloc, local16);
			il.Emit(OpCodes.Call, ParseValueAndSetFieldDelegateInvokeMethod);
			il.Emit(OpCodes.Ldloc, local12);
			il.Emit(OpCodes.Unbox_Any, type);
			il.Emit(OpCodes.Stloc, local11);
			il.Emit(OpCodes.Ldloc, local);
			il.Emit(OpCodes.Ldloca, local11);
			il.Emit(OpCodes.Call, getMethod);
			if (keyType.IsValueType)
			{
				il.Emit(OpCodes.Box, keyType);
			}
			il.Emit(OpCodes.Ldloca, local11);
			il.Emit(OpCodes.Call, getMethod2);
			if (valueType.IsValueType)
			{
				il.Emit(OpCodes.Box, valueType);
			}
			il.Emit(OpCodes.Callvirt, DictionaryAddMethod);
		}
		else
		{
			MethodInfo meth = RegisterDictionaryWantsCrossRefMethod.MakeGenericMethod(keyType, valueType);
			il.Emit(OpCodes.Ldloc, local);
			il.Emit(OpCodes.Ldloc, local5);
			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Callvirt, XmlNodeGetNameMethod);
			il.Emit(OpCodes.Call, meth);
		}
		il.MarkLabel(label2);
		il.Emit(OpCodes.Ldloc, local4);
		il.Emit(OpCodes.Ldc_I4_1);
		il.Emit(OpCodes.Add);
		il.Emit(OpCodes.Stloc, local4);
		il.Emit(OpCodes.Br, label);
		il.MarkLabel(label3);
		il.Emit(OpCodes.Ldloc, local);
	}

	private static void EmitIlToCreateAndPopulateComplexType(ILGenerator il, Type typeBeingDeserialized, LocalBuilder instantiatedObjectLocal)
	{
		LocalBuilder local = il.DeclareLocal(typeof(XmlNodeList));
		LocalBuilder local2 = il.DeclareLocal(typeof(int));
		LocalBuilder local3 = il.DeclareLocal(typeof(int));
		LocalBuilder local4 = il.DeclareLocal(typeof(XmlNode));
		LocalBuilder local5 = il.DeclareLocal(typeof(FieldInfo));
		LocalBuilder local6 = il.DeclareLocal(typeof(Type));
		LocalBuilder localBuilder = il.DeclareLocal(typeof(object));
		LocalBuilder local7 = il.DeclareLocal(typeof(XmlAttribute));
		LocalBuilder local8 = il.DeclareLocal(typeof(XmlAttribute));
		LocalBuilder local9 = il.DeclareLocal(typeof(string));
		LocalBuilder local10 = il.DeclareLocal(typeof(string));
		Label label = il.DefineLabel();
		Label label2 = il.DefineLabel();
		Label label3 = il.DefineLabel();
		Label label4 = il.DefineLabel();
		Label label5 = il.DefineLabel();
		Label label6 = il.DefineLabel();
		Label label7 = il.DefineLabel();
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Call, XmlInheritanceGetResolvedNodeForMethod);
		il.Emit(OpCodes.Starg_S, 2);
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Callvirt, XmlNodeGetChildNodesMethod);
		il.Emit(OpCodes.Dup);
		il.Emit(OpCodes.Stloc, local);
		il.Emit(OpCodes.Callvirt, XmlNodeListGetCountMethod);
		il.Emit(OpCodes.Stloc, local2);
		if (typeBeingDeserialized.IsValueType)
		{
			il.Emit(OpCodes.Ldloca, instantiatedObjectLocal);
			il.Emit(OpCodes.Initobj, typeBeingDeserialized);
		}
		else
		{
			il.Emit(OpCodes.Ldarg_3);
			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Call, MakeInstanceOfTypeForEmptyNodeMethod);
			il.Emit(OpCodes.Stloc, instantiatedObjectLocal);
		}
		il.Emit(OpCodes.Ldc_I4_0);
		il.Emit(OpCodes.Stloc, local3);
		il.MarkLabel(label);
		il.Emit(OpCodes.Ldloc, local3);
		il.Emit(OpCodes.Ldloc, local2);
		il.Emit(OpCodes.Bge, label3);
		if (typeBeingDeserialized.IsValueType)
		{
			il.Emit(OpCodes.Ldloc, instantiatedObjectLocal);
			il.Emit(OpCodes.Box, typeBeingDeserialized);
			il.Emit(OpCodes.Stloc, localBuilder);
		}
		il.Emit(OpCodes.Ldloc, local);
		il.Emit(OpCodes.Ldloc, local3);
		il.Emit(OpCodes.Callvirt, XmlNodeListGetItemMethod);
		il.Emit(OpCodes.Stloc, local4);
		il.Emit(OpCodes.Ldloc, local4);
		il.Emit(OpCodes.Isinst, typeof(XmlComment));
		il.Emit(OpCodes.Brtrue, label2);
		il.Emit(OpCodes.Ldarg_3);
		il.Emit(OpCodes.Ldloc, local4);
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Call, ResolveFieldForNodeMethod);
		il.Emit(OpCodes.Dup);
		il.Emit(OpCodes.Stloc, local5);
		il.Emit(OpCodes.Brfalse, label2);
		il.Emit(OpCodes.Ldloc, local5);
		il.Emit(OpCodes.Ldloc, local4);
		il.Emit(OpCodes.Call, ResolveTypeForNodeFromFieldMethod);
		il.Emit(OpCodes.Stloc, local6);
		il.Emit(OpCodes.Ldloc, local6);
		il.Emit(OpCodes.Call, GenTypesIsDefMethod);
		il.Emit(OpCodes.Brfalse, label4);
		il.Emit(OpCodes.Ldloc, local4);
		il.Emit(OpCodes.Callvirt, XmlNodeGetInnerTextMethod);
		il.Emit(OpCodes.Call, GenTextIsNullOrEmptyMethod);
		il.Emit(OpCodes.Brfalse, label5);
		il.Emit(OpCodes.Ldloc, local5);
		il.Emit(OpCodes.Ldloc, typeBeingDeserialized.IsValueType ? localBuilder : instantiatedObjectLocal);
		il.Emit(OpCodes.Ldnull);
		il.Emit(OpCodes.Callvirt, FieldSetValueMethod);
		il.Emit(OpCodes.Br, label2);
		il.MarkLabel(label5);
		il.Emit(OpCodes.Ldnull);
		il.Emit(OpCodes.Stloc, local9);
		il.Emit(OpCodes.Ldnull);
		il.Emit(OpCodes.Stloc, local10);
		il.Emit(OpCodes.Ldloc, local4);
		il.Emit(OpCodes.Callvirt, XmlNodeGetAttributesMethod);
		il.Emit(OpCodes.Ldstr, "MayRequire");
		il.Emit(OpCodes.Callvirt, XmlAttributeCollectionGetItemByNameMethod);
		il.Emit(OpCodes.Dup);
		il.Emit(OpCodes.Stloc, local7);
		il.Emit(OpCodes.Brfalse, label6);
		il.Emit(OpCodes.Ldloc, local7);
		il.Emit(OpCodes.Callvirt, XmlAttributeGetValueMethod);
		il.Emit(OpCodes.Call, StringToLowerMethod);
		il.Emit(OpCodes.Stloc, local9);
		il.MarkLabel(label6);
		il.Emit(OpCodes.Ldloc, local4);
		il.Emit(OpCodes.Callvirt, XmlNodeGetAttributesMethod);
		il.Emit(OpCodes.Ldstr, "MayRequireAnyOf");
		il.Emit(OpCodes.Callvirt, XmlAttributeCollectionGetItemByNameMethod);
		il.Emit(OpCodes.Dup);
		il.Emit(OpCodes.Stloc, local8);
		il.Emit(OpCodes.Brfalse, label7);
		il.Emit(OpCodes.Ldloc, local8);
		il.Emit(OpCodes.Callvirt, XmlAttributeGetValueMethod);
		il.Emit(OpCodes.Call, StringToLowerMethod);
		il.Emit(OpCodes.Stloc, local10);
		il.MarkLabel(label7);
		il.Emit(OpCodes.Ldloc, typeBeingDeserialized.IsValueType ? localBuilder : instantiatedObjectLocal);
		il.Emit(OpCodes.Ldloc, local5);
		il.Emit(OpCodes.Ldloc, local4);
		il.Emit(OpCodes.Callvirt, XmlNodeGetInnerTextMethod);
		il.Emit(OpCodes.Ldloc, local9);
		il.Emit(OpCodes.Ldloc, local10);
		il.Emit(OpCodes.Ldnull);
		il.Emit(OpCodes.Call, RegisterObjectWantsCrossRefMethod);
		il.Emit(OpCodes.Br, label2);
		il.MarkLabel(label4);
		il.Emit(OpCodes.Ldloc, local6);
		il.Emit(OpCodes.Call, GetFieldSetterForTypeMethod);
		il.Emit(OpCodes.Ldloc, typeBeingDeserialized.IsValueType ? localBuilder : instantiatedObjectLocal);
		il.Emit(OpCodes.Ldloc, local5);
		il.Emit(OpCodes.Ldloc, local4);
		il.Emit(OpCodes.Ldloc, local6);
		il.Emit(OpCodes.Call, ParseValueAndSetFieldDelegateInvokeMethod);
		il.MarkLabel(label2);
		if (typeBeingDeserialized.IsValueType)
		{
			il.Emit(OpCodes.Ldloc, localBuilder);
			il.Emit(OpCodes.Unbox_Any, typeBeingDeserialized);
			il.Emit(OpCodes.Stloc, instantiatedObjectLocal);
		}
		il.Emit(OpCodes.Ldloc, local3);
		il.Emit(OpCodes.Ldc_I4_1);
		il.Emit(OpCodes.Add);
		il.Emit(OpCodes.Stloc, local3);
		il.Emit(OpCodes.Br, label);
		il.MarkLabel(label3);
		MethodInfo methodInfo = XmlToObjectUtils.PostLoadMethodOf(typeBeingDeserialized);
		if ((object)methodInfo != null)
		{
			il.Emit(OpCodes.Ldloc, instantiatedObjectLocal);
			il.Emit(OpCodes.Call, methodInfo);
		}
		else
		{
			il.Emit(OpCodes.Nop);
		}
	}

	private static void EmitIlToHandleEmptyNode(ILGenerator il, Type typeBeingDeserialized, LocalBuilder localForResultValue, Label labelToJumpIfNotEmpty, Label endLabel)
	{
		LocalBuilder local = il.DeclareLocal(typeof(XmlAttribute));
		Label label = il.DefineLabel();
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Callvirt, XmlNodeGetAttributesMethod);
		il.Emit(OpCodes.Ldstr, "IsNull");
		il.Emit(OpCodes.Callvirt, XmlAttributeCollectionGetItemByNameMethod);
		il.Emit(OpCodes.Dup);
		il.Emit(OpCodes.Stloc, local);
		il.Emit(OpCodes.Brfalse, label);
		il.Emit(OpCodes.Ldloc, local);
		il.Emit(OpCodes.Callvirt, XmlAttributeGetValueMethod);
		il.Emit(OpCodes.Ldstr, "true");
		il.Emit(OpCodes.Ldc_I4, 3);
		il.Emit(OpCodes.Call, StringEqualsWithComparisonModeMethod);
		il.Emit(OpCodes.Brfalse, label);
		if (typeBeingDeserialized.IsValueType)
		{
			il.Emit(OpCodes.Ldloca, localForResultValue);
			il.Emit(OpCodes.Initobj, typeBeingDeserialized);
		}
		else
		{
			il.Emit(OpCodes.Ldnull);
			il.Emit(OpCodes.Stloc, localForResultValue);
		}
		il.Emit(OpCodes.Br, endLabel);
		il.MarkLabel(label);
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Callvirt, XmlNodeHasChildNodesMethod);
		il.Emit(OpCodes.Brtrue, labelToJumpIfNotEmpty);
		il.Emit(OpCodes.Ldarg_3);
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Call, MakeInstanceOfTypeForEmptyNodeMethod);
		if (typeBeingDeserialized.IsValueType)
		{
			il.Emit(OpCodes.Unbox_Any, typeBeingDeserialized);
		}
		il.Emit(OpCodes.Stloc, localForResultValue);
		il.Emit(OpCodes.Br, endLabel);
	}

	private static void EmitIlToErrorAndMakeDefaultValueForCdata(ILGenerator il, Type typeBeingDeserialized, Label labelToJumpIfNotCdata, Label labelToJumpAfterCheck, LocalBuilder localForResultValue)
	{
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Callvirt, XmlNodeGetFirstChildMethod);
		il.Emit(OpCodes.Brfalse, labelToJumpIfNotCdata);
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Callvirt, XmlNodeGetFirstChildMethod);
		il.Emit(OpCodes.Callvirt, XmlNodeGetNodeTypeMethod);
		il.Emit(OpCodes.Ldc_I4, 4);
		il.Emit(OpCodes.Bne_Un, labelToJumpIfNotCdata);
		il.Emit(OpCodes.Ldstr, "CDATA can only be used for strings. Bad xml: ");
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Callvirt, XmlNodeGetOuterXmlMethod);
		il.Emit(OpCodes.Call, StringConcatMethod);
		il.Emit(OpCodes.Call, LogErrorMethod);
		if (typeBeingDeserialized.IsValueType)
		{
			il.Emit(OpCodes.Ldloca_S, localForResultValue);
			il.Emit(OpCodes.Initobj, typeBeingDeserialized);
		}
		else
		{
			il.Emit(OpCodes.Ldnull);
			il.Emit(OpCodes.Stloc, localForResultValue);
		}
		il.Emit(OpCodes.Br, labelToJumpAfterCheck);
	}

	private static void EmitIlToHandleSingleTextNodeViaParseHelper(ILGenerator il, Type typeBeingDeserialized, LocalBuilder localForResultValue, Label labelToJumpIfNotText, Label endLabel)
	{
		LocalBuilder local = il.DeclareLocal(typeof(XmlNode));
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Call, GetNodeOnlyChildMethod);
		il.Emit(OpCodes.Dup);
		il.Emit(OpCodes.Stloc, local);
		il.Emit(OpCodes.Brfalse, labelToJumpIfNotText);
		il.Emit(OpCodes.Ldloc, local);
		il.Emit(OpCodes.Callvirt, XmlNodeGetNodeTypeMethod);
		il.Emit(OpCodes.Ldc_I4, 3);
		il.Emit(OpCodes.Bne_Un, labelToJumpIfNotText);
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Callvirt, XmlNodeGetInnerTextMethod);
		il.Emit(OpCodes.Ldarg_3);
		il.Emit(OpCodes.Call, ParseHelperFromStringNonGenericMethod);
		if (typeBeingDeserialized.IsValueType)
		{
			il.Emit(OpCodes.Unbox_Any, typeBeingDeserialized);
			il.Emit(OpCodes.Stloc, localForResultValue);
		}
		else
		{
			il.Emit(OpCodes.Castclass, typeBeingDeserialized);
			il.Emit(OpCodes.Stloc, localForResultValue);
		}
		il.Emit(OpCodes.Br, endLabel);
	}

	public static FieldInfo ResolveFieldForNode(Type typeBeingDeserialized, XmlNode node, XmlNode parentForDebug)
	{
		return XmlToObjectUtils.DoFieldSearch(typeBeingDeserialized.GetInnerIfNullable(), node, parentForDebug);
	}

	public static Type ResolveTypeForNode(FieldInfo fieldBeingSet, XmlNode node)
	{
		return ResolveTypeForNode(fieldBeingSet.FieldType, node);
	}

	public static Type ResolveTypeForNode(Type defaultType, XmlNode node)
	{
		XmlAttribute xmlAttribute = node.Attributes?["Class"];
		if (xmlAttribute == null)
		{
			return defaultType;
		}
		Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly(xmlAttribute.Value, defaultType.GetInnerIfNullable().Namespace);
		if (typeInAnyAssembly != null)
		{
			return typeInAnyAssembly;
		}
		throw new ArgumentException("Could not find type named " + xmlAttribute.Value + " from node " + node.OuterXml, "node");
	}

	public static XmlNode GetNodeOnlyChild(XmlNode node)
	{
		XmlNode firstChild = node.FirstChild;
		if (firstChild == null || firstChild.NextSibling != null)
		{
			return null;
		}
		return firstChild;
	}

	public static object MakeInstanceOfTypeForEmptyNode(Type type, XmlNode node)
	{
		type = ResolveTypeForNode(type, node);
		if (type.IsValueType)
		{
			return RuntimeHelpers.GetUninitializedObject(type);
		}
		try
		{
			return Activator.CreateInstance(type);
		}
		catch (MissingMethodException innerException)
		{
			throw new InvalidOperationException("Cannot deserialize XML node " + node.OuterXml + " to type " + type.FullName + " due to missing parameterless constructor.", innerException);
		}
	}

	public static bool ValidateMayRequires(string mayRequire, string mayRequireAny)
	{
		if (!mayRequire.NullOrEmpty() && !ModLister.AllModsActiveNoSuffix(mayRequire.Split(',')))
		{
			if (DirectXmlCrossRefLoader.MistypedMayRequire(mayRequire))
			{
				Log.Error("Faulty MayRequire: " + mayRequire);
			}
			return false;
		}
		if (!mayRequireAny.NullOrEmpty() && !ModLister.AnyModActiveNoSuffix(mayRequireAny.Split(',')))
		{
			return false;
		}
		return true;
	}

	private static string GetDynamicMethodNameSuffixForType(Type type)
	{
		return type.FullName?.Replace('.', '_') ?? "UnknownType";
	}

	private static Type GetInnerIfNullable(this Type type)
	{
		return Nullable.GetUnderlyingType(type) ?? type;
	}

	private static bool TypeCanBeDeserializedWithSharedBody(Type type)
	{
		if (type.IsValueType)
		{
			return false;
		}
		if (type.IsGenericType || type.IsConstructedGenericType)
		{
			return false;
		}
		if (XmlToObjectUtils.CustomDataLoadMethodOf(type) != null)
		{
			return false;
		}
		MethodInfo methodInfo = XmlToObjectUtils.PostLoadMethodOf(type);
		if ((object)methodInfo != null && methodInfo.DeclaringType != typeof(Editable))
		{
			return false;
		}
		if (ParseHelper.HandlesType(type))
		{
			return false;
		}
		return true;
	}
}
