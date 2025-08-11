using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.ENUMs;
using minimal_api.Dominio.ModelViews;
using minimal_api.Dominio.Servicos;
using minimal_api.Infraestrutura.Db;
using minimal_api.Infraestrutura.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using minimal_api.Dominio.Configuration;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

#region Builder
var builder = WebApplication.CreateBuilder(args);

var JwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();


builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettings.Key)),
        ValidateAudience = false,
        ValidateIssuer = false
    };
});

builder.Services.AddAuthorization();


builder.Services.AddScoped<IAdminstradorServico, AdminstradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT desta maneira: {seu token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

builder.Services.AddDbContext<DbContexto>(options => options.UseMySql(builder.Configuration.GetConnectionString("MySql"),
ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))));

var app = builder.Build();
#endregion

#region Home

app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");

#endregion

#region Adminstradores

string GerarTokenJwt(Administrador administrador)
{
    if (string.IsNullOrEmpty(JwtSettings.Key))
    {
        return string.Empty;
    }
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettings.Key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>()
    {
        new Claim("Email", administrador.Email),
        new Claim("Perfil", administrador.Perfil),
        new Claim(ClaimTypes.Role, administrador.Perfil)
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.UtcNow.AddHours(5),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("adminstradores/login", ([FromBody] LoginDTO loginDTO, IAdminstradorServico adminstradorServico) =>
{
    var adm = adminstradorServico.Login(loginDTO);
    if (adm != null)
    {
        string token = GerarTokenJwt(adm);
        return Results.Ok(new AdministradorLogado
        {
            Email = adm.Email,
            Perfil = adm.Perfil,
            Token = token
        });
    }
    else
    {
        return Results.Unauthorized();
    }
}).AllowAnonymous().WithTags("Administradores");

app.MapPost("administradores", ([FromBody] AdministradorDto administradorDto, IAdminstradorServico administradorServico) =>
{
    var validacao = new ErrosValidacao
    {
        Mensagens = new List<string>()
    };

    if (string.IsNullOrEmpty(administradorDto.Email))
        validacao.Mensagens.Add("Email não pode ser vazio");

    if (string.IsNullOrEmpty(administradorDto.Senha))
        validacao.Mensagens.Add("Senha não pode ser vazia");

    if (!Enum.IsDefined(typeof(PerfilEnum), administradorDto.Perfil))
        validacao.Mensagens.Add("Perfil inválido");

    if (validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    var administrador = new Administrador
    {
        Email = administradorDto.Email,
        Perfil = administradorDto.Perfil.ToString(),
        Senha = administradorDto.Senha
    };

    administradorServico.Adicionar(administrador);

    return Results.Created($"/administrador/{administrador.Id}", new AdministradorModelView
    {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute {Roles = "Administrador"})
.WithTags("Administradores");

app.MapGet("/Administradores", ([FromQuery] int? pagina, IAdminstradorServico adminstradorServico) =>
{
    var admins = new List<AdministradorModelView>();
    var adminstradores = adminstradorServico.Todos(pagina);

    foreach (var adm in adminstradores)
    {
        admins.Add(new AdministradorModelView
        {
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil
        });
    }
    return Results.Ok(admins);
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Administrador" })
.WithTags("Administradores");

app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdminstradorServico adminstradorServico) =>
{
    var adminstrador = adminstradorServico.BuscarPorId(id);
    if (adminstrador == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(new AdministradorModelView
    {
        Id = adminstrador.Id,
        Email = adminstrador.Email,
        Perfil = adminstrador.Perfil
    });
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute {Roles = "Administrador"})
.WithTags("Administradores");

#endregion

#region Veiculos

ErrosValidacao validaDto(VeiculoDTO veiculoDto)
{
    var validacao = new ErrosValidacao
    {
        Mensagens = new List<string>()
    };

    if (string.IsNullOrEmpty(veiculoDto.Nome))
    {
        validacao.Mensagens.Add("O nome não pode ser vazio");
    }
    if (string.IsNullOrEmpty(veiculoDto.Marca))
    {
        validacao.Mensagens.Add("A marca não pode ser vazio");
    }
    if (veiculoDto.Ano < 1900) { }
    {
        validacao.Mensagens.Add("Veículo muito antigo, aceito apenas acima de 1900");
    }

    return validacao;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDto, IVeiculoServico veiculoServico) =>
{
    var validacao = validaDto(veiculoDto);
    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    var veiculo = new Veiculo
    {
        Nome = veiculoDto.Nome,
        Ano = veiculoDto.Ano,
        Marca = veiculoDto.Marca
    };
    veiculoServico.Salvar(veiculo);

    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute {Roles = "Administrador,Editor"})
.WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.ObterVeiculos(pagina);
    return Results.Ok(veiculos);
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute {Roles = "Administrador,Editor"})
.WithTags("Veiculos"); ;

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscarPorId(id);
    if (veiculo == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(veiculo);
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute {Roles = "Administrador,Editor"})
.WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, [FromBody] VeiculoDTO veiculoDto, IVeiculoServico veiculoServico) =>
{
    var validacao = validaDto(veiculoDto);
    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }
    var veiculo = veiculoServico.BuscarPorId(id);
    if (veiculo == null) return Results.NotFound();


    veiculo.Nome = veiculoDto.Nome;
    veiculo.Marca = veiculoDto.Marca;
    veiculo.Ano = veiculoDto.Ano;

    veiculoServico.Atualizar(veiculo);

    return Results.Ok(veiculo);
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute {Roles = "Administrador"})
.WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscarPorId(id);
    if (veiculo == null) return Results.NotFound();

    veiculoServico.Deletar(veiculo);

    return Results.NoContent();
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute {Roles = "Administrador"})
.WithTags("Veiculos");

#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.Run();

#endregion