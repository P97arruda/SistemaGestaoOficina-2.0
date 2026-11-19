using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SistemaGestaoOficina.Data.Entities;

namespace SistemaGestaoOficina.Data.Repository
{
    public class MarcacaoRepository : GenericRepository<Marcacao>, IMarcacaoRepository
    {
        private readonly DataContext _context;
        public MarcacaoRepository(DataContext context) : base(context)
        {
            _context = context;
        }


        public async Task<bool> ExisteConflitoAsync(Marcacao marcacao)
        {
            return await GetAll().AnyAsync(m =>
                m.MecanicoId == marcacao.MecanicoId &&
                m.DataHora == marcacao.DataHora &&
                m.Id != marcacao.Id &&
                m.Estado != "Cancelada");
        }

        public async Task<bool> DadosValidosAsync(Marcacao marcacao)
        {
            var clienteExiste = await _context.Users
                .AnyAsync(u => u.Id == marcacao.UserId);

            var veiculoValido = await _context.Set<Veiculo>()
                .AnyAsync(v => v.Id == marcacao.VeiculoId
                            && v.UserId == marcacao.UserId);

            var mecanicoValido = await _context.Set<Mecanico>()
                .AnyAsync(m => m.Id == marcacao.MecanicoId && m.Ativo);

            return clienteExiste && veiculoValido && mecanicoValido;
        }

        public async Task<List<User>> GetClientesAsync()
        {
            return await _context.Users
                .Where(u => _context.UserRoles.Any(ur =>
                    ur.UserId == u.Id &&
                    _context.Roles.Any(r =>
                        r.Id == ur.RoleId &&
                        r.Name == "Cliente")))
                .OrderBy(u => u.FirstName)
                .ToListAsync();
        }

        public async Task<List<Veiculo>> GetVeiculosAsync()
        {
            return await _context.Set<Veiculo>()
                .ToListAsync();
        }

        public async Task<List<Mecanico>> GetMecanicosAsync()
        {
           return await _context.Set<Mecanico>()
                .Where(m => m.Ativo)
                .OrderBy(m => m.Nome)
                .ToListAsync();
        }

        public async Task<List<TipoReparacao>> GetTiposReparacaoAsync()
        {
            return await _context.Set<TipoReparacao>()
                .OrderBy(t => t.Nome)
                .ToListAsync();
        }

        public async Task<bool> ExisteMarcacaoDuplicadaAsync(Marcacao marcacao)
        {
            return await GetAll().AnyAsync(m => m.VeiculoId == marcacao.VeiculoId && m.Estado == "Agendada" && m.Id != marcacao.Id);
        }
    }
}
