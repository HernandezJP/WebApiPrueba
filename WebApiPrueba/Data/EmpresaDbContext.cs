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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Departamento
            modelBuilder.Entity<Departamento>(e =>
            {
                e.ToTable("departamento");
                e.HasKey(x => x.IdDepartamento);
                e.Property(x => x.IdDepartamento).HasColumnName("id_departamento"); 
                e.Property(x => x.NombreDepto).HasColumnName("nombre_depto").HasMaxLength(75).IsRequired();
                e.HasIndex(x => x.NombreDepto).IsUnique();
                e.Property(x => x.Presupuesto).HasColumnName("presupuesto").HasColumnType("decimal(18,2)").IsRequired();
            });

            modelBuilder.Entity<Empleado>(e =>
            {
                e.ToTable("empleado");
                e.HasKey(x => x.IdEmpleado);                                      
                e.Property(x => x.IdEmpleado).HasColumnName("id_empleado");
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

        }
    }
}
