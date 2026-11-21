using SistemaGestaoOficina.Data.Entities;

namespace SistemaGestaoOficina.Data.Repository
{
    public interface IReparacaoRepository : IGenericRepository<Reparacao>
    {
        Task<Reparacao?> GetByIdWithDetailsAsync(int id);

        Task<bool> EliminarReparacaoAsync(int id);
    }
}
