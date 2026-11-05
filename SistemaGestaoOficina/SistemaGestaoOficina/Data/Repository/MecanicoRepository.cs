using SistemaGestaoOficina.Data.Entities;

namespace SistemaGestaoOficina.Data.Repository
{
    public class MecanicoRepository : GenericRepository<Mecanico>, IMecanicoRepository
    {
        public MecanicoRepository(DataContext context) : base(context)
        {
        }
    }
}
