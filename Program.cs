using CatalogoApi.Context;
using CatalogoApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<CatalogoApiContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var app = builder.Build();

app.MapGet("/", () => "Catalogo de Produtos - 2023");

app.MapPost("/categorias", async (Categoria categoria, CatalogoApiContext db) =>
{
    db.Categorias.Add(categoria);
    await db.SaveChangesAsync();

    return Results.Created($"/categorias/{categoria.Id}", categoria);
});

app.MapGet("/categorias", async (CatalogoApiContext db) => 
    await db.Categorias
        .ToListAsync());

app.MapGet("/categorias/{id:int}", async (int id, CatalogoApiContext db) =>
    await db.Categorias
    .FindAsync(id) is Categoria categoria
    ? Results.Ok(categoria)
    : Results.NotFound());

app.MapPut("/categorias/{id:int}", async (int id, Categoria categoria, CatalogoApiContext db) =>
{
    if (db.Categorias is null) return Results.StatusCode(StatusCodes.Status500InternalServerError);

    if (categoria.Id != id) return Results.BadRequest();

    var categoriaDb = await db.Categorias.FindAsync(id);

    if (categoriaDb is null) return Results.NotFound();

    categoriaDb.Nome = categoria.Nome;
    categoriaDb.Descricao = categoria.Descricao;

    await db.SaveChangesAsync();

    return Results.Ok(categoriaDb);
});

app.MapDelete("/categorias/{id:int}", async (int id, CatalogoApiContext db) =>
{
    if (db.Categorias is null) return Results.StatusCode(StatusCodes.Status500InternalServerError);

    var categoria = await db.Categorias.FindAsync(id);

    if (categoria is null) return Results.NotFound();

    db.Categorias.Remove(categoria);

    await db.SaveChangesAsync();

    return Results.NoContent();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();