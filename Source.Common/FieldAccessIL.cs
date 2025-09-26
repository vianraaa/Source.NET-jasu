using CommunityToolkit.HighPerformance;

using SharpCompress.Common;

using Source.Common;

using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

using static Source.Common.Formats.Keyvalues.KeyValues;

namespace Source.Common
{

	/// <summary>
	/// Define how to build IL methods for types.
	/// </summary>
	public interface IFieldAccessor
	{
		public string Name { get; }
		public Type DeclaringType { get; }
		public Type FieldType { get; }

		public T GetValue<T>(object instance);
		public bool SetValue<T>(object instance, in T value);
	}

	file static class ILCast<From, To>
	{
		public delegate void DynamicCastFn(in From from, out To to);
		public static readonly DynamicCastFn Cast;


		static ILCast() {
			DynamicMethod method = new DynamicMethod($"ILCast<{typeof(From)}, {typeof(To)}", typeof(void), [typeof(From).MakeByRefType(), typeof(To).MakeByRefType()]);
			ILGenerator generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Ldarg_0);

			// Try to implicitly cast.
			MethodInfo? implicitCheck = ILAssembler.TryGetImplicitConversion(typeof(From), typeof(To));
			if (implicitCheck != null) {
				generator.Emit(OpCodes.Ldobj, typeof(From));
				generator.Emit(OpCodes.Call, implicitCheck);
				generator.Emit(OpCodes.Stobj);
				goto compile;
			}

			// Generic value type conversion.
			Type from = typeof(From), to = typeof(To);
			if (from.IsPrimitive && to.IsPrimitive) {
				var convOp = ILAssembler.GetConvOpcode(from, to, out OpCode code, out _);
				if (convOp) {
					generator.Emit(OpCodes.Ldobj, from);
					generator.Emit(code);
					generator.Emit(OpCodes.Stobj, to);
					goto compile;
				}
				else
					throw new Exception();
			}


			// Generic reference type conversion.
			if (!from.IsValueType && !to.IsValueType) {
				generator.Emit(OpCodes.Ldind_Ref);
				if (to != typeof(object))
					generator.Emit(OpCodes.Castclass, to);
				generator.Emit(OpCodes.Stind_Ref);
				goto compile;
			}

			throw new NotImplementedException("You should never get to this point!");
		compile:
			generator.Emit(OpCodes.Ret);
			Cast = method.CreateDelegate<DynamicCastFn>();
		}
	}

	file static class ILAccess<T>
	{
		public delegate T GetFn(object instance);
		public delegate void SetFn(object instance, in T value);
		static readonly ConditionalWeakTable<DynamicAccessor, GetFn> getFns = [];
		static readonly ConditionalWeakTable<DynamicAccessor, SetFn> setFns = [];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static GetFn Get(DynamicAccessor accessor) {
			if (getFns.TryGetValue(accessor, out GetFn? fn))
				return fn;

			DynamicMethod method = new DynamicMethod($"ILAccess<{typeof(T).Name}>_GetFn", typeof(T), [typeof(object)]);
			ILGenerator il = method.GetILGenerator();

			il.Emit(OpCodes.Ldarg_0);
			if (accessor.TargetType != typeof(object))
				il.Emit(OpCodes.Castclass, accessor.TargetType);
			for (int i = 0, c = accessor.Members.Count; i < c; i++) {
				MemberInfo member = accessor.Members[i];
				switch (member) {
					case FieldInfo field:
						if (i == c - 1)
							il.Emit(OpCodes.Ldfld, field);
						else
							il.Emit(OpCodes.Ldflda, field);

						break;
					default: throw new NotImplementedException("Cannot support the current member info");
				}
			}


			MethodInfo? implicitCheck = ILAssembler.TryGetImplicitConversion(accessor.StoringType, typeof(T));
			if (implicitCheck != null) {
				il.Emit(OpCodes.Ldobj, accessor.StoringType);
				il.Emit(OpCodes.Call, implicitCheck);
				il.Emit(OpCodes.Stobj);
			}
			else if (ILAssembler.GetConvOpcode(accessor.StoringType, typeof(T), out OpCode convCode, out _))
				il.Emit(convCode);

			il.Emit(OpCodes.Ret);

			getFns.Add(accessor, fn = method.CreateDelegate<GetFn>());
			return fn;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SetFn Set(DynamicAccessor accessor) {
			if (setFns.TryGetValue(accessor, out SetFn? fn))
				return fn;

			DynamicMethod method = new DynamicMethod($"ILAccess<{typeof(T).Name}>_SetFn", typeof(void), [typeof(object), typeof(T).MakeByRefType()]);
			ILGenerator il = method.GetILGenerator();

			il.LoggedEmit(OpCodes.Ldarg_0);
			if (accessor.TargetType != typeof(object))
				il.LoggedEmit(OpCodes.Castclass, accessor.TargetType);
			for (int i = 0, c = accessor.Members.Count - 1; i < c; i++) {
				MemberInfo member = accessor.Members[i];
				switch (member) {
					case FieldInfo field:
						il.LoggedEmit(OpCodes.Ldflda, field);

						break;
					default: throw new NotImplementedException("Cannot support the current member info");
				}
			}

			il.LoggedEmit(OpCodes.Ldarg_1);

			MethodInfo? implicitCheck = ILAssembler.TryGetImplicitConversion(typeof(T), accessor.StoringType);
			if (implicitCheck != null) {
				il.LoggedEmit(OpCodes.Ldobj, accessor.StoringType);
				il.LoggedEmit(OpCodes.Call, implicitCheck);
				il.LoggedEmit(OpCodes.Stobj);
			}
			else if (
				ILAssembler.GetLoadOpcode(typeof(T), out OpCode loadCode)
				&& ILAssembler.GetConvOpcode(typeof(T), accessor.StoringType, out OpCode convCode, out bool unsignedInput)
				) {
				if (unsignedInput && (convCode == OpCodes.Conv_R4 || convCode == OpCodes.Conv_R8))
					il.LoggedEmit(OpCodes.Conv_R_Un);
				else
					il.LoggedEmit(loadCode);
				il.LoggedEmit(convCode);
			}
			switch (accessor.Members[^1]) {
				case FieldInfo field:
					il.LoggedEmit(OpCodes.Stfld, field);
					break;
				default: throw new NotImplementedException("Cannot support the current member info");
			}
			il.LoggedEmit(OpCodes.Ret);

			setFns.Add(accessor, fn = method.CreateDelegate<SetFn>());
			return fn;
		}
	}


	public class DynamicArrayAccessor : DynamicAccessor
	{
		public readonly int Length;
		public DynamicArrayAccessor(Type targetType, ReadOnlySpan<char> expression) : base(targetType, expression) {
			throw new NotImplementedException();
		}
		public DynamicArrayIndexAccessor AtIndex(object instance, int index) {
			throw new NotImplementedException();
		}
	}
	public class DynamicArrayIndexAccessor : DynamicAccessor
	{
		public readonly DynamicArrayAccessor BaseArrayAccessor;
		public readonly bool HadNegativeIndex;
		public readonly int Index;

		public DynamicArrayIndexAccessor(Type targetType, ReadOnlySpan<char> expression, int index) : base(targetType, expression) {
			throw new NotImplementedException();
		}
	}

	public class DynamicAccessor : IFieldAccessor
	{
		public readonly List<MemberInfo> Members = [];

		Type buildingTargetType;
		public readonly Type TargetType;
		public readonly Type StoringType;

		public string Name { get; }
		Type IFieldAccessor.DeclaringType => TargetType;

		Type IFieldAccessor.FieldType => throw new NotImplementedException();

		void HandleIndex(ReadOnlySpan<char> index) {
			if (index.IsEmpty)
				return;

			MemberInfo[] memberInfos = buildingTargetType.GetMember(new string(index), (BindingFlags)~0);
			foreach (var member in memberInfos) {
				switch (member) {
					case FieldInfo field:
						Members.Add(field);
						buildingTargetType = field.FieldType;
						return;
					case PropertyInfo prop:
						Members.Add(prop);
						buildingTargetType = prop.PropertyType;
						return;
				}
			}

			throw new KeyNotFoundException($"Cannot find an appropriate member named '{index}' in the current target type '{buildingTargetType.Name}'. Ensure naming is correct.");
		}
		public DynamicAccessor(Type targetType, ReadOnlySpan<char> expression) {
			Name = new(expression);

			TargetType = targetType;
			buildingTargetType = targetType;
			int lastPeriod = 0;
			for (int i = 0; i < expression.Length + 1; i++) {
				// Final chance to handle a string index
				if (i == expression.Length) {
					ReadOnlySpan<char> index = expression[lastPeriod..i];
					HandleIndex(index);
					break;
				}
				char c = expression[i];
				if (c == '.') {
					ReadOnlySpan<char> index = expression[lastPeriod..i];
					HandleIndex(index);
					lastPeriod = i + 1; // advance past the period for string reading
				}
			}

			StoringType = Members.Last() switch {
				FieldInfo f => f.FieldType,
				PropertyInfo p => p.PropertyType,
				_ => throw new Exception()
			};
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public T GetValue<T>(object instance) => ILAccess<T>.Get(this)(instance);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public void SetValue<T>(object instance, in T value) => ILAccess<T>.Set(this)(instance, in value);

		bool IFieldAccessor.SetValue<T>(object instance, in T value) {
			throw new NotImplementedException();
		}
	}

	public static class ILAssembler
	{
		public static void LoggedEmit(this ILGenerator il, OpCode code) {
			Console.WriteLine(code);
			il.Emit(code);
		}
		public static void LoggedEmit(this ILGenerator il, OpCode code, FieldInfo field) {
			Console.WriteLine($"{code} {field}");
			il.Emit(code, field);
		}
		public static void LoggedEmit(this ILGenerator il, OpCode code, MethodInfo method) {
			Console.WriteLine($"{code} {method}");
			il.Emit(code, method);
		}
		public static void LoggedEmit(this ILGenerator il, OpCode code, Type type) {
			Console.WriteLine($"{code} {type}");
			il.Emit(code, type);
		}
		public static MethodInfo? TryGetImplicitConversion(Type baseType, Type targetType) {
			return baseType.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.Where(mi => mi.Name == "op_Implicit" && mi.ReturnType == targetType)
				.FirstOrDefault(mi => {
					ParameterInfo? pi = mi.GetParameters().FirstOrDefault();
					return pi != null && pi.ParameterType == baseType;
				});
		}

		public static bool GetLoadOpcode(Type from, out OpCode opcode) {
			return (opcode = Type.GetTypeCode(from) switch {
				TypeCode.Byte => OpCodes.Ldind_U1,
				TypeCode.UInt16 => OpCodes.Ldind_U2,
				TypeCode.UInt32 => OpCodes.Ldind_U4,
				TypeCode.UInt64 => OpCodes.Ldind_I8,
				TypeCode.SByte => OpCodes.Ldind_I1,
				TypeCode.Int16 => OpCodes.Ldind_I2,
				TypeCode.Int32 => OpCodes.Ldind_I4,
				TypeCode.Int64 => OpCodes.Ldind_I8,
				TypeCode.Single => OpCodes.Ldind_R4,
				TypeCode.Double => OpCodes.Ldind_R8,
				_ => OpCodes.Nop
			}) != OpCodes.Nop;
		}
		public static bool GetConvOpcode(Type from, Type to, out OpCode opcode, out bool isUnsignedInput) {
			opcode = OpCodes.Nop;
			var fromcode = Type.GetTypeCode(from);
			isUnsignedInput = fromcode switch {
				TypeCode.Boolean or TypeCode.Byte or TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64 => true,
				_ => false
			};
			if (!from.IsPrimitive || !to.IsPrimitive)
				return false;

			if (from == to)
				return false;

			var code = Type.GetTypeCode(to);

			switch (fromcode) {
				default:
					opcode = code switch {
						TypeCode.SByte => OpCodes.Conv_I1,
						TypeCode.Byte => OpCodes.Conv_U1,
						TypeCode.Int16 => OpCodes.Conv_I2,
						TypeCode.UInt16 => OpCodes.Conv_U2,
						TypeCode.Int32 => OpCodes.Conv_I4,
						TypeCode.UInt32 => OpCodes.Conv_U4,
						TypeCode.Int64 => OpCodes.Conv_I8,
						TypeCode.UInt64 => OpCodes.Conv_U8,
						TypeCode.Single => OpCodes.Conv_R4,
						TypeCode.Double => OpCodes.Conv_R8,
						_ => OpCodes.Nop
					};
					break;
			}
			return opcode != OpCodes.Nop;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DynamicCast<From, To>(in From from, out To to) => ILCast<From, To>.Cast(in from, out to);
	}
}

namespace Source
{
	public static class FIELD<T>
	{
		public static DynamicAccessor OF(ReadOnlySpan<char> expression) => new(typeof(T), expression);
		public static DynamicArrayAccessor OF_ARRAY(ReadOnlySpan<char> expression) => new(typeof(T), expression);
		public static DynamicArrayIndexAccessor OF_ARRAYINDEX(ReadOnlySpan<char> expression, int index) => new(typeof(T), expression, index);
		public static DynamicArrayIndexAccessor OF_VECTORELEM(ReadOnlySpan<char> expression, int index) => new(typeof(T), expression, -index);
	}
}
