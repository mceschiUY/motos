# Build multi-stage del sitio Angular de Motos (SiteMotos).
# Contexto de build: la carpeta web/ (ver docker-compose.yml -> frontend.build.context).

FROM node:20-alpine AS build
WORKDIR /app

# Restore primero (capa cacheable): solo el manifiesto de dependencias.
COPY package*.json ./
RUN npm ci

# Resto del código y build de producción (outputPath: dist/sitio-core).
COPY . .
RUN npm run build -- --configuration production

# Etapa de producción: nginx sirve los estáticos.
# nginx.conf es una config completa (events/http) → va a /etc/nginx/nginx.conf.
FROM nginx:alpine
COPY --from=build /app/dist/sitio-core/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
