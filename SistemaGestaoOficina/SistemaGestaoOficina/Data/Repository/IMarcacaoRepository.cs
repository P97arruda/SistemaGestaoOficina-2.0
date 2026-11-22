using SistemaGestaoOficina.Data.Entities;

namespace SistemaGestaoOficina.Data.Repository
{
    public interface IMarcacaoRepository : IGenericRepository<Marcacao>
    {
        Task<bool> ExisteConflitoAsync(Marcacao marcacao);

        Task<bool> DadosValidosAsync(Marcacao marcacao);

        Task<List<User>> GetClientesAsync();

        Task<List<Veiculo>> GetVeiculosAsync();

        Task<List<Mecanico>> GetMecanicosAsync();

        Task<List<TipoReparacao>> GetTiposReparacaoAsync();

        Task<bool> ExisteMarcacaoDuplicadaAsync(Marcacao marcacao);

        Task<List<Marcacao>> GetMarcacoesDisponiveisAsync();

        bool DataFutura(DateTime dataHora);

        bool HorarioValido(DateTime dataHora);

        Task<List<string>> ValidarMarcacaoAsync(Marcacao marcacao);

        Task<List<Veiculo>> GetVeiculosPorClienteAsync(string? userId);
    }
}