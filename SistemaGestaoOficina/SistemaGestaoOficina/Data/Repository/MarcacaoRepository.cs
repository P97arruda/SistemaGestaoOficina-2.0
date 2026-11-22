using Microsoft.EntityFrameworkCore;
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
                .AnyAsync(v => v.Id == marcacao.VeiculoId &&
                               v.UserId == marcacao.UserId);

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
            return await GetAll().AnyAsync(m =>
                m.VeiculoId == marcacao.VeiculoId &&
                m.Estado == "Agendada" &&
                m.Id != marcacao.Id);
        }

        public async Task<List<Marcacao>> GetMarcacoesDisponiveisAsync()
        {
            return await _context.Set<Marcacao>()
                .Include(m => m.User)
                .Include(m => m.Veiculo)
                .Where(m => m.Estado == "Agendada")
                .Where(m => !_context.Set<Reparacao>()
                    .Any(r => r.MarcacaoId == m.Id))
                .OrderByDescending(m => m.DataHora)
                .ToListAsync();
        }

        public bool DataFutura(DateTime dataHora)
        {
            return dataHora > DateTime.Now;
        }

        public bool HorarioValido(DateTime dataHora)
        {
            return dataHora.DayOfWeek != DayOfWeek.Sunday &&
                   dataHora.Minute == 0 &&
                   dataHora.Second == 0 &&
                   dataHora.Hour >= 9 &&
                   dataHora.Hour <
                       (dataHora.DayOfWeek == DayOfWeek.Saturday ? 13 : 18);
        }

        public async Task<List<string>> ValidarMarcacaoAsync(Marcacao marcacao)
        {
            var erros = new List<string>();

            if (!await DadosValidosAsync(marcacao))
            {
                erros.Add("Cliente, veículo ou mecânico inválido.");
                return erros;
            }

            if (!DataFutura(marcacao.DataHora))
            {
                erros.Add("A data deve ser futura.");
                return erros;
            }

            if (!HorarioValido(marcacao.DataHora))
            {
                erros.Add("Selecione um horário válido da oficina.");
                return erros;
            }

            if (await ExisteConflitoAsync(marcacao))
            {
                erros.Add("O mecânico já tem uma marcação neste horário.");
                return erros;
            }

            if (await ExisteMarcacaoDuplicadaAsync(marcacao))
            {
                erros.Add("Este cliente já possui uma marcação agendada para este veículo e serviço. Cancele a marcação anterior antes de criar outra.");
            }

            return erros;
        }

        public async Task<List<Veiculo>> GetVeiculosPorClienteAsync(string? userId)
        {
            return await _context.Set<Veiculo>()
                .Where(v => string.IsNullOrEmpty(userId) || v.UserId == userId)
                .ToListAsync();
        }
    }
}