using SistemaGestaoOficina.Data.Entities;

namespace SistemaGestaoOficina.Data.Repository
{
    public class TipoReparacaoRepository : GenericRepository<TipoReparacao>, ITipoReparacaoRepository
    {
        public TipoReparacaoRepository(DataContext context) : base(context)
        {
            
        }
    }
}
