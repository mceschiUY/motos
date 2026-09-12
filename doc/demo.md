# Guion de demo — un día en la distribuidora (10 minutos)

> Sigue la historia de `plan.md` §2 y la regla "escenas, no tablas" (2026-09-12): la demo
> recorre solo escenas artesanales; los CRUD generados quedan bajo **Administración** y no se
> muestran. Datos: seed local (`Seed_Dominio_Motos.sql`, anclado a hoy). Login `pablo` / `pablo`.
> Los números de abajo son los del seed el día que se escribió; cambian con la fecha.

Antes de empezar: `dotnet run --project src/ApiMotos` y `cd web && npm start`, abrir
`http://localhost:4210`, rol de vista **Gerencia** (ícono de brújula en el cabezal) y tema a gusto.

| # | Escena | Ruta | Quién | Minutos |
|---|---|---|---|---|
| 1 | Hoy | `/` | Gerencia | 2 |
| 2 | Seguir el paquete | `/envio/11` → `/seguimiento/MT-2026-0111` | Cliente de la tienda | 1,5 |
| 3 | Salir a vender | `/cliente/2` | Vendedor | 1,5 |
| 4 | Armar el pedido | `/catalogo` → `/catalogo/:id` → `/pedidos/nuevo?clienteId=2` | Vendedor, en la tienda | 2 |
| 5 | Preparar y despachar | `/pedido` (kanban) → `/variante/70` | Depósito | 1,5 |
| 6 | Cerrar el mes | `/vendedor/1` → `/comisiones` | Gerencia | 1,5 |

## 1. Hoy (`/`) — "¿qué pasa hoy y dónde tengo que mirar?"
- Entrar. El home es el **centro de control**: saludo, fecha larga y cinco KPI con color:
  pedidos a despachar (4), visitas de hoy, envíos fuera de SLA, ventas del mes en US$ y
  SKU con stock bajo. Cada KPI es un link.
- Columna izquierda, **"Qué hacer hoy"**: el feed de acciones ordenado por urgencia. Leer dos
  en voz alta: *"Envío MT-2026-0111 a Salto fuera de SLA · 14 días en facturación · límite 4"*
  y *"Botas Alpinestars SMX-6 v2 42 negro: quedan 2"*.
- Centro: pipeline de pedidos del mes por estado y **Equipo** (meta, avance, comisión por
  vendedor). Derecha: top productos, stock por depósito y "Recién pasó".
- Decir: *"Nada de esto es una tabla: es lo que el gerente necesita decidir a las 9 de la mañana."*
- Click en la acción del envío fuera de SLA.

## 2. Seguir el paquete (`/envio/11`) — "¿dónde está mi pedido?"
- Línea de estados estilo courier: Recibido ✓, **Confirmado** (actual, en rojo: 14 días vs
  límite 4), Despachado, Entregado. Agencia DAC, cliente Casa Bike Salto (link al 360).
- "Próximo paso": botón **Confirmar** ejecuta la transición real del ciclo. (Opcional: no
  ejecutarla para conservar el ejemplo de SLA vencido.)
- Bitácora: **Agregar observación** abre el form generado como diálogo.
- **Compartir**: link público + **QR**. Escanear con el celular o abrir
  `http://localhost:4210/seguimiento/MT-2026-0111` en una ventana de incógnito: sin login,
  sin precios, con los estados y las novedades. *"Esto es lo que ve el dueño de la tienda."*

## 3. Salir a vender (`/cliente/2`, Casa Bike Salto) — "¿a quién visito y qué le llevo?"
- Cambiar el rol de vista a **Vendedor**: el menú se reduce y el inicio pasa a ser la agenda
  (`/agenda`, con la ruta del día y el mapa). Desde una parada, abrir el cliente.
- **Cliente 360**: tipo, ciudad, contacto, vendedor asignado, **semáforo** con motivo
  ("Visitado hace 3 días"), notas ("Cierra de 13 a 15"). KPI: días sin visita, pedidos del
  año, ticket promedio, pedidos abiertos, envíos en curso.
- Línea de tiempo única: WhatsApp del 9, pedido PED-0008 en preparación, visita reprogramada…
  Top productos del cliente y mapa.
- Botones: **Registrar visita** (diálogo precargado con el cliente) y **Nuevo pedido**.

## 4. Armar el pedido (`/catalogo` → `/catalogo/:id` → `/pedidos/nuevo?clienteId=2`)
- Catálogo con fotos, "desde US$", stock y SKU; destacados primero, etiqueta "Nuevo",
  filtros por marca y categoría. Abrir el **Shoei NXR2**: ficha comercial, matriz talla ×
  color con existencias y precio, margen. **Imprimir** deja la hoja con membrete.
- "Agregar al pedido" desde una celda: el armado se abre con el SKU en el carrito y el
  cliente ya elegido si venís del 360. Buscar otro SKU, ver el aviso cuando se pide más de lo
  que hay, total en vivo, **Crear pedido** (queda en borrador).
- Decir: *"Esto lo hace el vendedor en el mostrador de la tienda, desde el teléfono."*

## 5. Preparar y despachar (`/pedido` en kanban → `/variante/70`)
- Rol de vista **Depósito**: el inicio es el **kanban de pedidos**. Arrastrar PED-0007 de
  "preparado" a "despachado": el hook descuenta el Kardex y **nace el envío** con su código.
- Abrir la ficha del pedido: proceso guiado, líneas, botón de WhatsApp con el texto armado.
- Desde una línea, o desde el feed de Hoy, abrir la **ficha de SKU** (`/variante/70`, Botas
  SMX-6 42 negro): stock por depósito, comprometido, **"se acaba en N días"**, Kardex como
  línea de tiempo con saldo corrido y el `PED-000n` clickeable, historial de precios.
  `/movimientostock/1` redirige a la misma ficha con el movimiento resaltado.
- **Registrar movimiento** y **Editar datos** abren los forms generados como diálogos.

## 6. Cerrar el mes (`/vendedor/1` → `/comisiones`)
- Volver al rol **Gerencia**. Desde "Equipo" en Hoy, abrir a **Andrés Ferreira**: ranking del
  mes, meta con barra y marca de "esperado a hoy", comisión sellada y **proyectada** ("si
  entrega lo que tiene abierto"), slider *"¿y si vende US$ 2.000 más?"*, ventas por semana,
  esta semana, mis clientes con semáforo, pedidos abiertos.
- `/comisiones`: liquidación por vendedor y mes con export CSV.
- Cierre: *"Catálogo, vendedores, pedidos, depósito, envíos y comisiones en un solo relato; y
  cada pantalla que vieron es un link desde la anterior."*

## Si algo falla
- El seed se re-siembra vaciando el dominio con `Limpiar_Datos_Dominio.sql` y reiniciando la API.
- Si el QR no abre desde el celular, el celular tiene que ver la IP de la máquina (no
  `localhost`): usar `http://<ip>:4210/seguimiento/<codigo>` y el CORS de `appsettings`.
- El asistente de voz sigue oculto (necesita `Asistente:OpenAIApiKey`).
- Lo que **no** hay que mostrar: reportes programados (se guardan pero no se ejecutan) y las
  listas de Administración.
