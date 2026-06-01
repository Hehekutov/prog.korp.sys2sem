using BlazorShared;

namespace BlazorServer.Data;

public static class DbInitializer
{
    public static void Seed(AppDbContext db)
    {
        if (db.Products.Any())
        {
            return;
        }

        db.Products.AddRange(
            new Product
            {
                Name = "Ноутбук Orion 14",
                Category = "Электроника",
                Price = 84990,
                Quantity = 12,
                Description = "Лёгкий ноутбук для учебы, офиса и командировок."
            },
            new Product
            {
                Name = "Смартфон Nord S",
                Category = "Электроника",
                Price = 39990,
                Quantity = 24,
                Description = "Смартфон с AMOLED-экраном и быстрой зарядкой."
            },
            new Product
            {
                Name = "Кресло ErgoLine",
                Category = "Офис",
                Price = 18490,
                Quantity = 7,
                Description = "Эргономичное кресло с регулировкой поясничной поддержки."
            });

        db.SaveChanges();
    }
}
