namespace ApiMotos.Application.Agregates.ParametroSLAs.Queries.ParametroSLAs
{
    public class ParametroSLAsDto
    {
        public int Id { get; set; }
        public string Etapa { get; set; }
        public int RangoAlertaUmbralAdvertenciaDias { get; set; }
        public int RangoAlertaLimiteDias { get; set; }
    }
}
