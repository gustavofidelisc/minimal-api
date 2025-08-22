using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Servicos;
using minimal_api.Infraestrutura.Db;

namespace Test.Domain.Servicos
{
    [TestClass]
    public class AdministradorService
    {
        private DbContexto CriarContextoDeTeste()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.GetFullPath(Path.Combine(assemblyPath ?? "", "..", "..", ".."));
            var builder = new ConfigurationBuilder()
                .SetBasePath(path ?? Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var Configuration = builder.Build();

            return new DbContexto(Configuration);
        }

        [TestMethod]
        public void TestarSalvarAdminstrador()
        {

            // Arrange variaveis criadas para 
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Adminstradores");
            var adm = new Administrador();
            adm.Email = "teste@teste.com";
            adm.Senha = "teste";
            adm.Perfil = "Administrador";
            var administradorServico = new AdminstradorServico(context);
            //Act

            administradorServico.Adicionar(adm);
            

            // Assert 
            Assert.AreEqual(1, administradorServico.Todos(1).Count());

        }
        [TestMethod]
        public void TestarBuscarPorId()
        {

            // Arrange variaveis criadas para 
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Adminstradores");
            var adm = new Administrador();
            adm.Email = "teste@teste.com";
            adm.Senha = "teste";
            adm.Perfil = "Administrador";
            var administradorServico = new AdminstradorServico(context);
            //Act
            administradorServico.Adicionar(adm);
            adm = administradorServico.BuscarPorId(adm.Id);
            

            // Assert 
            Assert.AreEqual(1, adm.Id);

        }
    }
}