using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.Entidades;

namespace Test.Domain.Entidades
{
    [TestClass]
    public class VeiculoTest
    {
        [TestMethod]
        public void TestarGetSetPropriedades()
        {
            //Arrange 
            var veiculo = new Veiculo();

            //act

            veiculo.Ano = 2021;
            veiculo.Id = 1;
            veiculo.Marca = "teste";
            veiculo.Nome = "teste";

            //Assert
            Assert.AreEqual(1, veiculo.Id);
            Assert.AreEqual(2021, veiculo.Ano);
            Assert.AreEqual("teste", veiculo.Marca);
            Assert.AreEqual("teste", veiculo.Nome);
        }
    }
}