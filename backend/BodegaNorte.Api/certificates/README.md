# Certificado raíz de Supabase

`prod-ca-2021.crt` es un certificado público, sin claves privadas ni credenciales.

- Fuente: enlace «Download certificate» del panel Database → Settings de Supabase.
- URL oficial: https://supabase-downloads.s3-ap-southeast-1.amazonaws.com/prod/ssl/prod-ca-2021.crt
- SHA-1 del certificado: A4518A0933AF6949482CCA3014C007C369DF9F6F.
- Vencimiento: 26 de abril de 2031.

El backend lo copia a la salida de compilación y publicación. Para los hosts de
Supabase, `DatabaseConnection` aplica `VerifyFull` y utiliza este certificado
cuando la cadena no especifica otro `Root Certificate`. Se verifican tanto la
cadena de confianza como el nombre del servidor; no se desactiva TLS.

Documentación: https://supabase.com/docs/guides/platform/ssl-enforcement
