using Microsoft.EntityFrameworkCore;
using SistemaGestaoOficina.Data.Entities;

namespace SistemaGestaoOficina.Data
{
    public class VeiculoRepository : GenericRepository<Veiculo>, IVeiculoRepository
    {
        private readonly DataContext _context;
        public VeiculoRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<Veiculo> GetAllWithUsers()
        {
            return _context.Set<Veiculo>()
                .Include(v => v.User)
                .AsNoTracking();
        }
    }
}
