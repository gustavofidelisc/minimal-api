using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.ENUMs;

namespace minimal_api.Infraestrutura.Db
{
    public class DbContexto : DbContext
    {
        private readonly IConfiguration _configuracaoAppSettings;
        public DbContexto(IConfiguration configuracaoAppSettings)
        {
            _configuracaoAppSettings = configuracaoAppSettings;
        }
        public DbSet<Administrador> Administrators { get; set; } = default!;
        public DbSet<Veiculo> Veiculos { get; set; } = default!;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var stringConexao = _configuracaoAppSettings.GetConnectionString("MySql")?.ToString();
                if (!string.IsNullOrEmpty(stringConexao))
                {
                    optionsBuilder.UseMySql(stringConexao, ServerVersion.AutoDetect(stringConexao));
                    // access docker container docker exec -it mysql_container mysql -u root -p
                }
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Administrador>().HasData(
                new Administrador
                {
                    Id = 1,
                    Email = "adm@teste.com",
                    Senha = "123456",
                    Perfil = PerfilEnum.Administrador.ToString()
                }
            );
        }
    }
}