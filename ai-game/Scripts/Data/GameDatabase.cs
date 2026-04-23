using System.Collections.Generic;
using System.Linq;

namespace AiGame.Data;

public sealed class GameDatabase
{
    public SceneSkinConfig Skin { get; private set; } = new();
    public List<ProductConfig> Products { get; private set; } = new();
    public List<CustomerConfig> Customers { get; private set; } = new();
    public List<DecorConfig> Decors { get; private set; } = new();
    public List<BlessingConfig> Blessings { get; private set; } = new();

    public static GameDatabase Load()
    {
        var database = new GameDatabase
        {
            Skin = JsonLoader.Load<SceneSkinConfig>("res://Data/scene_skin.json"),
            Products = JsonLoader.Load<ProductConfigList>("res://Data/products.json").Items,
            Customers = JsonLoader.Load<CustomerConfigList>("res://Data/customers.json").Items,
            Decors = JsonLoader.Load<DecorConfigList>("res://Data/decors.json").Items,
            Blessings = JsonLoader.Load<BlessingConfigList>("res://Data/blessings.json").Items,
        };

        return database;
    }

    public ProductConfig? GetProduct(string id) => Products.FirstOrDefault(x => x.Id == id);
    public CustomerConfig? GetCustomer(string id) => Customers.FirstOrDefault(x => x.Id == id);
    public DecorConfig? GetDecor(string id) => Decors.FirstOrDefault(x => x.Id == id);
    public BlessingConfig? GetBlessing(string id) => Blessings.FirstOrDefault(x => x.Id == id);

    public IEnumerable<ProductConfig> GetProductsForRun(int shiftLevel, int reputation) =>
        Products.Where(x => x.UnlockShift <= shiftLevel && x.UnlockReputation <= reputation);

    public IEnumerable<CustomerConfig> GetCustomersForRun(int shiftLevel) =>
        Customers.Where(x => x.UnlockShift <= shiftLevel);

    public IEnumerable<DecorConfig> GetDecorsForRun(int shiftLevel) =>
        Decors.Where(x => x.UnlockShift <= shiftLevel);

    public IEnumerable<BlessingConfig> GetBlessingsForRun(int shiftLevel) =>
        Blessings.Where(x => x.UnlockShift <= shiftLevel);
}
