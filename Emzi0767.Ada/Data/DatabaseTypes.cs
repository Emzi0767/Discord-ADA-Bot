﻿// This file is part of ADA project
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

using NpgsqlTypes;

namespace Emzi0767.Ada.Data
{
    /// <summary>
    /// Represents kind of an entity with associated ID.
    /// </summary>
    public enum DatabaseEntityKind
    {
        /// <summary>
        /// Defines that the entity is a user.
        /// </summary>
        [PgName("user")]
        User,

        /// <summary>
        /// Defines that the entity is a channel.
        /// </summary>
        [PgName("channel")]
        Channel,

        /// <summary>
        /// Defines that the entity is a guild.
        /// </summary>
        [PgName("guild")]
        Guild
    }

    /// <summary>
    /// Represents kind of a tag in the database.
    /// </summary>
    public enum DatabaseTagKind
    {
        /// <summary>
        /// Defines that the tag is bound to a channel.
        /// </summary>
        [PgName("channel")]
        Channel,

        /// <summary>
        /// Defines that the tag is bound to a guild.
        /// </summary>
        [PgName("guild")]
        Guild,

        /// <summary>
        /// Defines that the tag is not bound, and it will appear and be usable everywhere.
        /// </summary>
        [PgName("global")]
        Global
    }

    /// <summary>
    /// Represents kind of a moderator action in the database.
    /// </summary>
    public enum DatabaseModeratorActionKind
    {
        /// <summary>
        /// Defines that the action was a kick.
        /// </summary>
        [PgName("kick")]
        Kick,

        /// <summary>
        /// Defines that the action was a softban (a ban quickly followed by unban).
        /// </summary>
        [PgName("softban")]
        Softban,

        /// <summary>
        /// Defines that the action was a mute.
        /// </summary>
        [PgName("mute")]
        Mute,

        /// <summary>
        /// Defines that the action was an unmnute.
        /// </summary>
        [PgName("unmute")]
        Unmute,

        /// <summary>
        /// Defines that the action was a ban.
        /// </summary>
        [PgName("ban")]
        Ban,

        /// <summary>
        /// Defines that the action was an unban.
        /// </summary>
        [PgName("unban")]
        Unban,

        /// <summary>
        /// Defines that the action was a prune (kicking all inactive members).
        /// </summary>
        [PgName("prune")]
        Prune
    }

    /// <summary>
    /// Represents kind of RSS feed, which determines which RSS parser to use.
    /// </summary>
    public enum DatabaseRssFeedKind
    {
        /// <summary>
        /// Defines that the feed is an RSS feed.
        /// </summary>
        [PgName("rss")]
        RSS,

        /// <summary>
        /// Defines that the feed is an Atom feed.
        /// </summary>
        [PgName("atom")]
        Atom
    }
}
