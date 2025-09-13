using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Enumerables;

using K4os.Hash.xxHash;

using Microsoft.Extensions.DependencyInjection;

using Source.Common.Commands;
using Source.Common.Engine;
using Source.Common.Networking;
using Source.Common.Utilities;

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
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

public static class BitVecBase
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static byte ByteMask(int bit) => (byte)(1 << (bit % 8));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsBitSet(this Span<byte> bytes, int bit) {
		byte b = bytes[bit >> 3];
		return (b & ByteMask(bit)) != 0;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Set(this Span<byte> bytes, int bit) {
		ref byte b = ref bytes[bit >> 3];
		b |= ByteMask(bit);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Clear(this Span<byte> bytes, int bit) {
		ref byte b = ref bytes[bit >> 3];
		b &= (byte)~ByteMask(bit);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Set(this Span<byte> bytes, int bit, bool newVal) {
		ref byte b = ref bytes[bit >> 3];
		if (newVal)
			b |= ByteMask(bit);
		else
			b &= (byte)~ByteMask(bit);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int FindNextSetBit(this Span<byte> bytes, int startBit) {
		while (!IsBitSet(bytes, startBit))
			startBit++;
		return startBit;
	}
}

/// <summary>
/// An inline bit-vector array of MAX_EDICTS >> 3 bytes.
/// </summary>
[InlineArray(Constants.MAX_EDICTS >> 3)]
public struct MaxEdictsBitVec
{
	public byte bytes;
	public bool IsBitSet(int bit) => BitVecBase.IsBitSet(this, bit);
	public void Set(int bit) => BitVecBase.Set(this, bit);
	public void Clear(int bit) => BitVecBase.Clear(this, bit);
	public void Set(int bit, bool newVal) => BitVecBase.Set(this, bit, newVal);
	public int FindNextSetBit(int startBit) => BitVecBase.FindNextSetBit(this, startBit);
}

public class ClassMemoryPool<T> where T : class, new()
{
	readonly ConcurrentDictionary<T, bool> valueStates = [];

	public T Alloc() {
		foreach (var kvp in valueStates) {
			if (kvp.Value == false) { // We found something free
				valueStates[kvp.Key] = true;
				return kvp.Key;
			}
		}

		// Make an new instance of the class
		var instance = new T();
		valueStates[instance] = true;
		return instance;
	}

	public bool IsMemoryPoolAllocated(T value) => valueStates.TryGetValue(value, out _);
	public void Free(T value) {
		if (!valueStates.TryGetValue(value, out bool state))
			AssertMsg(false, $"Passed an instance of {typeof(T).Name} to {nameof(Free)}(T value) that was not allocated by {nameof(Alloc)}()");
		else if (state == false)
			AssertMsg(false, $"Attempted to free {typeof(T).Name} instance twice in ClassPool<T>, please verify\n");
		else {
			value.ClearInstantiatedReference();
			valueStates[value] = false;
		}
	}
}

public class StructMemoryPool<T> where T : struct
{
	readonly RefStack<T> instances = [];
	readonly ConcurrentDictionary<int, bool> valueStates = [];

	public ref T Alloc() {
		foreach (var kvp in valueStates) {
			ref T existing = ref instances[kvp.Key];
			if (kvp.Value == false) {
				valueStates[kvp.Key] = true;
				return ref existing;
			}
		}

		lock (instances) {
			ref T instance = ref instances.Push();
			valueStates[instances.Count - 1] = true;
			return ref instance;
		}
	}

	public unsafe bool IsMemoryPoolAllocated(ref T value) {
		lock (instances) {
			for (int i = 0; i < instances.Count; i++) {
				ref T instance = ref instances[i];
				if (Unsafe.AreSame(ref value, ref instance))
					return true;
			}
		}

		return false;
	}


	public void Free(ref T value) {
		lock (instances) {
			for (int i = 0; i < instances.Count; i++) {
				ref T instance = ref instances[i];
				if (Unsafe.AreSame(ref value, ref instance)) {
					if (!valueStates.TryGetValue(i, out bool state))
						AssertMsg(false, $"Passed an instance of {typeof(T).Name} to {nameof(Free)}(T value) that was not allocated by {nameof(Alloc)}()");
					else if (state == false)
						AssertMsg(false, $"Attempted to free {typeof(T).Name} instance twice in StructPool<T>, please verify\n");
					else {
						valueStates[i] = false;
						instance = default; // Zero out the instance
					}
				}
			}
		}
	}
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
	public static bool IsValidIndex<T>(this List<T> list, int index) => index >= 0 && index < list.Count;
	public static bool IsValidIndex<T>(this List<T> list, long index) => index >= 0 && index < list.Count;
	public static void EnsureCount<T>(this List<T> list, int ensureTo) where T : class, new() {
		while (list.Count < ensureTo) {
			list.Add(new T());
		}
	}
	/// <summary>
	/// Each value in the span is null-checked. If null, a new instance is created with no constructor ran. If not null, the existing instance
	/// has all of its fields reset. The latter behavior may break everything and needs further testing.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="array"></param>
	public static void ClearInstantiatedReferences<T>(this T[] array) where T : class => ClearInstantiatedReferences(array.AsSpan());
	public static void ClearInstantiatedReferences<T>(this List<T> array) where T : class => ClearInstantiatedReferences(array.AsSpan());
	private static readonly ConcurrentDictionary<Type, Action<object>> _clearers = new();
	public static void ClearInstantiatedReferences<T>(this Span<T> array) where T : class {
		Action<object> clearer = _clearers.GetOrAdd(typeof(T), CreateClearer);

		foreach (ref T item in array)
			if (item == null)
				item = (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
			else
				clearer(item);
	}
	public static void ClearInstantiatedReference<T>(this T target) where T : class {
		Action<object> clearer = _clearers.GetOrAdd(typeof(T), CreateClearer);
		clearer(target);
	}
	public static Action<object> CreateClearer(Type type) {
		var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (fields.Length == 0)
			return _ => { }; // nothing to clear

		// DynamicMethod signature: void Clear(object target)
		var dm = new DynamicMethod("Clear_" + type.Name, null, new[] { typeof(object) }, true);
		var il = dm.GetILGenerator();

		foreach (var field in fields) {
			il.Emit(OpCodes.Ldarg_0); // load object
			il.Emit(OpCodes.Castclass, type); // cast to actual type

			if (field.FieldType.IsValueType) {
				var local = il.DeclareLocal(field.FieldType);
				il.Emit(OpCodes.Ldloca_S, local);
				il.Emit(OpCodes.Initobj, field.FieldType);
				il.Emit(OpCodes.Ldloc, local);
			}
			else {
				il.Emit(OpCodes.Ldnull);
			}

			il.Emit(OpCodes.Stfld, field);
		}

		il.Emit(OpCodes.Ret);

		return (Action<object>)dm.CreateDelegate(typeof(Action<object>));
	}

	/// <summary>
	/// Creates an array of class instances, where the class instances are not null, but also uninitialized (ie. a reference to an object exists,
	/// but no constructor etc was ran).
	/// </summary>
	/// <returns></returns>
	public static T[] BlankInstantiatedArray<T>(nuint length) where T : class {
		T[] ret = new T[length];
		ClearInstantiatedReferences(ret);
		return ret;
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
	public static bool ReadToStruct<T>(this Stream sr, ref T str) where T : struct {
		Span<byte> block = MemoryMarshal.AsBytes(new Span<T>(ref str));
		return sr.Read(block) != 0;
	}
	public static Span<char> SliceNullTerminatedString(this Span<char> span) {
		int index = span.IndexOf('\0');
		if (index == -1)
			return span;
		return span[..index];
	}

	public static ReadOnlySpan<char> GetFileExtension(this ReadOnlySpan<char> filepath) {
		for (int length = filepath.Length, i = length - 1; i >= 0; i--) {
			if (filepath[i] == '.')
				return filepath[(i + 1)..];
		}
		return "";
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

	public static void FileBase(this ReadOnlySpan<char> inSpan, Span<char> outSpan) {
		// Strip inSpan until we reach a .
		while (inSpan.Length > 0 && inSpan[^1] != '.')
			inSpan = inSpan[..^1];
		// Strip the period
		inSpan = inSpan[..^1];

		// Then repeat the same process, except for a slash
		int lenPtr = inSpan.Length - 1;
		while (lenPtr > 0 && (inSpan[lenPtr] != '/' && inSpan[lenPtr] != '\\'))
			lenPtr--;
		// This should be the final span
		inSpan = inSpan[(lenPtr + 1)..];
		// Then copy 
		inSpan.ClampedCopyTo(outSpan);
	}

	public static unsafe ulong Hash(this string target, bool invariant = true) => Hash((ReadOnlySpan<char>)target, invariant);
	public static char Nibble(this char c) {
		if ((c >= '0') && (c <= '9'))
			return (char)(c - '0');

		if ((c >= 'A') && (c <= 'F'))
			return (char)(c - 'A' + 0x0a);

		if ((c >= 'a') && (c <= 'f'))
			return (char)(c - 'a' + 0x0a);

		return '0';
	}

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
		return underlying.AsSpan()[current..(current + Math.Min(length, underlying.Length - 1))];
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

public static class SpanExts
{
	public static int ClampedCopyTo<T>(this ReadOnlySpan<T> source, Span<T> dest) {
		if (dest.Length < source.Length) {
			// We only copy as much as we can fit.
			source[..dest.Length].CopyTo(dest);
			return dest.Length;
		}

		source.CopyTo(dest);
		return source.Length;
	}

	public static int ClampedCopyTo<T>(this Span<T> source, Span<T> dest) {
		if (dest.Length < source.Length) {
			// We only copy as much as we can fit.
			source[..dest.Length].CopyTo(dest);
			return dest.Length;
		}

		source.CopyTo(dest);
		return source.Length;
	}
}