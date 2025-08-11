using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;

namespace minimal_api.Infraestrutura.Interfaces
{
    public interface IVeiculoServico
    {
        public List<Veiculo>? ObterVeiculos(int? Pagina, string? marca = null, string? nome = null);
        Veiculo? BuscarPorId(int id);
        void Salvar(Veiculo veiculo);
        void Atualizar(Veiculo veiculo);
        void Deletar(Veiculo veiculo);
    }
}