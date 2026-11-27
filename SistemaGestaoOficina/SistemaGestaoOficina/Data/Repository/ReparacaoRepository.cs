
using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data.Entities;

namespace SistemaGestaoOficina.Data.Repository
{
    public class ReparacaoRepository : GenericRepository<Reparacao>, IReparacaoRepository
    {
        private readonly DataContext _context;

        public ReparacaoRepository(DataContext context) : base(context)
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
                .Include(r => r.ReparacaoPecas)
                    .ThenInclude(rp => rp.Peca)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> EliminarReparacaoAsync(int id)
        {
            var reparacao = await _context.Reparacoes
                .FirstOrDefaultAsync(r => r.Id == id);

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

        // CONCLUIR UMA REPARAÇÃO EXISTENTE
        public async Task<bool> ConcluirExistenteAsync(
            int marcacaoId,
            DateTime dataInicio,
            DateTime dataFim,
            List<ReparacaoPeca> pecas,
            List<int> mecanicoIds,
            decimal valorPorMecanico)
        {
            // 1. Procurar a reparação existente
            var reparacao = await _context.Set<Reparacao>()
                .Include(r => r.Marcacao)
                .Include(r => r.ReparacaoPecas)
                .Include(r => r.MecanicoReparacaos)
                .FirstOrDefaultAsync(r => r.MarcacaoId == marcacaoId);

            if (reparacao == null ||
                reparacao.Concluida ||
                reparacao.Cancelada ||
                reparacao.Marcacao == null ||
                reparacao.Marcacao.Estado != "Agendada")
            {
                return false;
            }

            // 2. Validar as datas
            if (dataFim < dataInicio)
            {
                return false;
            }

            // 3. Atualizar as datas e o serviço
            reparacao.DataInicio = dataInicio;
            reparacao.DataFim = dataFim;
            reparacao.TipoServico = reparacao.Marcacao.TipoServico;

            // 4. Adicionar ou atualizar peças
            foreach (var item in pecas)
            {
                var existente = reparacao.ReparacaoPecas
                    .FirstOrDefault(p => p.PecaId == item.PecaId);

                if (existente != null)
                {
                    existente.Quantidade = item.Quantidade;
                    existente.PrecoUnitario = item.PrecoUnitario;
                }
                else
                {
                    reparacao.ReparacaoPecas.Add(new ReparacaoPeca
                    {
                        PecaId = item.PecaId,
                        Quantidade = item.Quantidade,
                        PrecoUnitario = item.PrecoUnitario
                    });
                }
            }

            // 5. Adicionar mecânicos sem duplicação
            foreach (var mecanicoId in mecanicoIds.Distinct())
            {
                bool existe = reparacao.MecanicoReparacaos
                    .Any(m => m.MecanicoId == mecanicoId);

                if (!existe)
                {
                    reparacao.MecanicoReparacaos.Add(
                        new MecanicoReparacao
                        {
                            MecanicoId = mecanicoId
                        });
                }
            }

            // 6. Recalcular custos
            reparacao.CustoMaoDeObra =
                reparacao.MecanicoReparacaos.Count * valorPorMecanico;

            decimal totalPecas = reparacao.ReparacaoPecas
                .Sum(p => p.Quantidade * p.PrecoUnitario);

            reparacao.CustoTotal =
                reparacao.CustoMaoDeObra + totalPecas;

            // 7. Concluir a reparação
            reparacao.Concluida = true;

            // 8. Concluir a marcação
            reparacao.Marcacao.Estado = "Concluida";

            // 9. Guardar as alterações
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
