using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Enumerables;

using K4os.Hash.xxHash;

using Microsoft.Extensions.DependencyInjection;

using Source.Common.Commands;
using Source.Common.Engine;

using System;
using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Source;

/// <summary>
/// General purpose realm enumeration
/// </summary>
public enum Realm
{
	Client,
	Server,
	Menu
}
public static class BitVecExts
{
	/// <summary>
	/// Checks if the bit is set.
	/// </summary>
	/// <param name="bools"></param>
	/// <param name="bit"></param>
	/// <returns></returns>
	public static bool IsBitSet(this bool[] bools, int bit) => bools[bit];
	/// <summary>
	/// Sets the bit to 1.
	/// </summary>
	/// <param name="bools"></param>
	/// <param name="bit"></param>
	public static void Set(this bool[] bools, int bit) => bools[bit] = true;
	/// <summary>
	/// Sets the bit to 0.
	/// </summary>
	/// <param name="bools"></param>
	/// <param name="bit"></param>
	public static void Clear(this bool[] bools, int bit) => bools[bit] = false;
}
public static class ClassUtils
{
	public static void EnsureCount<T>(this List<T> list, int ensureTo) where T : class, new() {
		while (list.Count < ensureTo) {
			list.Add(new T());
		}
	}

	private static bool ParametersMatch(ParameterInfo[] parameters, ImmutableArray<Type> argTypes) {
		if (parameters.Length != argTypes.Length)
			return false;

		for (int i = 0; i < parameters.Length; i++) {
			Type paramType = parameters[i].ParameterType;
			Type argType = argTypes[i];

			// Allow for null arguments and assignable types
			if (argType == typeof(object))
				continue;

			if (!paramType.IsAssignableFrom(argType))
				return false;
		}

		return true;
	}

	public static T? InitSubsystem<T>(this IEngineAPI api, params object?[] parms) where T : class
		=> InitSubsystem<T>(api, out _, parms);
	public static T? InitSubsystem<T>(this IEngineAPI api, out string? error, params object?[] parms)
		where T : class {
		// Argument types for the method call.
		var argTypes = parms.Select(arg => arg?.GetType() ?? typeof(object)).ToImmutableArray();
		// The instance from IEngineAPI
		var instance = api.GetRequiredService<T>();
		// Find a method that has
		//     1. "Init" as the name.
		//     2. Matches the parameters the callee provided.
		var method = typeof(T)
			.GetMethods()
			.Where(x => x.Name == "Init")
			.FirstOrDefault(m => ParametersMatch(m.GetParameters(), argTypes));
		// If the method returns booleans, return whatever the call provides
		if (method != null && method.ReturnType == typeof(bool)) {
			bool ok = (bool)(method.Invoke(instance, parms) ?? true);
			error = ok ? null : $"The subsystem '{typeof(T).Name}' failed to initialize.";
			return ok ? instance : null;
		}
		// Method invoke, return true
		try {
			method?.Invoke(instance, parms);
		}
		// If we don't do this try catch block then we don't get the inner exception in call stacks
		// and instead get the call stack of the target invocation exception (which is pretty useless
		// in this case)
		catch (TargetInvocationException ex) when (ex.InnerException != null) {
			ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
			throw;
		}
		error = null;
		return instance;
	}

	// Some methods for reading binary data into arrays/value references
	public static unsafe bool ReadNothing(this BinaryReader reader, int howMany) {
		while (howMany > 0) {
			if (reader.PeekChar() == -1)
				return false;
			reader.Read();
			howMany--;
		}
		return true;
	}
	public static unsafe bool ReadInto<T>(this BinaryReader reader, ref T into) where T : unmanaged {
		int sizeOfOne = sizeof(T);
		Span<byte> byteAlloc = stackalloc byte[sizeOfOne];
		if (sizeOfOne != reader.Read(byteAlloc))
			return false; // Not enough data.
		into = MemoryMarshal.Cast<byte, T>(byteAlloc)[0];
		return true;
	}
	public static unsafe bool ReadInto<T>(this BinaryReader reader, Span<T> into) where T : unmanaged {
		int sizeOfOne = sizeof(T);
		int sizeOfAll = sizeOfOne * into.Length;
		Span<byte> byteAlloc = stackalloc byte[sizeOfAll];
		if (sizeOfAll != reader.Read(byteAlloc))
			return false; // Not enough data.

		MemoryMarshal.Cast<byte, T>(byteAlloc).CopyTo(into);
		return true;
	}
	public static string? ReadString(this BinaryReader reader, int length) {
		Span<char> str = stackalloc char[length];
		for (int i = 0; i < length; i++) {
			if (reader.PeekChar() == -1)
				break;

			str[i] = reader.ReadChar();
		}
		return new(str);
	}
}

public static class UnmanagedUtils
{
	public static void SliceNullTerminatedStringInPlace(this ref Span<char> span) {
		int index = span.IndexOf('\0');
		if (index == -1)
			return;
		span = span[..index];
	}

	public static Span<char> SliceNullTerminatedString(this Span<char> span) {
		int index = span.IndexOf('\0');
		if (index == -1)
			return span;
		return span[..index];
	}

	public static ReadOnlySpan<char> SliceNullTerminatedString(this ReadOnlySpan<char> span) {
		int index = span.IndexOf('\0');
		if (index == -1)
			return span;
		return span[..index];
	}

	public static void EnsureCount<T>(this List<T> list, int ensureTo) where T : unmanaged {
		list.EnsureCapacity(ensureTo);
		while (list.Count < ensureTo) {
			list.Add(new T());
		}
	}

	public static unsafe ulong Hash(this ReadOnlySpan<char> str, bool invariant = true) {
		if (str == null || str.Length == 0)
			return 0;

		ulong hash;

		if (invariant) {
			bool veryLarge = str.Length > 1024;
			if (veryLarge) {
				char[] lowerBuffer = ArrayPool<char>.Shared.Rent(str.Length);
				str.ToLowerInvariant(lowerBuffer);
				hash = XXH64.DigestOf(MemoryMarshal.Cast<char, byte>(lowerBuffer));
				ArrayPool<char>.Shared.Return(lowerBuffer, true);
			}
			else {
				Span<char> lowerBuffer = stackalloc char[str.Length];
				str.ToLowerInvariant(lowerBuffer);
				hash = XXH64.DigestOf(MemoryMarshal.Cast<char, byte>(lowerBuffer));
			}
		}
		else 
			hash = XXH64.DigestOf(MemoryMarshal.Cast<char, byte>(str));

		return hash;
	}

	public static unsafe ulong Hash<T>(this T target) where T : unmanaged {
		ref T t = ref target;
		fixed (T* ptr = &t) {
			Span<byte> data = new(ptr, Unsafe.SizeOf<T>());
			return XXH64.DigestOf(data);
		}
	}

	public static unsafe ulong Hash<T>(this Span<T> target) where T : unmanaged {
		if (target == null) return 0;
		ReadOnlySpan<byte> data = MemoryMarshal.Cast<T, byte>(target);
		return XXH64.DigestOf(data);
	}

	public static unsafe ulong Hash(this string target, bool invariant = true) => Hash((ReadOnlySpan<char>)target, invariant);

	public static unsafe void ZeroOut<T>(this T[] array) where T : unmanaged {
		fixed (T* ptr = array) {
			for (int i = 0, c = array.Length; i < c; i++)
				ptr[i] = default;
		}
	}

	public static string[] Split(this ReadOnlySpan<char> input, char separator) {
		Span<Range> ranges = stackalloc Range[64];
		var splits = input.Split(ranges, ' ');
		string[] array = new string[splits];
		for (int i = 0; i < splits; i++) {
			array[i] = new(input[ranges[i]]);
		}
		return array;
	}

	public static void EatWhiteSpace(this StringReader buffer) {
		if (buffer.IsValid()) {
			while (buffer.IsValid()) {
				if (!char.IsWhiteSpace(buffer.PeekChar()))
					break;
				buffer.Read();
			}
		}
	}
	public static bool EatCPPComment(this StringReader buffer) {
		if (buffer.IsValid()) {
			ReadOnlySpan<char> peek = buffer.Peek(2);
			if (!peek.Equals("//", StringComparison.OrdinalIgnoreCase))
				return false;

			buffer.Seek(2, SeekOrigin.Current);

			for (char c = buffer.GetChar(); buffer.IsValid(); c = buffer.GetChar()) {
				if (c == '\n')
					break;
			}
			return true;
		}

		return false;
	}

	// Wow this sucks
	private static readonly FieldInfo? posField = typeof(StringReader)
		.GetField("_pos", BindingFlags.NonPublic | BindingFlags.Instance);
	private static readonly FieldInfo? strField = typeof(StringReader)
		.GetField("_s", BindingFlags.NonPublic | BindingFlags.Instance);
	private static void EnsureAvailable() {
		if (posField == null || strField == null)
			throw new InvalidOperationException("Reflection failed: StringReader internal fields not found. Implementation may have changed.");
	}
	public static int TellMaxPut(this StringReader buffer) {
		string? underlying = (string?)strField!.GetValue(buffer);
		if (underlying == null)
			return -1;

		return underlying.Length;
	}
	public static int TellGet(this StringReader buffer) {
		int current = (int)posField!.GetValue(buffer)!;
		return current;
	}
	public static ReadOnlySpan<char> Peek(this StringReader buffer, int length) {
		string? underlying = (string?)strField!.GetValue(buffer);
		if (underlying == null)
			return null;

		int current = (int)posField!.GetValue(buffer)!;
		return underlying.AsSpan()[current..(current + Math.Min(length, underlying.Length))];
	}
	public static ReadOnlySpan<char> PeekToEnd(this StringReader buffer) {
		string? underlying = (string?)strField!.GetValue(buffer);
		if (underlying == null)
			return null;

		int current = (int)posField!.GetValue(buffer)!;
		return underlying.AsSpan()[current..];
	}
	public static int Seek(this StringReader reader, int offset, SeekOrigin origin) {
		if (reader == null) throw new ArgumentNullException(nameof(reader));
		EnsureAvailable();

		string? underlying = (string?)strField!.GetValue(reader);
		if (underlying == null)
			throw new InvalidOperationException("Underlying string is null.");

		int length = underlying.Length;
		int current = (int)posField!.GetValue(reader)!;
		long target;
		switch (origin) {
			case SeekOrigin.Begin:
				target = offset;
				break;
			case SeekOrigin.Current:
				target = current + offset;
				break;
			case SeekOrigin.End:
				target = length + offset;
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
		}

		if (target < 0) target = 0;
		if (target > length) target = length;

		int newPos = (int)target;
		posField.SetValue(reader, newPos);
		return newPos;
	}


	public static char PeekChar(this StringReader buffer) => (char)buffer.Peek();
	public static char GetChar(this StringReader buffer) => (char)buffer.Read();
	public static bool IsValid(this StringReader buffer) => buffer.Peek() >= 0;
	public static int ParseToken(this StringReader buffer, in CharacterSet breaks, Span<char> tokenBuf, bool parseComments = true) {
		while (true) {
			if (!buffer.IsValid())
				return -1;

			buffer.EatWhiteSpace();
			if (parseComments) {
				if (!buffer.EatCPPComment())
					break;
			}
			else
				break;
		}

		char c = buffer.GetChar();
		if (c == '\"') {
			int len = 0;
			while (buffer.IsValid()) {
				c = buffer.GetChar();
				if (c == '\"' || c == 0) {
					return len;
				}
				tokenBuf[len] = c;
				if (++len == tokenBuf.Length) {
					return tokenBuf.Length;
				}
			}

			return len;
		}

		if (breaks.Contains(c)) {
			tokenBuf[0] = c;
			return 1;
		}

		int wordLen = 0;
		while (true) {
			if (!buffer.IsValid())
				break;

			tokenBuf[wordLen] = c;
			if (++wordLen == tokenBuf.Length) {
				return tokenBuf.Length;
			}

			c = buffer.GetChar();


			if (breaks.Contains(c) || c == '\"' || (c > '\0' && c <= ' ')) {
				buffer.Seek(-1, SeekOrigin.Current);
				break;
			}
		}

		return wordLen;
	}
}

/// <summary>
/// Various C# reflection utilities
/// </summary>
public static class ReflectionUtils
{
	public static bool TryToDelegate<T>(this MethodInfo m, object? instance, [NotNullWhen(true)] out T? asDelegate) where T : Delegate {
		return (asDelegate =
			(T?)(instance == null
				? Delegate.CreateDelegate(typeof(T), m, false)
				: Delegate.CreateDelegate(typeof(T), instance, m, false))
			) != null;
	}

	public static bool TryExtractMethodDelegate<T>(this Type type, object? instance, Func<MethodInfo, bool> preFilter, [NotNullWhen(true)] out T? asDelegate) where T : Delegate {
		if (TryFindMatchingMethod(type, typeof(T), preFilter, out MethodInfo? methodInfo) && TryToDelegate(methodInfo, instance, out asDelegate))
			return true;

		asDelegate = null;
		return false;
	}

	public static bool DoesMethodMatch(this MethodInfo m, Type[] delegateParams, Type delegateReturn, Func<MethodInfo, bool>? preFilter = null) {
		if (preFilter != null)
			return preFilter(m);

		if (m.ReturnType != delegateReturn)
			return false;

		var methodParams = m.GetParameters().Select(p => p.ParameterType).ToArray();
		if (methodParams.Length != delegateParams.Length)
			return false;

		for (int i = 0; i < methodParams.Length; i++) {
			if (methodParams[i] != delegateParams[i])
				return false;
		}

		return true;
	}
	public static MethodInfo? FindMatchingMethod(this Type targetType, Type[] delegateParams, Type delegateReturn, Func<MethodInfo, bool>? preFilter = null)
		=> targetType
			.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
			.FirstOrDefault(m => DoesMethodMatch(m, delegateParams, delegateReturn, preFilter));
	public static MethodInfo? FindMatchingMethod(this Type targetType, Type delegateType, Func<MethodInfo, bool>? preFilter = null) {
		if (!typeof(Delegate).IsAssignableFrom(delegateType))
			throw new ArgumentException("delegateType must be a delegate", nameof(delegateType));

		var invoke = delegateType.GetMethod("Invoke")!;
		var delegateParams = invoke.GetParameters().Select(p => p.ParameterType).ToArray();
		var delegateReturn = invoke.ReturnType;
		return FindMatchingMethod(targetType, delegateParams, delegateReturn, preFilter);
	}
	public static MethodInfo? FindMatchingMethod<T>(this Type targetType, Func<MethodInfo, bool>? preFilter = null) where T : Delegate => FindMatchingMethod(targetType, typeof(T), preFilter);


	public static bool TryFindMatchingMethod(this Type targetType, Type[] delegateParams, Type delegateReturn, Func<MethodInfo, bool>? preFilter, [NotNullWhen(true)] out MethodInfo? info) {
		info = FindMatchingMethod(targetType, delegateParams, delegateReturn, preFilter);
		return info != null;
	}

	public static bool TryFindMatchingMethod(this Type targetType, Type delegateType, Func<MethodInfo, bool>? preFilter, [NotNullWhen(true)] out MethodInfo? info) {
		info = FindMatchingMethod(targetType, delegateType, preFilter);
		return info != null;
	}

	public static bool TryFindMatchingMethod<T>(this Type targetType, Func<MethodInfo, bool>? preFilter, [NotNullWhen(true)] out MethodInfo? info) where T : Delegate {
		info = FindMatchingMethod<T>(targetType, preFilter);
		return info != null;
	}
	static IEnumerable<Type> safeTypeGet(Assembly assembly) {
		if (!IsOkAssembly(assembly))
			yield break;

		IEnumerable<Type?> types;
		try {
			types = assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException e) {
			types = e.Types;
		}

		foreach (var t in types.Where(t => t != null))
			yield return t!;
	}

	public static bool IsOkAssembly(Assembly assembly) {
		// ugh, what a hack - but for now, this is the only way to get things sanely. Need a better way.
		if (!assembly.GetName().Name!.StartsWith("Source") && !assembly.GetName().Name!.StartsWith("Game"))
			return false;

		return true;
	}

	public static IEnumerable<Assembly> GetAssemblies()
		=> AppDomain.CurrentDomain.GetAssemblies().Where(IsOkAssembly);
	public static IEnumerable<Type> GetLoadedTypes()
		=> AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(safeTypeGet);

	public static IEnumerable<KeyValuePair<Type, T>> GetLoadedTypesWithAttribute<T>() where T : Attribute {
		foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(safeTypeGet)) {
			T? attr = type.GetCustomAttribute<T>();
			if (attr != null)
				yield return new(type, attr);
		}
	}

	public static IEnumerable<KeyValuePair<Type, T>> GetTypesWithAttribute<T>(this Assembly assembly) where T : Attribute {
		foreach (var type in assembly.GetTypes()) {
			T? attr = type.GetCustomAttribute<T>();
			if (attr != null)
				yield return new(type, attr);
		}
	}

	public static IEnumerable<KeyValuePair<ConstructorInfo, T>> GetConstructorsWithAttribute<T>(this Type type) where T : Attribute {
		foreach (var constructor in type.GetConstructors()) {
			T? attr = type.GetCustomAttribute<T>();
			if (attr != null)
				yield return new(constructor, attr);
		}
	}
	public static IEnumerable<KeyValuePair<PropertyInfo, T>> GetPropertiesWithAttribute<T>(this Type type) where T : Attribute {
		foreach (var prop in type.GetProperties()) {
			T? attr = type.GetCustomAttribute<T>();
			if (attr != null)
				yield return new(prop, attr);
		}
	}
	public static IEnumerable<KeyValuePair<FieldInfo, T>> GetFieldsWithAttribute<T>(this Type type) where T : Attribute {
		foreach (var field in type.GetFields()) {
			T? attr = type.GetCustomAttribute<T>();
			if (attr != null)
				yield return new(field, attr);
		}
	}
	public static IEnumerable<KeyValuePair<MethodInfo, T>> GetMethodsWithAttribute<T>(this Type type) where T : Attribute {
		foreach (var method in type.GetMethods()) {
			T? attr = type.GetCustomAttribute<T>();
			if (attr != null)
				yield return new(method, attr);
		}
	}
}

/// <summary>
/// Marks the class as being injectable into the <see cref="IEngineAPI"/> dependency injection collection.
/// <br/>
/// Is handled by <see cref="Source.Engine.EngineBuilder"/> later on.
/// </summary
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EngineComponentAttribute : Attribute;