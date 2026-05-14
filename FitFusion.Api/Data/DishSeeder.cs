using FitFusion.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitFusion.Api.Data;

public static class DishSeeder
{
    public static async Task SeedAsync(FitFusionDbContext db, ILogger logger, CancellationToken ct = default)
    {
        if (await db.Dishes.AnyAsync(ct))
        {
            logger.LogInformation("Dishes table already contains data; skipping seed.");
            return;
        }

        db.Dishes.AddRange(Dishes);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} dishes.", Dishes.Count);
    }

    private static readonly IReadOnlyList<Dish> Dishes =
    [
        // Breakfast
        D("tostada-tomate-aceite", "Tostada con tomate y aceite", "Pan tostado con tomate rallado y AOVE.", "breakfast", 120, 215, 5.6f, 32f, 7.4f, "vegan,low-sodium,traditional"),
        D("yogur-avena-miel", "Yogur con avena y miel", "Yogur natural con copos de avena y miel.", "breakfast,snack", 220, 150, 6.5f, 23f, 3.8f, "vegetarian,traditional"),
        D("tortilla-francesa-jamon", "Tortilla francesa con jamon", "Tortilla de dos huevos con jamon cocido.", "breakfast,dinner", 180, 180, 15f, 1.5f, 12f, "gluten-free,lactose-free,high-protein,traditional", containsPork: true),
        D("bowl-fruta-yogur-griego", "Bowl de fruta con yogur griego", "Yogur griego con fruta fresca y semillas.", "breakfast,snack", 250, 112, 7.5f, 15f, 2.6f, "vegetarian,gluten-free,high-protein,low-sodium"),
        D("porridge-avena", "Porridge de avena", "Avena cocida con bebida vegetal y canela.", "breakfast", 260, 105, 3.5f, 18f, 2.4f, "vegan,low-sodium"),
        D("tostada-aguacate", "Tostada de aguacate", "Pan integral con aguacate, limon y pimienta.", "breakfast,any", 150, 210, 5f, 24f, 10f, "vegan,low-sodium"),
        D("cafe-leche-croissant", "Cafe con leche y croissant", "Cafe con leche semidesnatada y croissant pequeno.", "breakfast", 220, 250, 6.5f, 31f, 11f, "vegetarian"),
        D("macedonia-fruta", "Macedonia de fruta", "Fruta de temporada troceada con zumo de naranja.", "breakfast,snack", 220, 62, 0.8f, 14f, 0.2f, "vegan,gluten-free,low-sodium"),
        D("pan-integral-queso-fresco", "Pan integral con queso fresco", "Tosta integral con queso fresco y oregano.", "breakfast,snack", 170, 165, 9.5f, 21f, 4.2f, "vegetarian,high-protein,low-sodium"),
        D("smoothie-platano-proteina", "Smoothie de platano y proteina", "Batido de platano con leche y proteina whey.", "breakfast,snack", 320, 112, 11f, 15f, 2.8f, "vegetarian,gluten-free,high-protein"),
        D("huevos-revueltos-espinacas", "Huevos revueltos con espinacas", "Huevos con espinacas salteadas y aceite de oliva.", "breakfast,dinner", 220, 145, 11f, 2f, 10f, "vegetarian,gluten-free,lactose-free,high-protein,low-carb,low-sodium"),
        D("tostada-crema-cacahuete-platano", "Tostada de cacahuete y platano", "Pan integral con crema de cacahuete y platano.", "breakfast,snack", 170, 270, 9f, 30f, 13f, "vegan,high-protein", containsNuts: true),
        D("bizcocho-avena-casero", "Bizcocho de avena casero", "Bizcocho ligero de avena, huevo y yogur.", "breakfast,snack", 120, 230, 8f, 29f, 8f, "vegetarian"),
        D("requeson-frutos-rojos", "Requeson con frutos rojos", "Requeson con frutos rojos y canela.", "breakfast,snack", 200, 115, 11f, 9f, 3.2f, "vegetarian,gluten-free,high-protein,low-carb,low-sodium"),
        D("pan-tortilla-pavo", "Pan con tortilla y pavo", "Bocadillo pequeno con tortilla y pechuga de pavo.", "breakfast,any", 210, 210, 14f, 23f, 7f, "high-protein"),

        // Lunch
        D("lentejas-estofadas", "Lentejas estofadas", "Lentejas guisadas con verduras, sin chorizo.", "lunch", 380, 116, 7.8f, 18f, 2.6f, "vegan,gluten-free,high-protein,traditional"),
        D("garbanzos-espinacas", "Garbanzos con espinacas", "Garbanzos salteados con espinacas y pimenton.", "lunch", 360, 135, 7f, 20f, 3.8f, "vegan,gluten-free,high-protein,traditional"),
        D("paella-pollo", "Paella de pollo", "Arroz con pollo, verduras y azafran.", "lunch", 420, 145, 8f, 20f, 4.2f, "gluten-free,lactose-free,high-protein,traditional"),
        D("macarrones-bolonesa", "Macarrones a la bolonesa", "Pasta con salsa de tomate y carne picada.", "lunch", 420, 165, 8f, 23f, 5.5f, "high-protein"),
        D("arroz-verduras", "Arroz con verduras", "Arroz salteado con verduras de temporada.", "lunch", 360, 128, 3f, 25f, 2.5f, "vegan,gluten-free,lactose-free,low-sodium"),
        D("merluza-horno-patatas", "Merluza al horno con patatas", "Merluza con patata panadera y verduras.", "lunch,dinner", 380, 118, 11f, 12f, 3.2f, "gluten-free,lactose-free,high-protein,low-fat,traditional", containsSeafood: true),
        D("pollo-curry-arroz", "Pollo al curry con arroz", "Pechuga de pollo con curry suave y arroz.", "lunch", 420, 158, 10f, 19f, 5f, "gluten-free,lactose-free,high-protein"),
        D("pisto-huevo", "Pisto con huevo", "Pisto manchego con huevo a la plancha.", "lunch,dinner", 340, 112, 5.5f, 10f, 6f, "vegetarian,gluten-free,lactose-free,traditional"),
        D("cocido-madrileno", "Cocido madrileno", "Garbanzos, verduras y carnes del cocido clasico.", "lunch", 480, 155, 9.5f, 13f, 8f, "gluten-free,high-protein,traditional", containsPork: true),
        D("albondigas-salsa", "Albondigas en salsa", "Albondigas caseras con salsa de tomate.", "lunch", 360, 190, 12f, 8f, 12f, "high-protein,traditional"),
        D("fabada-asturiana", "Fabada asturiana", "Alubias con compango en version tradicional.", "lunch", 450, 168, 9f, 16f, 8f, "gluten-free,high-protein,traditional", containsPork: true),
        D("ensaladilla-rusa", "Ensaladilla rusa", "Patata, verdura, atun y huevo con mayonesa ligera.", "lunch,any", 280, 165, 5.2f, 12f, 10f, "gluten-free,lactose-free,traditional", containsSeafood: true),
        D("salmon-plancha-quinoa", "Salmon a la plancha con quinoa", "Salmon con quinoa y verduras salteadas.", "lunch,dinner", 380, 180, 14f, 11f, 9f, "gluten-free,lactose-free,high-protein,low-sodium", containsSeafood: true),
        D("espaguetis-carbonara", "Espaguetis carbonara", "Pasta con huevo, queso y panceta.", "lunch", 420, 205, 9f, 24f, 10f, "high-protein", containsPork: true),
        D("arroz-tres-delicias", "Arroz tres delicias", "Arroz salteado con huevo, guisantes, zanahoria y pavo.", "lunch", 360, 150, 7f, 22f, 4.5f, "gluten-free,lactose-free"),
        D("croquetas-caseras-ensalada", "Croquetas caseras con ensalada", "Croquetas de pollo acompanadas de ensalada.", "lunch", 300, 230, 9f, 20f, 12f, "traditional"),
        D("pollo-asado-patatas", "Pollo asado con patatas", "Pollo asado al horno con patatas y especias.", "lunch", 430, 155, 12f, 12f, 6f, "gluten-free,lactose-free,high-protein,traditional"),
        D("lubina-horno", "Lubina al horno", "Lubina con verduras y patata al horno.", "lunch,dinner", 360, 112, 12f, 9f, 3.2f, "gluten-free,lactose-free,high-protein,low-sodium,traditional", containsSeafood: true),
        D("wok-ternera-verduras", "Wok de ternera con verduras", "Ternera salteada con verduras y salsa ligera.", "lunch,dinner", 360, 145, 13f, 8f, 6f, "gluten-free,lactose-free,high-protein,low-carb"),
        D("ternera-guisada-patatas", "Ternera guisada con patatas", "Guiso de ternera magra con patata y zanahoria.", "lunch", 420, 138, 10f, 13f, 5.5f, "gluten-free,lactose-free,high-protein,traditional"),
        D("fideua-marisco", "Fideua de marisco", "Fideos con caldo de pescado y marisco.", "lunch", 390, 170, 9f, 22f, 5.5f, "lactose-free,traditional", containsSeafood: true),
        D("arroz-cubana", "Arroz a la cubana", "Arroz blanco con tomate, huevo y platano.", "lunch", 380, 155, 5f, 28f, 4f, "vegetarian,gluten-free,lactose-free,traditional"),
        D("estofado-pavo-verduras", "Estofado de pavo con verduras", "Pavo guisado con verduras y patata.", "lunch", 400, 120, 13f, 11f, 3f, "gluten-free,lactose-free,high-protein,low-fat"),
        D("pasta-pesto-pollo", "Pasta al pesto con pollo", "Pasta con pesto, pollo y tomate cherry.", "lunch", 420, 190, 10f, 22f, 8f, "high-protein", containsNuts: true),
        D("judias-verdes-jamon-huevo", "Judias verdes con jamon y huevo", "Judias verdes con huevo cocido y jamon serrano.", "lunch,dinner", 320, 105, 8f, 8f, 5f, "gluten-free,lactose-free,high-protein,low-carb,traditional", containsPork: true),
        D("alubias-blancas-verduras", "Alubias blancas con verduras", "Alubias guisadas con verduras y laurel.", "lunch", 380, 124, 7.5f, 19f, 2.8f, "vegan,gluten-free,high-protein,traditional"),
        D("quiche-verduras", "Quiche de verduras", "Quiche al horno con puerro, calabacin y queso.", "lunch,dinner", 260, 225, 8f, 16f, 15f, "vegetarian"),
        D("hamburguesa-lentejas-arroz", "Hamburguesa de lentejas con arroz", "Medallon vegetal de lentejas con arroz integral.", "lunch,dinner", 360, 145, 7f, 23f, 4f, "vegan,gluten-free,high-protein"),
        D("bacalao-tomate", "Bacalao con tomate", "Bacalao guisado con tomate casero.", "lunch,dinner", 340, 112, 13f, 6f, 3f, "gluten-free,lactose-free,high-protein,low-carb,traditional", containsSeafood: true),
        D("cuscus-garbanzos-verduras", "Cuscus con garbanzos y verduras", "Cuscus especiado con garbanzos y verduras.", "lunch", 380, 150, 6.2f, 26f, 3.8f, "vegan,high-protein"),

        // Dinner
        D("tortilla-patatas", "Tortilla de patatas", "Tortilla espanola de patata y cebolla.", "dinner,lunch", 260, 170, 6f, 13f, 10f, "vegetarian,gluten-free,lactose-free,traditional"),
        D("ensalada-cesar-pollo", "Ensalada Cesar con pollo", "Lechuga, pollo, picatostes y salsa Cesar ligera.", "dinner,lunch", 300, 155, 12f, 8f, 8f, "high-protein"),
        D("crema-calabacin", "Crema de calabacin", "Crema suave de calabacin y patata sin nata.", "dinner", 300, 62, 1.8f, 8f, 2.2f, "vegan,gluten-free,low-sodium,traditional"),
        D("pescado-vapor-verduras", "Pescado al vapor con verduras", "Pescado blanco con verduras al vapor.", "dinner", 320, 88, 12f, 5f, 1.5f, "gluten-free,lactose-free,high-protein,low-carb,low-sodium", containsSeafood: true),
        D("pavo-plancha-ensalada", "Pavo a la plancha con ensalada", "Pavo a la plancha con ensalada verde.", "dinner", 300, 105, 17f, 2f, 3f, "gluten-free,lactose-free,high-protein,low-carb,low-sodium"),
        D("sopa-fideos", "Sopa de fideos", "Caldo suave con fideos y verduras.", "dinner", 300, 55, 2.5f, 9f, 1.2f, "traditional"),
        D("verduras-horno-tofu", "Verduras al horno con tofu", "Tofu dorado con verduras al horno.", "dinner,lunch", 340, 110, 7f, 9f, 5.5f, "vegan,gluten-free,high-protein,low-sodium"),
        D("wok-pollo-verduras", "Wok de pollo y verduras", "Pollo salteado con verduras crujientes.", "dinner,lunch", 330, 118, 13f, 7f, 4f, "gluten-free,lactose-free,high-protein,low-carb"),
        D("hamburguesa-casera-ensalada", "Hamburguesa casera con ensalada", "Hamburguesa de ternera con ensalada fresca.", "dinner", 360, 185, 11f, 7f, 12f, "gluten-free,lactose-free,high-protein,low-carb"),
        D("pizza-casera-vegetales", "Pizza casera con vegetales", "Pizza fina con tomate, mozzarella y verduras.", "dinner", 300, 230, 9f, 27f, 10f, "vegetarian"),
        D("sopa-minestrone", "Sopa minestrone", "Sopa italiana de verduras, legumbres y pasta.", "dinner", 330, 78, 3.5f, 13f, 1.8f, "vegan,low-sodium"),
        D("brocoli-gratinado-queso", "Brocoli gratinado con queso", "Brocoli al horno con bechamel ligera y queso.", "dinner", 260, 135, 7f, 9f, 7f, "vegetarian,gluten-free"),
        D("revuelto-setas-esparragos", "Revuelto de setas y esparragos", "Huevos revueltos con setas y esparragos.", "dinner,breakfast", 240, 115, 9f, 3f, 7f, "vegetarian,gluten-free,lactose-free,high-protein,low-carb,low-sodium"),
        D("ensalada-garbanzos-atun", "Ensalada de garbanzos y atun", "Garbanzos, atun, tomate, pimiento y huevo.", "dinner,lunch,any", 320, 145, 9f, 14f, 5f, "gluten-free,lactose-free,high-protein", containsSeafood: true),
        D("crema-calabaza", "Crema de calabaza", "Crema de calabaza asada con jengibre.", "dinner", 300, 58, 1.5f, 10f, 1.5f, "vegan,gluten-free,low-sodium,traditional"),
        D("calamares-plancha-ensalada", "Calamares a la plancha con ensalada", "Calamares con ensalada verde y limon.", "dinner,lunch", 300, 112, 15f, 4f, 4f, "gluten-free,lactose-free,high-protein,low-carb", containsSeafood: true),
        D("pechuga-pollo-limon", "Pechuga de pollo al limon", "Pollo al limon con verduras salteadas.", "dinner,lunch", 320, 110, 16f, 5f, 3f, "gluten-free,lactose-free,high-protein,low-carb,low-sodium"),
        D("berenjenas-rellenas-verduras", "Berenjenas rellenas de verduras", "Berenjena al horno rellena de verduras y arroz.", "dinner", 330, 92, 2.8f, 15f, 2.5f, "vegan,gluten-free,low-sodium,traditional"),
        D("tacos-pollo-verduras", "Tacos de pollo y verduras", "Tortillas de maiz con pollo, verduras y yogur.", "dinner,any", 320, 165, 11f, 18f, 6f, "gluten-free,high-protein"),
        D("salteado-heura-verduras", "Salteado de heura con verduras", "Proteina vegetal salteada con verduras.", "dinner,lunch", 320, 128, 13f, 7f, 4.5f, "vegan,high-protein,low-carb"),
        D("gazpacho-andaluz-huevo", "Gazpacho andaluz con huevo", "Gazpacho frio con huevo cocido picado.", "dinner,lunch", 300, 70, 3.8f, 7f, 3f, "vegetarian,gluten-free,lactose-free,low-sodium,traditional"),
        D("dorada-plancha-verduras", "Dorada a la plancha con verduras", "Dorada con verduras salteadas.", "dinner,lunch", 330, 108, 13f, 5f, 3.5f, "gluten-free,lactose-free,high-protein,low-carb,low-sodium", containsSeafood: true),
        D("arroz-salteado-tofu", "Arroz salteado con tofu", "Arroz integral con tofu y verduras.", "dinner,lunch", 360, 132, 6.5f, 20f, 3.5f, "vegan,gluten-free,high-protein"),
        D("ensalada-quinoa-verduras", "Ensalada de quinoa y verduras", "Quinoa con tomate, pepino, pimiento y aceitunas.", "dinner,lunch,any", 300, 120, 4.2f, 17f, 4f, "vegan,gluten-free,low-sodium"),
        D("crema-puerros-patata", "Crema de puerros y patata", "Crema ligera de puerros y patata.", "dinner", 300, 70, 1.7f, 11f, 2f, "vegan,gluten-free,low-sodium"),

        // Snack
        D("fruta-temporada", "Fruta de temporada", "Pieza o bol pequeno de fruta fresca.", "snack,midmorning,latenight", 180, 60, 0.7f, 14f, 0.2f, "vegan,gluten-free,low-sodium"),
        D("yogur-natural", "Yogur natural", "Yogur natural sin azucar.", "snack,midmorning,latenight", 125, 64, 4f, 5f, 3.3f, "vegetarian,gluten-free,low-sodium"),
        D("frutos-secos-punado", "Frutos secos", "Punado pequeno de nueces y almendras.", "snack,midmorning", 30, 610, 18f, 12f, 55f, "vegan,gluten-free,high-protein,low-carb,low-sodium", containsNuts: true),
        D("tortita-arroz-crema-cacahuete", "Tortita de arroz con crema de cacahuete", "Tortita de arroz con crema de cacahuete.", "snack,midmorning", 60, 370, 11f, 36f, 20f, "vegan,gluten-free,high-protein", containsNuts: true),
        D("hummus-zanahoria-apio", "Hummus con zanahoria y apio", "Hummus con bastones de zanahoria y apio.", "snack,midmorning,any", 160, 128, 5.5f, 14f, 5.6f, "vegan,gluten-free,low-sodium"),
        D("queso-fresco-tomate", "Queso fresco con tomate", "Queso fresco con tomate y oregano.", "snack,midmorning", 160, 95, 9f, 4f, 4.5f, "vegetarian,gluten-free,high-protein,low-carb,low-sodium"),
        D("chocolate-negro-85", "Chocolate negro 85%", "Chocolate negro en racion pequena.", "snack,latenight", 25, 580, 9f, 22f, 46f, "vegan,gluten-free,low-sodium"),
        D("galletas-avena-caseras", "Galletas de avena caseras", "Galletas caseras de avena y platano.", "snack,midmorning", 70, 310, 7f, 48f, 9f, "vegan,low-sodium"),
        D("smoothie-verde", "Smoothie verde", "Batido de espinaca, manzana y limon.", "snack,midmorning", 250, 46, 1.2f, 10f, 0.4f, "vegan,gluten-free,low-sodium"),
        D("batido-proteina", "Batido de proteina", "Batido de leche con proteina whey.", "snack,midmorning,latenight", 300, 105, 12f, 9f, 2f, "vegetarian,gluten-free,high-protein,low-sodium"),
        D("bocadillo-pequeno-pavo", "Bocadillo pequeno de pavo", "Pan integral con pechuga de pavo.", "snack,midmorning,any", 160, 205, 13f, 25f, 4.5f, "high-protein"),
        D("zanahoria-guacamole", "Zanahoria con guacamole", "Bastones de zanahoria con guacamole suave.", "snack,midmorning", 150, 112, 1.8f, 10f, 7f, "vegan,gluten-free,low-sodium"),
        D("kefir-frutos-rojos", "Kefir con frutos rojos", "Kefir natural con frutos rojos.", "snack,midmorning,latenight", 200, 72, 4.5f, 8f, 2.5f, "vegetarian,gluten-free,low-sodium"),
        D("tostada-atun", "Tostada de atun", "Tostada integral con atun y tomate.", "snack,midmorning,any", 150, 185, 13f, 18f, 5f, "high-protein", containsSeafood: true),
        D("datiles-nueces", "Datiles con nueces", "Datiles rellenos de nuez.", "snack,latenight", 60, 380, 6f, 48f, 18f, "vegan,gluten-free,low-sodium", containsNuts: true),

        // Any
        D("ensalada-mixta", "Ensalada mixta", "Lechuga, tomate, cebolla, zanahoria y huevo.", "any", 260, 72, 3.5f, 6f, 4f, "vegetarian,gluten-free,lactose-free,low-sodium,traditional"),
        D("sandwich-integral-pavo", "Sandwich integral de pavo", "Sandwich de pan integral con pavo y tomate.", "any,midmorning,snack", 210, 190, 12f, 24f, 5f, "high-protein"),
        D("wrap-pollo", "Wrap de pollo", "Wrap de pollo con lechuga, tomate y yogur.", "any,lunch,dinner", 280, 170, 12f, 18f, 6f, "high-protein"),
        D("wrap-vegetal-hummus", "Wrap vegetal de hummus", "Tortilla con hummus y verduras crudas.", "any,lunch,dinner", 260, 152, 5.5f, 22f, 5f, "vegan"),
        D("omelette-verduras", "Omelette de verduras", "Omelette con calabacin, pimiento y cebolla.", "any,breakfast,dinner", 220, 120, 9f, 4f, 8f, "vegetarian,gluten-free,lactose-free,high-protein,low-carb,low-sodium"),
        D("arroz-integral-atun-tomate", "Arroz integral con atun y tomate", "Arroz integral con atun, tomate y aceitunas.", "any,lunch,dinner", 320, 142, 9f, 19f, 4f, "gluten-free,lactose-free,high-protein", containsSeafood: true),
        D("bowl-mediterraneo-pollo", "Bowl mediterraneo de pollo", "Pollo, arroz, verduras, aceitunas y yogur.", "any,lunch,dinner", 380, 155, 11f, 18f, 5.5f, "gluten-free,high-protein"),
        D("bowl-vegano-quinoa", "Bowl vegano de quinoa", "Quinoa, garbanzos, verduras y tahini.", "any,lunch,dinner", 360, 145, 6.2f, 19f, 5.8f, "vegan,gluten-free,high-protein", containsNuts: true),
        D("crema-legumbres", "Crema de legumbres", "Crema de lentejas y verduras.", "any,lunch,dinner", 320, 98, 5.6f, 14f, 2.8f, "vegan,gluten-free,high-protein,low-sodium"),
        D("tosta-salmon-queso", "Tosta de salmon y queso", "Tosta integral con salmon ahumado y queso crema.", "any,breakfast,snack", 170, 240, 12f, 18f, 11f, "high-protein", containsSeafood: true),
        D("ensalada-pasta-verduras", "Ensalada de pasta y verduras", "Pasta fria con verduras y aceite de oliva.", "any,lunch,dinner", 300, 150, 4.5f, 23f, 4.5f, "vegan,low-sodium"),
        D("burrito-alubias", "Burrito de alubias", "Tortilla de maiz con alubias, arroz y pico de gallo.", "any,lunch,dinner", 320, 155, 6f, 25f, 4.5f, "vegan,gluten-free,high-protein"),
        D("escalivada-pan", "Escalivada con pan", "Pimiento, berenjena y cebolla asados con pan.", "any,dinner,lunch", 260, 120, 3f, 18f, 4f, "vegan,traditional,low-sodium"),
        D("patatas-bravas-horno", "Patatas bravas al horno", "Patata asada con salsa brava casera.", "any,lunch,dinner", 260, 142, 2.5f, 24f, 4.5f, "vegan,gluten-free,lactose-free,traditional"),
        D("tosta-escalivada-anchoas", "Tosta de escalivada con anchoas", "Pan con escalivada y anchoas.", "any,lunch,dinner", 170, 185, 9f, 20f, 7f, "high-protein,traditional", containsSeafood: true),
    ];

    private static Dish D(
        string id,
        string name,
        string description,
        string suitableSlots,
        float defaultPortionG,
        float kcalPer100g,
        float proteinPer100g,
        float carbsPer100g,
        float fatsPer100g,
        string tags,
        bool containsPork = false,
        bool containsSeafood = false,
        bool containsNuts = false)
    {
        var allTags = tags
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (allTags.Contains("vegan"))
        {
            allTags.Add("vegetarian");
            allTags.Add("lactose-free");
        }

        if (!containsPork)
        {
            allTags.Add("no-pork");
        }

        if (!containsSeafood)
        {
            allTags.Add("no-seafood");
        }

        if (!containsNuts)
        {
            allTags.Add("nut-free");
        }

        return new Dish
        {
            Id = id,
            Name = name,
            Description = description,
            SuitableSlots = suitableSlots,
            DefaultPortionG = defaultPortionG,
            KcalPer100g = kcalPer100g,
            ProteinPer100g = proteinPer100g,
            CarbsPer100g = carbsPer100g,
            FatsPer100g = fatsPer100g,
            Tags = string.Join(',', allTags.OrderBy(x => x, StringComparer.Ordinal)),
        };
    }
}
