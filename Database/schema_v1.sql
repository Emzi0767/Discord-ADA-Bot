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


-- moderation_logs
-- Holds a complete log of all actions taken by guild's moderators.


-- rss_feeds
-- Contains all RSS feeds attached to guilds. These will be periodically 
-- checked and posted by the bot.


-- scheduler_data
-- Contains information about all actions to be taken by the asynchronous task 
-- scheduler.

