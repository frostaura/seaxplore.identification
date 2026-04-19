using Microsoft.EntityFrameworkCore;
using OceanCare.Api.Domain.Interfaces;
using OceanCare.Api.Domain.Models;

namespace OceanCare.Api.Infrastructure.Data;

public class DataSeeder(OceanCareDbContext db, IEmbeddingPlugin embedding)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        await db.Database.EnsureCreatedAsync(ct);

        if (await db.Admins.AnyAsync(ct)) return;

        // Seed admin user (default: admin / OceanCare2024!)
        db.Admins.Add(new Admin
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OceanCare2024!")
        });

        // Seed categories
        var coral = new Category { Name = "Coral", Description = "Marine invertebrates in class Anthozoa" };
        var fish = new Category { Name = "Fish", Description = "Aquatic vertebrates with gills" };
        var jellyfish = new Category { Name = "Jellyfish", Description = "Free-swimming marine animals" };
        var seaTurtle = new Category { Name = "Sea Turtle", Description = "Marine reptiles of superfamily Chelonioidea" };
        db.Categories.AddRange(coral, fish, jellyfish, seaTurtle);

        // Seed attributes
        var colorAttr = new MarineAttribute { Name = "Color", Description = "Primary coloration", DataType = "string" };
        var patternAttr = new MarineAttribute { Name = "Pattern", Description = "Surface pattern or markings", DataType = "string" };
        var sizeAttr = new MarineAttribute { Name = "Size", Description = "Typical size range", DataType = "string" };
        var depthAttr = new MarineAttribute { Name = "Depth Range", Description = "Typical ocean depth habitat (meters)", DataType = "string" };
        var habitatAttr = new MarineAttribute { Name = "Habitat", Description = "Primary ocean habitat", DataType = "string" };
        var dietAttr = new MarineAttribute { Name = "Diet", Description = "Typical diet", DataType = "string" };
        var biolumAttr = new MarineAttribute { Name = "Bioluminescent", Description = "Whether the organism produces its own light", DataType = "boolean" };
        var conservAttr = new MarineAttribute { Name = "Conservation Status", Description = "IUCN conservation status", DataType = "string" };
        db.Attributes.AddRange(colorAttr, patternAttr, sizeAttr, depthAttr, habitatAttr, dietAttr, biolumAttr, conservAttr);

        await db.SaveChangesAsync(ct);

        // Seed marine species with real data
        var speciesData = GetSpeciesSeedData(coral, fish, jellyfish, seaTurtle,
            colorAttr, patternAttr, sizeAttr, depthAttr, habitatAttr, dietAttr, biolumAttr, conservAttr);

        foreach (var (species, attributeValues) in speciesData)
        {
            db.Species.Add(species);
            await db.SaveChangesAsync(ct);

            foreach (var av in attributeValues)
            {
                av.SpeciesId = species.Id;
                db.SpeciesAttributeValues.Add(av);
            }
            await db.SaveChangesAsync(ct);

            // Generate and store embedding
            var searchText = BuildSearchText(species, attributeValues);
            var vector = await embedding.GenerateEmbeddingAsync(searchText, ct);
            db.SearchEmbeddings.Add(new SearchEmbedding
            {
                SpeciesId = species.Id,
                EmbeddingJson = System.Text.Json.JsonSerializer.Serialize(vector)
            });
            await db.SaveChangesAsync(ct);
        }
    }

    private static string BuildSearchText(Species species, List<SpeciesAttributeValue> attrs)
    {
        var parts = new List<string>
        {
            species.CommonName,
            species.ScientificName,
            species.Description,
            species.Category?.Name ?? ""
        };
        parts.AddRange(attrs.Select(a => $"{a.Attribute?.Name}: {a.Value}"));
        return string.Join(". ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    private static List<(Species Species, List<SpeciesAttributeValue> Attrs)> GetSpeciesSeedData(
        Category coral, Category fish, Category jellyfish, Category seaTurtle,
        MarineAttribute colorAttr, MarineAttribute patternAttr, MarineAttribute sizeAttr,
        MarineAttribute depthAttr, MarineAttribute habitatAttr, MarineAttribute dietAttr,
        MarineAttribute biolumAttr, MarineAttribute conservAttr)
    {
        return
        [
            // CORALS
            (new Species
            {
                CommonName = "Brain Coral",
                ScientificName = "Diploria labyrinthiformis",
                Description = "A stony coral with a distinctive brain-like surface covered in grooves and ridges. Typically yellow-green to brown in color. Found in shallow tropical reefs.",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/a5/Diploria_labyrinthiformis_%28Brain_coral%29.jpg/640px-Diploria_labyrinthiformis_%28Brain_coral%29.jpg",
                Category = coral
            }, [
                new SpeciesAttributeValue { Attribute = colorAttr, Value = "Yellow-green, brown, grey" },
                new SpeciesAttributeValue { Attribute = patternAttr, Value = "Labyrinthine grooves and ridges" },
                new SpeciesAttributeValue { Attribute = sizeAttr, Value = "Up to 1.8 meters in diameter" },
                new SpeciesAttributeValue { Attribute = depthAttr, Value = "1–40 meters" },
                new SpeciesAttributeValue { Attribute = habitatAttr, Value = "Tropical coral reefs" },
                new SpeciesAttributeValue { Attribute = dietAttr, Value = "Zooplankton, photosynthesis via zooxanthellae" },
                new SpeciesAttributeValue { Attribute = biolumAttr, Value = "false" },
                new SpeciesAttributeValue { Attribute = conservAttr, Value = "Least Concern" }
            ]),
            (new Species
            {
                CommonName = "Staghorn Coral",
                ScientificName = "Acropora cervicornis",
                Description = "A fast-growing branching coral with long cylindrical branches resembling deer antlers. Tan to gold-brown with white or pale branch tips.",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/7/7d/Staghorn_coral.jpg/640px-Staghorn_coral.jpg",
                Category = coral
            }, [
                new SpeciesAttributeValue { Attribute = colorAttr, Value = "Tan, gold-brown, pale tips" },
                new SpeciesAttributeValue { Attribute = patternAttr, Value = "Branching antler-like structure" },
                new SpeciesAttributeValue { Attribute = sizeAttr, Value = "Up to 3 meters in height" },
                new SpeciesAttributeValue { Attribute = depthAttr, Value = "0–20 meters" },
                new SpeciesAttributeValue { Attribute = habitatAttr, Value = "Caribbean reef slopes" },
                new SpeciesAttributeValue { Attribute = dietAttr, Value = "Zooplankton, photosynthesis" },
                new SpeciesAttributeValue { Attribute = biolumAttr, Value = "false" },
                new SpeciesAttributeValue { Attribute = conservAttr, Value = "Critically Endangered" }
            ]),
            (new Species
            {
                CommonName = "Red Sea Whip Coral",
                ScientificName = "Ellisella ceratophyta",
                Description = "A bright red to orange sea whip coral that grows in long, unbranched or sparsely branched whip-like colonies. Common in Indo-Pacific reefs.",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/14/Sea_Whip_Coral_Ellisella_sp.jpg/640px-Sea_Whip_Coral_Ellisella_sp.jpg",
                Category = coral
            }, [
                new SpeciesAttributeValue { Attribute = colorAttr, Value = "Bright red, orange" },
                new SpeciesAttributeValue { Attribute = patternAttr, Value = "Long whip-like unbranched colonies" },
                new SpeciesAttributeValue { Attribute = sizeAttr, Value = "Up to 2 meters long" },
                new SpeciesAttributeValue { Attribute = depthAttr, Value = "10–100 meters" },
                new SpeciesAttributeValue { Attribute = habitatAttr, Value = "Indo-Pacific reefs, current-swept walls" },
                new SpeciesAttributeValue { Attribute = dietAttr, Value = "Plankton, suspended particles" },
                new SpeciesAttributeValue { Attribute = biolumAttr, Value = "false" },
                new SpeciesAttributeValue { Attribute = conservAttr, Value = "Not Evaluated" }
            ]),
            (new Species
            {
                CommonName = "Blue Coral",
                ScientificName = "Heliopora coerulea",
                Description = "The only octocoral that produces a massive calcium carbonate skeleton. Its skeleton is vivid blue, though the living tissue is brownish-grey.",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/f/f1/Heliopora_coerulea.jpg/640px-Heliopora_coerulea.jpg",
                Category = coral
            }, [
                new SpeciesAttributeValue { Attribute = colorAttr, Value = "Blue skeleton, brownish-grey tissue" },
                new SpeciesAttributeValue { Attribute = patternAttr, Value = "Massive colony with blue skeleton" },
                new SpeciesAttributeValue { Attribute = sizeAttr, Value = "Up to 1 meter in diameter" },
                new SpeciesAttributeValue { Attribute = depthAttr, Value = "1–20 meters" },
                new SpeciesAttributeValue { Attribute = habitatAttr, Value = "Indo-Pacific reef flats" },
                new SpeciesAttributeValue { Attribute = dietAttr, Value = "Photosynthesis via zooxanthellae" },
                new SpeciesAttributeValue { Attribute = biolumAttr, Value = "false" },
                new SpeciesAttributeValue { Attribute = conservAttr, Value = "Vulnerable" }
            ]),
            (new Species
            {
                CommonName = "Mushroom Coral",
                ScientificName = "Fungia fungites",
                Description = "A solitary, free-living coral shaped like a mushroom cap. Has a flat disc with prominent ridges radiating from the center. Can be white, pink, or brown.",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/0/0e/Fungia_fungites.jpg/640px-Fungia_fungites.jpg",
                Category = coral
            }, [
                new SpeciesAttributeValue { Attribute = colorAttr, Value = "White, pink, brown, green" },
                new SpeciesAttributeValue { Attribute = patternAttr, Value = "Disc-shaped with radial ridges" },
                new SpeciesAttributeValue { Attribute = sizeAttr, Value = "5–30 cm in diameter" },
                new SpeciesAttributeValue { Attribute = depthAttr, Value = "1–30 meters" },
                new SpeciesAttributeValue { Attribute = habitatAttr, Value = "Indo-Pacific sandy reef slopes" },
                new SpeciesAttributeValue { Attribute = dietAttr, Value = "Zooplankton, photosynthesis" },
                new SpeciesAttributeValue { Attribute = biolumAttr, Value = "false" },
                new SpeciesAttributeValue { Attribute = conservAttr, Value = "Least Concern" }
            ]),

            // FISH
            (new Species
            {
                CommonName = "Clownfish",
                ScientificName = "Amphiprion ocellaris",
                Description = "A small, vibrantly colored reef fish with iconic orange and white striped pattern with black borders. Lives in symbiosis with sea anemones in the Indo-Pacific.",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/ad/Clown_fish_in_Pacific_Ocean.jpg/640px-Clown_fish_in_Pacific_Ocean.jpg",
                Category = fish
            }, [
                new SpeciesAttributeValue { Attribute = colorAttr, Value = "Orange with white stripes edged in black" },
                new SpeciesAttributeValue { Attribute = patternAttr, Value = "Three white vertical stripes on orange body" },
                new SpeciesAttributeValue { Attribute = sizeAttr, Value = "7–11 cm" },
                new SpeciesAttributeValue { Attribute = depthAttr, Value = "1–15 meters" },
                new SpeciesAttributeValue { Attribute = habitatAttr, Value = "Tropical coral reefs, sea anemones" },
                new SpeciesAttributeValue { Attribute = dietAttr, Value = "Algae, plankton, small invertebrates" },
                new SpeciesAttributeValue { Attribute = biolumAttr, Value = "false" },
                new SpeciesAttributeValue { Attribute = conservAttr, Value = "Least Concern" }
            ]),
            (new Species
            {
                CommonName = "Mandarin Fish",
                ScientificName = "Synchiropus splendidus",
                Description = "One of the most vivid and colorful fish in the ocean. Its body is covered in a psychedelic pattern of blue, orange, green, and yellow. Has no scales and instead secrets a toxic mucus.",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/d/d4/Synchiropus_splendidus_2_Luc_Viatour.jpg/640px-Synchiropus_splendidus_2_Luc_Viatour.jpg",
                Category = fish
            }, [
                new SpeciesAttributeValue { Attribute = colorAttr, Value = "Vivid blue, orange, green, yellow" },
                new SpeciesAttributeValue { Attribute = patternAttr, Value = "Psychedelic swirling pattern of multiple bright colors" },
                new SpeciesAttributeValue { Attribute = sizeAttr, Value = "5–8 cm" },
                new SpeciesAttributeValue { Attribute = depthAttr, Value = "1–18 meters" },
                new SpeciesAttributeValue { Attribute = habitatAttr, Value = "Indo-Pacific lagoons, inshore reefs" },
                new SpeciesAttributeValue { Attribute = dietAttr, Value = "Small invertebrates, copepods" },
                new SpeciesAttributeValue { Attribute = biolumAttr, Value = "false" },
                new SpeciesAttributeValue { Attribute = conservAttr, Value = "Least Concern" }
            ]),
            (new Species
            {
                CommonName = "Lionfish",
                ScientificName = "Pterois volitans",
                Description = "A venomous reef fish native to the Indo-Pacific with dramatic red, white, and brown stripes. Has feathery pectoral fins and long spines with venom. Now an invasive species in the Atlantic.",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/0/0b/Pterois_volitans_Manado-e.jpg/640px-Pterois_volitans_Manado-e.jpg",
                Category = fish
            }, [
                new SpeciesAttributeValue { Attribute = colorAttr, Value = "Red, white, brown, black" },
                new SpeciesAttributeValue { Attribute = patternAttr, Value = "Bold vertical red and white stripes with feathery fins" },
                new SpeciesAttributeValue { Attribute = sizeAttr, Value = "25–40 cm" },
                new SpeciesAttributeValue { Attribute = depthAttr, Value = "1–50 meters" },
                new SpeciesAttributeValue { Attribute = habitatAttr, Value = "Tropical reefs, rocky crevices" },
                new SpeciesAttributeValue { Attribute = dietAttr, Value = "Fish, crustaceans, invertebrates" },
                new SpeciesAttributeValue { Attribute = biolumAttr, Value = "false" },
                new SpeciesAttributeValue { Attribute = conservAttr, Value = "Least Concern" }
            ]),
            (new Species
            {
                CommonName = "Blue Tang",
                ScientificName = "Paracanthurus hepatus",
                Description = "A beautiful deep blue surgeonfish with a yellow tail and distinctive black marking. Known as Dory from Finding Nemo. Common on coral reefs in the Indo-Pacific.",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/4/4a/Paracanthurus_hepatus.jpg/640px-Paracanthurus_hepatus.jpg",
                Category = fish
            }, [
                new SpeciesAttributeValue { Attribute = colorAttr, Value = "Vivid blue body, black pattern, yellow tail" },
                new SpeciesAttributeValue { Attribute = patternAttr, Value = "Blue with black palette-shaped marking and yellow tail" },
                new SpeciesAttributeValue { Attribute = sizeAttr, Value = "15–31 cm" },
                new SpeciesAttributeValue { Attribute = depthAttr, Value = "2–40 meters" },
                new SpeciesAttributeValue { Attribute = habitatAttr, Value = "Indo-Pacific coral reefs" },
                new SpeciesAttributeValue { Attribute = dietAttr, Value = "Algae, plankton" },
                new SpeciesAttributeValue { Attribute = biolumAttr, Value = "false" },
                new SpeciesAttributeValue { Attribute = conservAttr, Value = "Least Concern" }
            ]),
            (new Species
            {
                CommonName = "Bioluminescent Dragonfish",
                ScientificName = "Aristostomias scintillans",
                Description = "A deep-sea predatory fish with bioluminescent photophores along its body and around its eyes. Uses red bioluminescent light invisible to most deep-sea prey. Has large fang-like teeth.",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e0/Malacosteus_niger.jpg/640px-Malacosteus_niger.jpg",
                Category = fish
            }, [
                new SpeciesAttributeValue { Attribute = colorAttr, Value = "Dark black with red/blue bioluminescent photophores" },
                new SpeciesAttributeValue { Attribute = patternAttr, Value = "Row of light organs along belly, large fangs" },
                new SpeciesAttributeValue { Attribute = sizeAttr, Value = "15–30 cm" },
                new SpeciesAttributeValue { Attribute = depthAttr, Value = "200–2000 meters" },
                new SpeciesAttributeValue { Attribute = habitatAttr, Value = "Deep ocean mesopelagic zone" },
                new SpeciesAttributeValue { Attribute = dietAttr, Value = "Fish, crustaceans" },
                new SpeciesAttributeValue { Attribute = biolumAttr, Value = "true" },
                new SpeciesAttributeValue { Attribute = conservAttr, Value = "Data Deficient" }
            ]),
            (new Species
            {
                CommonName = "Parrotfish",
                ScientificName = "Scarus guacamaia",
                Description = "A large, vibrantly colored reef fish with distinctive fused teeth resembling a beak. Colors range from green to blue-green with pink and orange accents. Plays key role in reef health.",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/58/Rainbow_Parrotfish.jpg/640px-Rainbow_Parrotfish.jpg",
                Category = fish
            }, [
                new SpeciesAttributeValue { Attribute = colorAttr, Value = "Green, blue-green, pink, orange" },
                new SpeciesAttributeValue { Attribute = patternAttr, Value = "Mosaic of green-blue scales with pink accents and beak-like teeth" },
                new SpeciesAttributeValue { Attribute = sizeAttr, Value = "40–120 cm" },
                new SpeciesAttributeValue { Attribute = depthAttr, Value = "1–30 meters" },
                new SpeciesAttributeValue { Attribute = habitatAttr, Value = "Caribbean and Atlantic coral reefs" },
                new SpeciesAttributeValue { Attribute = dietAttr, Value = "Coral polyps, algae" },
                new SpeciesAttributeValue { Attribute = biolumAttr, Value = "false" },
                new SpeciesAttributeValue { Attribute = conservAttr, Value = "Endangered" }
            ]),

            // JELLYFISH
            (new Species
            {
                CommonName = "Moon Jellyfish",
                ScientificName = "Aurelia aurita",
                Description = "A translucent jellyfish with four horseshoe-shaped gonads visible through its bell. Nearly colorless to pale white or blue, with a faint pinkish or lavender tint. Common in coastal waters worldwide.",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/9e/Jellyfish.jpg/640px-Jellyfish.jpg",
                Category = jellyfish
            }, [
                new SpeciesAttributeValue { Attribute = colorAttr, Value = "Transparent, pale blue-white, faint purple" },
                new SpeciesAttributeValue { Attribute = patternAttr, Value = "Four-leaf clover gonads visible through transparent bell" },
                new SpeciesAttributeValue { Attribute = sizeAttr, Value = "25–40 cm bell diameter" },
                new SpeciesAttributeValue { Attribute = depthAttr, Value = "0–200 meters" },
                new SpeciesAttributeValue { Attribute = habitatAttr, Value = "Coastal waters worldwide" },
                new SpeciesAttributeValue { Attribute = dietAttr, Value = "Zooplankton, fish eggs, small crustaceans" },
                new SpeciesAttributeValue { Attribute = biolumAttr, Value = "false" },
                new SpeciesAttributeValue { Attribute = conservAttr, Value = "Least Concern" }
            ]),
            (new Species
            {
                CommonName = "Pacific Sea Nettle",
                ScientificName = "Chrysaora fuscescens",
                Description = "A large, striking jellyfish with a golden-brown bell and long trailing tentacles and oral arms that can reach 5 meters. Deep orange-brown with white stripes on the bell.",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/4/40/Jellyfish_at_the_Monterey_Bay_Aquarium.jpg/640px-Jellyfish_at_the_Monterey_Bay_Aquarium.jpg",
                Category = jellyfish
            }, [
                new SpeciesAttributeValue { Attribute = colorAttr, Value = "Golden-brown, orange, white" },
                new SpeciesAttributeValue { Attribute = patternAttr, Value = "Orange-brown bell with radiating white stripes, long tentacles" },
                new SpeciesAttributeValue { Attribute = sizeAttr, Value = "30–50 cm bell, tentacles up to 5 meters" },
                new SpeciesAttributeValue { Attribute = depthAttr, Value = "0–300 meters" },
                new SpeciesAttributeValue { Attribute = habitatAttr, Value = "Eastern Pacific Ocean" },
                new SpeciesAttributeValue { Attribute = dietAttr, Value = "Zooplankton, small fish, other jellyfish" },
                new SpeciesAttributeValue { Attribute = biolumAttr, Value = "false" },
                new SpeciesAttributeValue { Attribute = conservAttr, Value = "Not Evaluated" }
            ]),
            (new Species
            {
                CommonName = "Comb Jelly",
                ScientificName = "Mnemiopsis leidyi",
                Description = "A ctenophore (not a true jellyfish) with transparent, oval body and eight rows of iridescent cilia that produce rainbow-like bioluminescent flashes in the dark. One of the most striking bioluminescent organisms.",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c9/Mnemiopsis_leidyi.jpg/640px-Mnemiopsis_leidyi.jpg",
                Category = jellyfish
            }, [
                new SpeciesAttributeValue { Attribute = colorAttr, Value = "Transparent, iridescent rainbow colors when lit" },
                new SpeciesAttributeValue { Attribute = patternAttr, Value = "Eight rows of cilia producing iridescent rainbow light" },
                new SpeciesAttributeValue { Attribute = sizeAttr, Value = "5–12 cm long" },
                new SpeciesAttributeValue { Attribute = depthAttr, Value = "0–200 meters" },
                new SpeciesAttributeValue { Attribute = habitatAttr, Value = "Coastal and estuarine waters" },
                new SpeciesAttributeValue { Attribute = dietAttr, Value = "Zooplankton, fish larvae" },
                new SpeciesAttributeValue { Attribute = biolumAttr, Value = "true" },
                new SpeciesAttributeValue { Attribute = conservAttr, Value = "Not Evaluated" }
            ]),

            // SEA TURTLES
            (new Species
            {
                CommonName = "Green Sea Turtle",
                ScientificName = "Chelonia mydas",
                Description = "A large sea turtle named for the greenish color of its fat. Has a smooth, olive to dark brown shell (carapace) with yellow or white underside. One of the largest hard-shelled turtles.",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/2/20/Sea_turtle.jpg/640px-Sea_turtle.jpg",
                Category = seaTurtle
            }, [
                new SpeciesAttributeValue { Attribute = colorAttr, Value = "Olive to dark brown shell, yellow-white underside" },
                new SpeciesAttributeValue { Attribute = patternAttr, Value = "Smooth carapace with pale streaks" },
                new SpeciesAttributeValue { Attribute = sizeAttr, Value = "80–120 cm shell length, 130–200 kg" },
                new SpeciesAttributeValue { Attribute = depthAttr, Value = "0–50 meters" },
                new SpeciesAttributeValue { Attribute = habitatAttr, Value = "Tropical and subtropical oceans worldwide" },
                new SpeciesAttributeValue { Attribute = dietAttr, Value = "Seagrass, algae (adults are herbivores)" },
                new SpeciesAttributeValue { Attribute = biolumAttr, Value = "false" },
                new SpeciesAttributeValue { Attribute = conservAttr, Value = "Endangered" }
            ]),
            (new Species
            {
                CommonName = "Hawksbill Sea Turtle",
                ScientificName = "Eretmochelys imbricata",
                Description = "A critically endangered sea turtle with a distinctive narrow, pointed beak. Has beautifully patterned amber-colored carapace with dark streaks and yellow or cream underside. Critical for coral reef health.",
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/7/77/Hawksbill_sea_turtle.JPG/640px-Hawksbill_sea_turtle.JPG",
                Category = seaTurtle
            }, [
                new SpeciesAttributeValue { Attribute = colorAttr, Value = "Amber, dark brown, yellow" },
                new SpeciesAttributeValue { Attribute = patternAttr, Value = "Beautifully patterned amber-brown carapace with dark streaks" },
                new SpeciesAttributeValue { Attribute = sizeAttr, Value = "60–90 cm, 40–75 kg" },
                new SpeciesAttributeValue { Attribute = depthAttr, Value = "0–25 meters" },
                new SpeciesAttributeValue { Attribute = habitatAttr, Value = "Tropical coral reefs, lagoons" },
                new SpeciesAttributeValue { Attribute = dietAttr, Value = "Sea sponges, jellyfish, sea anemones" },
                new SpeciesAttributeValue { Attribute = biolumAttr, Value = "false" },
                new SpeciesAttributeValue { Attribute = conservAttr, Value = "Critically Endangered" }
            ])
        ];
    }
}
