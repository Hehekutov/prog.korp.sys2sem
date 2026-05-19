using Prac5.Models;

namespace Prac5.Data;

public static class DbInitializer
{
    public static void Seed(ProductionDbContext context)
    {
        context.Database.EnsureCreated();

        if (context.Products.Any())
        {
            return;
        }

        var materials = new[]
        {
            new Material { Id = 1, Name = "Листовая сталь", Quantity = 320m, UnitOfMeasure = "кг", MinimalStock = 150m },
            new Material { Id = 2, Name = "Электронные модули", Quantity = 80m, UnitOfMeasure = "шт", MinimalStock = 40m },
            new Material { Id = 3, Name = "Порошковая краска", Quantity = 55m, UnitOfMeasure = "кг", MinimalStock = 60m },
            new Material { Id = 4, Name = "Крепеж", Quantity = 1000m, UnitOfMeasure = "шт", MinimalStock = 250m },
            new Material { Id = 5, Name = "Алюминиевый профиль", Quantity = 95m, UnitOfMeasure = "м", MinimalStock = 70m }
        };

        var products = new[]
        {
            new Product
            {
                Id = 1,
                Name = "Промышленный шкаф управления",
                Description = "Сборный шкаф для размещения контроллеров и силовой автоматики.",
                Specifications = "{\"power\":\"380V\",\"protection\":\"IP54\"}",
                Category = "Электрооборудование",
                MinimalStock = 10,
                ProductionTimePerUnit = 45
            },
            new Product
            {
                Id = 2,
                Name = "Конвейерный модуль",
                Description = "Механический модуль для транспортировки заготовок между участками.",
                Specifications = "{\"length\":\"2m\",\"load\":\"120kg\"}",
                Category = "Механика",
                MinimalStock = 4,
                ProductionTimePerUnit = 120
            },
            new Product
            {
                Id = 3,
                Name = "Датчик контроля качества",
                Description = "Компактный датчик для проверки наличия деталей на линии.",
                Specifications = "{\"interface\":\"IO-Link\",\"range\":\"0.2m\"}",
                Category = "Автоматизация",
                MinimalStock = 25,
                ProductionTimePerUnit = 30
            }
        };

        var productMaterials = new[]
        {
            new ProductMaterial { ProductId = 1, MaterialId = 1, QuantityNeeded = 8m },
            new ProductMaterial { ProductId = 1, MaterialId = 2, QuantityNeeded = 2m },
            new ProductMaterial { ProductId = 1, MaterialId = 3, QuantityNeeded = 0.7m },
            new ProductMaterial { ProductId = 1, MaterialId = 4, QuantityNeeded = 20m },
            new ProductMaterial { ProductId = 2, MaterialId = 1, QuantityNeeded = 28m },
            new ProductMaterial { ProductId = 2, MaterialId = 2, QuantityNeeded = 4m },
            new ProductMaterial { ProductId = 2, MaterialId = 3, QuantityNeeded = 1.5m },
            new ProductMaterial { ProductId = 2, MaterialId = 4, QuantityNeeded = 60m },
            new ProductMaterial { ProductId = 2, MaterialId = 5, QuantityNeeded = 6m },
            new ProductMaterial { ProductId = 3, MaterialId = 2, QuantityNeeded = 1m },
            new ProductMaterial { ProductId = 3, MaterialId = 4, QuantityNeeded = 6m }
        };

        var lines = new[]
        {
            new ProductionLine { Id = 1, Name = "Линия A1 - сборка шкафов", Status = ProductionLineStatus.Active, EfficiencyFactor = 1.10f, CurrentWorkOrderId = 1 },
            new ProductionLine { Id = 2, Name = "Линия B2 - механическая сборка", Status = ProductionLineStatus.Active, EfficiencyFactor = 0.85f },
            new ProductionLine { Id = 3, Name = "Линия C3 - тестирование", Status = ProductionLineStatus.Stopped, EfficiencyFactor = 1.00f }
        };

        var today = DateTime.Today;
        var orders = new[]
        {
            new WorkOrder
            {
                Id = 1,
                ProductId = 1,
                ProductionLineId = 1,
                Quantity = 8,
                StartDate = today.AddHours(9),
                EstimatedEndDate = today.AddHours(14).AddMinutes(30),
                Status = WorkOrderStatus.InProgress,
                ProgressPercent = 35
            },
            new WorkOrder
            {
                Id = 2,
                ProductId = 3,
                ProductionLineId = 2,
                Quantity = 20,
                StartDate = today.AddDays(1).AddHours(10),
                EstimatedEndDate = today.AddDays(1).AddHours(21).AddMinutes(45),
                Status = WorkOrderStatus.Pending,
                ProgressPercent = 0
            },
            new WorkOrder
            {
                Id = 3,
                ProductId = 2,
                Quantity = 3,
                StartDate = today.AddDays(2).AddHours(8),
                EstimatedEndDate = today.AddDays(2).AddHours(14),
                Status = WorkOrderStatus.Pending,
                ProgressPercent = 0
            }
        };

        context.Materials.AddRange(materials);
        context.Products.AddRange(products);
        context.ProductMaterials.AddRange(productMaterials);
        context.ProductionLines.AddRange(lines);
        context.WorkOrders.AddRange(orders);
        context.SaveChanges();
    }
}
