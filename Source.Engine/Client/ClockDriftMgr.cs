namespace Source.Engine.Client;

public class ClockDriftMgr(ClientState cl, Host Host, CommonHostState host_state)
{
	public static bool Enabled = true;

	const float cl_clockdrift_max_ms = 150;
	const int cl_clock_correction_force_server_tick = 999;
	const int cl_clock_correction_adjustment_min_offset = 10;
	const int cl_clock_correction_adjustment_max_offset = 90;
	const int cl_clock_correction_adjustment_max_amount = 200;

	public void Clear() {
		ClientTick = 0;
		ServerTick = 0;
		CurClockOffset = 0;
		for (int i = 0; i < NUM_CLOCKDRIFT_SAMPLES; i++) {
			ClockOffsets[i] = 0;
		}
	}
	public void SetServerTick(int tick) {
		int maxDriftTicks = Host.TimeToTicks(cl_clockdrift_max_ms / 1000f);
		int clientTick = cl.GetClientTickCount() + 0; // simticks this frame?

		if (cl_clock_correction_force_server_tick == 999) {
			if (clientTick == 0 || Math.Abs(tick - clientTick) > maxDriftTicks) {
				cl.SetClientTickCount(tick - 0);
				if (cl.GetClientTickCount() < cl.OldTickCount) {
					cl.OldTickCount = cl.GetClientTickCount();
				}
				for (int i = 0; i < NUM_CLOCKDRIFT_SAMPLES; i++) {
					ClockOffsets[i] = 0;
				}
			}
		}
		else {
			cl.SetClientTickCount(tick + cl_clock_correction_force_server_tick);
		}

		ClockOffsets[CurClockOffset] = clientTick - ServerTick;
		CurClockOffset = (CurClockOffset + 1) % NUM_CLOCKDRIFT_SAMPLES;
	}
	public float AdjustFrameTime(float inputFrameTime) {
		float adjustmentThisFrame = 0;
		float adjustmentPerSec = 0;

		float flCurDiffInSeconds = GetCurrentClockDifference() * (float)host_state.IntervalPerTick;
		float flCurDiffInMS = flCurDiffInSeconds * 1000.0f;

		if (flCurDiffInMS > cl_clock_correction_adjustment_min_offset) {
			adjustmentPerSec = -GetClockAdjustmentAmount(flCurDiffInMS);
			adjustmentThisFrame = inputFrameTime * adjustmentPerSec;
			adjustmentThisFrame = Math.Max(adjustmentThisFrame, -flCurDiffInSeconds);
		}
		else if (flCurDiffInMS < -cl_clock_correction_adjustment_min_offset) {
			adjustmentPerSec = GetClockAdjustmentAmount(-flCurDiffInMS);
			adjustmentThisFrame = inputFrameTime * adjustmentPerSec;
			adjustmentThisFrame = Math.Min(adjustmentThisFrame, -flCurDiffInSeconds);
		}

		AdjustAverageDifferenceBy(adjustmentThisFrame);
		return inputFrameTime + adjustmentThisFrame;
	}
	void AdjustAverageDifferenceBy(float amountInSeconds) {
		float c = GetCurrentClockDifference();
		if (c < 0.05f)
			return;

		float flAmountInTicks = amountInSeconds / (float)host_state.IntervalPerTick;
		float factor = 1 + flAmountInTicks / c;

		for (int i = 0; i < NUM_CLOCKDRIFT_SAMPLES; i++)
			ClockOffsets[i] *= factor;
	}
	public float GetCurrentClockDifference() {
		float total = 0;
		for (int i = 0; i < NUM_CLOCKDRIFT_SAMPLES; i++)
			total += ClockOffsets[i];

		return total / NUM_CLOCKDRIFT_SAMPLES;
	}
	static float Remap(float source, float sourceFrom, float sourceTo, float targetFrom, float targetTo) {
		return targetFrom + (source - sourceFrom) * (targetTo - targetFrom) / (sourceTo - sourceFrom);
	}
	public static float GetClockAdjustmentAmount(float curDiffInMS) {
		curDiffInMS = Math.Clamp(curDiffInMS, cl_clock_correction_adjustment_min_offset, cl_clock_correction_adjustment_max_offset);

		float flReturnValue = Remap(curDiffInMS,
			cl_clock_correction_adjustment_min_offset, cl_clock_correction_adjustment_max_offset,
			0, cl_clock_correction_adjustment_max_amount / 1000.0f);

		return flReturnValue;
	}
	const int NUM_CLOCKDRIFT_SAMPLES = 16;
	public float[] ClockOffsets = new float[NUM_CLOCKDRIFT_SAMPLES];
	public int CurClockOffset;
	public int ServerTick;
	public int ClientTick;

}
