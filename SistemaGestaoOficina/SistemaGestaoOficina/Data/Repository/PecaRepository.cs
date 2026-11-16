using SistemaGestaoOficina.Data.Entities;

namespace SistemaGestaoOficina.Data.Repository
{
    public class PecaRepository : GenericRepository<Peca>, IPecaRepository
    {
        public PecaRepository(DataContext context) : base(context)
        {
            
        }
    }
}
