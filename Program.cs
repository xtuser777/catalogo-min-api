using System.Text;
using CatalogoApi.Context;
using CatalogoApi.Models;
using CatalogoApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiCatalogo", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = @"JWT Authorization header using the Bearer scheme.
                    Enter 'Bearer'[space].Example: \'Bearer 12345abcdef\'",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<CatalogoApiContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
    
builder.Services.AddSingleton<ITokenService>(new TokenService());

builder.Services.AddAuthentication
                 (JwtBearerDefaults.AuthenticationScheme)
                 .AddJwtBearer(options =>
                 {
                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         ValidateIssuer = true,
                         ValidateAudience = true,
                         ValidateLifetime = true,
                         ValidateIssuerSigningKey = true,

                         ValidIssuer = builder.Configuration["Jwt:Issuer"],
                         ValidAudience = builder.Configuration["Jwt:Audience"],
                         IssuerSigningKey = new SymmetricSecurityKey
                         (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                     };
                 });

builder.Services.AddAuthorization();

var app = builder.Build();

//endpoint para login
app.MapPost("/login", [AllowAnonymous] (UserModel userModel, ITokenService tokenService) =>
{
    if (userModel == null)
    {
        return Results.BadRequest("Login Inválido");
    }
    if (userModel.UserName == "macoratti" && userModel.Password == "numsey#123")
    {
        var tokenString = tokenService.GerarToken(app.Configuration["Jwt:Key"],
            app.Configuration["Jwt:Issuer"],
            app.Configuration["Jwt:Audience"],
            userModel);
        return Results.Ok(new { token = tokenString });
    }
    else
    {
        return Results.BadRequest("Login Inválido");
    }
}).Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status200OK)
    .WithName("Login")
    .WithTags("Autenticacao");

app.MapGet("/", () => "Catalogo de Produtos - 2023");

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.Run();