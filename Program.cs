using System;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace spatial_test
{
	class Program
	{
		static void Main(string[] args)
		{
			var dbSql = new MyContext(new DbContextOptionsBuilder<MyContext>()
				.UseSqlServer("server=.\\SQLEXPRESS; Database=spatialtest; Trusted_Connection=Yes;",
				x => x.UseNetTopologySuite())
			.Options);

			var dbNpg = new MyContext(new DbContextOptionsBuilder<MyContext>()
				.UseNpgsql("Host=localhost;Database=spatialtest;Username=postgres;Password=secret",
				x => x.UseNetTopologySuite())
			.Options);

			Console.WriteLine("SQL Server");
			DoTest(dbSql);

			Console.WriteLine("\nPostgres with PostGIS");
			DoTest(dbNpg);
		}

		static void DoTest(MyContext ctx)
		{
			ctx.Database.EnsureDeleted();
			ctx.Database.EnsureCreated();
			ctx.Places.Add(new Place { Name = "Oxford", Point = new Point(-1.2577263, 51.7520209) { SRID = 4326 } });
			ctx.Places.Add(new Place { Name = "Swindon", Point = new Point(-1.7797176, 51.5557739) { SRID = 4326 } });
			ctx.Places.Add(new Place { Name = "Reading", Point = new Point(-0.9781303, 51.4542645) { SRID = 4326 } });
			ctx.SaveChanges();

			var reading = new Point(-0.9781303, 51.4542645) { SRID = 4326 };

			var distances = ctx.Places.Select(x => new { x.Name, Distance = x.Point.Distance(reading) }).ToList();

			distances.ForEach(x => Console.WriteLine($"{x.Name:-10} {x.Distance}"));
		}
	}

	public class Place
	{
		public int PlaceID { get; set; }
		public string Name { get; set; }
		public Point Point { get; set; }
	}

	public class MyContext : DbContext
	{
		public DbSet<Place> Places { get; set; }

		public MyContext(DbContextOptions<MyContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.HasPostgresExtension("postgis");
			if (Database.IsNpgsql()) builder.Entity<Place>().Property(x => x.Point).HasColumnType("geography (point)");
		}
	}
}
