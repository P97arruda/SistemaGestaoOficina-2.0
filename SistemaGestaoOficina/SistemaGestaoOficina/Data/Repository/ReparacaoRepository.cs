using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data.Entities;

namespace SistemaGestaoOficina.Data.Repository
{
    public class ReparacaoRepository: GenericRepository<Reparacao>, IReparacaoRepository
    {
        private readonly DataContext _context;

        public ReparacaoRepository(DataContext context): base(context)
        {
            _context = context;
        }

        public async Task<Reparacao?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Set<Reparacao>()
                .Include(r => r.Marcacao)
                    .ThenInclude(m => m!.User)
                .Include(r => r.Marcacao)
                    .ThenInclude(m => m!.Veiculo)
                .Include(r => r.MecanicoReparacaos)
                    .ThenInclude(mr => mr.Mecanico)
                .FirstOrDefaultAsync(r => r.Id == id);
        }


        public async Task<bool> EliminarReparacaoAsync(int id)
        {
            var reparacao = await _context.Reparacoes.FirstOrDefaultAsync(r => r.Id == id);

            if (reparacao == null)
            {
                return false;
            }

            // Não permite cancelar reparações concluídas
            // ou que já estejam canceladas.
            if (reparacao.Concluida || reparacao.Cancelada)
            {
                return false;
            }

            // Cancelamento lógico: mantém os dados no histórico.
            reparacao.Cancelada = true;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}