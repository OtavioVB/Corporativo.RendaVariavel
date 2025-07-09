using Confluent.Kafka;

namespace Corporativo.RendaVariavel.Infrascructure.Mensageria.Produtores;

public sealed record ProdutorDeMensagemConfiguracao
{
    public ProducerConfig Configuracao { get; set; } = new ProducerConfig();
    public RetentativaConfiguracao Retentativa { get; set; } = new RetentativaConfiguracao();
    public TempoEsgotadoConfiguracao TempoEsgotado { get; set; } = new TempoEsgotadoConfiguracao();
    public string NomeDoTopico { get; set; } = string.Empty;
}

public sealed record TempoEsgotadoConfiguracao
{
    public int Segundos { get; set; } = 0;
}

public sealed record RetentativaConfiguracao
{
    public bool RetentativaHabilitada { get; set; } = false;
    public int NumeroDeRetentativas { get; set; } = 0;
    public int IntervaloEmMilisegundos { get; set; } = 0;
}
