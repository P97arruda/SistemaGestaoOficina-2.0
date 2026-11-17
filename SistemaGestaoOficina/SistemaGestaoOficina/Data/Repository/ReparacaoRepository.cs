using SistemaGestaoOficina.Data.Entities;

namespace SistemaGestaoOficina.Data.Repository
{
    public class ReparacaoRepository : GenericRepository<Reparacao>, IReparacaoRepository
    {
        public ReparacaoRepository(DataContext context) : base(context)
        {
            
        }
    }
}
