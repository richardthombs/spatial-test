# EF Core Spatial Distance returns different values on SQL Server vs Postgres

For the SQL Server tests, I used SQL Server Express 14.0.1000.

For the Postgres tests, I used the postgis/postgis docker container launched as follows:

```
docker run --name postgres -e POSTGRES_PASSWORD=secret -d -p 5432:5432 --rm postgis/postgis
```

## The test
Using this `Place` class:

```c#
public class Place
{
	public int PlaceID { get; set; }
	public string Name { get; set; }
	public Point Point { get; set; }
}
```

I create 3 instances
```c#
ctx.Places.Add(new Place { Name = "Oxford", Point = new Point(-1.2577263, 51.7520209) { SRID = 4326 } });
ctx.Places.Add(new Place { Name = "Swindon", Point = new Point(-1.7797176, 51.5557739) { SRID = 4326 } });
ctx.Places.Add(new Place { Name = "Reading", Point = new Point(-0.9781303, 51.4542645) { SRID = 4326 } });
```

Then query the distance from a point:
```c#
var distances = ctx.Places
	.Select(x => new { x.Name, Distance = x.Point.Distance(reading) })
	.ToList();
```

I do this for SQL Server and for Postgres and get very different distances:

```
SQL Server
Oxford 38376.197446871825
Swindon 56790.50143570342
Reading 0

Postgres with PostGIS
Oxford 0.4084517070070321
Swindon 0.8079890827292466
Reading 0
```

SQL Server seems to be in meters, but I have no clue what is going on with Postgres!