using SistemaGestaoOficina.Data.Entities;

namespace SistemaGestaoOficina.Data
{
    public interface IVeiculoRepository : IGenericRepository<Veiculo>
    {
        IQueryable<Veiculo> GetAllWithUsers();
    }
}
