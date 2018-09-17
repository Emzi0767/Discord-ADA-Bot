// This file is part of ADA project
//
// Copyright 2018 Emzi0767
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Emzi0767.Ada.Data;
using Microsoft.EntityFrameworkCore;

namespace Emzi0767.Ada.Services
{
    /// <summary>
    /// Connection context service for ADA's database.
    /// </summary>
    public partial class DatabaseContext : DbContext
    {
        /// <summary>
        /// Gets or sets metadata for this database.
        /// </summary>
        public virtual DbSet<DatabaseMetadata> Metadata { get; set; }

        /// <summary>
        /// Gets or sets configured per-guild prefixes.
        /// </summary>
        public virtual DbSet<DatabasePrefix> Prefixes { get; set; }

        /// <summary>
        /// Gets or sets blocked entities.
        /// </summary>
        public virtual DbSet<DatabaseBlockedEntity> BlockedEntities { get; set; }

        /// <summary>
        /// Gets or sets defined tags.
        /// </summary>
        public virtual DbSet<DatabaseTag> Tags { get; set; }

        /// <summary>
        /// Gets or sets defined tag revisions.
        /// </summary>
        public virtual DbSet<DatabaseTagRevision> TagRevisions { get; set; }

        /// <summary>
        /// Gets or sets per-guild bot configurations.
        /// </summary>
        public virtual DbSet<DatabaseGuildSettings> GuildSettings { get; set; }

        /// <summary>
        /// Gets or sets logs of major actions taken by moderators.
        /// </summary>
        public virtual DbSet<DatabaseModerationLog> ModerationLogs { get; set; }

        /// <summary>
        /// Gets or sets moderator-defined notes for users.
        /// </summary>
        public virtual DbSet<DatabaseModeratorNote> ModeratorNotes { get; set; }

        /// <summary>
        /// Gets or sets moderator-issued user warnings.
        /// </summary>
        public virtual DbSet<DatabaseModeratorWarning> ModeratorWarnings { get; set; }

        /// <summary>
        /// Gets or sets saved rolestates.
        /// </summary>
        public virtual DbSet<DatabaseRoleState> RoleStates { get; set; }

        /// <summary>
        /// Gets or sets defined RSS feeds.
        /// </summary>
        public virtual DbSet<DatabaseRssFeed> RssFeeds { get; set; }

        /// <summary>
        /// Gets or sets actions scheduled by asynchronous task scheduler.
        /// </summary>
        public virtual DbSet<DatabaseSchedulerData> SchedulerActions { get; set; }

        private ConnectionStringProvider ConnectionStringProvider { get; }

        /// <summary>
        /// Creates a new database context with specified connection string provider.
        /// </summary>
        /// <param name="csp">Connection string provider to use when connecting to PostgreSQL.</param>
        public DatabaseContext(ConnectionStringProvider csp)
        {
            this.ConnectionStringProvider = csp;
        }

        /// <summary>
        /// Creates a new database context with specified context options and connection string provider.
        /// </summary>
        /// <param name="options">Database context options.</param>
        /// <param name="csp">Connection string provider to use when connecting to PostgreSQL.</param>
        public DatabaseContext(DbContextOptions<DatabaseContext> options, ConnectionStringProvider csp)
            : base(options)
        {
            this.ConnectionStringProvider = csp;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseNpgsql(this.ConnectionStringProvider.GetConnectionString());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ForNpgsqlHasEnum(null, "entity_kind", new[] { "user", "channel", "guild" })
                .ForNpgsqlHasEnum(null, "tag_kind", new[] { "channel", "guild", "global" })
                .ForNpgsqlHasEnum(null, "moderator_action_kind", new[] { "kick", "softban", "mute", "unmute", "ban", "unban", "prune" })
                .ForNpgsqlHasEnum(null, "rss_feed_kind", new[] { "rss", "atom" })
                .HasPostgresExtension("fuzzystrmatch");

            modelBuilder.Entity<DatabaseMetadata>(entity =>
            {
                entity.Property(e => e.MetaKey).ValueGeneratedNever();
            });

            modelBuilder.Entity<DatabasePrefix>(entity =>
            {
                entity.HasIndex(e => e.GuildId)
                    .HasName("cc_prefixes_guild_id_key")
                    .IsUnique();

                entity.Property(e => e.GuildId).ValueGeneratedNever();

                entity.Property(e => e.EnableDefault).HasDefaultValueSql("true");
            });

            modelBuilder.Entity<DatabaseBlockedEntity>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.Kind });
            });

            modelBuilder.Entity<DatabaseTag>(entity =>
            {
                entity.HasKey(e => new { e.Kind, e.ContainerId, e.Name });
            });

            modelBuilder.Entity<DatabaseTagRevision>(entity =>
            {
                entity.HasKey(e => new { e.Name, e.CreatedAt });

                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.Revisions)
                    .HasForeignKey(d => new { d.Kind, d.ContainerId, d.Name })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("tag_revisions_container_id_fkey");
            });

            modelBuilder.Entity<DatabaseGuildSettings>(entity =>
            {
                entity.Property(e => e.GuildId).ValueGeneratedNever();
            });

            modelBuilder.Entity<DatabaseModerationLog>(entity =>
            {
                entity.HasKey(e => new { e.GuildId, e.ActionId });

                entity.Property(e => e.Reason).HasDefaultValueSql("NULL::character varying");
            });

            modelBuilder.Entity<DatabaseModeratorNote>(entity =>
            {
                entity.HasKey(e => new { e.GuildId, e.UserId });
            });

            modelBuilder.Entity<DatabaseModeratorWarning>(entity =>
            {
                entity.HasKey(e => new { e.GuildId, e.UserId, e.WarningId });
            });

            modelBuilder.Entity<DatabaseRoleState>(entity =>
            {
                entity.HasKey(e => new { e.GuildId, e.UserId });
            });

            modelBuilder.Entity<DatabaseRssFeed>(entity =>
            {
                entity.HasKey(e => new { e.FeedId, e.GuildId });

                entity.HasIndex(e => new { e.ChannelId, e.FeedUrl })
                    .HasName("rss_feeds_channel_id_feed_url_key")
                    .IsUnique();

                entity.Property(e => e.ItemTag).HasDefaultValueSql("''::text");
            });

            modelBuilder.Entity<DatabaseSchedulerData>(entity =>
            {
                entity.HasKey(e => new { e.ActionId, e.UserId });

                entity.Property(e => e.ActionId).ValueGeneratedOnAdd();
            });
        }
    }
}
