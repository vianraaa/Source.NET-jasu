using CommunityToolkit.HighPerformance;

using SharpCompress.Common;

using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

using static Source.Common.Formats.Keyvalues.KeyValues;

namespace Source.Common;

public static class DynamicConversion
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void CastConvert<From, To>(in From from, ref To to) => DynamicConversion<From, To>.CastConvert(in from, ref to);
}

delegate To CastFn<From, To>(in From from);
public static class DynamicConversion<From, To> {
	static CastFn<From, To> castFn;
	static DynamicConversion() {
		var param = Expression.Parameter(typeof(From));
		var body = Expression.Convert(param, typeof(To));
		castFn = Expression.Lambda<CastFn<From, To>>(body, param).Compile();
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void CastConvert(in From from, ref To to) => to = castFn(in from);
}
public static class FieldAccess
{
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

public interface IFieldILGenerator {
	public string Name { get; }
	public void GenerateGet<T>(ILGenerator il);
	public void GenerateGetRef<T>(ILGenerator il);
	public void GenerateSet<T>(ILGenerator il);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T GetValue<T>(IFieldILGenerator self, object instance) => FieldAccess<T>.Getter(self)(instance);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref T GetValueRef<T>(IFieldILGenerator self, object instance) => ref FieldAccess<T>.RefGetter(self)(instance);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool SetValue<T>(IFieldILGenerator self, object instance, in T value) {
		FieldAccess<T>.Setter(self)(instance, in value);
		return true;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void CopyStringToField(IFieldILGenerator self, object instance, string? str) => Warning("FieldAccess.CopyStringToField isn't implemented yet\n");
}

/// <summary>
/// Define how to build IL methods for types.
/// </summary>
public interface IFieldAccessor
{
	public string Name { get; }
	public bool IsStatic { get; }
	public Type DeclaringType { get; }
	public Type FieldType { get; }

	public T GetValue<T>(object instance);
	public ref T GetValueRef<T>(object instance);
	public bool SetValue<T>(object instance, in T value);
	public void CopyStringToField(object instance, string? str);
}

public static class FieldAccess<T>
{
	public delegate T Get(object o);
	public delegate ref T GetRef(object o);
	public delegate void Set(object o, in T v);

	internal static readonly Dictionary<IFieldILGenerator, Get> Getters = [];
	internal static readonly Dictionary<IFieldILGenerator, GetRef> RefGetters = [];
	internal static readonly Dictionary<IFieldILGenerator, Set> Setters = [];

	public static Get Getter(IFieldILGenerator field) {
		if (Getters.TryGetValue(field, out var g))
			return g;

		var method = new DynamicMethod($"get_{field.Name}", typeof(T), [typeof(object)], typeof(FieldAccess<T>).Module, true);
		var il = method.GetILGenerator();
		field.GenerateGet<T>(il);

		Getters[field] = g = (Get)method.CreateDelegate(typeof(Get));
		return g;
	}

	public static GetRef RefGetter(IFieldILGenerator field) {
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
		field.GenerateGetRef<T>(il);

		RefGetters[field] = g = (GetRef)method.CreateDelegate(typeof(GetRef));
		return g;
	}

	public static Set Setter(IFieldILGenerator field) {
		if (Setters.TryGetValue(field, out var s))
			return s;

		var method = new DynamicMethod($"set_{field.Name}", typeof(void), [typeof(object), typeof(T).MakeByRefType()], typeof(FieldAccess<T>).Module, true);
		var il = method.GetILGenerator();
		field.GenerateSet<T>(il);

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

public class BasicFieldInfo(FieldInfo Field) : IFieldAccessor, IFieldILGenerator
{
	public string Name => Field.Name;
	public bool IsStatic => Field.IsStatic;
	public Type DeclaringType => Field.DeclaringType!;
	public Type FieldType => Field.FieldType!;

	public void GenerateGet<T>(ILGenerator il) {
		if (!IsStatic) {
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(DeclaringType!.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, DeclaringType);
			il.Emit(OpCodes.Ldfld, Field);
		}
		else {
			il.Emit(OpCodes.Ldsfld, Field);
		}

		il.Emit(OpCodes.Ret);
	}

	public void GenerateGetRef<T>(ILGenerator il) {
		if (!IsStatic) {
			il.Emit(OpCodes.Ldarg_0);
			if (DeclaringType!.IsValueType) {
				il.Emit(OpCodes.Unbox, DeclaringType);
				il.Emit(OpCodes.Ldflda, Field);
			}
			else {
				il.Emit(OpCodes.Castclass, DeclaringType);
				il.Emit(OpCodes.Ldflda, Field);
			}
		}
		else
			il.Emit(OpCodes.Ldsflda, Field);

		il.Emit(OpCodes.Ret);
	}

	public void GenerateSet<T>(ILGenerator il) {
		if (!IsStatic) {
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(DeclaringType!.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, DeclaringType);
			il.Emit(OpCodes.Ldarg_1);

			if (typeof(T).IsValueType)
				il.Emit(OpCodes.Ldobj, typeof(T));
			else
				il.Emit(OpCodes.Ldind_Ref);

			il.Emit(OpCodes.Stfld, Field);
		}
		else {
			il.Emit(OpCodes.Ldarg_1);

			if (typeof(T).IsValueType)
				il.Emit(OpCodes.Ldobj, typeof(T));
			else
				il.Emit(OpCodes.Ldind_Ref);

			il.Emit(OpCodes.Stsfld, Field);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void IFieldAccessor.CopyStringToField(object instance, string? str) => IFieldILGenerator.CopyStringToField(this, instance, str);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	T IFieldAccessor.GetValue<T>(object instance) => IFieldILGenerator.GetValue<T>(this, instance);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	ref T IFieldAccessor.GetValueRef<T>(object instance) => ref IFieldILGenerator.GetValueRef<T>(this, instance);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	bool IFieldAccessor.SetValue<T>(object instance, in T value) => IFieldILGenerator.SetValue(this, instance, in value);
}

public class ArrayFieldIndexInfo : IFieldAccessor, IFieldILGenerator
{
	public readonly ArrayFieldInfo BaseArrayField;
	public readonly Type ElementType;
	public readonly int Index;
	public readonly bool NegativeIndex;
	public readonly string name;
	public ArrayFieldIndexInfo(ArrayFieldInfo baseArrayField, int index) {
		if (baseArrayField.Type == ArrayFieldType.InlineArray && Math.Abs(index) >= baseArrayField.Length)
			throw new IndexOutOfRangeException("Out of range index given the inline array target.");

		BaseArrayField = baseArrayField;
		ElementType = baseArrayField.ElementType;
		Index = index;
		name = $"{BaseArrayField.Name}[{index}]";

		NegativeIndex = index < 0;
		if (NegativeIndex)
			Index = -Index;
	}

	public Type FieldType => ElementType;
	public Type? DeclaringType => BaseArrayField.DeclaringType;
	public string Name => name;
	public bool IsStatic => BaseArrayField.BaseField.IsStatic;

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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void IFieldAccessor.CopyStringToField(object instance, string? str) => IFieldILGenerator.CopyStringToField(this, instance, str);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	T IFieldAccessor.GetValue<T>(object instance) => IFieldILGenerator.GetValue<T>(this, instance);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	ref T IFieldAccessor.GetValueRef<T>(object instance) => ref IFieldILGenerator.GetValueRef<T>(this, instance);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	bool IFieldAccessor.SetValue<T>(object instance, in T value) => IFieldILGenerator.SetValue(this, instance, in value);
}
public class ArrayFieldInfo : IFieldAccessor, IFieldILGenerator
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

	public Type FieldType => BaseField.FieldType;
	public Type? DeclaringType => BaseField.DeclaringType;
	public string Name => name;
	public bool IsStatic => BaseField.IsStatic;
	// Instance is passed here because in the future we should sanity-check our target field against the 
	// incoming untrusted index. Although a bad index wouldn't cause pandamonium beyond an out of range exception,
	// it would be nice to handle it without an exception being raised in the future/being able to Assert for it.
	public IFieldAccessor? GetIndexFieldInfo(object instance, int index) {
		if (!FieldAccessors.TryGetValue(index, out ArrayFieldIndexInfo? ret))
			FieldAccessors[index] = ret = new(this, index);

		return ret;
	}

	public void GenerateGet<T>(ILGenerator il) => throw new NotImplementedException();
	public void GenerateGetRef<T>(ILGenerator il) => throw new NotImplementedException();
	public void GenerateSet<T>(ILGenerator il) => throw new NotImplementedException();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void IFieldAccessor.CopyStringToField(object instance, string? str) => IFieldILGenerator.CopyStringToField(this, instance, str);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	T IFieldAccessor.GetValue<T>(object instance) => IFieldILGenerator.GetValue<T>(this, instance);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	ref T IFieldAccessor.GetValueRef<T>(object instance) => ref IFieldILGenerator.GetValueRef<T>(this, instance);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	bool IFieldAccessor.SetValue<T>(object instance, in T value) => IFieldILGenerator.SetValue(this, instance, in value);
}


public delegate Value GetFn<Instance, Value>(Instance instance);
public delegate void SetFn<Instance, Value>(Instance instance, in Value value);
public class GetSetAccessor<Instance, Value>(GetFn<Instance, Value> get, SetFn<Instance, Value>? set = null, string? name = null) : IFieldAccessor
{
	public string Name => name;
	public bool IsStatic => false;
	public Type DeclaringType => typeof(Instance);
	public Type FieldType => typeof(Value);

	public void CopyStringToField(object instance, string? str) {
		Warning("CopyStringToField needs work\n");
	}
	public ref T GetValueRef<T>(object instance)=> throw new NotImplementedException("There's no good way to do this right now. TODO: If we run into this, consider removing it altogether in favor of get/sets.");
	public T GetValue<T>(object instance) {
		Value valueRef = get((Instance)instance);
		T output = default;
		DynamicConversion.CastConvert(in valueRef, ref output);
		return output;
	}
	public bool SetValue<T>(object instance, in T value) {
		if (set == null)
			return false;
		Value writeTarget = default;
		DynamicConversion.CastConvert(in value, ref writeTarget);
		set((Instance)instance, in writeTarget);
		return true;
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
	public static string ParseNameField(string? name) {
		ArgumentNullException.ThrowIfNull(name);
		return name;
	}
	public static GetSetAccessor<Instance, Value> FIELDOF<Instance, Value>(GetFn<Instance, Value> get, SetFn<Instance, Value>? set = null, [CallerArgumentExpression(nameof(get))] string? name = null) {
		return new GetSetAccessor<Instance, Value>(get, set, name);
	}
	/// <summary>
	/// Generic C# <see cref="FieldInfo"/> retrieval.
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	/// <exception cref="NullReferenceException"></exception>
	/// <exception cref="KeyNotFoundException"></exception>
	public static BasicFieldInfo FIELDOF(string name) {
		Type? t = WhoCalledMe(2);
		return new BasicFieldInfo(baseField(t, name));
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
}