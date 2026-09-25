using SistemaGestaoOficina.Data.Entities;


namespace SistemaGestaoOficina.Data
{
    public class ClienteRepository : GenericRepository<Cliente>, IClienteRepository
    {
        public ClienteRepository(DataContext context) : base(context)
        {

        }


    }
}
