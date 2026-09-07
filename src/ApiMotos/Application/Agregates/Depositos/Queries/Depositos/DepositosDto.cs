namespace ApiMotos.Application.Agregates.Depositos.Queries.Depositos
{
    public class DepositosDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; }
    }
}
