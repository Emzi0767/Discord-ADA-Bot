namespace Emzi0767.Net.Discord.AdaBot.Core
{
    public enum AdaPermission : uint
    {
        None =                  0x00000000u,
        CreateInstantInvite =   0x00000001u,
        KickMembers =           0x00000002u,
        BanMembers =            0x00000004u,
        Administrator =         0x00000008u,
        ManageChannels =        0x00000010u,
        ManageGuild =           0x00000020u,
        AddReactions =          0x00000040u,
        ReadMessages =          0x00000400u,
        SendMessages =          0x00000800u,
        SendTtsMessages =       0x00001000u,
        ManageMessages =        0x00002000u,
        EmbedLinks =            0x00004000u,
        AttachFiles =           0x00008000u,
        ReadMessageHistory =    0x00010000u,
        MentionEveryone =       0x00020000u,
        UseExternalEmojis =     0x00040000u,
        UseVoice =              0x00100000u,
        Speak =                 0x00200000u,
        MuteMembers =           0x00400000u,
        DeafenMembers =         0x00800000u,
        MoveMembers =           0x01000000u,
        UserVoiceDetection =    0x02000000u,
        ChangeNickname =        0x04000000u,
        ManageNicknames =       0x08000000u,
        ManageRoles =           0x10000000u,
        ManageWebhooks =        0x20000000u,
        ManageEmoji =           0x40000000u
    }
}
