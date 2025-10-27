# WebApiPrueba

API REST desarrollada en **.NET 8** con **ASP.NET Core Web API** para la gestión de empleados, departamentos y autenticación mediante **JWT (JSON Web Tokens)**.  
El proyecto utiliza **Entity Framework Core** para la conexión con **SQL Server** y expone endpoints CRUD y reportes a través de **Swagger**.

---

## 🧱 Arquitectura

- **Framework:** ASP.NET Core Web API (.NET 8)  
- **ORM:** Entity Framework Core  
- **Base de datos:** SQL Server 
- **Autenticación:** JWT Bearer con clave simétrica  

### Estructura principal del proyecto
- **Controllers/** → Controladores de la API (Autenticación, Departamentos, Empleados, Reportes).  
- **Data/** → Contexto de base de datos (`EmpresaDbContext`).  
- **Models/Entities/** → Entidades del dominio.  
- **Models/Dtos/** → Objetos de transferencia de datos (DTOs).  

---

## 🔐 Decisión de autenticación
Se implementa **autenticación JWT Bearer** para proteger los endpoints.  
El token se genera en el login y se incluye en cada solicitud posterior mediante el encabezado:

