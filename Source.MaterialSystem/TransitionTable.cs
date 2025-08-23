using CommunityToolkit.HighPerformance;

using OpenGL;

using Source.Common.Hashing;
using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;
using Source.Common.Utilities;

using System.Diagnostics.Metrics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Source.MaterialSystem;



public struct SnapshotShaderState
{
	public ShadowShaderState ShaderState;
	public ShadowStateId_t ShadowStateId;
	public ushort Reserved;
	public uint Reserved2;
}

public enum RenderStateFunc : byte
{
	DepthTest = 0,
	ZWriteEnable,
	ColorWriteEnable,
	AlphaTest,
	FillMode,
	Lighting,
	SpecularEnable,
	SRGBWriteEnable,
	AlphaBlend,
	SeparateAlphaBlend,
	CullEnable,
	VertexBlendEnable,
	FogMode,
	ActivateFixedFunction,
	TextureEnable,
	DiffuseMaterialSource,
	DisableFogGammaCorrection,
	EnableAlphaToCoverage,

	Count,
}

public enum TextureStateFunc : byte
{
	TexCoordIndex = 0,
	SRGBReadEnable,
	Fetch4Enable,
	ShadowFilterEnable,
	ColorTextureStage,
	AlphaTextureStage,
	Count
}

public delegate void ApplyStateFunc(in ShadowState shadowState, int arg);

[StructLayout(LayoutKind.Explicit)]
public record struct TransitionOp
{
	[FieldOffset(0)] public byte Bits;
	[FieldOffset(0)] public byte Info;

	public byte OpCode {
		get => (byte)(Info & 0x7F);      // lower 7 bits
		set => Info = (byte)((Info & 0x80) | (value & 0x7F));
	}

	public bool IsTextureCode {
		readonly get => (Info & 0x80) != 0;        // highest bit
		set {
			if (value)
				Info |= 0x80;
			else
				Info &= 0x7F;
		}
	}
}



public class TransitionTable
{
	public const int TEXTURE_STAGE_BIT_COUNT = 4;
	public const int TEXTURE_STAGE_MAX_STAGE = 1 << TEXTURE_STAGE_BIT_COUNT;
	public const int TEXTURE_STAGE_MASK = TEXTURE_STAGE_MAX_STAGE - 1;
	public const int TEXTURE_OP_BIT_COUNT = 7 - TEXTURE_STAGE_BIT_COUNT;
	public const int TEXTURE_OP_SHIFT = TEXTURE_STAGE_BIT_COUNT;
	public const int TEXTURE_OP_MASK = ((1 << TEXTURE_OP_BIT_COUNT) - 1) << TEXTURE_OP_SHIFT;

	public HardwareConfig HardwareConfig;

	public ApplyStateFunc[] RenderFunctionTable;
	public ApplyStateFunc[] TextureFunctionTable;

	public readonly ShaderShadowGl46 ShaderShadow;

	public TransitionTable(ShaderShadowGl46 shaderShadow) {
		ShaderShadow = shaderShadow;
		RenderFunctionTable = [
			ApplyDepthTest,
			ApplyZWriteEnable,
			ApplyColorWriteEnable,
			ApplyAlphaTest,
			ApplyFillMode,
			ApplyLighting,
			ApplySpecularEnable,
			ApplySRGBWriteEnable,
			ApplyAlphaBlend,
			ApplySeparateAlphaBlend,
			ApplyCullEnable,
			ApplyVertexBlendEnable,
			ApplyFogMode,
			ApplyActivateFixedFunction,
			ApplyTextureEnable,			// Enables textures on *all* stages
			ApplyDiffuseMaterialSource,
			ApplyDisableFogGammaCorrection,
			ApplyAlphaToCoverage,
		];
	}
	public void ApplyDepthTest(in ShadowState state, int arg) {
		SetZEnable(state.ZEnable > 0);
		if (state.ZEnable != GL_FALSE)
			SetZFunc(state.ZFunc);

		if(CurrentState.ZBias != (PolygonOffsetMode)state.ZBias) {
			ShaderAPI.ApplyZBias(in state);
			CurrentState.ZBias = (PolygonOffsetMode)state.ZBias;
		}

	}

	private void SetZEnable(bool enable) {
		if(CurrentState.ZEnable != enable) {
			glToggle(GL_DEPTH_TEST, enable);
			CurrentState.ZEnable = enable;
		}
	}

	private void SetZFunc(uint zFunc) {
		if(CurrentState.ZFunc != zFunc) {
			glDepthFunc((ShaderCompareFunc)zFunc switch {
				ShaderCompareFunc.Never => GL_NEVER,
				ShaderCompareFunc.Less => GL_LESS,
				ShaderCompareFunc.Equal => GL_EQUAL,
				ShaderCompareFunc.LessEqual => GL_LEQUAL,
				ShaderCompareFunc.Greater => GL_GREATER,
				ShaderCompareFunc.NotEqual => GL_NOTEQUAL,
				ShaderCompareFunc.GreaterEqual => GL_GEQUAL,
				ShaderCompareFunc.Always => GL_ALWAYS,
				_ => throw new NotImplementedException()
			});
			CurrentState.ZFunc = zFunc;
		}
	}

	public void ApplyZWriteEnable(in ShadowState state, int arg) {
		Warning("WARNING: Tried to send ZWriteEnable to GPU, not implemented!!!\n");
		BoardState.ZWriteEnable = state.ZWriteEnable;
	}
	public void ApplyColorWriteEnable(in ShadowState state, int arg) {
		Warning("WARNING: Tried to send ColorWriteEnable to GPU, not implemented!!!\n");
		BoardState.ColorWriteEnable = state.ColorWriteEnable;
	}
	public void ApplyAlphaTest(in ShadowState state, int arg) {
		if (CurrentState.AlphaTestEnable != state.AlphaTestEnable) {
			Warning("WARNING: Tried to send AlphaTest to GPU, not implemented!!!\n");
			CurrentState.AlphaTestEnable = state.AlphaTestEnable;
		}

		if (state.AlphaTestEnable) {
			// Set the blend state here...
			if (CurrentState.AlphaFunc != state.AlphaFunc) {
				Warning("WARNING: Tried to send AlphaFunc to GPU, not implemented!!!\n");
				CurrentState.AlphaFunc = state.AlphaFunc;
			}

			if (CurrentState.AlphaRef != state.AlphaRef) {
				Warning("WARNING: Tried to send AlphaRef to GPU, not implemented!!!\n");
				CurrentState.AlphaRef = state.AlphaRef;
			}
		}
	}
	public void ApplyFillMode(in ShadowState state, int arg) {
	}
	public void ApplyLighting(in ShadowState state, int arg) {
	}
	public void ApplySpecularEnable(in ShadowState state, int arg) { 
	}
	public void ApplySRGBWriteEnable(in ShadowState state, int arg) {
	}
	public void ApplyAlphaBlend(in ShadowState state, int arg) {
		if (CurrentState.AlphaBlendEnable != state.AlphaBlendEnable) {
			Warning("WARNING: Tried to send AlphaBlendEnable to GPU, not implemented!!!\n");
			CurrentState.AlphaBlendEnable = state.AlphaBlendEnable;
		}

		if (state.AlphaBlendEnable) {
			// Set the blend state here...
			if (CurrentState.SrcBlend != state.SrcBlend) {
				Warning("WARNING: Tried to send SrcBlend to GPU, not implemented!!!\n");
				CurrentState.SrcBlend = state.SrcBlend;
			}

			if (CurrentState.DestBlend != state.DestBlend) {
				Warning("WARNING: Tried to send AlphaRef to GPU, not implemented!!!\n");
				CurrentState.DestBlend = state.DestBlend;
			}

			if (CurrentState.BlendOp != state.BlendOp) {
				Warning("WARNING: Tried to send BlendOp to GPU, not implemented!!!\n");
				CurrentState.BlendOp = state.BlendOp;
			}
		}
	}
	public void ApplySeparateAlphaBlend(in ShadowState state, int arg) {
		if (CurrentState.SeparateAlphaBlendEnable != state.SeparateAlphaBlendEnable) {
			Warning("WARNING: Tried to send SeparateAlphaBlendEnable to GPU, not implemented!!!\n");
			CurrentState.SeparateAlphaBlendEnable = state.SeparateAlphaBlendEnable;
		}

		if (state.SeparateAlphaBlendEnable) {
			// Set the blend state here...
			if (CurrentState.SrcBlendAlpha != state.SrcBlendAlpha) {
				Warning("WARNING: Tried to send SrcBlendAlpha to GPU, not implemented!!!\n");
				CurrentState.SrcBlendAlpha = state.SrcBlendAlpha;
			}

			if (CurrentState.DestBlendAlpha != state.DestBlendAlpha) {
				Warning("WARNING: Tried to send DestBlendAlpha to GPU, not implemented!!!\n");
				CurrentState.DestBlendAlpha = state.DestBlendAlpha;
			}

			if (CurrentState.BlendOpAlpha != state.BlendOpAlpha) {
				Warning("WARNING: Tried to send BlendOpAlpha to GPU, not implemented!!!\n");
				CurrentState.BlendOpAlpha = state.BlendOpAlpha;
			}
		}
	}
	public void ApplyCullEnable(in ShadowState state, int arg) {
		ShaderAPI.ApplyCullEnable(state.CullEnable);
	}
	public void ApplyVertexBlendEnable(in ShadowState state, int arg) {
		ShaderAPI.ApplyVertexBlendEnable(state.VertexBlendEnable);
	}
	public void ApplyFogMode(in ShadowState state, int arg) {
		ShaderAPI.ApplyFogMode(state.FogMode);
	}
	public void ApplyActivateFixedFunction(in ShadowState state, int arg) {
		ShaderAPI.ApplyCullEnable(state.CullEnable);
	}
	public void ApplyTextureEnable(in ShadowState state, int arg) {
		int i;
		int nSamplerCount = HardwareConfig.GetSamplerCount();
		for (i = 0; i < nSamplerCount; ++i) {
			ShaderAPI.ApplyTextureEnable(in state, i);
		}
	}
	public void ApplyDiffuseMaterialSource(in ShadowState state, int arg) {

	}
	public void ApplyDisableFogGammaCorrection(in ShadowState state, int arg) {
	}
	public void ApplyAlphaToCoverage(in ShadowState state, int arg) {
		ShaderAPI.ApplyAlphaToCoverage(state.EnableAlphaToCoverage);
	}

	List<TransitionOp> TransitionOps = [];

	public void AddTransition(RenderStateFunc func) {
		TransitionOps.Add(new() {
			IsTextureCode = false,
			OpCode = (byte)func
		});
	}
	public void AddTextureTransition(TextureStateFunc func, int stage) {
		TransitionOps.Add(new() {
			IsTextureCode = true,
			OpCode = TextureOp(func, stage)
		});
	}
	public int CreateNormalTransitions(in ShadowState fromState, in ShadowState toState, bool force) {
		int numOps = 0;

		// Special case for alpha blending to eliminate extra transitions
		bool blendEnableDifferent = (toState.AlphaBlendEnable != fromState.AlphaBlendEnable);
		bool srcBlendDifferent = toState.AlphaBlendEnable && (toState.SrcBlend != fromState.SrcBlend);
		bool destBlendDifferent = toState.AlphaBlendEnable && (toState.DestBlend != fromState.DestBlend);
		bool blendOpDifferent = toState.AlphaBlendEnable && (toState.BlendOp != fromState.BlendOp);
		if (force || blendOpDifferent || blendEnableDifferent || srcBlendDifferent || destBlendDifferent) {
			AddTransition(RenderStateFunc.AlphaBlend);
			++numOps;
		}

		// Shouldn't have m_SeparateAlphaBlendEnable set unless m_AlphaBlendEnable is also set.
		Assert(toState.AlphaBlendEnable || !toState.SeparateAlphaBlendEnable);
		bool blendSeparateAlphaEnableDifferent = (toState.SeparateAlphaBlendEnable != fromState.SeparateAlphaBlendEnable);
		bool srcBlendAlphaDifferent = toState.SeparateAlphaBlendEnable && (toState.SrcBlendAlpha != fromState.SrcBlendAlpha);
		bool destBlendAlphaDifferent = toState.SeparateAlphaBlendEnable && (toState.DestBlendAlpha != fromState.DestBlendAlpha);
		bool blendOpAlphaDifferent = toState.SeparateAlphaBlendEnable && (toState.BlendOpAlpha != fromState.BlendOpAlpha);
		if (force || blendOpAlphaDifferent || blendSeparateAlphaEnableDifferent || srcBlendAlphaDifferent || destBlendAlphaDifferent) {
			AddTransition(RenderStateFunc.SeparateAlphaBlend);
			++numOps;
		}

		bool bAlphaTestEnableDifferent = (toState.AlphaTestEnable != fromState.AlphaTestEnable);
		bool bAlphaFuncDifferent = toState.AlphaTestEnable && (toState.AlphaFunc != fromState.AlphaFunc);
		bool bAlphaRefDifferent = toState.AlphaTestEnable && (toState.AlphaRef != fromState.AlphaRef);
		if (force || bAlphaTestEnableDifferent || bAlphaFuncDifferent || bAlphaRefDifferent) {
			AddTransition(RenderStateFunc.AlphaTest);
			++numOps;
		}

		bool bDepthTestEnableDifferent = (toState.ZEnable != fromState.ZEnable);
		bool bDepthFuncDifferent = (toState.ZEnable != Gl46.GL_FALSE) && (toState.ZFunc != fromState.ZFunc);
		bool bDepthBiasDifferent = (toState.ZBias != fromState.ZBias);
		if (force || bDepthTestEnableDifferent || bDepthFuncDifferent || bDepthBiasDifferent) {
			AddTransition(RenderStateFunc.DepthTest);
			++numOps;
		}

		if (force || (toState.DisableFogGammaCorrection != fromState.DisableFogGammaCorrection)) {
			AddTransition(RenderStateFunc.DisableFogGammaCorrection);
			++numOps;
		}

		return numOps;
	}

	public static byte TextureOp(TextureStateFunc func, int stage) {
		Assert(stage < TEXTURE_STAGE_MAX_STAGE);
		return (byte)((((int)func << TEXTURE_OP_SHIFT) & TEXTURE_OP_MASK) | (stage & TEXTURE_STAGE_MASK));
	}

	public static void GetTextureOp(byte bits, out TextureStateFunc func, out int stage) {
		stage = bits & TEXTURE_STAGE_MASK;
		func = (TextureStateFunc)((bits & TEXTURE_OP_MASK) >> TEXTURE_OP_SHIFT);
	}

	RefStack<ShadowState> ShadowStateList = [];
	RefStack<SnapshotShaderState> SnapshotList = [];
	public ref ShadowState GetSnapshot(StateSnapshot_t snapshotID) {
		Assert(snapshotID >= 0 && snapshotID < SnapshotList.Count);
		return ref ShadowStateList[SnapshotList[snapshotID].ShadowStateId];
	}
	public ref ShadowShaderState GetSnapshotShader(StateSnapshot_t snapshotID) {
		Assert(snapshotID >= 0 && snapshotID < SnapshotList.Count);
		return ref SnapshotList[snapshotID].ShaderState;
	}

	internal StateSnapshot_t TakeSnapshot() {
		ShaderShadow.ComputeAggregateShadowState();

		ref ShadowState currentState = ref ShaderShadow.GetShadowState();
		ShadowStateId_t shadowStateId = FindShadowState(ref currentState);

		if (shadowStateId == -1) {
			shadowStateId = CreateShadowState(ref currentState);

			for (short to = 0; to < shadowStateId; to++) {
				CreateTransitionTableEntry(to, shadowStateId);
			}

			for (short from = 0; from < shadowStateId; from++) {
				CreateTransitionTableEntry(shadowStateId, from);
			}
		}

		ref ShadowShaderState currentShaderState = ref ShaderShadow.GetShadowShaderState();
		StateSnapshot_t snapshotId = FindStateSnapshot(shadowStateId, ref currentShaderState);
		if (snapshotId == -1) {
			snapshotId = CreateStateSnapshot(shadowStateId, ref currentShaderState);
		}

		return snapshotId;
	}

	private unsafe StateSnapshot_t CreateStateSnapshot(ShadowStateId_t shadowStateId, ref ShadowShaderState currentShaderState) {
		SnapshotList.Push();
		StateSnapshot_t snapshotId = (StateSnapshot_t)SnapshotList.Count;
		ref SnapshotShaderState shaderState = ref SnapshotList[snapshotId];
		shaderState.ShadowStateId = shadowStateId;
		shaderState.ShaderState = currentShaderState;

		CRC32_t checksum = new();
		fixed (SnapshotShaderState* pTemp = &shaderState) {
			CRC32.Init(ref checksum);
			CRC32.ProcessBuffer(ref checksum, pTemp, sizeof(SnapshotShaderState));
			CRC32.Final(ref checksum);
		}

		SnapshotDict[checksum] = snapshotId;
		return snapshotId;
	}

	readonly SortedList<CRC32_t, ShadowStateId_t> ShadowStateDict = [];
	readonly SortedList<CRC32_t, ShadowStateId_t> SnapshotDict = [];
	readonly SortedList<uint, TransitionList> UniqueTransitions = [];

	private unsafe StateSnapshot_t FindStateSnapshot(ShadowStateId_t id, ref ShadowShaderState currentShaderState) {
		SnapshotShaderState* temp = stackalloc SnapshotShaderState[1];
		temp->ShadowStateId = id;
		temp->ShaderState = currentShaderState;
		CRC32_t checksum = new();
		CRC32.Init(ref checksum);
		CRC32.ProcessBuffer(ref checksum, temp, sizeof(SnapshotShaderState));
		CRC32.Final(ref checksum);

		if (!SnapshotDict.TryGetValue(checksum, out StateSnapshot_t snapshot))
			return -1;

		return snapshot;
	}

	public void UseSnapshot(StateSnapshot_t snapshotId) {
		ShadowStateId_t id = SnapshotList[snapshotId].ShadowStateId;
		if (CurrentSnapshotId != snapshotId) {
			// First apply things that are in the transition table
			if (CurrentShadowId != id) {
				ref TransitionList transition = ref transitionTable[id][CurrentShadowId];
				ApplyTransition(in transition, id);
			}

			// NOTE: There is an opportunity here to set non-dynamic state that we don't
			// store in the transition list if we ever need it.

			CurrentSnapshotId = snapshotId;
		}

		// NOTE: This occurs regardless of whether the snapshot changed because it depends
		// on dynamic state (namely, the dynamic vertex + pixel shader index)
		// Followed by things that are not
		ApplyShaderState(ShadowStateList[id], SnapshotList[snapshotId].ShaderState);
	}

	internal ShaderSystem ShaderManager;

	private void ApplyShaderState(in ShadowState shadowState, in ShadowShaderState shaderState) {
		if (!shadowState.UsingFixedFunction) {
			ShaderManager.SetVertexShader(in shaderState.VertexShader);
			ShaderManager.SetPixelShader(in shaderState.PixelShader);
		}
		else {
			ShaderManager.SetVertexShader(VertexShaderHandle.INVALID);
			ShaderManager.SetPixelShader(PixelShaderHandle.INVALID);
		}
	}

	CurrentState CurrentState;
	ShadowState BoardState;
	ShadowShaderState BoardShaderState;
	StateSnapshot_t DefaultStateSnapshot = -1;
	TransitionList DefaultTransition;

	public void UseDefaultState() {
		CurrentState.OverrideDepthEnable = false;
		CurrentState.OverrideAlphaWriteEnable = false;
		CurrentState.OverrideColorWriteEnable = false;
		CurrentState.ForceDepthFuncEquals = false;
		CurrentState.LinearColorSpaceFrameBufferEnable = false;
		ApplyTransition(in DefaultTransition, DefaultStateSnapshot);

		ShaderManager.SetVertexShader(VertexShaderHandle.INVALID);
		ShaderManager.SetPixelShader(PixelShaderHandle.INVALID);

		CurrentSnapshotId = -1;
	}

	public void TakeDefaultStateSnapshot() {
		if (DefaultStateSnapshot == -1) {
			DefaultStateSnapshot = TakeSnapshot();
			CreateTransitionTableEntry(DefaultStateSnapshot, -1);
		}
	}

	internal ShaderAPIGl46 ShaderAPI;
	internal IShaderDevice ShaderDevice;

	private void ApplyTransition(in TransitionList list, int snapshot) {
		if (ShaderDevice.IsDeactivated())
			return;

		int firstOp = (int)list.FirstOperation;
		int opCount = (int)list.NumOperations;

		ApplyTransitionList(snapshot, firstOp, opCount);
		PerformShadowStateOverrides();

		CurrentShadowId = (StateSnapshot_t)snapshot;
	}

	private void PerformShadowStateOverrides() {

	}

	private void ApplyTransitionList(int snapshot, int firstOp, int opCount) {
		if(opCount > 0) {
			ref ShadowState shadowState = ref ShadowStateList[snapshot];
			ref TransitionOp transitionOp = ref TransitionOps.AsSpan()[firstOp];

			for (int i = 0; i < opCount; ++i) {
				if (transitionOp.IsTextureCode) {
					GetTextureOp(transitionOp.OpCode, out TextureStateFunc code, out int stage);
					TextureFunctionTable[(int)code](in shadowState, stage);
				}
				else {
					RenderFunctionTable[transitionOp.OpCode](in shadowState, 0);
				}
			}
		}
	}

	StateSnapshot_t CurrentSnapshotId;
	ShadowStateId_t CurrentShadowId;

	public int CurrentSnapshot() => (int)CurrentSnapshotId;
	public uint FindIdenticalTransitionList(uint firstElem, ushort numOps, uint firstTest) {
		Span<TransitionOp> transitions = TransitionOps.AsSpan();
		// As it turns out, this works most of the time
		if (firstTest != INVALID_TRANSITION_OP) {
			ref TransitionOp currOp = ref transitions[(int)firstElem];
			ref TransitionOp testOp = ref transitions[(int)firstTest];
			if (memcmpb(currOp, testOp, numOps * Unsafe.SizeOf<TransitionOp>()))
				return firstTest;
		}

		// Look for a common list
		ref TransitionOp op = ref transitions[(int)firstElem];

		for (int i = 0, count = UniqueTransitions.Count; i < count; ++i) {
			TransitionList list = UniqueTransitions[(uint)i];

			// We can early out here because we've sorted the unique transitions
			// descending by count 
			if (list.NumOperations < numOps)
				return INVALID_TRANSITION_OP;

			// If we don't find a match in the first 
			int potentialMatch;
			int lastTest = (int)(list.FirstOperation + list.NumOperations - numOps);
			for (potentialMatch = (int)list.FirstOperation; potentialMatch <= lastTest; ++potentialMatch) {
				// Find the first match
				ref TransitionOp testOp2 = ref transitions[potentialMatch];
				if (testOp2.Bits == op.Bits)
					break;
			}

			// No matches found, continue
			if (potentialMatch > lastTest)
				continue;

			// Ok, found a match of the first op, lets see if they all match
			if (numOps == 1)
				return (uint)potentialMatch;

			ref TransitionOp currOp = ref transitions[(int)(firstElem + 1)];
			ref TransitionOp testOp = ref transitions[(int)(potentialMatch + 1)];
			if (!memcmpb(currOp, testOp, (numOps - 1) * Unsafe.SizeOf<TransitionOp>()))
				return (uint)potentialMatch;
		}

		return INVALID_TRANSITION_OP;
	}
	private void CreateTransitionTableEntry(StateSnapshot_t to, StateSnapshot_t from) {
		uint firstElem = (uint)TransitionOps.Count;
		ushort numOps = 0;

		ref ShadowState toState = ref ShadowStateList[to];
		ref ShadowState fromState = ref ((from >= 0) ? ref ShadowStateList[from] : ref ShadowStateList[to]);
		bool force = (from < 0);
		if (force || (toState.ZWriteEnable != fromState.ZWriteEnable)) {
			AddTransition(RenderStateFunc.ZWriteEnable);
			++numOps;
		}
		if (force || (toState.ColorWriteEnable != fromState.ColorWriteEnable)) {
			AddTransition(RenderStateFunc.ColorWriteEnable);
			++numOps;
		}
		if (force || (toState.FillMode != fromState.FillMode)) {
			AddTransition(RenderStateFunc.FillMode);
			++numOps;
		}
		if (force || (toState.Lighting != fromState.Lighting)) {
			AddTransition(RenderStateFunc.Lighting);
			++numOps;
		}
		if (force || (toState.SpecularEnable != fromState.SpecularEnable)) {
			AddTransition(RenderStateFunc.SpecularEnable);
			++numOps;
		}
		if (force || (toState.SRGBWriteEnable != fromState.SRGBWriteEnable)) {
			AddTransition(RenderStateFunc.SRGBWriteEnable);
			++numOps;
		}
		// Some code for the non-trivial transitions
		numOps += (ushort)CreateNormalTransitions(in fromState, in toState, force);

		// NOTE: From here on down are transitions that depend on dynamic state
		// and which can therefore not appear in the state block
		if (force || (toState.CullEnable != fromState.CullEnable)) {
			AddTransition(RenderStateFunc.CullEnable);
			++numOps;
		}
		if (force || (toState.EnableAlphaToCoverage != fromState.EnableAlphaToCoverage)) {
			AddTransition(RenderStateFunc.EnableAlphaToCoverage);
			++numOps;
		}
		if (force || (toState.VertexBlendEnable != fromState.VertexBlendEnable)) {
			AddTransition(RenderStateFunc.VertexBlendEnable);
			++numOps;
		}

		if (force || (toState.FogMode != fromState.FogMode)) {
			AddTransition(RenderStateFunc.FogMode);
			++numOps;
		}

		bool differentTexturesEnabled = false;
		int samplerCount = HardwareConfig.GetSamplerCount();
		for (int i = 0; i < samplerCount; ++i) {
			if (toState.SamplerState[i].TextureEnable != fromState.SamplerState[i].TextureEnable) {
				differentTexturesEnabled = true;
				break;
			}
		}

		if (force || differentTexturesEnabled) {
			AddTransition(RenderStateFunc.TextureEnable);
			++numOps;
		}

		// Look for identical transition lists, and use those instead...
		ref TransitionList transition = ref ((from >= 0) ? ref transitionTable[to][from] : ref DefaultTransition);
		Assert(numOps <= 255);
		transition.NumOperations = numOps;

		// This condition can happen, and is valid. It occurs when we snapshot
		// state but do not generate a transition function for that state
		if (numOps == 0) {
			transition.FirstOperation = INVALID_TRANSITION_OP;
			return;
		}

		// An optimization to try to early out of the identical transition check
		// taking advantage of the fact that the matrix is usually diagonal.
		uint nFirstTest = INVALID_TRANSITION_OP;
		if (from >= 0) {
			ref TransitionList diagonalList = ref transitionTable[from][to];
			if (diagonalList.NumOperations == numOps) {
				nFirstTest = diagonalList.FirstOperation;
			}
		}

		uint identicalListFirstElem = FindIdenticalTransitionList(firstElem, numOps, nFirstTest);
		if (identicalListFirstElem == INVALID_TRANSITION_OP) {
			transition.FirstOperation = firstElem;
			UniqueTransitions.Add(transition.FirstOperation, transition);
			Assert((int)firstElem + (int)numOps < 16777215);

			if ((int)firstElem + (int)numOps >= 16777215) {
				Warning("**** WARNING: Transition table overflow.\n");
			}
		}
		else {
			// Remove the transitions ops we made; use the duplicate copy
			transition.FirstOperation = identicalListFirstElem;
			TransitionOps.RemoveRange((int)firstElem, numOps);
		}
	}

	struct TransitionList
	{
		public uint FirstOperation;
		public uint NumOperations;
	}

	const int INVALID_TRANSITION_OP = 0xFFFFFF;
	List<RefStack<TransitionList>> transitionTable = [];

	private ShadowStateId_t CreateShadowState(ref ShadowState currentState) {
		nint newShaderState = ShadowStateList.AddToTail();
		ShadowStateList[(int)newShaderState] = currentState;

		// all existing states must transition to the new state
		nint i;
		for (i = 0; i < newShaderState; ++i) {
			// Add a new transition to all existing states
			nint newElem = transitionTable[(int)i].AddToTail();
			transitionTable[(int)i][(int)newElem].FirstOperation = INVALID_TRANSITION_OP;
			transitionTable[(int)i][(int)newElem].NumOperations = 0;
		}

		// Add a new vector for this transition
		transitionTable.Add(new());
		nint newTransitionElem = transitionTable.Count - 1;
		transitionTable[(int)newTransitionElem].EnsureCapacity(32);
		Assert(newShaderState == newTransitionElem);

		for (i = 0; i <= newShaderState; ++i) {
			// Add a new transition from all existing states
			nint newElem = transitionTable[(int)newShaderState].AddToTail();
			transitionTable[(int)newShaderState][(int)newElem].FirstOperation = INVALID_TRANSITION_OP;
			transitionTable[(int)newShaderState][(int)newElem].NumOperations = 0;
		}

		CRC32_t checksum = new();
		CRC32.Init(ref checksum);
		unsafe {
			CRC32.ProcessBuffer(ref checksum, Unsafe.AsPointer(ref ShadowStateList[(int)newShaderState]), Unsafe.SizeOf<ShadowState>());
		}
		CRC32.Final(ref checksum);

		SnapshotDict[checksum] = (ShadowStateId_t)newShaderState;

		return (ShadowStateId_t)newShaderState;
	}

	private unsafe ShadowStateId_t FindShadowState(ref ShadowState currentState) {
		CRC32_t checksum = new();
		CRC32.Init(ref checksum);
		CRC32.ProcessBuffer(ref checksum, Unsafe.AsPointer(ref currentState), Unsafe.SizeOf<ShadowState>());
		CRC32.Final(ref checksum);

		int nDictCount = ShadowStateDict.Count;
		if (ShadowStateDict.TryGetValue(checksum, out ShadowStateId_t v))
			return v;

		return (ShadowStateId_t)(-1);
	}

	public unsafe ref ShadowState CurrentShadowState() {
		if (CurrentShadowId == -1)
			return ref Unsafe.NullRef<ShadowState>();

		Assert(CurrentShadowId >= 0 && (CurrentShadowId < ShadowStateList.Count));
		return ref ShadowStateList[CurrentShadowId];
	}

	internal void Init() {

	}
}
