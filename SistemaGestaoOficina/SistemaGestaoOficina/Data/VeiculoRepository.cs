using SistemaGestaoOficina.Data.Entities;

namespace SistemaGestaoOficina.Data
{
    public class VeiculoRepository : GenericRepository<Veiculo>, IVeiculoRepository
    {
        public VeiculoRepository(DataContext context) : base(context)
        {

        }
    }
}
