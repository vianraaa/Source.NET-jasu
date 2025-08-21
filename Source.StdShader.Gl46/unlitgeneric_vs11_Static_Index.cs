using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.StdShader.Gl46;

public class unlitgeneric_vs11_Static_Index
{
	int m_nDETAIL;
#if DEBUG
	bool m_bDETAIL;
#endif
	public void SetDETAIL(int i) {
		Assert(i >= 0 && i <= 1);
		m_nDETAIL = i;
#if DEBUG
		m_bDETAIL = true;
#endif
	}
	public void SetDETAIL(bool i) {
		m_nDETAIL = i ? 1 : 0;
#if DEBUG
		m_bDETAIL = true;
#endif
	}
	int m_nENVMAP;
#if DEBUG
	bool m_bENVMAP;
#endif
	public void SetENVMAP(int i) {
		Assert(i >= 0 && i <= 1);
		m_nENVMAP = i;
#if DEBUG
		m_bENVMAP = true;
#endif
	}
	void SetENVMAP(bool i) {
		m_nENVMAP = i ? 1 : 0;
#if DEBUG
		m_bENVMAP = true;
#endif
	}
	int m_nENVMAPCAMERASPACE;
#if DEBUG
	bool m_bENVMAPCAMERASPACE;
#endif

	public void SetENVMAPCAMERASPACE(int i) {
	}
	public void SetENVMAPCAMERASPACE(bool i) {
	}

	int m_nENVMAPSPHERE;
#if DEBUG
	bool m_bENVMAPSPHERE;
#endif

	public void SetENVMAPSPHERE(int i) {
		Assert(i >= 0 && i <= 1);
		m_nENVMAPSPHERE = i;
#if DEBUG
		m_bENVMAPSPHERE = true;
#endif
	}
	public void SetENVMAPSPHERE(bool i) {
		m_nENVMAPSPHERE = i ? 1 : 0;
#if DEBUG
		m_bENVMAPSPHERE = true;
#endif
	}

	int m_nVERTEXCOLOR;
#if DEBUG
	bool m_bVERTEXCOLOR;
#endif

	public void SetVERTEXCOLOR(int i) {
		Assert(i >= 0 && i <= 1);
		m_nVERTEXCOLOR = i;
#if DEBUG
		m_bVERTEXCOLOR = true;
#endif
	}
	void SetVERTEXCOLOR(bool i) {
		m_nVERTEXCOLOR = i ? 1 : 0;
#if DEBUG
		m_bVERTEXCOLOR = true;
#endif
	}

	int m_nSEPARATEDETAILUVS;
#if DEBUG
	bool m_bSEPARATEDETAILUVS;
#endif

	public void SetSEPARATEDETAILUVS(int i) {
		Assert(i >= 0 && i <= 1);
		m_nSEPARATEDETAILUVS = i;
#if DEBUG
		m_bSEPARATEDETAILUVS = true;
#endif
	}
	void SetSEPARATEDETAILUVS(bool i) {
		m_nSEPARATEDETAILUVS = i ? 1 : 0;
#if DEBUG
		m_bSEPARATEDETAILUVS = true;
#endif
	}
	public unlitgeneric_vs11_Static_Index() {
#if DEBUG
		m_bDETAIL = false;
#endif // _DEBUG
		m_nDETAIL = 0;
#if DEBUG
		m_bENVMAP = false;
#endif // _DEBUG
		m_nENVMAP = 0;
#if DEBUG
		m_bENVMAPCAMERASPACE = true;
#endif // _DEBUG
		m_nENVMAPCAMERASPACE = 0;
#if DEBUG
		m_bENVMAPSPHERE = false;
#endif // _DEBUG
		m_nENVMAPSPHERE = 0;
#if DEBUG
		m_bVERTEXCOLOR = false;
#endif // _DEBUG
		m_nVERTEXCOLOR = 0;
#if DEBUG
		m_bSEPARATEDETAILUVS = false;
#endif // _DEBUG
		m_nSEPARATEDETAILUVS = 0;
	}
	public int GetIndex() {
		// Asserts to make sure that we aren't using any skipped combinations.
		// Asserts to make sure that we are setting all of the combination vars.
#if DEBUG
		bool bAllStaticVarsDefined = m_bDETAIL && m_bENVMAP && m_bENVMAPCAMERASPACE && m_bENVMAPSPHERE && m_bVERTEXCOLOR && m_bSEPARATEDETAILUVS;
		Assert(bAllStaticVarsDefined);
#endif // _DEBUG
		return (4 * m_nDETAIL) + (8 * m_nENVMAP) + (16 * m_nENVMAPCAMERASPACE) + (16 * m_nENVMAPSPHERE) + (32 * m_nVERTEXCOLOR) + (64 * m_nSEPARATEDETAILUVS) + 0;
	}
};
class unlitgeneric_vs11_Dynamic_Index
{

	int m_nDOWATERFOG;
#if DEBUG
	bool m_bDOWATERFOG;
#endif

	public void SetDOWATERFOG(int i) {
		Assert(i >= 0 && i <= 1);
		m_nDOWATERFOG = i;
#if DEBUG
		m_bDOWATERFOG = true;
#endif
	}
	public void SetDOWATERFOG(bool i) {
		m_nDOWATERFOG = i ? 1 : 0;
#if DEBUG
		m_bDOWATERFOG = true;
#endif
	}

	int m_nSKINNING;
#if DEBUG
	bool m_bSKINNING;
#endif

	public void SetSKINNING(int i) {
		Assert(i >= 0 && i <= 1);
		m_nSKINNING = i;
#if DEBUG
		m_bSKINNING = true;
#endif
	}
	public void SetSKINNING(bool i) {
		m_nSKINNING = i ? 1 : 0;
#if DEBUG
		m_bSKINNING = true;
#endif
	}

	public unlitgeneric_vs11_Dynamic_Index() {
#if DEBUG
		m_bDOWATERFOG = false;
#endif // _DEBUG
		m_nDOWATERFOG = 0;
#if DEBUG
		m_bSKINNING = false;
#endif // _DEBUG
		m_nSKINNING = 0;
	}
	public int GetIndex() {
		// Asserts to make sure that we aren't using any skipped combinations.
		// Asserts to make sure that we are setting all of the combination vars.
#if DEBUG
		bool bAllDynamicVarsDefined = m_bDOWATERFOG && m_bSKINNING;
		Assert(bAllDynamicVarsDefined);
#endif // _DEBUG
		return (1 * m_nDOWATERFOG) + (2 * m_nSKINNING) + 0;
	}
};