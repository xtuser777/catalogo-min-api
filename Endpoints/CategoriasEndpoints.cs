using CatalogoApi.Context;
using CatalogoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogoApi.Endpoints;

public static class CategoriasEndpoints
{
    public static void MapCategoriasEndpoints(this WebApplication app)
    {
        app.MapPost("/categorias", async (Categoria categoria, CatalogoApiContext db) =>
        {
            db.Categorias.Add(categoria);
            await db.SaveChangesAsync();

            return Results.Created($"/categorias/{categoria.Id}", categoria);
        }).WithTags("Categorias");

        app.MapGet("/categorias", async (CatalogoApiContext db) =>
            await db.Categorias
                .ToListAsync()).RequireAuthorization().WithTags("Categorias");

        app.MapGet("/categorias/{id:int}", async (int id, CatalogoApiContext db) =>
            await db.Categorias
            .FindAsync(id) is Categoria categoria
            ? Results.Ok(categoria)
            : Results.NotFound()).WithTags("Categorias");

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
        }).WithTags("Categorias");

        app.MapDelete("/categorias/{id:int}", async (int id, CatalogoApiContext db) =>
        {
            if (db.Categorias is null) return Results.StatusCode(StatusCodes.Status500InternalServerError);

            var categoria = await db.Categorias.FindAsync(id);

            if (categoria is null) return Results.NotFound();

            db.Categorias.Remove(categoria);

            await db.SaveChangesAsync();

            return Results.NoContent();
        }).WithTags("Categorias");
    }
}
