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
        public virtual DbSet<Source> Sources { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
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

                entity.Property(e => e.Source).IsRequired();

                entity.Property(e => e.Timestamp).HasColumnType("datetime");

                entity.Property(e => e.UserAgent).HasMaxLength(100);

                entity.Property(e => e.UserAgentVersion).HasMaxLength(20);

                entity.Property(e => e.VueInfo).HasMaxLength(250);
            });

            modelBuilder.Entity<Source>(entity =>
            {
                entity.ToTable("Source");

                entity.Property(e => e.Application)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Filename)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
