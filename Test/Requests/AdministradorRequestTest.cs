using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.ModelViews;
using Test.Helpers;

namespace Test.Requests
{
    [TestClass]
    public class AdministradorRequestTest
    {
        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            Setup.ClassInit(testContext);
        }

        [TestMethod]
        public async Task TestarGetSetPropriedades()
        {
            // Arrange variaveis criadas para 
            var loginDTO = new LoginDTO
            {
                Email = "admin1@teste.com",
                Senha = "123456"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginDTO),
                Encoding.UTF8,
                "application/json"
            );

            //Act ações que devem ser executas
            var response = await Setup.Client.PostAsync("/administradores/login", content);

            //Assert validações dos dados
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var administrador = JsonSerializer.Deserialize<AdministradorLogado>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(administrador?.Token ?? "");
            Assert.IsNotNull(administrador?.Email ?? "");
            Assert.IsNotNull(administrador?.Perfil ?? "");

        }
    }
}