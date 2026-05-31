using Microsoft.EntityFrameworkCore;
using MyRACIT.Data;

namespace MyRACIT.Tests.Helpers;

public static class DbFactory
{
    public static MyRacitDbContext Create(string name)
    {
        var opts = new DbContextOptionsBuilder<MyRacitDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new MyRacitDbContext(opts);
    }
}
