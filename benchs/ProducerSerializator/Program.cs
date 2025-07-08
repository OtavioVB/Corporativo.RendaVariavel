using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using ProtoBuf;
using System.Text;
using System.Text.Json;

namespace ProducerSerializator;

public sealed class Program
{
    static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}

[MemoryDiagnoser]
public class SerializationBenchmark
{
    private List<Message> _messages;

    [Params(100_000)]
    public int RunCount { get; set; }

    [Benchmark(Baseline = true, Description = "Json")]
    public void JsonSerialization()
    {
        var message = new Message()
        {
            Title = "0197eac2-d14d-79a9-bed6-ee1c4361ce38",
            Body = "0197eac2-d14d-79a9-bed6-ee1c4361ce38",
            Author = "0197eac2-d14d-79a9-bed6-ee1c4361ce38"
        };

        for (int i = 0; i < RunCount; i++)
        {
            JsonSerializer.Serialize(message);
        }
    }

    [Benchmark(Description = "Protobuf")]
    public void ProtobufSerialization()
    {
        var message = new Message()
        {
            Title = "0197eac2-d14d-79a9-bed6-ee1c4361ce38",
            Body = "0197eac2-d14d-79a9-bed6-ee1c4361ce38",
            Author = "0197eac2-d14d-79a9-bed6-ee1c4361ce38"
        };

        for (int i = 0; i < RunCount; i++)
        {
            using var memoryStream = new MemoryStream();

            Serializer.Serialize(memoryStream, message);

            Encoding.UTF8.GetString(memoryStream.ToArray());
        }
    }
}

[ProtoContract()]
public sealed class Message
{
    [ProtoMember(1)]
    public string Title { get; set; }

    [ProtoMember(2)]
    public string Body { get; set; }

    [ProtoMember(3)]
    public string Author { get; set; }
}