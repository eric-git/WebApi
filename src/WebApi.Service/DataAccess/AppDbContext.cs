using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebApi.Service.DataAccess.Entity;

namespace WebApi.Service.DataAccess;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<Relation> Relations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("game_pkey");

            entity.ToTable("game", "core");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(500)
                .HasColumnName("name");
            entity.Property(e => e.PlayerHealth).HasColumnName("player_health");
            entity.Property(e => e.PlayerName)
                .HasMaxLength(255)
                .HasColumnName("player_name");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
        });

        modelBuilder.Entity<Relation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("relation_pkey");

            entity.ToTable("relation", "core");

            entity.HasIndex(e => e.GameId, "idx_relation_game_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Attributes)
                .HasColumnType("jsonb")
                .HasColumnName("attributes");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.Name)
                .HasMaxLength(500)
                .HasColumnName("name");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");

            entity.HasOne(d => d.Game).WithMany(p => p.Relations)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("relation_game_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
