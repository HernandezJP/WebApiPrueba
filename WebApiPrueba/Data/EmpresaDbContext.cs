using Microsoft.EntityFrameworkCore;
using WebApiPrueba.Models.Entities;
namespace WebApiPrueba.Data
{
    public class EmpresaDbContext: DbContext
    {
        public EmpresaDbContext(DbContextOptions<EmpresaDbContext> options) : base(options)
        {
        }
        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Departamento
            modelBuilder.Entity<Departamento>(d =>
            {
                d.ToTable("departamento");
                d.HasKey(x => x.IdDepartamento);
                d.Property(x => x.IdDepartamento).HasColumnName("id_departamento"); 
                d.Property(x => x.NombreDepto).HasColumnName("nombre_depto").HasMaxLength(75).IsRequired();
                d.HasIndex(x => x.NombreDepto).IsUnique();
                d.Property(x => x.Presupuesto).HasColumnName("presupuesto").HasColumnType("decimal(18,2)").IsRequired();
            });

            modelBuilder.Entity<Empleado>(e =>
            {
                e.ToTable("empleado");
                e.HasKey(x => x.IdEmpleado);                                      
                e.Property(x => x.IdEmpleado).HasColumnName("id_empleado").ValueGeneratedOnAdd();
                e.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
                e.Property(x => x.Apellidos).HasColumnName("apellidos").HasMaxLength(125).IsRequired();
                e.Property(x => x.DocumentoCui).HasColumnName("documento_cui").HasMaxLength(22).IsRequired();
                e.Property(x => x.FechaIngreso).HasColumnName("fecha_ingreso").IsRequired();
                e.Property(x => x.SalarioActual).HasColumnName("salario_actual").HasColumnType("decimal(18,2)").IsRequired();
                e.Property(x => x.FechaUltimoAumento).HasColumnName("fecha_ultimo_aumento");
                e.Property(x => x.Puesto).HasColumnName("puesto").HasMaxLength(100);
                e.Property(x => x.Jerarquia).HasColumnName("jerarquia").HasMaxLength(100);
                e.Property(x => x.DepartamentoId).HasColumnName("departamento_id").IsRequired();

                e.HasOne(x => x.Departamento)
                 .WithMany(d => d.Empleados)
                 .HasForeignKey(x => x.DepartamentoId)
                 .HasConstraintName("FK_empleado_departamento");
            });

            modelBuilder.Entity<Rol>(r =>
            {
                 r.ToTable("rol");
                 r.HasKey(x => x.IdRol);
                 r.Property(x => x.IdRol).HasColumnName("id_rol").ValueGeneratedOnAdd();
                 r.Property(x => x.NombreRol).HasColumnName("nombre_rol").HasMaxLength(75).IsRequired();
                 r.HasIndex(x => x.NombreRol).IsUnique();
            });

            modelBuilder.Entity<Usuario>(u =>
            {
                u.ToTable("usuario");
                u.HasKey(x => x.IdUsuario);
                u.Property(x => x.IdUsuario).HasColumnName("id_usuario").ValueGeneratedOnAdd();
                u.Property(x => x.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
                u.HasIndex(x => x.Email).IsUnique();
                u.Property(x => x.Contrasena).HasColumnName("contrasena").HasMaxLength(100).IsRequired();
                u.Property(x => x.IdEmpleado).HasColumnName("id_empleado").IsRequired();
                u.Property(x => x.IdRol).HasColumnName("id_rol").IsRequired();

                u.HasOne(x => x.Empleado)
                .WithMany(e => e.Usuarios)
                .HasForeignKey(x => x.IdEmpleado)
                .HasConstraintName("FK_usuario_empleado");

                u.HasOne(x => x.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(x => x.IdRol)
                .HasConstraintName("FK_usuario_rol");
            });


        }
    }
}
