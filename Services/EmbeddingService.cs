// Services/EmbeddingService.cs
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace NewsRecommendation.Api.Services;

public class EmbeddingService
{
    private readonly InferenceSession _session;
    private readonly Dictionary<string, int> _vocab;

    public EmbeddingService()
    {
        var modelPath = Path.Combine("Models", "all-MiniLM-L6-v2.onnx");
        var vocabPath = Path.Combine("Models", "vocab.txt");

        if (!File.Exists(modelPath)) throw new FileNotFoundException($"模型未找到: {modelPath}");
        if (!File.Exists(vocabPath)) throw new FileNotFoundException($"词汇表未找到: {vocabPath}");

        _session = new InferenceSession(modelPath);
        _vocab = File.ReadLines(vocabPath)
            .Select((line, i) => new { Word = line.Trim(), Id = i })
            .ToDictionary(x => x.Word, x => x.Id);
    }

    public float[] GetEmbedding(string text)
    {
        var tokens = text.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var ids = tokens.Select(t => (long)_vocab.GetValueOrDefault(t, 0)).ToArray();

        var tensor = new DenseTensor<long>(ids, new[] { 1, ids.Length });
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", tensor)
        };

        using var results = _session.Run(inputs);
        var output = results.First().AsTensor<float>();

        var embedding = new float[384];
        for (int i = 0; i < 384; i++)
            embedding[i] = output[0, 0, i];

        return embedding;
    }
}