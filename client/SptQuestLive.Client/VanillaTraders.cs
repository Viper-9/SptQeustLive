using System.Collections.Generic;

namespace SptQuestLive.Client;

internal static class VanillaTraders
{
    private static readonly HashSet<string> Ids =
    [
        "54cb50c76803fa8b248b4571",
        "54cb57776803fa99248b456e",
        "579dc571d53a0658a154fbec",
        "58330581ace78e27b8b10cee",
        "5935c25fb3acc3127c3d8cd9",
        "5a7c2eca46aef81a7ca2145d",
        "5ac3b934156ae10c4430e83c",
        "5c0647fdd443bc2504c2d371",
        "638f541a29ffd1183d187f57",
        "656f0f98d80a697f855d34b1",
        "6617beeaa9cfa777ca915b7c",
    ];

    internal static bool Contains(string? traderId) => traderId is not null && Ids.Contains(traderId);
}
