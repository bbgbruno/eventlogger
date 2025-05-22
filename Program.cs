using Dapper;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

var corsPolicyName = "AllowAll";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicyName, policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors(corsPolicyName);
app.UseHttpsRedirection();

var connectionString = "Data Source=eventos.db";
using var connection = new SqliteConnection(connectionString);
connection.Open();

connection.Execute("""
    CREATE TABLE IF NOT EXISTS Evento (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Descricao TEXT NOT NULL,
        Data TEXT NOT NULL
    );
""");

// ✅ Endpoint para cadastrar evento
app.MapPost("/eventos", async (EventoDto evento) =>
{
    await connection.ExecuteAsync(
        "INSERT INTO Evento (Descricao, Data) VALUES (@Descricao, @Data)",
        evento);
    return Results.Ok();
});

// ✅ Endpoint para buscar eventos filtrando por descrição
app.MapGet("/eventos", async (string descricao) =>
{
    var eventos = await connection.QueryAsync<EventoDto>(
        "SELECT Descricao, Data FROM Evento WHERE Descricao LIKE @Descricao",
        new { Descricao = $"%{descricao}%" });

    return Results.Ok(eventos);
});

app.Run();


// ✅ Classe compatível com Dapper
public class EventoDto
{
    public EventoDto() { }  // Construtor sem parâmetros (obrigatório para Dapper)

    public string Descricao { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
}
