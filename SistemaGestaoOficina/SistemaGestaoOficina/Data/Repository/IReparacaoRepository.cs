using SistemaGestaoOficina.Data.Entities;

namespace SistemaGestaoOficina.Data.Repository
{
    public interface IReparacaoRepository : IGenericRepository<Reparacao>
    {
        Task<Reparacao?> GetByIdWithDetailsAsync(int id);

        Task<bool> EliminarReparacaoAsync(int id);

        // Concluir uma reparação já existente
        Task<bool> ConcluirExistenteAsync(
            int marcacaoId,
            DateTime dataInicio,
            DateTime dataFim,
            List<ReparacaoPeca> pecas,
            List<int> mecanicoIds,
            decimal valorPorMecanico);
    }
}
