using Source.Common.MaterialSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Source.MaterialSystem;

public sealed class MaterialVar : IMaterialVar
{
	public override void CopyFrom(IMaterialVar materialVar) {
		throw new NotImplementedException();
	}

	public override void GetFourCCValue(ulong type, out object? data) {
		throw new NotImplementedException();
	}

	public override IMaterial? GetMaterialValue() {
		throw new NotImplementedException();
	}

	public override Matrix4x4 GetMatrixValue() {
		throw new NotImplementedException();
	}

	public override string GetName() {
		return Name;
	}

	public override IMaterial GetOwningMaterial() {
		throw new NotImplementedException();
	}

	public override string GetStringValue() {
		throw new NotImplementedException();
	}

	public override ITexture GetTextureValue() {
		throw new NotImplementedException();
	}

	public override bool IsDefined() {
		throw new NotImplementedException();
	}

	public override bool MatrixIsIdentity() {
		throw new NotImplementedException();
	}

	public override void SetFloatValue(float val) {
		throw new NotImplementedException();
	}

	public override void SetFourCCValue(ulong type, object? data) {
		throw new NotImplementedException();
	}

	public override void SetIntValue(int val) {
		throw new NotImplementedException();
	}

	public override void SetMaterialValue(IMaterial? material) {
		throw new NotImplementedException();
	}

	public override void SetMatrixValue(in Matrix4x4 matrix) {
		throw new NotImplementedException();
	}

	public override void SetStringValue(ReadOnlySpan<char> val) {
		throw new NotImplementedException();
	}

	public override void SetTextureValue(ITexture? texture) {
		throw new NotImplementedException();
	}

	public override bool SetUndefined() {
		throw new NotImplementedException();
	}

	public override void SetValueAutodetectType(ReadOnlySpan<char> val) {
		throw new NotImplementedException();
	}

	public override void SetVecComponentValue(float val, int component) {
		throw new NotImplementedException();
	}

	public override void SetVecValue(ReadOnlySpan<float> val) {
		throw new NotImplementedException();
	}

	public override void SetVecValue(float x, float y) {
		throw new NotImplementedException();
	}

	public override void SetVecValue(float x, float y, float z) {
		throw new NotImplementedException();
	}

	public override void SetVecValue(float x, float y, float z, float w) {
		throw new NotImplementedException();
	}

	protected override float GetFloatValueInternal() {
		throw new NotImplementedException();
	}

	protected override int GetIntValueInternal() {
		throw new NotImplementedException();
	}

	protected override Span<float> GetVecValueInternal() {
		throw new NotImplementedException();
	}

	protected override void GetVecValueInternal(Span<float> val) {
		throw new NotImplementedException();
	}

	protected override int VectorSizeInternal() {
		throw new NotImplementedException();
	}
}
