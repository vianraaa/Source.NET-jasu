namespace Source.Common.MaterialSystem;

public interface IShaderSystem
{
	void BindTexture(Sampler sampler, ITexture texture, int frame = 0);
	void Draw(bool makeActualDrawCall = true);
	void Init();
	void ResetShaderState();
}
public interface IShaderDLL
{
	public IEnumerable<IShader> GetShaders();
}