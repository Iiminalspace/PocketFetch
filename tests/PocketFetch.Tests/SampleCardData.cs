namespace PocketFetch.Tests;

public static class SampleCardData
{
    public const string ButterfreeAndPotionJson = """
        [
          {
            "id": "a1-007", "name": "Butterfree", "set_code": "a1", "set_name": "Genetic Apex",
            "pack": "Pikachu", "release_date": "2024-10-30",
            "type": "Pokémon", "subtype": "Grass", "stage": "Stage 2", "evolves_from": "Metapod",
            "rarity": "◊◊◊", "pack_points": 150, "health": 120, "retreat": 1, "weakness": "Fire",
            "ability": { "exists": true, "name": "Powder Heal", "effect": "Heal 20 damage from each of your Pokémon." },
            "card_text": null,
            "attacks": {
              "1": { "cost": "GCC", "name": "Gust", "damage": 60, "effect": null },
              "2": { "cost": null, "name": null, "damage": null, "effect": null }
            },
            "tradable": true, "trade_cost": 1200, "artist": "Shin Nagasawa",
            "image_png": "https://example.test/a1/007.png",
            "flavour_text": "It flaps its wings at great speed.",
            "alternate_versions": [ { "set_code": "pa", "set_name": "Promo-A", "id": 13, "rarity": "Promo" } ]
          },
          {
            "id": "pa-001", "name": "Potion", "set_code": "pa", "set_name": "Promo-A",
            "pack": "Shop", "release_date": null,
            "type": "Trainer", "subtype": "Item", "stage": null, "evolves_from": null,
            "rarity": "Promo", "pack_points": null, "health": null, "retreat": null, "weakness": null,
            "ability": { "exists": false, "name": null, "effect": null },
            "card_text": "Heal 20 damage from 1 of your Pokémon.",
            "attacks": {
              "1": { "cost": null, "name": null, "damage": null, "effect": null },
              "2": { "cost": null, "name": null, "damage": null, "effect": null }
            },
            "tradable": false, "trade_cost": null, "artist": "5ban Graphics",
            "image_png": "https://example.test/pa/001.png",
            "flavour_text": null,
            "alternate_versions": []
          }
        ]
        """;
}
