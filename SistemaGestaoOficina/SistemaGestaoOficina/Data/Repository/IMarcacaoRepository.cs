using SistemaGestaoOficina.Data.Entities;

namespace SistemaGestaoOficina.Data.Repository
{
    public interface IMarcacaoRepository : IGenericRepository<Marcacao>
    {
        Task<bool> ExisteConflitoAsync(Marcacao marcacao);

        Task<bool> DadosValidosAsync(Marcacao marcacao);
    }
}
