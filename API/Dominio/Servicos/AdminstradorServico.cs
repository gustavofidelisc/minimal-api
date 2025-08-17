using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Infraestrutura.Db;
using minimal_api.Infraestrutura.Interfaces;

namespace minimal_api.Dominio.Servicos
{
    public class AdminstradorServico : IAdminstradorServico
    {
        private readonly DbContexto _dbContexto;
        public AdminstradorServico(DbContexto dbContexto)
        {
            _dbContexto = dbContexto;
        }

        public Administrador Adicionar(Administrador administrador)
        {

            _dbContexto.Administrators.Add(administrador);
            _dbContexto.SaveChanges();
            return administrador;
        }

        public Administrador? BuscarPorId(int? id)
        {
            return _dbContexto.Administrators.FirstOrDefault(Administrador => Administrador.Id == id);
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            var admin = _dbContexto.Administrators
                .FirstOrDefault(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha);

            return admin;
        }

        public List<Administrador> Todos(int? pagina)
        {
            var query = _dbContexto.Administrators.AsQueryable();

            int itensPorPagina = 10;

            if (pagina != null)
            {
                query = query.Skip(((int)pagina - 1 * itensPorPagina)).Take(itensPorPagina);
            }
            return query.ToList();
        }
    }
}