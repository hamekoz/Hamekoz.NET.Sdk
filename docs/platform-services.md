# Platform Services

Esta base introduce dos bloques reutilizables para el ecosistema Hamekoz.

## Hamekoz.Subscriptions

- Mantiene el catálogo de planes por aplicación.
- Garantiza que siempre exista un plan gratuito activo y marcado como default.
- Garantiza que cada usuario tenga una suscripción activa, aunque nunca haya contratado un plan pago.
- Permite asignar planes pagos o premium sin acoplar el dominio a un proveedor de pago concreto.

## Hamekoz.Features

- Resuelve features efectivos combinando tres capas: aplicación, plan de suscripción y usuario.
- Parte de los features semilla del plan activo y luego aplica grants y overrides.
- Permite usar el mismo patrón para Carta Universal, MiAgendaDocente y futuros servicios del ecosistema.

## Siguiente separación natural a microservicios

- Un servicio central de identidad y usuarios para todo Hamekoz.
- Un microservicio de suscripciones para catálogo, activaciones y renovaciones.
- Un microservicio de features para overrides globales, por plan y por usuario.
- Las apps de negocio consumen estos servicios y solo guardan configuración propia.
