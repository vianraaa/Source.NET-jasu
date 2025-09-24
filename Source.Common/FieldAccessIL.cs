using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

using static Source.Common.Formats.Keyvalues.KeyValues;

namespace Source.Common;

public static class FieldAccess
{
	public static T GetValueFast<T>(this FieldInfo field, object instance) => FieldAccess<T>.Getter(field)(instance);
	public static ref T GetValueRefFast<T>(this FieldInfo field, object instance) => ref FieldAccess<T>.RefGetter(field)(instance);
	public static void SetValueFast<T>(this FieldInfo field, object instance, in T value) => FieldAccess<T>.Setter(field)(instance, in value);

	public static void CopyStringToField(this FieldInfo field, object instance, string? str) {
		Warning("FieldAccess.CopyStringToField isn't implemented yet\n");
	}

	static readonly Dictionary<Type, bool> linearTypeResults = [];
	internal static bool TypesFieldsAreCompletelyLinear(Type baseFieldType) {
		if (linearTypeResults.TryGetValue(baseFieldType, out bool ret))
			return ret;

		var fields = baseFieldType.GetFields((BindingFlags)~0);
		if (fields.Length == 0)
			return linearTypeResults[baseFieldType] = true; // What to return here, even

		Type? t = fields[0].FieldType;
		for (int i = 1; i < fields.Length; i++)
			if (fields[i].FieldType != t)
				return linearTypeResults[baseFieldType] = false;

		return linearTypeResults[baseFieldType] = true;
	}
}

/// <summary>
/// Define how to build IL methods for types.
/// </summary>
public interface IFieldILGenerator
{
	public void GenerateGet<T>(ILGenerator il);
	public void GenerateGetRef<T>(ILGenerator il);
	public void GenerateSet<T>(ILGenerator il);
}

public static class FieldAccess<T>
{
	public delegate T Get(object o);
	public delegate ref T GetRef(object o);
	public delegate void Set(object o, in T v);

	internal static readonly Dictionary<FieldInfo, Get> Getters = [];
	internal static readonly Dictionary<FieldInfo, GetRef> RefGetters = [];
	internal static readonly Dictionary<FieldInfo, Set> Setters = [];

	public static Get Getter(FieldInfo field) {
		if (Getters.TryGetValue(field, out var g))
			return g;

		var method = new DynamicMethod($"get_{field.Name}", typeof(T), [typeof(object)], typeof(FieldAccess<T>).Module, true);
		var il = method.GetILGenerator();

		if (field is IFieldILGenerator fieldILGenerator)
			fieldILGenerator.GenerateGet<T>(il);
		else {
			if (!field.IsStatic) {
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(field.DeclaringType!.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, field.DeclaringType);
				il.Emit(OpCodes.Ldfld, field);
			}
			else {
				il.Emit(OpCodes.Ldsfld, field);
			}

			il.Emit(OpCodes.Ret);
		}

		Getters[field] = g = (Get)method.CreateDelegate(typeof(Get));
		return g;
	}

	public static GetRef RefGetter(FieldInfo field) {
		if (RefGetters.TryGetValue(field, out var g))
			return g;

		var method = new DynamicMethod(
			$"refget_{field.Name}",
			typeof(T).MakeByRefType(),
			[typeof(object)],
			typeof(FieldAccess<T>).Module,
			true
		);

		var il = method.GetILGenerator();

		if (field is IFieldILGenerator fieldILGenerator)
			fieldILGenerator.GenerateGetRef<T>(il);
		else {
			if (!field.IsStatic) {
				il.Emit(OpCodes.Ldarg_0);
				if (field.DeclaringType!.IsValueType) {
					il.Emit(OpCodes.Unbox, field.DeclaringType);
					il.Emit(OpCodes.Ldflda, field);
				}
				else {
					il.Emit(OpCodes.Castclass, field.DeclaringType);
					il.Emit(OpCodes.Ldflda, field);
				}
			}
			else
				il.Emit(OpCodes.Ldsflda, field);

			il.Emit(OpCodes.Ret);
		}
		RefGetters[field] = g = (GetRef)method.CreateDelegate(typeof(GetRef));
		return g;
	}

	public static Set Setter(FieldInfo field) {
		if (Setters.TryGetValue(field, out var s))
			return s;

		var method = new DynamicMethod($"set_{field.Name}", typeof(void), [typeof(object), typeof(T).MakeByRefType()], typeof(FieldAccess<T>).Module, true);
		var il = method.GetILGenerator();
		if (field is IFieldILGenerator fieldILGenerator)
			fieldILGenerator.GenerateSet<T>(il);
		else {
			if (!field.IsStatic) {
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(field.DeclaringType!.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, field.DeclaringType);
				il.Emit(OpCodes.Ldarg_1);

				if (typeof(T).IsValueType)
					il.Emit(OpCodes.Ldobj, typeof(T));
				else
					il.Emit(OpCodes.Ldind_Ref);

				il.Emit(OpCodes.Stfld, field);
			}
			else {
				il.Emit(OpCodes.Ldarg_1);

				if (typeof(T).IsValueType)
					il.Emit(OpCodes.Ldobj, typeof(T));
				else
					il.Emit(OpCodes.Ldind_Ref);

				il.Emit(OpCodes.Stsfld, field);
			}
		}

		il.Emit(OpCodes.Ret);
		Setters[field] = s = (Set)method.CreateDelegate(typeof(Set));
		return s;
	}
}

public enum ArrayFieldType
{
	StdArray,
	InlineArray,
	Vector3
}

public class ArrayFieldIndexInfo : FieldInfo, IFieldILGenerator
{
	public readonly ArrayFieldInfo BaseArrayField;
	public readonly Type ElementType;
	public readonly int Index;
	public readonly bool NegativeIndex;
	public readonly string name;
	public ArrayFieldIndexInfo(ArrayFieldInfo baseArrayField, int index) {
		if (index < 0)
			throw new IndexOutOfRangeException("Invalid operation");
		if (baseArrayField.Type == ArrayFieldType.InlineArray && index >= baseArrayField.Length)
			throw new IndexOutOfRangeException("Out of range index given the inline array target.");

		BaseArrayField = baseArrayField;
		ElementType = baseArrayField.ElementType;
		Index = index;
		name = $"{BaseArrayField.Name}[{index}]";

		NegativeIndex = index < 0;
		if (NegativeIndex)
			Index = -Index;
	}

	public override FieldAttributes Attributes => BaseArrayField.Attributes;
	public override RuntimeFieldHandle FieldHandle => throw new NotImplementedException();

	public override Type FieldType => ElementType;

	public override Type? DeclaringType => BaseArrayField.DeclaringType;

	public override string Name => name;

	public override Type? ReflectedType => BaseArrayField.ReflectedType;

	public override object[] GetCustomAttributes(bool inherit) => [];
	public override object[] GetCustomAttributes(Type attributeType, bool inherit) => [];
	public override bool IsDefined(Type attributeType, bool inherit) => false;
	public override object? GetValue(object? obj)
		=> throw new NotImplementedException("Please use the GetValueFast<T> method.");
	public override void SetValue(object? obj, object? value, BindingFlags invokeAttr, Binder? binder, CultureInfo? culture)
		=> throw new NotImplementedException("Please use the SetValueFast<T> method.");

	public void GenerateGet<T>(ILGenerator il) {
		var field = BaseArrayField.BaseField;
		if (!field.IsStatic) {
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(field.DeclaringType!.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, field.DeclaringType);
			il.Emit(OpCodes.Ldfld, field);
		}
		else
			il.Emit(OpCodes.Ldsfld, field);

		var baseFieldType = field.FieldType;
		if (baseFieldType.IsArray) {
			il.Emit(OpCodes.Ldc_I4, Index);
			il.Emit(OpCodes.Ldelema, ElementType);
		}
		else if (baseFieldType.IsValueType) {
			InlineArrayAttribute? attr = baseFieldType.GetCustomAttribute<InlineArrayAttribute>();
			if (attr != null) {
				var firstField = baseFieldType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)[0];

				var tempLocal = il.DeclareLocal(baseFieldType);
				il.Emit(OpCodes.Stloc, tempLocal);     // Store the inline array struct
				il.Emit(OpCodes.Ldloca, tempLocal);    // Load address of struct
				il.Emit(OpCodes.Ldflda, firstField);   // Get address of first element

				// Manual offset calculation if index > 0
				if (Index > 0) {
					// Push the index
					il.Emit(OpCodes.Ldc_I4, Index);
					// Push the size of one element
					il.Emit(OpCodes.Sizeof, ElementType);
					// Multiply: index * sizeof(element)
					il.Emit(OpCodes.Mul);
					// Add to the base address
					il.Emit(OpCodes.Add);
				}

				// Load the value from the calculated address
				il.Emit(OpCodes.Ldobj, ElementType);

				// Box if we're returning object
				if (typeof(T) == typeof(object) && ElementType.IsValueType) {
					il.Emit(OpCodes.Box, ElementType);
				}
			}
		}

		il.Emit(OpCodes.Ret);
	}

	public void GenerateGetRef<T>(ILGenerator il) {
		throw new NotImplementedException();
	}

	// UNCONFIRMED
	public void GenerateSet<T>(ILGenerator il) {
		var field = BaseArrayField.BaseField;
		if (!field.IsStatic) {
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(field.DeclaringType!.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, field.DeclaringType);
			il.Emit(OpCodes.Ldfld, field);
		}
		else
			il.Emit(OpCodes.Ldsfld, field);

		var baseFieldType = field.FieldType;
		if (baseFieldType.IsArray) {
			il.Emit(OpCodes.Ldc_I4, Index);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Stelem, ElementType);
		}
		else if (baseFieldType.IsValueType) {
			InlineArrayAttribute? attr = baseFieldType.GetCustomAttribute<InlineArrayAttribute>();
			if (attr != null || FieldAccess.TypesFieldsAreCompletelyLinear(baseFieldType)) {
				var firstField = baseFieldType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)[0];

				var tempLocal = il.DeclareLocal(baseFieldType);
				il.Emit(OpCodes.Stloc, tempLocal);     // Store the inline array struct
				il.Emit(OpCodes.Ldloca, tempLocal);    // Load address of struct
				il.Emit(OpCodes.Ldflda, firstField);   // Get address of first element

				// Manual offset calculation if index > 0
				if (Index > 0) {
					// Push the index
					il.Emit(OpCodes.Ldc_I4, Index);
					// Push the size of one element
					il.Emit(OpCodes.Sizeof, ElementType);
					// Multiply: index * sizeof(element)
					il.Emit(OpCodes.Mul);
					// Add to the base address
					il.Emit(OpCodes.Add);
				}

				// Store the value from the calculated address
				il.Emit(OpCodes.Ldarg_1);              // load the value to assign
				il.Emit(OpCodes.Stobj, ElementType);   // store value into computed address
			}
			else
				throw new NotImplementedException($"Cannot interpret an array index from the type '{baseFieldType}'.");
		}

		il.Emit(OpCodes.Ret);
	}
}
public class ArrayFieldInfo : FieldInfo, IFieldILGenerator
{
	public readonly Dictionary<long, ArrayFieldIndexInfo> FieldAccessors = [];

	public readonly FieldInfo BaseField;
	public readonly Type ElementType;
	public readonly int Length;
	public readonly ArrayFieldType Type;
	public ArrayFieldInfo(FieldInfo baseField) {
		if (baseField.FieldType.IsArray) {
			ElementType = baseField.FieldType.GetElementType()!;
			Length = -1;
			Type = ArrayFieldType.StdArray;
		}
		else if (baseField.FieldType == typeof(Vector3)) {
			ElementType = typeof(float);
			Length = 3;
			Type = ArrayFieldType.Vector3;
		}
		else {
			var inlineArrayAttr = baseField.FieldType.GetCustomAttribute<InlineArrayAttribute>();
			if (inlineArrayAttr == null)
				throw new ArgumentException($"Field {baseField.Name} is not an array or an InlineArray", nameof(baseField));

			ElementType = baseField.FieldType.GetElementType()
						   ?? baseField.FieldType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)[0]?.FieldType
						   ?? throw new InvalidOperationException($"Cannot determine InlineArray element type for {baseField.FieldType}");
			Length = inlineArrayAttr.Length;
			Type = ArrayFieldType.InlineArray;
		}

		BaseField = baseField;
		name = baseField.Name;
	}

	readonly string name;
	public override FieldAttributes Attributes => BaseField.Attributes;
	public override RuntimeFieldHandle FieldHandle => throw new NotImplementedException();

	public override Type FieldType => BaseField.FieldType;
	public override Type? DeclaringType => BaseField.DeclaringType;
	public override string Name => name;
	public override Type? ReflectedType => BaseField.ReflectedType;

	public override object[] GetCustomAttributes(bool inherit)
		=> [];
	public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		=> [];
	public override bool IsDefined(Type attributeType, bool inherit) => false;
	public override object? GetValue(object? obj)
		=> throw new NotImplementedException("Please use the GetValueFast<T> method.");
	public override void SetValue(object? obj, object? value, BindingFlags invokeAttr, Binder? binder, CultureInfo? culture)
		=> throw new NotImplementedException("Please use the SetValueFast<T> method.");

	// Instance is passed here because in the future we should sanity-check our target field against the 
	// incoming untrusted index. Although a bad index wouldn't cause pandamonium beyond an out of range exception,
	// it would be nice to handle it without an exception being raised in the future/being able to Assert for it.
	public FieldInfo? GetIndexFieldInfo(object instance, int index) {
		if (!FieldAccessors.TryGetValue(index, out ArrayFieldIndexInfo? ret))
			FieldAccessors[index] = ret = new(this, index);

		return ret;
	}

	public void GenerateGet<T>(ILGenerator il) {
		throw new NotImplementedException();
	}

	public void GenerateGetRef<T>(ILGenerator il) {
		throw new NotImplementedException();
	}

	public void GenerateSet<T>(ILGenerator il) {
		throw new NotImplementedException();
	}
}

public class StructElementFieldInfo : FieldInfo, IFieldILGenerator
{
	public override FieldAttributes Attributes => throw new NotImplementedException();
	public override RuntimeFieldHandle FieldHandle => throw new NotImplementedException();
	public override Type FieldType => throw new NotImplementedException();
	public override Type? DeclaringType => throw new NotImplementedException();
	public override string Name => throw new NotImplementedException();
	public override Type? ReflectedType => throw new NotImplementedException();
	public override object[] GetCustomAttributes(bool inherit) => throw new NotImplementedException();
	public override object[] GetCustomAttributes(Type attributeType, bool inherit) => throw new NotImplementedException();
	public override object? GetValue(object? obj) => throw new NotImplementedException();
	public override bool IsDefined(Type attributeType, bool inherit) => throw new NotImplementedException();
	public override void SetValue(object? obj, object? value, BindingFlags invokeAttr, Binder? binder, CultureInfo? culture) => throw new NotImplementedException();


	public void GenerateGet<T>(ILGenerator il) => throw new NotImplementedException();
	public void GenerateGetRef<T>(ILGenerator il) => throw new NotImplementedException();
	public void GenerateSet<T>(ILGenerator il) => throw new NotImplementedException();


	readonly Type[] types;
	readonly FieldInfo[] fields;

	public StructElementFieldInfo(Type type, string[] names) {
		ArgumentNullException.ThrowIfNull(type);
		ArgumentNullException.ThrowIfNull(names);

		types = new Type[names.Length + 1];
		fields = new FieldInfo[names.Length];
		Type? workingType = type;

		for (int i = 0; i < names.Length; i++) {
			fields[i] = workingType.GetField(names[i], (BindingFlags)~0)!;
			types[i] = workingType;
			workingType = fields[i].FieldType;
		}
		types[^1] = fields[^1].FieldType;
	}
}

public class StructArrayElementFieldInfo : FieldInfo, IFieldILGenerator
{
	public override FieldAttributes Attributes => throw new NotImplementedException();
	public override RuntimeFieldHandle FieldHandle => throw new NotImplementedException();
	public override Type FieldType => throw new NotImplementedException();
	public override Type? DeclaringType => throw new NotImplementedException();
	public override string Name => throw new NotImplementedException();
	public override Type? ReflectedType => throw new NotImplementedException();
	public override object[] GetCustomAttributes(bool inherit) => throw new NotImplementedException();
	public override object[] GetCustomAttributes(Type attributeType, bool inherit) => throw new NotImplementedException();
	public override object? GetValue(object? obj) => throw new NotImplementedException();
	public override bool IsDefined(Type attributeType, bool inherit) => throw new NotImplementedException();
	public override void SetValue(object? obj, object? value, BindingFlags invokeAttr, Binder? binder, CultureInfo? culture) => throw new NotImplementedException();


	public void GenerateGet<T>(ILGenerator il) => throw new NotImplementedException();
	public void GenerateGetRef<T>(ILGenerator il) => throw new NotImplementedException();
	public void GenerateSet<T>(ILGenerator il) => throw new NotImplementedException();


	readonly Type[] types;
	readonly FieldInfo[] fields;
	readonly int index;

	public StructArrayElementFieldInfo(Type type, string[] names, int index) {
		ArgumentNullException.ThrowIfNull(type);
		ArgumentNullException.ThrowIfNull(names);

		types = new Type[names.Length + 1];
		fields = new FieldInfo[names.Length];
		Type? workingType = type;

		for (int i = 0; i < names.Length; i++) {
			fields[i] = workingType.GetField(names[i], (BindingFlags)~0)!;
			types[i] = workingType;
			workingType = fields[i].FieldType;
		}
		types[^1] = fields[^1].FieldType;
		this.index = index;
	}
}

/// <summary>
/// Various methods that implement field accessors.
/// </summary>
public static class FieldAccessReflectionUtils
{
	static FieldInfo baseField(Type? t, string name) {
		if (t == null)
			throw new NullReferenceException("This doesnt work as well as we hoped!");
		return t.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			?? t.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
			?? t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			?? throw new KeyNotFoundException($"Could not find a public/private/instance/static field named '{name}' in the type '{t.Name}'.");
	}
	/// <summary>
	/// Generic C# <see cref="FieldInfo"/> retrieval.
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	/// <exception cref="NullReferenceException"></exception>
	/// <exception cref="KeyNotFoundException"></exception>
	public static FieldInfo FIELDOF(string name) {
		Type? t = WhoCalledMe(2);
		return baseField(t, name);
	}
	/// <summary>
	/// A field representing an array.
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	/// <exception cref="NullReferenceException"></exception>
	/// <exception cref="KeyNotFoundException"></exception>
	public static ArrayFieldInfo FIELDOF_ARRAY(string name) {
		Type? t = WhoCalledMe(2);
		return new ArrayFieldInfo(baseField(t, name));
	}
	/// <summary>
	/// A field representing an array - but also specifying the index of the array.
	/// </summary>
	/// <param name="name"></param>
	/// <param name="index"></param>
	/// <returns></returns>
	/// <exception cref="NullReferenceException"></exception>
	/// <exception cref="KeyNotFoundException"></exception>
	public static ArrayFieldIndexInfo FIELDOF_ARRAYINDEX(string name, int index) {
		Type? t = WhoCalledMe(2);
		return new ArrayFieldIndexInfo(new ArrayFieldInfo(baseField(t, name)), index);
	}
	public static ArrayFieldIndexInfo FIELDOF_VECTORELEM(string name, int index) {
		Type? t = WhoCalledMe(2);
		return new ArrayFieldIndexInfo(new ArrayFieldInfo(baseField(t, name)), -index);
	}
	public static StructElementFieldInfo FIELDOF_STRUCTELEM(string[] fields) {
		Type? t = WhoCalledMe(2);
		return new StructElementFieldInfo(t!, fields);
	}
	public static StructArrayElementFieldInfo FIELDOF_STRUCTARRAYELEM(string[] fields, int index) {
		Type? t = WhoCalledMe(2);
		return new StructArrayElementFieldInfo(t!, fields, index);
	}
}