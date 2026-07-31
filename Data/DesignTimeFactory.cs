//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Design;

//namespace PingPong.API.Data
//{
//    public class DesignTimeFactory : IDesignTimeDbContextFactory<PingPongDbContext>
//    {
//        public PingPongDbContext CreateDbContext(string[] args)
//        {
//            var optionsBuilder = new DbContextOptionsBuilder<PingPongDbContext>();
//            optionsBuilder.UseNpgsql(ConfigurationBuilder.GetConnectionString("DefaultConnection"));

//            return new PingPongDbContext(optionsBuilder.Options);
//        }
//    }
//}
