# Entity Framework Core

## 1. Entity Framework Core Deep Dive

**Core Components**:

- **DbContext**: Main class for database interaction (Unit of Work pattern)
- **DbSet<T>**: Represents a database table/collection
- **Change Tracking**: Automatic detection of entity changes
- **Migrations**: Version-controlled database schema updates

```csharp
public class AppDbContext : DbContext
{
   public DbSet<Blog> Blogs { get; set; }
   public DbSet<Post> Posts { get; set; }
   public DbSet<Tag> Tags { get; set; }
   protected override void OnConfiguring(DbContextOptionsBuilder options)
       => options.UseSqlServer("YourConnectionString");
}
```

## 2. Entity Configuration Expansion

### Data Annotations (Attribute-Based)

```csharp
[Table("BlogEntries")]
public class Blog
{
   [Key]
   [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
   public int Id { get; set; }
   [Required]
   [Column("BlogUrl", TypeName = "varchar(200)")]
   public string Url { get; set; }
   [NotMapped]
   public string TempIdentifier { get; set; }
}
```

### Fluent API (Advanced Configuration)

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
   modelBuilder.Entity<Blog>(entity =>
   {
       entity.ToTable("Blogs");
       entity.HasKey(b => b.Id);
       entity.Property(b => b.Url)
           .IsRequired()
           .HasMaxLength(200)
           .HasColumnName("BlogUrl");
   });
}
```

## 3. Many-to-Many with Conventions (EF Core 5+)

**Automatic Join Table Creation**:

```csharp
public class Blog
{
   public int BlogId { get; set; }
   public string Url { get; set; }
   // Navigation property
   public ICollection<Tag> Tags { get; set; }
}
public class Tag
{
   public int TagId { get; set; }
   public string Name { get; set; }
   // Navigation property
   public ICollection<Blog> Blogs { get; set; }
}
```

**Resulting Database Schema**:

- **Tables**:
- Blogs (BlogId, Url)
- Tags (TagId, Name)
- BlogTag (BlogsBlogId, TagsTagId) - Auto-created join table
**Convention Requirements**:

1. Both entities must have collection navigation properties
2. Collection properties must be `ICollection<T>`
3. Join table name: `<FirstEntity><SecondEntity>` (alphabetical order)

## 4. EF Core Conventions Expanded

**Key Default Behaviors**:

- **Primary Key**: Property named `Id` or `<EntityName>Id`
- **Table Names**: Matches `DbSet<T>` property name
- **String Properties**: `nvarchar(max)` NULL unless `[Required]`
- **Foreign Keys**: `<NavigationPropertyName>Id`
- **Cascade Delete**: Required relationships cascade delete

## 5. Relationships Deep Dive

### Relationship Types

| **Type**       | **Configuration**                          | **Example**                     |
|----------------|--------------------------------------------|---------------------------------|
| One-to-One     | `.HasOne().WithOne()`                     | Author ↔ AuthorBio             |
| One-to-Many    | `.HasOne().WithMany()`                    | Blog ↔ Posts                   |
| Many-to-Many   | `.HasMany().WithMany()`                   | Blogs ↔ Tags (via join table)  |

### Explicit Many-to-Many with Join Entity

```csharp
public class BlogTag
{
   public int BlogId { get; set; }
   public Blog Blog { get; set; }
   public int TagId { get; set; }
   public Tag Tag { get; set; }
}
// In DbContext
public DbSet<BlogTag> BlogTags { get; set; }
```

## 6. Loading Strategies Expanded

### Eager Loading

```csharp
var blogs = context.Blogs
   .Include(b => b.Posts)
       .ThenInclude(p => p.Comments)
   .Include(b => b.Tags)
   .ToList();
```

### Lazy Loading Requirements

1. Install `Microsoft.EntityFrameworkCore.Proxies`
2. Enable in DbContext:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder options)
   => options.UseLazyLoadingProxies()
             .UseSqlServer(connectionString);
```

### Explicit Loading

```csharp
var blog = context.Blogs.First();
context.Entry(blog)
   .Collection(b => b.Posts)
   .Load();
```

## 7. Data Annotations vs Fluent API Comparison

| **Feature**            | **Data Annotations**                     | **Fluent API**                |
|------------------------|------------------------------------------|-------------------------------|
| **Complex Types**       | `[ComplexType]`                         | `.OwnsOne()`/`.OwnsMany()`    |
| **Inheritance**         | Limited support                         | TPH/TPT configuration         |
| **Indexes**             | `[Index]` (EF Core 6+)                  | `.HasIndex()`                 |
| **Precedence**          | Overridden by Fluent API                | Final configuration           |
| **Value Conversions**   | Not supported                           | `.HasConversion()`            |

## 8. Migration Workflow Details

1. **Create Migration**:

  ```bash
  dotnet ef migrations add AddPostRating
  ```

2. **Migration File**:

  ```csharp
  public partial class AddPostRating : Migration
  {
      protected override void Up(MigrationBuilder migrationBuilder)
      {
          migrationBuilder.AddColumn<int>(
              name: "Rating",
              table: "Posts",
              type: "int",
              nullable: false,
              defaultValue: 0);
      }
  }
  ```

3. **Apply Migrations**:

  ```bash
  dotnet ef database update
  ```

## 9. Performance Best Practices

1. **No-Tracking Queries**:

  ```csharp
  var blogs = context.Blogs.AsNoTracking().ToList();
  ```

2. **Batching**:

  ```csharp
  context.AddRange(listOf1000Entities);
  await context.SaveChangesAsync();
  ```

3. **SQL Profiling**:

  ```csharp
  options.UseLoggerFactory(LoggerFactory.Create(b => b.AddConsole()));
  ```

## 10. Advanced Relationships

### Owned Entities (Complex Types)

```csharp
[Owned]
public class Address
{
   public string Street { get; set; }
   public string City { get; set; }
}
public class Author
{
   public Address BusinessAddress { get; set; }
}
```

### Table-Per-Hierarchy (TPH)

```csharp
modelBuilder.Entity<Person>()
   .HasDiscriminator<string>("PersonType")
   .HasValue<Student>("Student")
   .HasValue<Instructor>("Instructor");
```

# Key EF Core Concepts

| **Concept**            | **Description**                                                                 |
|------------------------|-------------------------------------------------------------------------------|
| **Shadow Properties**  | Properties not in entity class (`modelBuilder.Entity<T>().Property<DateTime>("CreatedDate")`) |
| **Global Query Filters**| Apply filters to all queries (`modelBuilder.Entity<T>().HasQueryFilter(...)`) |
| **Value Conversions**  | Convert property values for storage (`.HasConversion(v => v.ToString(), v => (Color)Enum.Parse(typeof(Color), v)`) |
| **Alternate Keys**     | Unique non-primary key constraints (`.HasAlternateKey(u => u.Email)`)         |
| **Raw SQL**            | Execute custom SQL (`context.Blogs.FromSqlRaw("SELECT * FROM Blogs")`)        |

```csharp
// Global Query Filter Example
modelBuilder.Entity<Blog>().HasQueryFilter(b => !b.IsDeleted);
```
