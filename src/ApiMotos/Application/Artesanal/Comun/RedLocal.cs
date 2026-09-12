using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ApiMotos.Application.Artesanal.Comun
{
    /// <summary>
    /// Red local para la demo desde el celular (plan Etapa F2, 2026-09-12): qué IPs tiene esta
    /// máquina y si un origen CORS viene de la red privada. Solo tiene sentido en Development:
    /// en producción el front y la API se sirven por el mismo origen (Docker) y el CORS es la
    /// lista de appsettings.
    /// </summary>
    public static class RedLocal
    {
        /// <summary>IPv4 privadas de las interfaces activas (sin loopback ni link-local), la de menor métrica primero.</summary>
        public static IReadOnlyList<string> DireccionesIPv4()
        {
            var lista = new List<string>();
            try
            {
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up) continue;
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
                    foreach (var ua in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ua.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                        if (!EsPrivada(ua.Address)) continue;
                        // Las virtuales (VirtualBox, Hyper-V, WSL, VPN) suelen tener 172.x o nombres delatores: van al final.
                        var virtual_ = ni.Description.Contains("Virtual", StringComparison.OrdinalIgnoreCase)
                                    || ni.Description.Contains("Hyper-V", StringComparison.OrdinalIgnoreCase)
                                    || ni.Description.Contains("WSL", StringComparison.OrdinalIgnoreCase)
                                    || ni.Description.Contains("VPN", StringComparison.OrdinalIgnoreCase);
                        var ip = ua.Address.ToString();
                        if (virtual_) lista.Add(ip); else lista.Insert(0, ip);
                    }
                }
            }
            catch
            {
                // Sin permisos o sin red: lista vacía, el front avisa.
            }
            return lista.Distinct().ToList();
        }

        /// <summary>true si el origen (http://host:puerto) apunta a localhost o a una IP privada.</summary>
        public static bool EsOrigenDeRedLocal(string? origen)
        {
            if (string.IsNullOrWhiteSpace(origen) || !Uri.TryCreate(origen, UriKind.Absolute, out var uri)) return false;
            if (uri.IsLoopback || uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)) return true;
            return IPAddress.TryParse(uri.Host, out var ip) && EsPrivada(ip);
        }

        private static bool EsPrivada(IPAddress ip)
        {
            if (ip.AddressFamily != AddressFamily.InterNetwork) return false;
            var b = ip.GetAddressBytes();
            if (b[0] == 169 && b[1] == 254) return false;           // link-local
            return b[0] == 10
                || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)
                || (b[0] == 192 && b[1] == 168);
        }
    }
}
