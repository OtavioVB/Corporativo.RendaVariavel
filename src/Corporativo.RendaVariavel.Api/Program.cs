using Corporativo.RendaVariavel.Infrascructure.BancoDeDados;

namespace Corporativo.RendaVariavel.Api;

public sealed class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.RegistrarInjecaoDeDependenciaDoBancoDeDados(
            configurador: builder.Configuration);
        
        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapControllers();
        app.Run();
    }
}

