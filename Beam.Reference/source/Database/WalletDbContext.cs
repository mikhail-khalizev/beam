using System;
using System.IO;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Beam.Database
{
    public partial class WalletDbContext : DbContext
    {
        public WalletDbContext()
        {
        }

        public WalletDbContext(DbContextOptions<WalletDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Asset> Assets { get; set; }
        public virtual DbSet<AssetsEvent> AssetsEvents { get; set; }
        public virtual DbSet<Bb> Bbs { get; set; }
        public virtual DbSet<Dummy> Dummies { get; set; }
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<Kernel> Kernels { get; set; }
        public virtual DbSet<Param> Params { get; set; }
        public virtual DbSet<Peer> Peers { get; set; }
        public virtual DbSet<ShieldedStatistic> ShieldedStatistics { get; set; }
        public virtual DbSet<State> States { get; set; }
        public virtual DbSet<Stream> Streams { get; set; }
        public virtual DbSet<Tip> Tips { get; set; }
        public virtual DbSet<TipsReachable> TipsReachables { get; set; }
        public virtual DbSet<Txo> Txos { get; set; }
        public virtual DbSet<UniqueStorage> UniqueStorages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.

                var path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Beam Wallet",
                    "node.db");

                optionsBuilder.UseSqlite($"Filename={path}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.HasIndex(e => e.Owner, "IdxAssetsOwn");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("ID");
            });

            modelBuilder.Entity<AssetsEvent>(entity =>
            {
                entity.HasNoKey();

                entity.HasIndex(e => new { e.Id, e.Height, e.Seq }, "IdxAssetsEvents_1");

                entity.HasIndex(e => new { e.Height, e.Seq }, "IdxAssetsEvents_2");

                entity.Property(e => e.Id).HasColumnName("ID");
            });

            modelBuilder.Entity<Bb>(entity =>
            {
                entity.HasIndex(e => new { e.Channel, e.Id }, "IdxBbsCSeq");

                entity.HasIndex(e => e.Key, "IdxBbsKey");

                entity.HasIndex(e => new { e.Time, e.Id }, "IdxBbsTSeq");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Key).IsRequired();

                entity.Property(e => e.Message).IsRequired();
            });

            modelBuilder.Entity<Dummy>(entity =>
            {
                entity.HasIndex(e => e.SpendHeight, "IdxDummiesH");

                entity.Property(e => e.Id).HasColumnName("ID");
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasNoKey();

                entity.HasIndex(e => new { e.Height, e.Body }, "IdxEvents");

                entity.HasIndex(e => e.Key, "IdxEventsKey");

                entity.Property(e => e.Body).IsRequired();

                entity.Property(e => e.Key).IsRequired();
            });

            modelBuilder.Entity<Kernel>(entity =>
            {
                entity.HasNoKey();

                entity.HasIndex(e => new { e.Commitment, e.Height }, "IdxKernels");

                entity.Property(e => e.Commitment).IsRequired();
            });

            modelBuilder.Entity<Param>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("ID");
            });

            modelBuilder.Entity<Peer>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.Key).IsRequired();
            });

            modelBuilder.Entity<ShieldedStatistic>(entity =>
            {
                entity.HasKey(e => e.Height);

                entity.ToTable("ShieldedStatistic");

                entity.Property(e => e.Height).ValueGeneratedNever();
            });

            modelBuilder.Entity<State>(entity =>
            {
                entity.HasKey(e => new { e.Height, e.Hash });

                entity.HasIndex(e => e.Txos, "IdxStatesTxos");

                entity.HasIndex(e => e.ChainWork, "IdxStatesWrk");

                entity.Property(e => e.ChainWork).IsRequired();

                entity.Property(e => e.Definition).IsRequired();

                entity.Property(e => e.HashPrev).IsRequired();

                entity.Property(e => e.Kernels).IsRequired();
            });

            modelBuilder.Entity<Stream>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("ID");

                entity.Property(e => e.Value).IsRequired();
            });

            modelBuilder.Entity<Tip>(entity =>
            {
                entity.HasKey(e => new { e.Height, e.State });
            });

            modelBuilder.Entity<TipsReachable>(entity =>
            {
                entity.HasKey(e => e.State);

                entity.ToTable("TipsReachable");

                entity.HasIndex(e => e.ChainWork, "IdxTipsReachableWrk");

                entity.Property(e => e.State).ValueGeneratedNever();

                entity.Property(e => e.ChainWork).IsRequired();
            });

            modelBuilder.Entity<Txo>(entity =>
            {
                entity.ToTable("Txo");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("ID");

                entity.Property(e => e.Value).IsRequired();
            });

            modelBuilder.Entity<UniqueStorage>(entity =>
            {
                entity.HasKey(e => e.Key);

                entity.ToTable("UniqueStorage");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
