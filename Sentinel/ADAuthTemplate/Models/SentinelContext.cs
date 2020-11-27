using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Sentinel.Models
{
    public partial class SentinelContext : DbContext
    {
        public SentinelContext()
        {
        }

        public SentinelContext(DbContextOptions<SentinelContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ErrorLog> ErrorLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
//                optionsBuilder.UseSqlServer("Data Source=.\\SQLEXPRESS;Initial Catalog=Sentinel;Trusted_connection=true");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ErrorLog>(entity =>
            {
                entity.ToTable("ErrorLog");

                entity.Property(e => e.Application).HasMaxLength(100);

                entity.Property(e => e.ClientIp)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("ClientIP");

                entity.Property(e => e.Device).HasMaxLength(100);

                entity.Property(e => e.Message).IsRequired();

                entity.Property(e => e.Os)
                    .HasMaxLength(100)
                    .HasColumnName("OS");

                entity.Property(e => e.Osversion)
                    .HasMaxLength(20)
                    .HasColumnName("OSVersion");

                entity.Property(e => e.Source)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Timestamp).HasColumnType("datetime");

                entity.Property(e => e.UserAgent).HasMaxLength(100);

                entity.Property(e => e.UserAgentVersion).HasMaxLength(20);

                entity.Property(e => e.VueInfo).HasMaxLength(250);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
