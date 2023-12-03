using CatalogoApi.Context;
using CatalogoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogoApi.Endpoints;

public static class ProdutosEndpoints
{
    public static void MapProdutosEndpoints(this WebApplication app)
    {
        app.MapPost("/produtos", async (Produto produto, CatalogoApiContext db) =>
        {
            if (db.Produtos is null) return Results.StatusCode(StatusCodes.Status500InternalServerError);
            db.Produtos.Add(produto);
            await db.SaveChangesAsync();

            return Results.Created($"/produtos/{produto.Id}", produto);
        }).WithTags("Produtos");

        app.MapGet("/produtos", async (CatalogoApiContext db) =>
            await db.Produtos
                .ToListAsync()).RequireAuthorization().WithTags("Produtos");

        app.MapGet("/produtos/{id:int}", async (int id, CatalogoApiContext db) =>
            await db.Produtos
            .FindAsync(id) is Produto produto
            ? Results.Ok(produto)
            : Results.NotFound()).WithTags("Produtos");

        app.MapPut("/produtos/{id:int}", async (int id, Produto produto, CatalogoApiContext db) =>
        {
            if (db.Produtos is null) return Results.StatusCode(StatusCodes.Status500InternalServerError);

            if (produto.Id != id) return Results.BadRequest();

            var produtoDb = await db.Produtos.FindAsync(id);

            if (produtoDb is null) return Results.NotFound();

            produtoDb.Nome = produto.Nome;
            produtoDb.Descricao = produto.Descricao;

            await db.SaveChangesAsync();

            return Results.Ok(produtoDb);
        }).WithTags("Produtos");

        app.MapDelete("/produtos/{id:int}", async (int id, CatalogoApiContext db) =>
        {
            if (db.Produtos is null) return Results.StatusCode(StatusCodes.Status500InternalServerError);

            var produto = await db.Produtos.FindAsync(id);

            if (produto is null) return Results.NotFound();

            db.Produtos.Remove(produto);

            await db.SaveChangesAsync();

            return Results.NoContent();
        }).WithTags("Produtos");
    }
}
