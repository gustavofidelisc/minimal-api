using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;

namespace minimal_api.Infraestrutura.Interfaces
{
    public interface IAdminstradorServico
    {
        Administrador? Login(LoginDTO loginDTO);
        Administrador Adicionar(Administrador administrador);

        List<Administrador> Todos(int? pagina);
        Administrador? BuscarPorId(int? id);
        
    }
}