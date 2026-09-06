// Repositorios generados (consolidado): shells sobre GenericRepository<T>.
// Mantenido por el generador — un bloque namespace por entidad.

namespace ApiMotos.Infrastructure.Agregates.Clientes.Persistence
{
    using NSpecifications;
    using ApiMotos.Domain.Agregates.Clientes;
    using ApiMotos.Infrastructure.Common;

    public class ClienteRepositorio : GenericRepository<Cliente>, IClienteRepositorio
    {
        public ClienteRepositorio(ApiMotos.Infrastructure.Generated.GeneratedContext pContexto) : base(pContexto)
        {
        }

        public List<Cliente> GetClientes(Spec<Cliente> specification) => GetBySpec(specification);
        public List<Cliente> GetClientes(Spec<Cliente> specification, int skip, int take) => GetBySpec(specification, skip, take);
        public Task<List<Cliente>> GetClientesAsync(Spec<Cliente> specification) => GetBySpecAsync(specification);
        public Task<List<Cliente>> GetClientesAsync(Spec<Cliente> specification, int skip, int take) => GetBySpecAsync(specification, skip, take);
    }
}

namespace ApiMotos.Infrastructure.Agregates.Envios.Persistence
{
    using NSpecifications;
    using ApiMotos.Domain.Agregates.Envios;
    using ApiMotos.Infrastructure.Common;

    public class EnvioRepositorio : GenericRepository<Envio>, IEnvioRepositorio
    {
        public EnvioRepositorio(ApiMotos.Infrastructure.Generated.GeneratedContext pContexto) : base(pContexto)
        {
        }

        public List<Envio> GetEnvios(Spec<Envio> specification) => GetBySpec(specification);
        public List<Envio> GetEnvios(Spec<Envio> specification, int skip, int take) => GetBySpec(specification, skip, take);
        public Task<List<Envio>> GetEnviosAsync(Spec<Envio> specification) => GetBySpecAsync(specification);
        public Task<List<Envio>> GetEnviosAsync(Spec<Envio> specification, int skip, int take) => GetBySpecAsync(specification, skip, take);
    }
}

namespace ApiMotos.Infrastructure.Agregates.Agencias.Persistence
{
    using NSpecifications;
    using ApiMotos.Domain.Agregates.Agencias;
    using ApiMotos.Infrastructure.Common;

    public class AgenciaRepositorio : GenericRepository<Agencia>, IAgenciaRepositorio
    {
        public AgenciaRepositorio(ApiMotos.Infrastructure.Generated.GeneratedContext pContexto) : base(pContexto)
        {
        }

        public List<Agencia> GetAgencias(Spec<Agencia> specification) => GetBySpec(specification);
        public List<Agencia> GetAgencias(Spec<Agencia> specification, int skip, int take) => GetBySpec(specification, skip, take);
        public Task<List<Agencia>> GetAgenciasAsync(Spec<Agencia> specification) => GetBySpecAsync(specification);
        public Task<List<Agencia>> GetAgenciasAsync(Spec<Agencia> specification, int skip, int take) => GetBySpecAsync(specification, skip, take);
    }
}

namespace ApiMotos.Infrastructure.Agregates.Observaciones.Persistence
{
    using NSpecifications;
    using ApiMotos.Domain.Agregates.Observaciones;
    using ApiMotos.Infrastructure.Common;

    public class ObservacionRepositorio : GenericRepository<Observacion>, IObservacionRepositorio
    {
        public ObservacionRepositorio(ApiMotos.Infrastructure.Generated.GeneratedContext pContexto) : base(pContexto)
        {
        }

        public List<Observacion> GetObservaciones(Spec<Observacion> specification) => GetBySpec(specification);
        public List<Observacion> GetObservaciones(Spec<Observacion> specification, int skip, int take) => GetBySpec(specification, skip, take);
        public Task<List<Observacion>> GetObservacionesAsync(Spec<Observacion> specification) => GetBySpecAsync(specification);
        public Task<List<Observacion>> GetObservacionesAsync(Spec<Observacion> specification, int skip, int take) => GetBySpecAsync(specification, skip, take);
    }
}

namespace ApiMotos.Infrastructure.Agregates.ParametroSLAs.Persistence
{
    using NSpecifications;
    using ApiMotos.Domain.Agregates.ParametroSLAs;
    using ApiMotos.Infrastructure.Common;

    public class ParametroSLARepositorio : GenericRepository<ParametroSLA>, IParametroSLARepositorio
    {
        public ParametroSLARepositorio(ApiMotos.Infrastructure.Generated.GeneratedContext pContexto) : base(pContexto)
        {
        }

        public List<ParametroSLA> GetParametroSLAs(Spec<ParametroSLA> specification) => GetBySpec(specification);
        public List<ParametroSLA> GetParametroSLAs(Spec<ParametroSLA> specification, int skip, int take) => GetBySpec(specification, skip, take);
        public Task<List<ParametroSLA>> GetParametroSLAsAsync(Spec<ParametroSLA> specification) => GetBySpecAsync(specification);
        public Task<List<ParametroSLA>> GetParametroSLAsAsync(Spec<ParametroSLA> specification, int skip, int take) => GetBySpecAsync(specification, skip, take);
    }
}
