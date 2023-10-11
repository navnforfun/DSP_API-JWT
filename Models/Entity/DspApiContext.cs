﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DSP_API.Models.Entity;

public partial class DspApiContext : DbContext
{
    public DspApiContext()
    {
    }

    public DspApiContext(DbContextOptions<DspApiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Box> Boxs { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<File> Files { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vote> Votes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=DSP_API;User Id=sa;Password=123456;Trust Server Certificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Box>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Boxs_UserId");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Img).HasMaxLength(255);
            entity.Property(e => e.IsAvailable)
                .IsRequired()
                .HasDefaultValueSql("(CONVERT([bit],(0)))");
            entity.Property(e => e.Pass).HasMaxLength(255);
            entity.Property(e => e.ShareCode).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.Url).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.Boxes)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Boxs_Accounts_UserId");

            entity.HasMany(d => d.Users).WithMany(p => p.BoxesNavigation)
                .UsingEntity<Dictionary<string, object>>(
                    "BoxShare",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("sharebox"),
                    l => l.HasOne<Box>().WithMany()
                        .HasForeignKey("BoxId")
                        .HasConstraintName("boxshare"),
                    j =>
                    {
                        j.HasKey("BoxId", "UserId").HasName("PK__BoxShare__60252D3E54EEA8C4");
                        j.ToTable("BoxShares");
                        j.HasIndex(new[] { "BoxId" }, "IX_BoxShares_BoxId");
                    });
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("Comment");

            entity.HasIndex(e => e.BoxId, "IX_Comment_BoxId");

            entity.HasOne(d => d.Box).WithMany(p => p.Comments).HasForeignKey(d => d.BoxId);

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("usercommer");
        });

        modelBuilder.Entity<File>(entity =>
        {
            entity.HasIndex(e => e.BoxId, "IX_Files_BoxId");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Size).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Box).WithMany(p => p.Files).HasForeignKey(d => d.BoxId);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasMany(d => d.Users).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("roleuser"),
                    l => l.HasOne<Role>().WithMany().HasForeignKey("RoleId"),
                    j =>
                    {
                        j.HasKey("RoleId", "UserId").HasName("PK__UserRole__F9B31440DEF2BBCD");
                        j.ToTable("UserRoles");
                    });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Accounts");

            entity.HasIndex(e => e.Username, "username").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Img).HasMaxLength(255);
            entity.Property(e => e.JobTitle).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(255);
        });

        modelBuilder.Entity<Vote>(entity =>
        {
            entity.HasIndex(e => e.BoxId, "IX_Votes_BoxId");

            entity.HasOne(d => d.Box).WithMany(p => p.Votes).HasForeignKey(d => d.BoxId);

            entity.HasOne(d => d.User).WithMany(p => p.Votes)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("uservote");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
