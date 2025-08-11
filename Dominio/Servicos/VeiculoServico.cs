using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Infraestrutura.Db;
using minimal_api.Infraestrutura.Interfaces;
using minimal_api.Migrations;

namespace minimal_api.Dominio.Servicos
{
    public class VeiculoServico : IVeiculoServico
    {
        private readonly DbContexto _dbContexto;
        public VeiculoServico(DbContexto dbContexto)
        {
            _dbContexto = dbContexto;
        }

        public void Atualizar(Veiculo veiculo)
        {
            _dbContexto.Veiculos.Update(veiculo);
            _dbContexto.SaveChanges();
        }

        public Veiculo? BuscarPorId(int id)
        {
            return _dbContexto.Veiculos.FirstOrDefault(v => v.Id == id);
        }

        public void Deletar(Veiculo veiculo)
        {
            _dbContexto.Veiculos.Remove(veiculo);
            _dbContexto.SaveChanges();
        }

        public List<Veiculo>? ObterVeiculos(int? pagina, string? marca = null, string? nome = null)
        {
            var query = _dbContexto.Veiculos.AsQueryable();

            if (!string.IsNullOrEmpty(nome))
            {

                query = query.Where(v => EF.Functions.Like(v.Nome.ToLower(), $"%{nome.ToLower()}%"));
            }

            int itensPorPagina = 10;
            if (pagina != null)
            {
                query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);
            }

            return query.ToList();

        }

        public void Salvar(Veiculo veiculo)
        {
            _dbContexto.Veiculos.Add(veiculo);
            _dbContexto.SaveChanges();
        }
    }
}