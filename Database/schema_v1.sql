-- This file is a part of ADA project.
-- 
-- Copyright 2018 Emzi0767
-- 
-- Licensed under the Apache License, Version 2.0 (the "License");
-- you may not use this file except in compliance with the License.
-- You may obtain a copy of the License at
-- 
--   http://www.apache.org/licenses/LICENSE-2.0
-- 
-- Unless required by applicable law or agreed to in writing, software
-- distributed under the License is distributed on an "AS IS" BASIS,
-- WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
-- See the License for the specific language governing permissions and
-- limitations under the License.
-- 
-- ----------------------------------------------------------------------------
-- 
-- ADA's PostgreSQL database schema
-- 
-- Version:                  1
-- Bot version:              v5.0.0
-- Timestamp:                2018-09-16 14:56 +02:00
-- Author:                   Emzi0767
-- Project:                  ADA
-- License:                  Apache License 2.0
-- PostgreSQL version:       9.6 or above
-- Requires:                 fuzzystrmatch
-- 
-- ----------------------------------------------------------------------------
-- 
-- Extensions

-- fuzzystrmatch
-- PostgreSQL extension providing various fuzzy string matching functionality.
create extension fuzzystrmatch;

-- ----------------------------------------------------------------------------
-- 
-- Types

-- entity_kind
-- Determines entity type of the attached ID.
create type entity_kind as enum('user', 'channel', 'guild');

-- tag_kind
-- Determines the kind of tag, whether it's a channel-specific tag, a 
-- guild-specific tag, or a global tag.
create type tag_kind as enum('channel', 'guild', 'global');

-- moderator_action_kind
-- Determines the kind of a logged moderation action.
create type moderator_action_kind as enum('kick', 'softban', 'mute', 'unmute', 
  'ban', 'unban', 'prune');

-- ----------------------------------------------------------------------------
-- 
-- Tables

-- metadata
-- This table holds a key-value pairs, which hold various metadata about the 
-- database schema. This table is pre-populated.
create table metadata(
  meta_key text not null,
  meta_value text not null,
  primary key(meta_key)
);
insert into metadata(meta_key, meta_value) values
  ('schema_version', '1'),
  ('timestamp', '2018-09-16T14:56+02:00'),
  ('author', 'Emzi0767'),
  ('project', 'ADA'),
  ('license', 'Apache License 2.0');

-- prefixes
-- Holds information about prefixes set in various guilds and channels.
create table prefixes(
  guild_id bigint, -- snowflake
  prefixes text[] not null,
  enable_default boolean not null default true,
  primary key(guild_id)
);

-- blocked_entities
-- Holds information about blocked users, channels, and guilds, along with 
-- information about block reason.
create table blocked_entities(
  id bigint not null, -- snowflake
  kind entity_kind not null,
  reason text,
  since timestamp with time zone not null,
  primary key(id, kind)
);

-- tags
-- Holds all user-defined tags and relevant information.
create table tags(
  kind tag_kind not null,
  container_id bigint not null,
  name text not null,
  owner_id bigint not null,
  hidden boolean not null default false,
  latest_revision timestamp with time zone not null,
  primary key(kind, container_id, name)
);

-- tag_revisions.
-- Holds revisions of all defined tags.
create table tag_revisions(
  kind tag_kind not null,
  container_id bigint not null,
  name text not null,
  contents text not null,
  created_at timestamp with time zone not null,
  user_id bigint not null,
  primary key(name, created_at),
  foreign key(kind, container_id, name) references tags(kind, container_id, 
    name)
);

-- guild_settings
-- Contains all per-guild settings defined by guild administrators.
-- JSON as follows:
-- {
--   "invite_blocker": {
--     "enabled": false,
--     "exemptions": {
--       "users": [],
--       "roles": [],
--       "channels": []
--     }
--   },
--   "rolestate": {
--     "enabled": false,
--     "exemptions": {
--       "users": [],
--       "roles": [],
--       "channels": []
--     }
--   },
--   "modlog": {
--     "enabled": false,
--     "channel": 0
--   },
--   "joinlog": {
--     "enabled": false,
--     "channel": 0
--   },
--   "muting": {
--     "enabled": false,
--     "role": 0
--   },
--   "autorole": {
--     "enabled": false,
--     "roles": []
--   },
--   "clickrole": {
--     "enabled": false,
--     "add_roles": {},
--     "remove_roles": {}
--   },
--   "unhoister": {
--     "enabled": false,
--     "rename_to": null
--   },
--   "disabled_commands": [],
--   "stallman": {
--     "enabled": false
--   }
-- }
-- clickrole.add_roles and clickrole.remove_roles are both emoji->role_id 
--   mappings.
-- disabled_commands is a list of qualified names of commands that are to be 
--   unavailable in the guild. Only non-moderation commands can be disabled.
-- unhoister.rename_to will be used as new nickname for users who have been 
--   unhoisted. If it's null, offending characters will simply be removed.
create table guild_settings(
  guild_id bigint not null,
  settings json not null,
  primary key(guild_id)
);

-- moderation_logs
-- Holds a complete log of all actions taken by guild's moderators.
create table moderation_logs(
  guild_id bigint not null,
  action_id int not null,
  logged_at timestamp with time zone not null,
  kind moderator_action_kind not null,
  target bigint default null,
  moderator bigint default null,
  reason varchar(1000) default null,
  until timestamp with time zone default null,
  primary key(guild_id, action_id)
);

-- moderator_notes
-- Holds notes attached to various members by moderators.
create table moderator_notes(
  guild_id bigint not null,
  user_id bigint not null,
  contents text,
  primary key(guild_id, user_id)
);

-- moderator_warnings
-- Holds warnings issued to users by moderators.
create table moderator_warnings(
  guild_id bigint not null,
  user_id bigint not null,
  warning_id int not null,
  issuer_id bigint not null,
  issued_at timestamp with time zone not null,
  contents text not null,
  primary key(guild_id, user_id, warning_id)
);


-- rolestate
-- Contains role state information.
create table rolestate(
  guild_id bigint not null,
  user_id bigint not null,
  role_ids bigint[] not null,
  primary key(guild_id, user_id)
);

-- rss_feeds
-- Contains all RSS feeds attached to guilds. These will be periodically 
-- checked and posted by the bot.
create table rss_feeds(
  feed_id int not null,
  guild_id bigint not null,
  channel_id bigint not null,
  feed_url text not null,
  item_tag text not null default '',
  initalized boolean not null,
  item_cache text[] not null,
  primary key(feed_id, guild_id),
  unique(channel_id, feed_url)
);

-- scheduler_data
-- Contains information about all actions to be taken by the asynchronous task 
-- scheduler.
create sequence scheduler_data_action_id_seq;
create table scheduler_data(
  action_id int not null default nextval('scheduler_data_action_id_seq'),
  user_id bigint not null,
  guild_id bigint not null,
  dispatch_at timestamp with time zone not null,
  dispatch_handler_type text not null,
  action_data json,
  primary key(action_id, user_id)
);
alter sequence scheduler_data_action_id_seq owned by scheduler_data.action_id;
