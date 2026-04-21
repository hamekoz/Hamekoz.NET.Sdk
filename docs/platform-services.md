# Platform Services

Esta base introduce dos bloques reutilizables para el ecosistema Hamekoz.

## Hamekoz.Subscriptions

- Mantiene el catalogo de planes por aplicacion.
- Garantiza que siempre exista un plan gratuito activo y marcado como default.
- Garantiza que cada usuario tenga una suscripcion activa, aunque nunca haya contratado un plan pago.
- Permite asignar planes pagos o premium sin acoplar el dominio a un proveedor de pago concreto.

## Hamekoz.Features

- Resuelve features efectivos combinando tres capas: aplicacion, plan de suscripcion y usuario.
- Parte de los features semilla del plan activo y luego aplica grants y overrides.
- Permite usar el mismo patron para Carta Universal, MiAgendaDocente y futuros servicios del ecosistema.

## Siguiente separacion natural a microservicios

- Un servicio central de identidad y usuarios para todo Hamekoz.
- Un microservicio de suscripciones para catalogo, activaciones y renovaciones.
- Un microservicio de features para overrides globales, por plan y por usuario.
- Las apps de negocio consumen estos servicios y solo guardan configuracion propia.