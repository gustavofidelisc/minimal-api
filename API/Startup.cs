using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.Dominio.Configuration;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.ENUMs;
using minimal_api.Dominio.ModelViews;
using minimal_api.Dominio.Servicos;
using minimal_api.Infraestrutura.Db;
using minimal_api.Infraestrutura.Interfaces;

namespace minimal_api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            JwtSettings = Configuration.GetSection("Jwt").Get<JwtSettings>();
        }
        private JwtSettings JwtSettings;

        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddAuthentication(option =>
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

            services.AddAuthorization();


            services.AddScoped<IAdminstradorServico, AdminstradorServico>();
            services.AddScoped<IVeiculoServico, VeiculoServico>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
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

            services.AddDbContext<DbContexto>(options => options.UseMySql(Configuration.GetConnectionString("MySql"),
            ServerVersion.AutoDetect(Configuration.GetConnectionString("MySql"))));

        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                {
                    #region Home

                    endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");

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

                    endpoints.MapPost("adminstradores/login", ([FromBody] LoginDTO loginDTO, IAdminstradorServico adminstradorServico) =>
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

                    endpoints.MapPost("administradores", ([FromBody] AdministradorDto administradorDto, IAdminstradorServico administradorServico) =>
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
                    .RequireAuthorization(new AuthorizeAttribute { Roles = "Administrador" })
                    .WithTags("Administradores");

                    endpoints.MapGet("/Administradores", ([FromQuery] int? pagina, IAdminstradorServico adminstradorServico) =>
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

                    endpoints.MapGet("/administradores/{id}", ([FromRoute] int id, IAdminstradorServico adminstradorServico) =>
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
                    .RequireAuthorization(new AuthorizeAttribute { Roles = "Administrador" })
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

                    endpoints.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDto, IVeiculoServico veiculoServico) =>
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
                    .RequireAuthorization(new AuthorizeAttribute { Roles = "Administrador,Editor" })
                    .WithTags("Veiculos");

                    endpoints.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
                    {
                        var veiculos = veiculoServico.ObterVeiculos(pagina);
                        return Results.Ok(veiculos);
                    }).RequireAuthorization()
                    .RequireAuthorization(new AuthorizeAttribute { Roles = "Administrador,Editor" })
                    .WithTags("Veiculos"); ;

                    endpoints.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
                    {
                        var veiculo = veiculoServico.BuscarPorId(id);
                        if (veiculo == null)
                        {
                            return Results.NotFound();
                        }
                        return Results.Ok(veiculo);
                    }).RequireAuthorization()
                    .RequireAuthorization(new AuthorizeAttribute { Roles = "Administrador,Editor" })
                    .WithTags("Veiculos");

                    endpoints.MapPut("/veiculos/{id}", ([FromRoute] int id, [FromBody] VeiculoDTO veiculoDto, IVeiculoServico veiculoServico) =>
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
                    .RequireAuthorization(new AuthorizeAttribute { Roles = "Administrador" })
                    .WithTags("Veiculos");

                    endpoints.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
                    {
                        var veiculo = veiculoServico.BuscarPorId(id);
                        if (veiculo == null) return Results.NotFound();

                        veiculoServico.Deletar(veiculo);

                        return Results.NoContent();
                    }).RequireAuthorization()
                    .RequireAuthorization(new AuthorizeAttribute { Roles = "Administrador" })
                    .WithTags("Veiculos");
                    #endregion
                }
            });
        }


    }
}