// Repositorios generados (consolidado): shells sobre GenericRepository<T>.
// Mantenido por el generador — un bloque namespace por entidad.

namespace ApiMotos.Infrastructure.Agregates.Depositos.Persistence
{
    using NSpecifications;
    using ApiMotos.Domain.Agregates.Depositos;
    using ApiMotos.Infrastructure.Common;

    public class DepositoRepositorio : GenericRepository<Deposito>, IDepositoRepositorio
    {
        public DepositoRepositorio(ApiMotos.Infrastructure.Generated.GeneratedContext pContexto) : base(pContexto) { }

        public List<Deposito> GetDepositos(Spec<Deposito> specification) => GetBySpec(specification);
        public List<Deposito> GetDepositos(Spec<Deposito> specification, int skip, int take) => GetBySpec(specification, skip, take);
        public Task<List<Deposito>> GetDepositosAsync(Spec<Deposito> specification) => GetBySpecAsync(specification);
        public Task<List<Deposito>> GetDepositosAsync(Spec<Deposito> specification, int skip, int take) => GetBySpecAsync(specification, skip, take);
    }
}

namespace ApiMotos.Infrastructure.Agregates.MovimientosStock.Persistence
{
    using NSpecifications;
    using ApiMotos.Domain.Agregates.MovimientosStock;
    using ApiMotos.Infrastructure.Common;

    public class MovimientoStockRepositorio : GenericRepository<MovimientoStock>, IMovimientoStockRepositorio
    {
        public MovimientoStockRepositorio(ApiMotos.Infrastructure.Generated.GeneratedContext pContexto) : base(pContexto) { }

        public List<MovimientoStock> GetMovimientosStock(Spec<MovimientoStock> specification) => GetBySpec(specification);
        public List<MovimientoStock> GetMovimientosStock(Spec<MovimientoStock> specification, int skip, int take) => GetBySpec(specification, skip, take);
        public Task<List<MovimientoStock>> GetMovimientosStockAsync(Spec<MovimientoStock> specification) => GetBySpecAsync(specification);
        public Task<List<MovimientoStock>> GetMovimientosStockAsync(Spec<MovimientoStock> specification, int skip, int take) => GetBySpecAsync(specification, skip, take);
    }
}

namespace ApiMotos.Infrastructure.Agregates.Productos.Persistence
{
    using NSpecifications;
    using ApiMotos.Domain.Agregates.Productos;
    using ApiMotos.Infrastructure.Common;

    public class ProductoRepositorio : GenericRepository<Producto>, IProductoRepositorio
    {
        public ProductoRepositorio(ApiMotos.Infrastructure.Generated.GeneratedContext pContexto) : base(pContexto) { }

        public List<Producto> GetProductos(Spec<Producto> specification) => GetBySpec(specification);
        public List<Producto> GetProductos(Spec<Producto> specification, int skip, int take) => GetBySpec(specification, skip, take);
        public Task<List<Producto>> GetProductosAsync(Spec<Producto> specification) => GetBySpecAsync(specification);
        public Task<List<Producto>> GetProductosAsync(Spec<Producto> specification, int skip, int take) => GetBySpecAsync(specification, skip, take);
    }
}

namespace ApiMotos.Infrastructure.Agregates.Variantes.Persistence
{
    using NSpecifications;
    using ApiMotos.Domain.Agregates.Variantes;
    using ApiMotos.Infrastructure.Common;

    public class VarianteRepositorio : GenericRepository<Variante>, IVarianteRepositorio
    {
        public VarianteRepositorio(ApiMotos.Infrastructure.Generated.GeneratedContext pContexto) : base(pContexto) { }

        public List<Variante> GetVariantes(Spec<Variante> specification) => GetBySpec(specification);
        public List<Variante> GetVariantes(Spec<Variante> specification, int skip, int take) => GetBySpec(specification, skip, take);
        public Task<List<Variante>> GetVariantesAsync(Spec<Variante> specification) => GetBySpecAsync(specification);
        public Task<List<Variante>> GetVariantesAsync(Spec<Variante> specification, int skip, int take) => GetBySpecAsync(specification, skip, take);
    }
}

namespace ApiMotos.Infrastructure.Agregates.Marcas.Persistence
{
    using NSpecifications;
    using ApiMotos.Domain.Agregates.Marcas;
    using ApiMotos.Infrastructure.Common;

    public class MarcaRepositorio : GenericRepository<Marca>, IMarcaRepositorio
    {
        public MarcaRepositorio(ApiMotos.Infrastructure.Generated.GeneratedContext pContexto) : base(pContexto) { }

        public List<Marca> GetMarcas(Spec<Marca> specification) => GetBySpec(specification);
        public List<Marca> GetMarcas(Spec<Marca> specification, int skip, int take) => GetBySpec(specification, skip, take);
        public Task<List<Marca>> GetMarcasAsync(Spec<Marca> specification) => GetBySpecAsync(specification);
        public Task<List<Marca>> GetMarcasAsync(Spec<Marca> specification, int skip, int take) => GetBySpecAsync(specification, skip, take);
    }
}

namespace ApiMotos.Infrastructure.Agregates.Categorias.Persistence
{
    using NSpecifications;
    using ApiMotos.Domain.Agregates.Categorias;
    using ApiMotos.Infrastructure.Common;

    public class CategoriaRepositorio : GenericRepository<Categoria>, ICategoriaRepositorio
    {
        public CategoriaRepositorio(ApiMotos.Infrastructure.Generated.GeneratedContext pContexto) : base(pContexto) { }

        public List<Categoria> GetCategorias(Spec<Categoria> specification) => GetBySpec(specification);
        public List<Categoria> GetCategorias(Spec<Categoria> specification, int skip, int take) => GetBySpec(specification, skip, take);
        public Task<List<Categoria>> GetCategoriasAsync(Spec<Categoria> specification) => GetBySpecAsync(specification);
        public Task<List<Categoria>> GetCategoriasAsync(Spec<Categoria> specification, int skip, int take) => GetBySpecAsync(specification, skip, take);
    }
}

namespace ApiMotos.Infrastructure.Agregates.Tallas.Persistence
{
    using NSpecifications;
    using ApiMotos.Domain.Agregates.Tallas;
    using ApiMotos.Infrastructure.Common;

    public class TallaRepositorio : GenericRepository<Talla>, ITallaRepositorio
    {
        public TallaRepositorio(ApiMotos.Infrastructure.Generated.GeneratedContext pContexto) : base(pContexto) { }

        public List<Talla> GetTallas(Spec<Talla> specification) => GetBySpec(specification);
        public List<Talla> GetTallas(Spec<Talla> specification, int skip, int take) => GetBySpec(specification, skip, take);
        public Task<List<Talla>> GetTallasAsync(Spec<Talla> specification) => GetBySpecAsync(specification);
        public Task<List<Talla>> GetTallasAsync(Spec<Talla> specification, int skip, int take) => GetBySpecAsync(specification, skip, take);
    }
}

namespace ApiMotos.Infrastructure.Agregates.Colores.Persistence
{
    using NSpecifications;
    using ApiMotos.Domain.Agregates.Colores;
    using ApiMotos.Infrastructure.Common;

    public class ColorRepositorio : GenericRepository<Color>, IColorRepositorio
    {
        public ColorRepositorio(ApiMotos.Infrastructure.Generated.GeneratedContext pContexto) : base(pContexto) { }

        public List<Color> GetColores(Spec<Color> specification) => GetBySpec(specification);
        public List<Color> GetColores(Spec<Color> specification, int skip, int take) => GetBySpec(specification, skip, take);
        public Task<List<Color>> GetColoresAsync(Spec<Color> specification) => GetBySpecAsync(specification);
        public Task<List<Color>> GetColoresAsync(Spec<Color> specification, int skip, int take) => GetBySpecAsync(specification, skip, take);
    }
}

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
