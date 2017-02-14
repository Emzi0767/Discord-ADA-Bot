-- ADA PostgreSQL Database Schema

-- Per-guild settings
CREATE SEQUENCE ada_guild_settings_id_seq;
CREATE TABLE ada_guild_settings(
	id BIGINT PRIMARY KEY DEFAULT NEXTVAL('ada_guild_settings_id_seq'),
	guild_id BIGINT NOT NULL, -- UNSIGNED
	setting_name VARCHAR(255) NOT NULL,
	setting_value VARCHAR(32768),
	UNIQUE(guild_id, setting_name)
);
ALTER SEQUENCE ada_guild_settings_id_seq OWNED BY ada_guild_settings.id;

-- Moderation log backend
CREATE SEQUENCE ada_moderator_actions_id_seq;
CREATE TABLE ada_moderator_actions(
	id BIGINT PRIMARY KEY DEFAULT NEXTVAL('ada_moderator_actions_id_seq'),
	guild_id BIGINT NOT NULL, -- UNSIGNED
	target_user BIGINT NOT NULL, -- UNSIGNED
	action_type SMALLINT NOT NULL, 
	moderator BIGINT,
	reason VARCHAR(2000),
	action_timestamp TIMESTAMP WITH TIME ZONE NOT NULL, 
	until TIMESTAMP WITH TIME ZONE,
	attached_message_id BIGINT NOT NULL,
	UNIQUE(guild_id, target_user, action_type, action_timestamp)
);
ALTER SEQUENCE ada_moderator_actions_id_seq OWNED BY ada_moderator_actions.id;

-- RSS module
CREATE SEQUENCE ada_rss_feeds_id_seq;
CREATE TABLE ada_rss_feeds(
	id BIGINT PRIMARY KEY DEFAULT NEXTVAL('ada_rss_feeds_id_seq'),
	guild_id BIGINT NOT NULL, -- UNSIGNED
	channel_id BIGINT NOT NULL, -- UNSIGNED
	feed_url VARCHAR(256) NOT NULL,
	item_tag VARCHAR(32),
	initialized BOOLEAN NOT NULL,
	last_items TEXT NOT NULL,
	UNIQUE(channel_id, feed_url)
);
ALTER SEQUENCE ada_rss_feeds_id_seq OWNED BY ada_rss_feeds.id;

-- Tag storage
CREATE SEQUENCE ada_tags_id_seq;
CREATE TABLE ada_tags(
	id BIGINT PRIMARY KEY DEFAULT NEXTVAL('ada_tags_id_seq'),
	tag_name VARCHAR(2000) NOT NULL,
	tag_contents VARCHAR(2000) NOT NULL,
	channel_id BIGINT, -- UNSIGNED
	guild_id BIGINT, -- UNSIGNED
	owner_id BIGINT NOT NULL, -- UNSIGNED
	created TIMESTAMP WITH TIME ZONE NOT NULL,
	UNIQUE(tag_name, channel_id, guild_id)
);
ALTER SEQUENCE ada_tags_id_seq OWNED BY ada_tags.id;
