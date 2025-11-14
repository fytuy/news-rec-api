// Services/EmbeddingService.cs
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace NewsRecommendation.Api.Services;

public class EmbeddingService
{
    private InferenceSession? _session;
    private Dictionary<string, int>? _vocab;
    private bool _loaded = false;

    public EmbeddingService()
    {
        // 异步加载模型，避免启动时阻塞
        Task.Run(() => LoadModelAsync());
    }

    private void LoadModelAsync()
    {
        try
        {
            var modelPath = Path.Combine("Models", "all-MiniLM-L6-v2.onnx");
            var vocabPath = Path.Combine("Models", "vocab.txt");

            if (!File.Exists(modelPath)) throw new FileNotFoundException($"模型未找到: {modelPath}");
            if (!File.Exists(vocabPath)) throw new FileNotFoundException($"词汇表未找到: {vocabPath}");

            _session = new InferenceSession(modelPath);
            _vocab = File.ReadLines(vocabPath)
                .Select((line, i) => new { Word = line.Trim(), Id = i })
                .ToDictionary(x => x.Word, x => x.Id);

            _loaded = true;
            Console.WriteLine("模型加载完成 ✅");
        }
        catch (Exception ex)
        {
            Console.WriteLine("模型加载失败 ❌: " + ex);
        }
    }

    public float[]? GetEmbedding(string text)
    {
        if (!_loaded || _session == null || _vocab == null)
            throw new InvalidOperationException("模型尚未加载完成，请稍后再试");

        var tokens = text.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var ids = tokens.Select(t => (long)_vocab.GetValueOrDefault(t, 0)).ToArray();

        var tensor = new DenseTensor<long>(ids, new[] { 1, ids.Length });
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", tensor)
        };

        using var results = _session.Run(inputs);
        var output = results.First().AsTensor<float>();

        // 修复 .Last() 错误，直接取 Dimensions 最后一个元素
        var embedding = new float[output.Dimensions[output.Dimensions.Length - 1]];

        for (int i = 0; i < embedding.Length; i++)
            embedding[i] = output[0, 0, i];

        return embedding;
    }
}
