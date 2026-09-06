// =============================================================================
// MODELOS DE CONFIGURACION DEL SITIO
// =============================================================================

/**
 * Configuracion individual del sitio
 */
export interface ConfiguracionItem {
  id: number;
  clave: string;
  valor: string;
  tipo: 'string' | 'email' | 'url' | 'image';
  grupo: string;
  orden: number;
  descripcion?: string;
  activo: boolean;
  fechaCreacion: string;
  fechaActualizacion?: string;
}

/**
 * Configuracion del sitio agrupada por secciones
 */
export interface SiteConfig {
  // General
  sitioNombre: string;
  sitioSubtitulo: string;
  sitioLogoUrl: string;
  sitioFaviconUrl: string;

  // Contacto
  empresaNombre: string;
  empresaEmail: string;
  empresaTelefono: string;
  empresaDireccion: string;
  empresaHorario: string;

  // Redes Sociales
  socialFacebook: string;
  socialInstagram: string;
  socialLinkedin: string;
  socialTwitter: string;
  socialWhatsapp: string;
  socialYoutube: string;

  // Legal
  legalCopyright: string;
}

/**
 * Valores por defecto de configuracion
 */
export const DEFAULT_SITE_CONFIG: SiteConfig = {
  sitioNombre: 'SiteMotos',
  sitioSubtitulo: 'Panel de Control',
  sitioLogoUrl: '',
  sitioFaviconUrl: '',
  empresaNombre: 'Mi Empresa',
  empresaEmail: '',
  empresaTelefono: '',
  empresaDireccion: '',
  empresaHorario: '',
  socialFacebook: '',
  socialInstagram: '',
  socialLinkedin: '',
  socialTwitter: '',
  socialWhatsapp: '',
  socialYoutube: '',
  legalCopyright: ''
};

/**
 * Mapea el diccionario de claves-valores a SiteConfig
 */
export function mapToSiteConfig(data: Record<string, string>): SiteConfig {
  return {
    sitioNombre: data['sitio.nombre'] || DEFAULT_SITE_CONFIG.sitioNombre,
    sitioSubtitulo: data['sitio.subtitulo'] || DEFAULT_SITE_CONFIG.sitioSubtitulo,
    sitioLogoUrl: data['sitio.logo_url'] || '',
    sitioFaviconUrl: data['sitio.favicon_url'] || '',
    empresaNombre: data['empresa.nombre'] || '',
    empresaEmail: data['empresa.email'] || '',
    empresaTelefono: data['empresa.telefono'] || '',
    empresaDireccion: data['empresa.direccion'] || '',
    empresaHorario: data['empresa.horario'] || '',
    socialFacebook: data['social.facebook'] || '',
    socialInstagram: data['social.instagram'] || '',
    socialLinkedin: data['social.linkedin'] || '',
    socialTwitter: data['social.twitter'] || '',
    socialWhatsapp: data['social.whatsapp'] || '',
    socialYoutube: data['social.youtube'] || '',
    legalCopyright: data['legal.copyright'] || ''
  };
}

/**
 * Grupos de configuracion para el admin
 */
export const CONFIG_GROUPS = [
  { key: 'general', label: 'General', icon: 'settings' },
  { key: 'contacto', label: 'Contacto', icon: 'contact_mail' },
  { key: 'redes', label: 'Redes Sociales', icon: 'share' },
  { key: 'legal', label: 'Legal', icon: 'gavel' }
];

/**
 * Request para actualizar configuracion en batch
 */
export interface UpdateConfigBatchItem {
  id: number;
  valor: string;
}
