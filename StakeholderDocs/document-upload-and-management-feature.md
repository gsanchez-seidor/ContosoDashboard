# Característica de Carga y Gestión de Documentos - Requisitos

## Descripción General

Contoso Corporation necesita agregar capacidades de carga y gestión de documentos a la aplicación ContosoDashboard. Esta característica permitirá a los empleados cargar documentos relacionados con el trabajo, organizarlos por categoría y proyecto, y compartirlos con compañeros de equipo.

## Necesidad Empresarial

Actualmente, los empleados de Contoso almacenan documentos de trabajo en varios lugares (unidades locales, adjuntos de correo electrónico, unidades compartidas), lo que provoca:

- Dificultad para localizar documentos importantes cuando se necesitan
- Riesgos de seguridad derivados del uso compartido incontrolado de documentos
- Falta de visibilidad sobre qué documentos están asociados con proyectos o tareas específicas

La característica de carga y gestión de documentos aborda estos problemas proporcionando una ubicación centralizada y segura para documentos relacionados con el trabajo dentro de la aplicación de panel que los empleados ya utilizan diariamente.

## Usuarios Objetivo

Todos los empleados de Contoso que utilizan la aplicación ContosoDashboard tendrán acceso a las características de gestión de documentos, con permisos basados en sus roles existentes:

- **Empleados**: Cargar documentos personales y documentos para proyectos a los que están asignados
- **Líderes de Equipo**: Cargar documentos y ver/gestionar documentos cargados por sus miembros del equipo
- **Gerentes de Proyecto**: Cargar documentos y gestionar todos los documentos asociados con sus proyectos
- **Administradores**: Acceso completo a todos los documentos con fines de auditoría y cumplimiento

## Requisitos Principales

### 1. Carga de Documentos

**Selección y Carga de Archivos**

- Los usuarios deben poder seleccionar uno o varios archivos de su computadora para cargar
- Tipos de archivo soportados: PDF, documentos de Microsoft Office (Word, Excel, PowerPoint), archivos de texto e imágenes (JPEG, PNG)
- Tamaño máximo de archivo: 25 MB por archivo
- Los usuarios deben ver un indicador de progreso durante la carga
- El sistema debe mostrar mensajes de éxito o error después de completar la carga

**Metadatos del Documento**

- Al cargar, los usuarios deben proporcionar:
  - Título del documento (requerido)
  - Descripción (opcional)
  - Selección de categoría de lista predefinida (requerido): Documentos de Proyecto, Recursos del Equipo, Archivos Personales, Informes, Presentaciones, Otros
  - Proyecto asociado (opcional - si el documento se relaciona con un proyecto específico)
  - Etiquetas para búsqueda más fácil (opcional - los usuarios pueden agregar etiquetas personalizadas)
- El sistema debe capturar automáticamente:
  - Fecha y hora de carga
  - Cargado por (nombre de usuario)
  - Tamaño del archivo
  - Tipo de archivo (tipo MIME, por ejemplo, "application/pdf" - el campo debe acomodar 255 caracteres para documentos de Office)

**Validación y Seguridad**

- El sistema debe escanear archivos cargados en busca de virus y malware antes del almacenamiento
- El sistema debe rechazar archivos que excedan los límites de tamaño con mensajes de error claros
- El sistema debe rechazar tipos de archivo no compatible
- Los archivos cargados deben almacenarse de forma segura con controles de acceso apropiados

**Notas de Implementación para Almacenamiento de Archivos Locales**

**Patrón de Almacenamiento Offline:**
- Almacenar archivos en un directorio dedicado fuera de `wwwroot` por seguridad (por ejemplo, `AppData/uploads`)
- Generar rutas de archivo únicas ANTES de la inserción en la base de datos para prevenir violaciones de claves duplicadas
- Patrón recomendado: `{userId}/{projectId o "personal"}/{uniqueId}.{extension}` donde uniqueId es un GUID
- **Secuencia de carga: Generar ruta única → Guardar archivo en disco → Guardar metadatos en base de datos**
- **Esto previene registros de base de datos huérfanos si la ejecución del archivo falla**
- **Esto previene errores de clave duplicada de rutas vacías o no únicas**

**Consideraciones de Seguridad:**
- Los archivos almacenados fuera de `wwwroot` requieren controladores para servirlos (habilita verificaciones de autorización)
- Validar extensiones de archivo contra lista blanca antes de guardar
- Usar nombres de archivo basados en GUID para prevenir ataques de recorrido de directorio
- Nunca usar nombres de archivo proporcionados por el usuario directamente en rutas de archivo
- Implementar verificaciones de autorización en el controlador de descarga para prevenir acceso no autorizado

**Diseño de Migración a Azure:**
- Crear interfaz `IFileStorageService` con métodos: `UploadAsync()`, `DeleteAsync()`, `DownloadAsync()`, `GetUrlAsync()`
- La implementación local (`LocalFileStorageService`) utiliza operaciones `System.IO.File`
- La futura implementación `AzureBlobStorageService` utilizará Azure.Storage.Blobs SDK
- El mismo patrón de ruta funciona para nombres de blob de Azure: `{userId}/{projectId}/{guid}.{ext}`
- Intercambiar implementaciones a través de la configuración de inyección de dependencias
- No se requieren cambios en la lógica empresarial, UI o esquema de base de datos para la migración

### 2. Organización y Exploración de Documentos

**Vista Mis Documentos**

- Los usuarios deben poder ver una lista de todos los documentos que han cargado
- La vista debe mostrar: título del documento, categoría, fecha de carga, tamaño del archivo, proyecto asociado
- Los usuarios deben poder ordenar documentos por: título, fecha de carga, categoría, tamaño del archivo
- Los usuarios deben poder filtrar documentos por: categoría, proyecto asociado, rango de fechas

**Vista Documentos del Proyecto**

- Al ver un proyecto específico, los usuarios deben ver todos los documentos asociados con ese proyecto
- Todos los miembros del equipo del proyecto deben poder ver y descargar documentos del proyecto
- Los Gerentes de Proyecto deben poder cargar documentos a sus proyectos

**Búsqueda**

- Los usuarios deben poder buscar documentos por: título, descripción, etiquetas, nombre del usuario que cargó, proyecto asociado
- La búsqueda debe devolver resultados dentro de 2 segundos
- Los usuarios solo deben ver documentos a los que tienen permiso de acceso en los resultados de búsqueda

### 3. Acceso y Gestión de Documentos

**Descargar y Vista Previa**

- Los usuarios deben poder descargar cualquier documento al que tengan acceso
- Para tipos de archivo comunes (PDF, imágenes), los usuarios deben poder ver documentos en el navegador sin descargar

**Editar Metadatos**

- Los usuarios que cargaron un documento deben poder editar los metadatos del documento (título, descripción, categoría, etiquetas)
- Los usuarios deben poder reemplazar un archivo de documento con una versión actualizada

**Eliminar Documentos**

- Los usuarios deben poder eliminar documentos que cargaron
- Los Gerentes de Proyecto pueden eliminar cualquier documento en sus proyectos
- Los documentos eliminados deben eliminarse permanentemente después de la confirmación del usuario

**Compartir Documentos**

- Los propietarios de documentos deben poder compartir documentos con usuarios o equipos específicos
- Los usuarios que reciben documentos compartidos deben ser notificados a través de notificación en la aplicación
- Los documentos compartidos deben aparecer en la sección "Compartido conmigo" de los destinatarios

### 4. Integración con Características Existentes

**Integración de Tareas**

- Al ver una tarea, los usuarios deben poder ver y adjuntar documentos relacionados
- Los usuarios deben poder cargar un documento directamente desde una página de detalle de tarea
- Los documentos adjuntos a tareas deben asociarse automáticamente con el proyecto de la tarea

**Integración de Panel**

- Agregar un widget "Documentos Recientes" a la página de inicio del panel mostrando los últimos 5 documentos cargados por el usuario
- Agregar conteo de documentos a las tarjetas de resumen del panel

**Notificaciones**

- Los usuarios deben recibir notificaciones cuando alguien comparte un documento con ellos
- Los usuarios deben recibir notificaciones cuando se agrega un nuevo documento a uno de sus proyectos

### 5. Requisitos de Rendimiento

- La carga de documentos debe completarse en 30 segundos para archivos de hasta 25 MB (en una red típica)
- Las páginas de lista de documentos deben cargarse en 2 segundos para hasta 500 documentos
- La búsqueda de documentos debe devolver resultados en 2 segundos
- La vista previa de documentos debe cargarse en 3 segundos

### 6. Informes y Auditoría

**Seguimiento de Actividades**

- El sistema debe registrar todas las actividades relacionadas con documentos: cargas, descargas, eliminaciones, acciones de uso compartido
- Los administradores deben poder generar informes que muestren:
  - Tipos de documentos más cargados
  - Usuarios más activos en carga
  - Patrones de acceso a documentos

## Objetivos de Experiencia del Usuario

- **Simplicidad**: Cargar un documento no debe requerir más de 3 clics
- **Velocidad**: Las operaciones comunes (cargar, descargar, buscar) deben sentirse instantáneas
- **Claridad**: Los usuarios siempre deben saber qué sucede con los archivos cargados
- **Confianza**: Los usuarios deben confiar en que sus documentos están seguros y no se perderán

## Métricas de Éxito

La característica se considerará exitosa si, dentro de 3 meses del lanzamiento:

- El 70% de los usuarios del panel activos han cargado al menos un documento
- El tiempo promedio para localizar un documento se reduce a menos de 30 segundos
- El 90% de los documentos cargados están correctamente categorizados
- Cero incidentes de seguridad relacionados con acceso a documentos

## Restricciones Técnicas

- Debe funcionar **offline sin servicios en la nube** para fines de capacitación
- Debe usar **almacenamiento de sistema de archivos local** para documentos cargados
- Debe implementar **abstracciones de interfaz** (`IFileStorageService`) para futura migración en la nube
- Debe funcionar dentro de la arquitectura de la aplicación actual (sin reescrituras mayores)
- Debe cumplir con el sistema de autenticación simulada existente
- Cronograma de desarrollo: La característica debe estar lista para producción dentro de 8-10 semanas
- **Base de datos: DocumentId debe ser entero (no GUID) para consistencia con claves User/Project existentes**
- **Base de datos: Category debe almacenar valores de texto (no enum entero) por simplicidad**

## Enfoque de Implementación

La característica de gestión de documentos se construye usando una **arquitectura en capas** que separa responsabilidades y habilita la migración futura a la nube:

**Capa de Datos:**
- La entidad Document almacena metadatos (título, categoría, nombre de archivo, ruta de archivo, fecha de carga, usuario que cargó)
- DocumentId utiliza claves enteras (consistente con las tablas User y Project existentes)
- Category almacena valores de texto ("Documentos de Proyecto", "Archivos Personales", etc.) por simplicidad
- El campo FileType acomoda tipos MIME largos (255 caracteres para documentos de Office)
- FilePath acomoda nombres de archivo basados en GUID para seguridad (previene ataques de recorrido de directorios)
- La entidad DocumentShare rastrea relaciones de uso compartido entre usuarios

**Capa de Almacenamiento:**
- Archivos almacenados fuera de directorios accesibles por web (requisito de seguridad)
- La interfaz IFileStorageService abstrae la implementación de almacenamiento
- LocalFileStorageService para capacitación (utiliza sistema de archivos local)
- Futuro: Cambiar a AzureBlobStorageService para producción (sin cambios de código necesarios)
- Organización de archivos: `{userId}/{projectId o "personal"}/{guid}.{extension}`

**Capa de Lógica Empresarial:**
- DocumentService orquesta el flujo de carga:
  1. Validar archivo (límite de tamaño, lista blanca de extensión)
  2. Autorizar usuario (membresía de proyecto si carga al proyecto)
  3. Generar nombre de archivo único basado en GUID
  4. Guardar archivo en disco
  5. Crear registro de base de datos con ruta de archivo
  6. Enviar notificaciones a miembros del proyecto
- Las verificaciones de autorización previenen acceso no autorizado a documentos (protección IDOR)
- La capa de servicio refuerza todas las reglas de seguridad antes del acceso a datos

**Capa de Presentación:**
- Página Blazor Server para carga y visualización de documentos
- La carga de archivos utiliza el patrón MemoryStream (previene problemas de eliminación en Blazor)
- Una tabla receptiva muestra los documentos del usuario con metadatos
- El modal de carga valida la entrada antes del envío

Esta arquitectura garantiza seguridad, mantenibilidad y disposición para la nube mientras mantiene la implementación de capacitación simple y offline.

### Disposición para Migración a la Nube

Aunque esta característica debe funcionar offline para capacitación, debe diseñarse para una fácil migración a servicios de Azure:

**Requisitos de Implementación Offline:**
- Almacenar archivos en estructura de directorio local (por ejemplo, `AppData/uploads/{userId}/{projectId}/{guid}.ext`)
- Implementar `LocalFileStorageService : IFileStorageService` usando operaciones `System.IO`
- Las rutas de archivo almacenadas en la base de datos deben ser relativas y portátiles
- Sin dependencias de Azure SDK en la implementación de capacitación

**Patrón de Diseño de Migración a Azure:**

```csharp
// Abstracción de interfaz (implementar en versión de capacitación)
public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType);
    Task DeleteAsync(string filePath);
    Task<Stream> DownloadAsync(string filePath);
    Task<string> GetUrlAsync(string filePath, TimeSpan expiration);
}

// Capacitación: Implementación de LocalFileStorageService
// Producción: Implementación de AzureBlobStorageService
// Cambiar a través de appsettings.json e inyección de dependencias
```

**Beneficios de la Migración:**
- Intercambiar implementación de servicio sin cambiar controladores, páginas o lógica empresarial
- El esquema de base de datos permanece sin cambios (la columna FilePath funciona para rutas locales y nombres de blob)
- Implementación controlada por configuración (dev = local, producción = Azure)
- Los estudiantes aprenden patrones de abstracción estándar de la industria

### Requisitos de Implementación Específicos de Blazor

**Gestión de Estado del Componente de Carga de Archivos**

- Usar el atributo `@key` en el componente `InputFile` para forzar re-renderización después de carga exitosa
- Extraer metadatos de archivo (nombre, tamaño, contentType) en variables locales ANTES de abrir el flujo
- Copiar el flujo de `IBrowserFile` a `MemoryStream` inmediatamente para prevenir problemas de eliminación
- Borrar referencia de `IBrowserFile` (establecer en null) después de copiar el flujo para prevenir errores de reutilización
- Patrón de ejemplo:
  ```csharp
  var fileName = SelectedFile.Name;
  var fileSize = SelectedFile.Size;
  var contentType = SelectedFile.ContentType;
  
  using var memoryStream = new MemoryStream();
  using (var fileStream = SelectedFile.OpenReadStream(maxFileSize))
  {
      await fileStream.CopyToAsync(memoryStream);
  }
  memoryStream.Position = 0;
  
  SelectedFile = null; // Borrar referencia para prevenir reutilización
  StateHasChanged();
  ```

**Demandas de Autenticación**

- Asegurar que el flujo de Login incluya TODAS las demandas requeridas: NameIdentifier, Name, Email, Role, Department
- La demanda Department es requerida para autorización basada en equipos en uso compartido de documentos
- Las demandas faltantes causarán fallas de autorización en métodos de DocumentService

### Requisitos de Configuración de Base de Datos

**Estado Limpio para Pruebas:**

- Antes de probar la carga de documentos por primera vez, asegurar estado de base de datos limpio
- Si intentos de carga previos fallaron, soltar y recrear la base de datos para eliminar registros huérfanos:
  ```powershell
  sqllocaldb stop mssqllocaldb
  sqllocaldb delete mssqllocaldb
  # La base de datos será recreada automáticamente en la próxima ejecución
  ```
- Los registros huérfanos con valores FilePath vacíos causarán violaciones de clave duplicada
- Para LocalDB: `dotnet ef database drop --force` también funciona si las herramientas EF están instaladas

## Suposiciones

- El entorno de capacitación tiene almacenamiento en disco local disponible
- La mayoría de documentos serán menores a 10 MB en tamaño
- Los usuarios están familiarizados con conceptos básicos de gestión de archivos
- El almacenamiento de sistema de archivos local es aceptable para fines de capacitación
- La migración a la nube a Azure Blob Storage está planeada para implementación de producción
- Los usuarios pueden trabajar offline (no se requiere conexión a internet para funcionalidad principal)

## Fuera del Alcance

Las siguientes características NO están incluidas en esta versión inicial:

- Edición colaborativa en tiempo real de documentos
- Capabilidades de historial de versiones y reversión
- Flujos de trabajo avanzados de documentos (procesos de aprobación, enrutamiento de documentos)
- Integración con sistemas externos (SharePoint, OneDrive)
- Soporte para aplicación móvil (la versión inicial es solo web)
- Características de plantillas de documentos o generación de documentos
- Cuotas de almacenamiento y gestión de cuotas
- Funcionalidad de eliminación suave/papelera con recuperación

Estas pueden considerarse para mejoras futuras basadas en retroalimentación de usuarios y necesidades comerciales.

## Próximos Pasos

Una vez aprobados, estos requisitos se utilizarán para crear especificaciones detalladas utilizando la metodología de Desarrollo Impulsado por Especificaciones con GitHub Spec Kit.

