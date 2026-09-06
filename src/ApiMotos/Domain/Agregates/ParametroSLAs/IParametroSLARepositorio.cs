using ApiMotos.Domain.Common;
using NSpecifications;

namespace ApiMotos.Domain.Agregates.ParametroSLAs
{
    public interface IParametroSLARepositorio : IRepository<ParametroSLA, int>
    {
        public List<ParametroSLA> GetParametroSLAs(Spec<ParametroSLA> specification, int skip, int take);
        public List<ParametroSLA> GetParametroSLAs(Spec<ParametroSLA> specification);
        public Task<List<ParametroSLA>> GetParametroSLAsAsync(Spec<ParametroSLA> specification);
        public Task<List<ParametroSLA>> GetParametroSLAsAsync(Spec<ParametroSLA> specification, int skip, int take);
    }
}
