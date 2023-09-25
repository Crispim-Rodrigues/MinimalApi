using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("TarefasDB"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Endpoints
app.MapGet("/tarefas", async (AppDbContext db) => 
    await db.Tarefas.ToListAsync());

app.MapGet("/tarefas/{id}", async (int id, AppDbContext db) => 
    await db.Tarefas.FindAsync(id) is Tarefa tarefa ? Results.Ok(tarefa) : Results.NotFound());

app.MapGet("tarefas/concluida", async (AppDbContext db) => 
    await db.Tarefas.Where(t => t.IsConcluida).ToListAsync());

app.MapPost("/tarefas", async (Tarefa tarefa, AppDbContext db) =>
{
    db.Tarefas.Add(tarefa);
    await db.SaveChangesAsync();
    return Results.Created($"/tarefas/{tarefa.id}", tarefa);
});

app.MapPut("/tarefas/{id}", async (int id, Tarefa putTarefa, AppDbContext db) =>
{
    var tarefa = await db.Tarefas.FindAsync(id);
    if (tarefa is null) return Results.NotFound();

    tarefa.name = putTarefa.name;
    tarefa.IsConcluida = putTarefa.IsConcluida;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/tarefas/{id}", async (int id, AppDbContext db) =>
{
    if (await db.Tarefas.FindAsync(id) is Tarefa tarefa)
    {
        db.Tarefas.Remove(tarefa);
        await db.SaveChangesAsync();
        return Results.Ok(tarefa);
    }
    return Results.NotFound();
});


app.UseHttpsRedirection();

app.UseAuthorization();

app.Run();

class Tarefa
{
    public int id { get; set; }
    public string? name { get; set; }
    public bool IsConcluida { get; set; }
}

class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
    public DbSet<Tarefa> Tarefas => Set<Tarefa>();
}
