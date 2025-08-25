using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Infraestrutura.Interfaces;
using Test.Helpers;

namespace Test.Mocks
{
    public class AdministradorServicoMock : IAdminstradorServico
    {

        private static List<Administrador> _administradores = new List<Administrador>()
        {
            new Administrador
            {
                Id = 1,
                Senha = "123456",
                Email = "admin1@teste.com",
                Perfil = "Administrador"
            },
            new Administrador
            {
                Id = 2,
                Senha = "123456",
                Email = "admin2@teste.com",
                Perfil = "Editor"
            }
        };

        public Administrador Adicionar(Administrador administrador)
        {
            administrador.Id = _administradores.Count > 0 ? _administradores.Max(a => a.Id) + 1 : 1;
            _administradores.Add(administrador);
            return administrador;
        }

        public Administrador? BuscarPorId(int? id)
        {
            return _administradores.FirstOrDefault(a => a.Id == id);
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            return _administradores.FirstOrDefault(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha);
        }

        public List<Administrador> Todos(int? pagina)
        {
            return _administradores;
        }
    }
}