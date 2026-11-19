# PocketFetch

A Discord bot that looks up **Pokémon TCG Pocket** cards when someone writes a card name in double curly brackets, in the style of the Scryfall bot for Magic the Gathering. It uses `{{...}}` rather than Scryfall's `[[...]]`, so both bots can share a server.

```
Is {{Pikachu ex}} still worth running with {{!Zapdos ex}}?
```

## Commands

| You type | The bot replies with |
| --- | --- |
| `{{Pikachu ex}}` | Card text: type, HP, ability, attacks, weakness, retreat |
| `{{!Pikachu ex}}` | The full card image |
| `{{?Pikachu ex}}` | How to get it: set, pack, rarity, crafting cost, trading |
| `{{#Pikachu ex}}` | Every print of the card |
| `{{Pikachu ex\|A1}}` | The card from one set |
| `{{Pikachu ex\|Genetic Apex}}` | The same, using the set's name |
| `{{Pikachu ex\|A1\|96}}` | One exact print, by set and number |
| `{{sets}}` | Every set with its code |
| `{{help}}` | A short list of these commands |

Names and set names forgive case, accents, partial names and small typos (`{{charizrd ex}}` works). A message can ask for up to five cards. Prefixes combine with sets: `{{!Pikachu ex|A1|285}}`.

## How the code is organized

Each folder under `src/PocketFetch` is one area of responsibility:

| Folder | What it does |
| --- | --- |
| `Requests/` | Reads `{{...}}` requests out of message text |
| `Cards/` | Card model, catalog, and name matching |
| `CardData/` | Downloads the card list and keeps the catalog fresh |
| `Replies/` | Turns a card into a Discord embed |
| `Bot/` | Discord connection and message handling |

## Card data and license

Card data comes from [PocketDecks/pokemon-tcg-pocket-cards](https://github.com/PocketDecks/pokemon-tcg-pocket-cards) (v5), which gathers it from [Limitless TCG](https://pocket.limitlesstcg.com/cards). Card titles link to Limitless TCG.

The v5 data is licensed AGPL-3.0-or-later, so PocketFetch is released under the same license; see [LICENSE](LICENSE).

PocketFetch is a fan project. It is not affiliated with, endorsed by, or sponsored by Nintendo, Creatures, GAME FREAK, The Pokémon Company or DeNA. Pokémon and card images belong to their owners.
