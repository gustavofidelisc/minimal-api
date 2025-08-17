using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.ENUMs;

namespace minimal_api.Dominio.DTOs
{
    public class AdministradorDto
    {
        public string Email { get; set; } = default!;
        public string Senha { get; set; } = default!;
        public PerfilEnum Perfil { get; set; } = default!;

    }
}