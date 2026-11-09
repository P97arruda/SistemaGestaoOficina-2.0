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
                .AnyAsync(v => v.Id == marcacao.VeiculoId
                            && v.UserId == marcacao.UserId);

            var mecanicoValido = await _context.Set<Mecanico>()
                .AnyAsync(m => m.Id == marcacao.MecanicoId && m.Ativo);

            return clienteExiste && veiculoValido && mecanicoValido;
        }
    }
}
