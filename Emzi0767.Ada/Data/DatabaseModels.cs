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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;

namespace Emzi0767.Ada.Data
{
    /// <summary>
    /// Represents a table metadata property.
    /// </summary>
    [Table("metadata")]
    public partial class DatabaseMetadata
    {
        /// <summary>
        /// Gets or sets the name of the metadata property.
        /// </summary>
        [Key]
        [Column("meta_key")]
        public string MetaKey { get; set; }

        /// <summary>
        /// Gets or sets the value of the metadata property.
        /// </summary>
        [Required]
        [Column("meta_value")]
        public string MetaValue { get; set; }
    }

    /// <summary>
    /// Represents command prefix configuration for various guilds.
    /// </summary>
    [Table("prefixes")]
    public partial class DatabasePrefix
    {
        /// <summary>
        /// Gets or sets the guild ID for these prefixes.
        /// </summary>
        [Key]
        [Column("guild_id")]
        public long GuildId { get; set; }

        /// <summary>
        /// Gets or sets the prefixes in use for this guild.
        /// </summary>
        [Required]
        [Column("prefixes")]
        public string[] Prefixes { get; set; }

        /// <summary>
        /// Gets or sets whether the default prefixes should remain active in the guild.
        /// </summary>
        [Required]
        [Column("enable_default")]
        public bool? EnableDefault { get; set; }
    }

    /// <summary>
    /// Represents a collection of entities blocked from using the bot.
    /// </summary>
    [Table("blocked_entities")]
    public partial class DatabaseBlockedEntity
    {
        /// <summary>
        /// Gets or sets the entity's ID.
        /// </summary>
        [Column("id")]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the entity's kind.
        /// </summary>
        [Column("kind")]
        public DatabaseEntityKind Kind { get; set; }

        /// <summary>
        /// Gets or sets the reason why the entity was blocked.
        /// </summary>
        [Column("reason")]
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets when the entity was blocked.
        /// </summary>
        [Column("since", TypeName = "timestamp with time zone")]
        public DateTime Since { get; set; }
    }

    /// <summary>
    /// Represents a user-created tag.
    /// </summary>
    [Table("tags")]
    public partial class DatabaseTag
    {
        public DatabaseTag()
        {
            this.Revisions = new HashSet<DatabaseTagRevision>();
        }

        /// <summary>
        /// Gets or sets the kind of this tag.
        /// </summary>
        [Column("kind")]
        public DatabaseTagKind Kind { get; set; }

        /// <summary>
        /// Gets or sets the id of this container to which this tag is bound.
        /// </summary>
        [Column("container_id")]
        public long ContainerId { get; set; }

        /// <summary>
        /// Gets or sets the name of this tag.
        /// </summary>
        [Column("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the id of this tag's owner.
        /// </summary>
        [Column("owner_id")]
        public long OwnerId { get; set; }

        /// <summary>
        /// Gets or sets whether this tag is hidden.
        /// </summary>
        [Required]
        [Column("hidden")]
        public bool IsHidden { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the latest revision of the tag.
        /// </summary>
        [Column("latest_revision", TypeName = "timestamp with time zone")]
        public DateTime LatestRevision { get; set; }

        /// <summary>
        /// Gets or sets the revisions for this tag.
        /// </summary>
        [InverseProperty("Tag")]
        public ICollection<DatabaseTagRevision> Revisions { get; set; }
    }

    /// <summary>
    /// Represents a revision of a user-created tag.
    /// </summary>
    [Table("tag_revisions")]
    public partial class DatabaseTagRevision
    {
        /// <summary>
        /// Gets or sets the kind of related tag.
        /// </summary>
        [Column("kind")]
        public DatabaseTagKind Kind { get; set; }

        /// <summary>
        /// Gets or sets the id of this container to which related tag is bound.
        /// </summary>
        [Column("container_id")]
        public long ContainerId { get; set; }

        /// <summary>
        /// Gets or sets the name of related tag.
        /// </summary>
        [Column("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the contents of this revision.
        /// </summary>
        [Required]
        [Column("contents")]
        public string Contents { get; set; }

        /// <summary>
        /// Gets or sets the creation time of this revision.
        /// </summary>
        [Column("created_at", TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the id of this revision's author.
        /// </summary>
        [Column("user_id")]
        public long UserId { get; set; }

        /// <summary>
        /// Gets or sets the tag associated with this revision.
        /// </summary>
        [ForeignKey("Kind,ContainerId,Name")]
        [InverseProperty("Revisions")]
        public DatabaseTag Tag { get; set; }
    }

    /// <summary>
    /// Represents moderation configuration for a guild.
    /// </summary>
    [Table("guild_settings")]
    public partial class DatabaseGuildSettings
    {
        /// <summary>
        /// Gets or sets the guild to which these settings apply.
        /// </summary>
        [Key]
        [Column("guild_id")]
        public long GuildId { get; set; }

        /// <summary>
        /// Gets or sets the raw settings json.
        /// </summary>
        [Required]
        [Column("settings", TypeName = "json")]
        public string SettingsJson
        {
            get => JsonConvert.SerializeObject(this.Settings);
            set => this.Settings = JsonConvert.DeserializeObject<GuildConfiguration>(value);
        }
        
        /// <summary>
        /// Gets or sets the settings for this guild.
        /// </summary>
        [NotMapped]
        public GuildConfiguration Settings { get; set; }
    }

    /// <summary>
    /// Represents a logged action taken by a moderator.
    /// </summary>
    [Table("moderation_logs")]
    public partial class DatabaseModerationLog
    {
        /// <summary>
        /// Gets or sets the guild in which the action was taken.
        /// </summary>
        [Column("guild_id")]
        public long GuildId { get; set; }

        /// <summary>
        /// Gets or sets the incremental per-guild ID of the action.
        /// </summary>
        [Column("action_id")]
        public int ActionId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the action's occurence.
        /// </summary>
        [Column("logged_at", TypeName = "timestamp with time zone")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the kind of the action.
        /// </summary>
        [Column("kind")]
        public DatabaseModeratorActionKind Kind { get; set; }

        /// <summary>
        /// Gets or sets the ID of this action's target.
        /// </summary>
        [Column("target")]
        public long? Target { get; set; }

        /// <summary>
        /// Gets or sets the ID of the action's initiator.
        /// </summary>
        [Column("moderator")]
        public long? Moderator { get; set; }

        /// <summary>
        /// Gets or sets the reason for this action.
        /// </summary>
        [Column("reason")]
        [StringLength(1000)]
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets the effective end of this action. This is used for temporary actions, such as muting a user for certain amount of time.
        /// </summary>
        [Column("until", TypeName = "timestamp with time zone")]
        public DateTime? Until { get; set; }
    }

    /// <summary>
    /// Represents notes attached to a user by a moderator.
    /// </summary>
    [Table("moderator_notes")]
    public partial class DatabaseModeratorNote
    {
        /// <summary>
        /// Gets or sets the ID of the guild in which this note applies.
        /// </summary>
        [Column("guild_id")]
        public long GuildId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user to which this note applies.
        /// </summary>
        [Column("user_id")]
        public long UserId { get; set; }

        /// <summary>
        /// Gets or sets the contents of the note.
        /// </summary>
        [Column("contents")]
        public string Contents { get; set; }
    }

    /// <summary>
    /// Represents a warning issued to a user by a moderator.
    /// </summary>
    [Table("moderator_warnings")]
    public partial class DatabaseModeratorWarning
    {
        /// <summary>
        /// Gets or sets the ID of the guild in which this warning applies.
        /// </summary>
        [Column("guild_id")]
        public long GuildId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user to which this warning applies.
        /// </summary>
        [Column("user_id")]
        public long UserId { get; set; }

        /// <summary>
        /// Gets or sets the incremental ID of the warning for the user.
        /// </summary>
        [Column("warning_id")]
        public int WarningId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the moderator that issued the warning.
        /// </summary>
        [Column("issuer_id")]
        public long IssuerId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the warning being issued.
        /// </summary>
        [Column("issued_at", TypeName = "timestamp with time zone")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the warning's contents.
        /// </summary>
        [Required]
        [Column("contents")]
        public string Contents { get; set; }
    }

    /// <summary>
    /// Represents a user's rolestate within a guild. This is used to persist roles across rejoins.
    /// </summary>
    [Table("rolestate")]
    public partial class DatabaseRoleState
    {
        /// <summary>
        /// Gets or sets the ID of the guild for which this rolestate applies.
        /// </summary>
        [Column("guild_id")]
        public long GuildId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user to which this rolestate applies.
        /// </summary>
        [Column("user_id")]
        public long UserId { get; set; }

        /// <summary>
        /// Gets or sets the array of role IDs saved for this user.
        /// </summary>
        [Required]
        [Column("role_ids")]
        public long[] RoleIds { get; set; }
    }

    /// <summary>
    /// Represents information about an RSS feed.
    /// </summary>
    [Table("rss_feeds")]
    public partial class DatabaseRssFeed
    {
        /// <summary>
        /// Gets or sets the incremental ID of this RSS feed.
        /// </summary>
        [Column("feed_id")]
        public int FeedId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the guild in which this feed is defined.
        /// </summary>
        [Column("guild_id")]
        public long GuildId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the channel to which this feed is attached.
        /// </summary>
        [Column("channel_id")]
        public long ChannelId { get; set; }

        /// <summary>
        /// Gets or sets the URL of the RSS feed.
        /// </summary>
        [Required]
        [Column("feed_url")]
        public string FeedUrl { get; set; }

        /// <summary>
        /// Gets or sets the kind of this feed. This determines which RSS parser to use.
        /// </summary>
        [Column("kind")]
        public DatabaseRssFeedKind Kind { get; set; }

        /// <summary>
        /// Gets or sets the optional tag for all items posted from this feed. This can be used to distinguish items from various feeds.
        /// </summary>
        [Required]
        [Column("item_tag")]
        public string ItemTag { get; set; }

        /// <summary>
        /// Gets or sets whether the feed is initialized. If the feed is not initialized, all items from said feed are to be skipped.
        /// </summary>
        [Column("initalized")]
        public bool Initalized { get; set; }

        /// <summary>
        /// Gets or sets the array of already-processed items from this feed. This is used to prevent item duplication.
        /// </summary>
        [Required]
        [Column("item_cache")]
        public string[] ItemCacheInternal
        {
            get => this.ItemCache.ToArray();
            set => this.ItemCache = new HashSet<string>(value);
        }

        [NotMapped]
        public HashSet<string> ItemCache { get; set; }
    }

    /// <summary>
    /// Represents an action scheduled by the asynchronous task scheduler.
    /// </summary>
    [Table("scheduler_data")]
    public partial class DatabaseSchedulerData
    {
        /// <summary>
        /// Gets or sets the incremental ID of this action.
        /// </summary>
        [Column("action_id")]
        public int ActionId { get; set; }

        /// <summary>
        /// Gets or sets the user who scheduled the action.
        /// </summary>
        [Column("user_id")]
        public long UserId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the guild in which this action was scheduled.
        /// </summary>
        [Column("guild_id")]
        public long GuildId { get; set; }

        /// <summary>
        /// Gets or sets the date and time at which the action is to be dispatched.
        /// </summary>
        [Column("dispatch_at", TypeName = "timestamp with time zone")]
        public DateTime DispatchAt { get; set; }

        /// <summary>
        /// Gets or sets the type handling the dispatch.
        /// </summary>
        [Required]
        [Column("dispatch_handler_type")]
        public string DispatchHandlerType { get; set; }

        /// <summary>
        /// Gets or sets the JSON containing additional data for the dispatch.
        /// </summary>
        [Column("action_data", TypeName = "json")]
        public string ActionData { get; set; }
    }
}
