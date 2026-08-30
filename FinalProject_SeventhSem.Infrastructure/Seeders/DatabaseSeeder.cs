using FinalProject_SeventhSem.Application.Common.Settings;
using FinalProject_SeventhSem.Application.Interfaces;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Enums;
using FinalProject_SeventhSem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;


namespace FinalProject_SeventhSem.Infrastructure.Seeders;

public class DatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly AdminSeedSettings _adminSettings;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        AppDbContext context,
        IPasswordService passwordService,
        IOptions<AdminSeedSettings> adminSettings,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _passwordService = passwordService;
        _adminSettings = adminSettings.Value;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Running database seeder...");

        await SeedAdminAsync(ct);
        await SeedSkillsAsync(ct);
        await SeedStacksChaptersQuestionsAsync(ct);
        await SeedResourcesAsync(ct);

        _logger.LogInformation("Database seeder complete.");
    }


    private async Task SeedAdminAsync(CancellationToken ct)
    {
        if (await _context.Users.AnyAsync(u => u.Role == UserRole.Admin, ct))
        {
            _logger.LogInformation("Admin already exists — skipping admin seed.");
            return;
        }

        var adminUser = new User
        {
            Email = _adminSettings.Email.ToLowerInvariant(),
            PasswordHash = _passwordService.Hash(_adminSettings.Password),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(adminUser);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Admin created — Email: {Email} | Password: {Password}",
            _adminSettings.Email, _adminSettings.Password);
    }


    private async Task SeedSkillsAsync(CancellationToken ct)
    {
        if (await _context.Skills.AnyAsync(ct))
        {
            _logger.LogInformation("Skills already seeded — skipping.");
            return;
        }

        var skillData = new Dictionary<string, string[]>
        {
            ["C#"] = ["csharp", "c-sharp", "dotnet-csharp"],
            ["C"] = ["clang", "c-language", "c-programming"],      
            ["C++"] = ["cplusplus", "cpp", "c-plus-plus", "cplus"],
            ["JavaScript"] = ["js", "javascript","javascripts(basics)", "ecmascript"],
            ["TypeScript"] = ["ts", "typescript"],
            ["Python"] = ["python3", "py"],
            ["Java"] = ["java8", "java11", "java17"],
            ["SQL"] = ["sql-server", "mssql", "tsql"],
            ["HTML"] = ["html5"],
            ["CSS"] = ["css3", "stylesheet"],

            ["ASP.NET Core"] = ["aspnet", "aspnetcore", "asp-net-core", "dotnet-web"],

            ["ASP.NET Core Web API"] = ["aspnetcorewebapi", "asp-net-core-web-api"], 

            ["ASP.NET Core MVC"] = ["aspnetcoremvc", "asp-net-core-mvc"],
            ["Entity Framework Core"] = ["ef", "efcore", "entity-framework"],
            ["React"] = ["reactjs", "react-js"],
            ["React Native"] = ["react-native", "rn", "expo", "cross-platform-mobile", "mobile-react"],
            ["Flutter"] = ["flutter", "dart", "flutter-sdk", "cross-platform-flutter", "mobile-flutter"],
            ["iOS Development"] = ["ios", "swiftui", "uikit", "xcode", "apple-ios", "iphone-dev"],
            ["Android Development"] = ["android", "jetpack-compose", "android-studio", "kotlin-android", "android-sdk"],
            ["Angular"] = ["angularjs", "angular-js", "ng"],
            ["Vue.js"] = ["vue", "vuejs", "vue-js"],
            ["Node.js"] = ["nodejs", "node-js", "node"],
            ["Express.js"] = ["express", "expressjs"],
            ["Django"] = ["django-rest", "drf", "django REST Framework"],
            ["Spring Boot"] = ["spring", "springboot", "spring-framework"],
            ["Laravel"] = ["php-laravel"],


            ["SQL Server"] = ["mssql-server", "sqlserver", "ms-sql"],
            ["PostgreSQL"] = ["postgres", "postgresql", "psql"],
            ["MySQL"] = ["mysql-db"],
            ["MongoDB"] = ["mongo", "mongodb-nosql"],
            ["Redis"] = ["redis-cache"],
            ["SQLite"] = ["sqlite3"],

            ["Docker"] = ["docker-container", "containerization"],
            ["Git"] = ["git-scm", "version-control"],
            ["GitHub"] = ["github-actions"],
            ["REST API"] = ["restapi", "rest", "restful","restfuls", "web-api"],
            ["GraphQL"] = ["graphql-api"],
            ["Postman"] = ["api-testing"],
            ["Visual Studio"] = ["vs2022", "visual-studio"],
            ["VS Code"] = ["vscode", "visual-studio-code"],

            ["Clean Architecture"] = ["clean-arch", "onion-architecture"],
            ["CQRS"] = ["command-query", "cqrs-pattern"],
            ["Repository Pattern"] = ["repository", "repo-pattern"],
            ["MVC"] = ["model-view-controller", "mvc-pattern"],
            ["Microservices"] = ["microservice", "micro-services"],
            ["SOLID Principles"] = ["solid", "solid-design"],

            ["Unit Testing"] = ["unit-test", "xunit", "nunit", "mstest"],
            ["Integration Testing"] = ["integration-test"],

            ["Linux"] = ["ubuntu", "linux-os", "bash"],
            ["Agile"] = ["scrum", "agile-methodology", "kanban"],
            ["JWT"] = ["json-web-token", "jwt-auth", "bearer-token", "jws", "jwt-claims"],
            ["Machine Learning"] = ["ml", "machine-learning", "supervised", "unsupervised", "sklearn"],
        };

        foreach (var (skillName, aliases) in skillData)
        {
            var skill = new Skill { Name = skillName, CreatedAt = DateTime.UtcNow };
            _context.Skills.Add(skill);
            await _context.SaveChangesAsync(ct); 

            foreach (var alias in aliases)
            {
                _context.SkillAliases.Add(new SkillAlias
                {
                    SkillId = skill.Id,
                    Alias = alias.ToLowerInvariant(),
                    CreatedAt = DateTime.UtcNow
                });
            }
            await _context.SaveChangesAsync(ct);
        }

        _logger.LogInformation("Seeded {Count} skills.", skillData.Count);
    }




    public string PreprocessText(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return string.Empty;

        var text = Regex.Replace(rawText, @"-\s*\n\s*", "");

        text = Regex.Replace(text, @"[\r\n]+", " ");

        text = Regex.Replace(text, @"\bC#(?!\w)", "csharp", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bC\+\+(?!\w)", "cplusplus", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\b\.NET\b", "dotnet", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bASP\.NET\b", "aspnetcore", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bNode\.js\b", "nodejs", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bVue\.js\b", "vuejs", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bExpress\.js\b", "expressjs", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bNext\.js\b", "nextjs", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bC\b", "clang"); 

        text = Regex.Replace(text, @"[^a-zA-Z0-9\s]", " ");

        text = Regex.Replace(text, @"\s{2,}", " ");

        return text.ToLowerInvariant().Trim();
    }

    private async Task SeedStacksChaptersQuestionsAsync(CancellationToken ct)
    {
        if (await _context.Stacks.AnyAsync(ct))
        {
            _logger.LogInformation("Stacks already seeded — skipping.");
            return;
        }

        var data = GetStackSeedData();

        foreach (var (stackName, chapters) in data)
        {
            var stack = new Stack { Name = stackName, CreatedAt = DateTime.UtcNow };
            _context.Stacks.Add(stack);
            await _context.SaveChangesAsync(ct);

            foreach (var (chapterName, questions) in chapters)
            {
                var chapter = new Chapter
                {
                    StackId = stack.Id,
                    Name = chapterName,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Chapters.Add(chapter);
                await _context.SaveChangesAsync(ct);

                foreach (var q in questions)
                {
                    _context.Questions.Add(new Question
                    {
                        ChapterId = chapter.Id,
                        Text = q.Text,
                        OptionA = q.A,
                        OptionB = q.B,
                        OptionC = q.C,
                        OptionD = q.D,
                        CorrectOption = q.Correct,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync(ct);
            }
        }

        _logger.LogInformation("Seeded stacks, chapters, and questions.");
    }


    private async Task SeedResourcesAsync(CancellationToken ct)
    {
        if (await _context.Resources.AnyAsync(ct))
        {
            _logger.LogInformation("Resources already seeded — skipping.");
            return;
        }

        var skills = await _context.Skills.ToListAsync(ct);
        int? SkillId(string name) =>
            skills.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Id;

        var resources = new[]
        {
            new { Title = "C# Fundamentals — Microsoft Learn",        Url = "https://learn.microsoft.com/en-us/dotnet/csharp/",          Type = "Course",   Skills = new[]{ "C#" } },
            new { Title = "ASP.NET Core — Official Docs",             Url = "https://learn.microsoft.com/en-us/aspnet/core/",            Type = "Docs",     Skills = new[]{ "ASP.NET Core", "REST API" } },
            new { Title = "Entity Framework Core — Getting Started",  Url = "https://learn.microsoft.com/en-us/ef/core/get-started/",   Type = "Docs",     Skills = new[]{ "Entity Framework Core" } },
            new { Title = "Clean Architecture — Jason Taylor GitHub", Url = "https://github.com/jasontaylordev/CleanArchitecture",       Type = "Template", Skills = new[]{ "Clean Architecture", "CQRS" } },
            new { Title = "React — Official Tutorial",                Url = "https://react.dev/learn",                                  Type = "Course",   Skills = new[]{ "React", "JavaScript" } },
            new { Title = "TypeScript — Handbook",                    Url = "https://www.typescriptlang.org/docs/handbook/",            Type = "Docs",     Skills = new[]{ "TypeScript" } },
            new { Title = "Docker — Getting Started",                 Url = "https://docs.docker.com/get-started/",                    Type = "Course",   Skills = new[]{ "Docker" } },
            new { Title = "Git — Pro Git Book (Free)",                Url = "https://git-scm.com/book/en/v2",                          Type = "Book",     Skills = new[]{ "Git" } },
            new { Title = "SQL Server — T-SQL Fundamentals",          Url = "https://learn.microsoft.com/en-us/sql/t-sql/",            Type = "Docs",     Skills = new[]{ "SQL", "SQL Server" } },
            new { Title = "PostgreSQL — Tutorial",                    Url = "https://www.postgresqltutorial.com/",                     Type = "Course",   Skills = new[]{ "PostgreSQL" } },
            new { Title = "Python — Official Tutorial",               Url = "https://docs.python.org/3/tutorial/",                     Type = "Docs",     Skills = new[]{ "Python" } },
            new { Title = "REST API Design Best Practices",           Url = "https://restfulapi.net/",                                 Type = "Article",  Skills = new[]{ "REST API" } },
            new { Title = "JWT Authentication Guide",                 Url = "https://jwt.io/introduction",                             Type = "Article",  Skills = new[]{ "JWT" } },
            new { Title = "SOLID Principles — DotNetTutorials",      Url = "https://dotnettutorials.net/course/solid-design-principles/", Type = "Course", Skills = new[]{ "SOLID Principles" } },
            new { Title = "Unit Testing with xUnit — Microsoft",      Url = "https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test", Type = "Docs", Skills = new[]{ "Unit Testing" } },
            new { Title = "MongoDB — University Free Courses",        Url = "https://learn.mongodb.com/",                              Type = "Course",   Skills = new[]{ "MongoDB" } },
            new { Title = "Agile & Scrum — Scrum.org",               Url = "https://www.scrum.org/resources/what-scrum-guide",        Type = "Docs",     Skills = new[]{ "Agile" } },
            new { Title = "Node.js — Official Docs",                  Url = "https://nodejs.org/en/docs",                              Type = "Docs",     Skills = new[]{ "Node.js" } },
            new { Title = "Vue.js 3 — Official Guide",                Url = "https://vuejs.org/guide/introduction",                   Type = "Docs",     Skills = new[]{ "Vue.js" } },
            new { Title = "Linux Command Line Basics",                Url = "https://ubuntu.com/tutorials/command-line-for-beginners", Type = "Course",   Skills = new[]{ "Linux" } },
        };

        foreach (var r in resources)
        {
            var resource = new Resource
            {
                Title = r.Title,
                Url = r.Url,
                ResourceType = r.Type,
                CreatedAt = DateTime.UtcNow
            };
            _context.Resources.Add(resource);
            await _context.SaveChangesAsync(ct);

            foreach (var skillName in r.Skills)
            {
                var sid = SkillId(skillName);
                if (sid.HasValue)
                {
                    _context.ResourceSkillMappings.Add(new ResourceSkillMapping
                    {
                        ResourceId = resource.Id,
                        SkillId = sid.Value,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await _context.SaveChangesAsync(ct);
        }

        _logger.LogInformation("Seeded {Count} resources.", resources.Length);
    }


    private record QuestionData(string Text, string A, string B, string C, string D, string Correct);

    private static Dictionary<string, Dictionary<string, QuestionData[]>> GetStackSeedData()
    {
        return new Dictionary<string, Dictionary<string, QuestionData[]>>
        {
            [".NET"] = new()
            {
                ["C# Basics"] = new[]
                {
                new QuestionData("What keyword is used to prevent a class from being inherited in C#?", "abstract", "static", "sealed", "readonly", "C"),
                new QuestionData("Which of the following is a value type in C#?", "string", "object", "int", "class", "C"),
                new QuestionData("What does the 'async' keyword do in C#?", "Makes a method run on a separate thread", "Marks a method that can use 'await' for asynchronous operations", "Forces a method to return void", "Prevents a method from throwing exceptions", "B"),
                new QuestionData("What is the output of: Console.WriteLine(10 / 3); in C#?", "3.33", "3", "4", "3.0", "B"),
                new QuestionData("Which access modifier makes a member visible only within the same class?", "internal", "protected", "private", "public", "C"),
                new QuestionData("What is boxing in C#?", "Converting a reference type to a value type", "Converting a value type to an object reference", "Wrapping a method in a delegate", "Casting between numeric types", "B"),
                new QuestionData("Which collection type in C# ensures unique elements?", "List<T>", "Queue<T>", "HashSet<T>", "Stack<T>", "C"),
                new QuestionData("What is the difference between 'ref' and 'out' parameters in C#?", "'ref' requires initialisation before passing; 'out' does not", "'out' requires initialisation before passing; 'ref' does not", "They are identical", "'ref' is for value types only", "A"),
                new QuestionData("What does the '??' operator do in C#?", "Checks for null and throws an exception", "Returns the left operand if not null, otherwise returns the right", "Performs a null-safe method call", "Converts a nullable type to non-nullable", "B"),
                new QuestionData("Which keyword is used to define an interface in C#?", "abstract", "interface", "contract", "protocol", "B"),
            },
                ["Entity Framework Core"] = new[]
                {
                new QuestionData("What method is used to apply pending migrations in EF Core?", "Database.Update()", "Database.Migrate()", "Database.Apply()", "Database.Sync()", "B"),
                new QuestionData("What does DbContext represent in EF Core?", "A database table", "A session with the database", "A migration file", "A connection string", "B"),
                new QuestionData("Which attribute marks a property as the primary key in EF Core?", "[ForeignKey]", "[Required]", "[Key]", "[Index]", "C"),
                new QuestionData("What is the purpose of a migration in EF Core?", "To run stored procedures", "To seed the database with data", "To track and apply database schema changes", "To configure connection strings", "C"),
                new QuestionData("Which EF Core method adds a new entity to be tracked for insertion?", "Update()", "Attach()", "Add()", "Include()", "C"),
                new QuestionData("What does AsNoTracking() do in EF Core?", "Prevents the entity from being deleted", "Returns entities without change tracking for read-only queries", "Disables lazy loading", "Removes an entity from the context", "B"),
                new QuestionData("Which loading strategy fetches related data in a separate query automatically?", "Eager loading", "Explicit loading", "Lazy loading", "Deferred loading", "C"),
                new QuestionData("What method is used to eagerly load related entities in EF Core?", "ThenLoad()", "Include()", "Join()", "Attach()", "B"),
                new QuestionData("What does SaveChangesAsync() do in EF Core?", "Rolls back all tracked changes", "Persists all tracked changes to the database", "Detaches all entities", "Begins a transaction", "B"),
                new QuestionData("Which Fluent API method configures a one-to-many relationship?", "HasOne().WithOne()", "HasMany().WithOne()", "HasOne().WithMany()", "HasMany().WithMany()", "B"),
            },
                ["ASP.NET Core"] = new[]
                {
                new QuestionData("What interface is used to configure the HTTP request pipeline in ASP.NET Core?", "IServiceCollection", "IApplicationBuilder", "IWebHostEnvironment", "IConfiguration", "B"),
                new QuestionData("What attribute restricts an API endpoint to authenticated users?", "[AllowAnonymous]", "[Authorize]", "[ApiController]", "[Route]", "B"),
                new QuestionData("Which HTTP method is typically used to update a resource partially?", "PUT", "POST", "PATCH", "DELETE", "C"),
                new QuestionData("What does middleware in ASP.NET Core do?", "Defines database models", "Processes HTTP requests and responses in a pipeline", "Manages JWT tokens", "Configures Dependency Injection", "B"),
                new QuestionData("What does [FromBody] do in an ASP.NET Core action parameter?", "Reads from query string", "Reads from route data", "Reads from the request body as JSON", "Reads from a form field", "C"),
                new QuestionData("What is the purpose of app.UseRouting() in ASP.NET Core?", "Enables authentication", "Adds HTTPS redirection", "Enables endpoint routing in the pipeline", "Registers controllers", "C"),
                new QuestionData("Which attribute is used to define a route template on a controller action?", "[HttpGet]", "[Route]", "[Endpoint]", "[Path]", "B"),
                new QuestionData("What does IActionResult allow in ASP.NET Core controllers?", "Only returning JSON data", "Returning different HTTP response types", "Only returning status codes", "Returning database entities directly", "B"),
                new QuestionData("What is the purpose of Model Validation in ASP.NET Core?", "To encrypt request data", "To ensure incoming data meets defined rules before processing", "To log incoming requests", "To compress responses", "B"),
                new QuestionData("Which method registers controllers and API explorer in ASP.NET Core?", "services.AddMvc()", "services.AddControllers()", "services.AddEndpoints()", "services.AddRouting()", "B"),
            },
                ["Dependency Injection"] = new[]
                {
                new QuestionData("Which service lifetime creates a new instance for every request in ASP.NET Core?", "Singleton", "Transient", "Scoped", "Pooled", "C"),
                new QuestionData("What is the purpose of Dependency Injection?", "To write faster code", "To decouple class dependencies and improve testability", "To manage database connections", "To secure API endpoints", "B"),
                new QuestionData("Which method registers a service as a singleton in ASP.NET Core DI?", "services.AddScoped()", "services.AddTransient()", "services.AddSingleton()", "services.AddHosted()", "C"),
                new QuestionData("What does IServiceCollection represent?", "A list of HTTP middleware", "A container for registered services", "A database context", "A configuration file", "B"),
                new QuestionData("What is constructor injection in ASP.NET Core?", "Injecting services via a static method", "Providing dependencies through a class constructor", "Creating services manually in each method", "Using factory methods to create objects", "B"),
                new QuestionData("What happens if you inject a Scoped service into a Singleton service?", "It works correctly", "It throws a compile-time error", "It causes a captive dependency problem", "The scoped service becomes transient", "C"),
                new QuestionData("Which interface is used to resolve services manually from the DI container?", "IServiceCollection", "IServiceProvider", "IServiceLocator", "IServiceResolver", "B"),
                new QuestionData("What is the Transient service lifetime?", "One instance for the entire application", "One instance per HTTP request", "A new instance every time it is requested", "One instance per database transaction", "C"),
                new QuestionData("Which DI method allows registering a service with a factory function?", "services.Add()", "services.AddSingleton<T>(factory)", "services.Inject<T>()", "services.Register<T>()", "B"),
                new QuestionData("What does the Options pattern in ASP.NET Core provide?", "A way to handle HTTP errors", "Strongly typed access to configuration settings", "A method for caching responses", "A way to register middleware", "B"),
            },
            },

            ["JavaScript"] = new()
            {
                ["JS Fundamentals"] = new[]
                {
                new QuestionData("What is the result of typeof null in JavaScript?", "null", "undefined", "object", "string", "C"),
                new QuestionData("Which method removes the last element from an array?", "shift()", "pop()", "splice()", "delete()", "B"),
                new QuestionData("What does '===' check in JavaScript?", "Value equality only", "Type equality only", "Both value and type equality", "Reference equality only", "C"),
                new QuestionData("What is a closure in JavaScript?", "A loop that never ends", "A function that has access to its outer scope's variables", "A way to close the browser", "A built-in array method", "B"),
                new QuestionData("What does the 'const' keyword do in JavaScript?", "Creates a block-scoped variable that can be reassigned", "Creates a function-scoped variable", "Creates a block-scoped variable that cannot be reassigned", "Creates a global variable", "C"),
                new QuestionData("What is the event loop in JavaScript?", "A loop for handling DOM events only", "A mechanism that allows JS to perform non-blocking operations", "A built-in loop for arrays", "A way to repeat code indefinitely", "B"),
                new QuestionData("What does the spread operator (...) do in JavaScript?", "Creates a new function", "Expands an iterable into individual elements", "Merges two functions", "Declares a rest parameter", "B"),
                new QuestionData("Which method creates a new array with elements that pass a test?", "map()", "reduce()", "filter()", "find()", "C"),
                new QuestionData("What is prototypal inheritance in JavaScript?", "Classes inheriting from other classes", "Objects inheriting directly from other objects via the prototype chain", "A way to copy object properties", "A pattern for module imports", "B"),
                new QuestionData("What does 'use strict' do in JavaScript?", "Enables ES6 features", "Enforces stricter parsing and error handling", "Disables all console logs", "Makes all variables global", "B"),
            },
                ["Async JavaScript"] = new[]
                {
                new QuestionData("What does a Promise represent in JavaScript?", "A synchronous operation result", "An eventual completion or failure of an asynchronous operation", "A type of loop", "A built-in class for handling arrays", "B"),
                new QuestionData("Which keyword is used to wait for a Promise to resolve?", "wait", "sync", "await", "resolve", "C"),
                new QuestionData("What is the purpose of async/await?", "To create multithreaded code", "To write asynchronous code in a synchronous style", "To speed up synchronous code", "To prevent exceptions", "B"),
                new QuestionData("Which Promise method runs all promises in parallel and waits for all?", "Promise.race()", "Promise.any()", "Promise.all()", "Promise.first()", "C"),
                new QuestionData("What does Promise.race() return?", "All resolved values", "The value of the slowest promise", "The value of the first settled promise", "An array of all results", "C"),
                new QuestionData("How do you handle errors in an async/await function?", "Using .catch() only", "Using try/catch blocks", "Using the onerror event", "Errors cannot be caught in async functions", "B"),
                new QuestionData("What is a callback function in JavaScript?", "A function that calls itself", "A function passed as an argument to another function", "A function that returns a Promise", "A function that runs on page load", "B"),
                new QuestionData("What problem does Promise.allSettled() solve compared to Promise.all()?", "It runs promises sequentially", "It resolves even if some promises reject", "It cancels failed promises", "It retries failed promises automatically", "B"),
                new QuestionData("What does the fetch() API return?", "A string of JSON data", "A Promise that resolves to a Response object", "An XML object", "A direct JavaScript object", "B"),
                new QuestionData("What is the microtask queue in JavaScript?", "A queue for setTimeout callbacks", "A high-priority queue for Promise callbacks processed before the next task", "A queue for DOM events", "A queue for network requests", "B"),
            },
                ["React"] = new[]
                {
                new QuestionData("What is JSX in React?", "A JavaScript package manager", "A syntax extension that allows HTML-like code in JavaScript", "A state management library", "A type of CSS framework", "B"),
                new QuestionData("Which hook is used to manage state in a functional React component?", "useEffect", "useContext", "useState", "useRef", "C"),
                new QuestionData("What does useEffect do in React?", "Creates a new component", "Manages local state", "Performs side effects after render", "Routes between pages", "C"),
                new QuestionData("What is the virtual DOM in React?", "A physical copy of the DOM stored in memory", "A lightweight representation of the real DOM", "A new browser API", "A database for component data", "B"),
                new QuestionData("How do you pass data from a parent to a child component in React?", "Via state", "Via props", "Via context only", "Via localStorage", "B"),
                new QuestionData("What is the purpose of the key prop in React lists?", "To style list items", "To help React identify which items have changed", "To set the order of items", "To bind event handlers", "B"),
                new QuestionData("What does useRef return in React?", "A stateful value", "A mutable object whose .current property persists across renders", "A context value", "A callback function", "B"),
                new QuestionData("What is React Context used for?", "Fetching data from APIs", "Passing data through the component tree without prop drilling", "Managing component animations", "Handling form submissions", "B"),
                new QuestionData("When does useEffect run with an empty dependency array []?", "On every render", "Only once after the initial render", "Never", "Only when props change", "B"),
                new QuestionData("What is the purpose of React.memo()?", "To memoize expensive calculations", "To prevent unnecessary re-renders of functional components", "To cache API responses", "To manage global state", "B"),
            },
            },

            ["Python"] = new()
            {
                ["Python Basics"] = new[]
                {
                new QuestionData("What is the correct way to define a function in Python?", "function myFunc():", "def myFunc():", "func myFunc():", "define myFunc():", "B"),
                new QuestionData("Which data type is immutable in Python?", "list", "dict", "set", "tuple", "D"),
                new QuestionData("What does len() return?", "The last element of a list", "The number of elements in an object", "The memory size of an object", "The type of an object", "B"),
                new QuestionData("What is a Python decorator?", "A way to style Python code", "A function that modifies the behaviour of another function", "A built-in data type", "A loop construct", "B"),
                new QuestionData("What does 'self' refer to in a Python class method?", "The class itself", "The current instance of the class", "The parent class", "A static variable", "B"),
                new QuestionData("What is list comprehension in Python?", "A way to document lists", "A concise way to create lists using an expression and loop", "A method to sort lists", "A way to delete list elements", "B"),
                new QuestionData("What does the 'pass' keyword do in Python?", "Exits a loop", "Skips to the next iteration", "Acts as a placeholder for empty code blocks", "Returns None from a function", "C"),
                new QuestionData("Which method is called when an object is created from a class?", "__create__", "__new__", "__init__", "__start__", "C"),
                new QuestionData("What does the 'yield' keyword do in Python?", "Returns a value and exits the function", "Pauses a function and returns a value to the caller, resuming later", "Throws an exception", "Declares a global variable", "B"),
                new QuestionData("What is the difference between a list and a tuple in Python?", "Lists are faster than tuples", "Tuples are mutable, lists are immutable", "Lists are mutable, tuples are immutable", "There is no difference", "C"),
            },
                ["Django"] = new[]
                {
                new QuestionData("What command creates a new Django project?", "django create project", "django-admin startproject", "python manage.py newproject", "django init", "B"),
                new QuestionData("What is the purpose of models.py in Django?", "To define URL routes", "To define database models", "To configure middleware", "To write HTML templates", "B"),
                new QuestionData("What does ORM stand for in Django?", "Object Routing Module", "Object Relational Mapper", "Online Resource Manager", "Open Request Model", "B"),
                new QuestionData("Which file maps URL patterns to views in Django?", "views.py", "models.py", "urls.py", "settings.py", "C"),
                new QuestionData("What command applies database migrations in Django?", "python manage.py migrate", "python manage.py runmigrations", "django migrate", "python manage.py db update", "A"),
                new QuestionData("What is Django's MTV pattern?", "Model-Template-View", "Model-Type-View", "Middleware-Template-View", "Model-Transfer-Variable", "A"),
                new QuestionData("What does the Django admin site provide?", "A REST API for the frontend", "An auto-generated interface for managing application data", "A built-in authentication API", "A deployment dashboard", "B"),
                new QuestionData("What is the purpose of Django's settings.py?", "To define database models", "To store application configuration including database and installed apps", "To define URL patterns", "To write view logic", "B"),
                new QuestionData("Which Django class-based view is used to display a list of objects?", "DetailView", "CreateView", "ListView", "TemplateView", "C"),
                new QuestionData("What does makemigrations do in Django?", "Applies schema changes to the database", "Creates migration files based on model changes", "Deletes all existing migrations", "Seeds the database", "B"),
            },
            },

            ["Database"] = new()
            {
                ["SQL Fundamentals"] = new[]
                {
                new QuestionData("Which SQL clause filters rows after grouping?", "WHERE", "HAVING", "GROUP BY", "ORDER BY", "B"),
                new QuestionData("What does a PRIMARY KEY constraint ensure?", "Values are not null only", "Values are unique and not null", "Values are foreign keys", "Values are indexed only", "B"),
                new QuestionData("Which JOIN returns all rows from both tables including unmatched rows?", "INNER JOIN", "LEFT JOIN", "RIGHT JOIN", "FULL OUTER JOIN", "D"),
                new QuestionData("What does the DISTINCT keyword do in SQL?", "Orders the result set", "Removes duplicate rows from results", "Filters null values", "Joins two tables", "B"),
                new QuestionData("Which SQL command is used to modify existing data?", "INSERT", "DELETE", "UPDATE", "ALTER", "C"),
                new QuestionData("What does the GROUP BY clause do in SQL?", "Filters individual rows", "Sorts the result set", "Groups rows sharing a value for aggregate functions", "Joins multiple tables", "C"),
                new QuestionData("Which aggregate function returns the number of rows?", "SUM()", "AVG()", "COUNT()", "MAX()", "C"),
                new QuestionData("What is a subquery in SQL?", "A stored procedure", "A query nested inside another query", "A view definition", "A type of JOIN", "B"),
                new QuestionData("What does the BETWEEN operator do in SQL?", "Checks for null values", "Selects values within a given range", "Compares two strings", "Joins two tables", "B"),
                new QuestionData("Which SQL statement is used to remove all rows from a table without logging individual deletions?", "DELETE", "DROP", "TRUNCATE", "REMOVE", "C"),
            },
                ["Database Design"] = new[]
                {
                new QuestionData("What is database normalisation?", "Speeding up database queries", "Organising data to reduce redundancy and dependency", "Encrypting database tables", "Creating database backups", "B"),
                new QuestionData("What is a foreign key?", "A key used to encrypt data", "A column that references the primary key of another table", "A unique identifier within the same table", "A type of index", "B"),
                new QuestionData("Which normal form eliminates transitive dependencies?", "1NF", "2NF", "3NF", "BCNF", "C"),
                new QuestionData("What is an index in a database?", "A backup copy of a table", "A data structure that improves query performance", "A constraint on column values", "A type of JOIN", "B"),
                new QuestionData("What is an Entity-Relationship (ER) diagram?", "A chart showing server performance", "A visual representation of entities and their relationships in a database", "A diagram of network topology", "A flowchart of SQL queries", "B"),
                new QuestionData("What does ACID stand for in database transactions?", "Atomicity, Consistency, Isolation, Durability", "Accuracy, Consistency, Integrity, Durability", "Atomicity, Concurrency, Isolation, Data", "Access, Consistency, Integrity, Distribution", "A"),
                new QuestionData("What is a composite key?", "A key made up of encrypted values", "A primary key consisting of two or more columns", "A foreign key referencing multiple tables", "A key used for full-text search", "B"),
                new QuestionData("What is denormalisation?", "Removing all indexes from a table", "Intentionally introducing redundancy to improve read performance", "Converting a relational database to NoSQL", "Deleting duplicate rows", "B"),
                new QuestionData("What does a one-to-many relationship mean in database design?", "One record in table A maps to one record in table B", "One record in table A maps to many records in table B", "Many records in table A map to many records in table B", "Tables share the same primary key", "B"),
                new QuestionData("What is referential integrity?", "Ensuring all data is encrypted", "Ensuring foreign key values always reference a valid primary key", "Ensuring column names are unique", "Ensuring tables have at least one index", "B"),
            },
            },

            ["Web Development"] = new()
            {
                ["HTML & CSS"] = new[]
                {
                new QuestionData("Which HTML tag is used to create a hyperlink?", "<link>", "<href>", "<a>", "<url>", "C"),
                new QuestionData("What does CSS stand for?", "Computer Style Sheets", "Cascading Style Sheets", "Creative Style Sheets", "Colorful Style Sheets", "B"),
                new QuestionData("Which CSS property controls the space inside an element's border?", "margin", "padding", "border-spacing", "spacing", "B"),
                new QuestionData("What is Flexbox in CSS?", "A JavaScript animation library", "A CSS layout model for distributing space along a single axis", "A CSS preprocessor", "A browser extension", "B"),
                new QuestionData("What does the z-index CSS property control?", "The zoom level of an element", "The vertical stacking order of elements", "The horizontal position", "The font size", "B"),
                new QuestionData("What is the CSS Box Model?", "A 3D rendering model", "A model describing the rectangular boxes around HTML elements including margin, border, padding, and content", "A CSS grid layout technique", "A method for animating elements", "B"),
                new QuestionData("Which HTML element is used to define the structure of a web page's navigation?", "<div>", "<section>", "<nav>", "<aside>", "C"),
                new QuestionData("What does the CSS 'position: absolute' property do?", "Positions an element relative to the browser window", "Removes an element from the normal flow, positioned relative to its nearest positioned ancestor", "Fixes an element to the viewport", "Makes an element sticky while scrolling", "B"),
                new QuestionData("What is CSS Grid used for?", "Styling text elements", "Creating two-dimensional layouts with rows and columns", "Adding animations to elements", "Importing fonts", "B"),
                new QuestionData("What does the HTML 'alt' attribute on an image do?", "Sets the image size", "Provides alternative text if the image cannot be displayed", "Links the image to a URL", "Adds a tooltip on hover", "B"),
            },
                ["REST API"] = new[]
                {
                new QuestionData("Which HTTP status code indicates a successful resource creation?", "200 OK", "201 Created", "204 No Content", "301 Moved Permanently", "B"),
                new QuestionData("What does REST stand for?", "Remote Execution of Services and Tasks", "Representational State Transfer", "Reliable Endpoint Service Technology", "Resource Endpoint Specification Tool", "B"),
                new QuestionData("Which HTTP method is idempotent and used to retrieve data?", "POST", "PUT", "GET", "PATCH", "C"),
                new QuestionData("What does a 404 HTTP status code mean?", "Server error", "Unauthorised", "Resource not found", "Bad request", "C"),
                new QuestionData("What is the purpose of an HTTP header?", "To store response body data", "To carry metadata about the request or response", "To define URL routes", "To compress response data", "B"),
                new QuestionData("What does a 401 HTTP status code mean?", "Forbidden", "Not Found", "Unauthorised", "Bad Gateway", "C"),
                new QuestionData("What is CORS in web development?", "A CSS layout technique", "A security feature controlling which origins can access a server's resources", "A JavaScript runtime", "A caching strategy", "B"),
                new QuestionData("Which HTTP method is used to completely replace a resource?", "POST", "PATCH", "DELETE", "PUT", "D"),
                new QuestionData("What is an API rate limit?", "The maximum size of a request body", "A restriction on how many API requests a client can make in a given time period", "A limit on response time", "A cap on the number of endpoints", "B"),
                new QuestionData("What does idempotent mean in the context of HTTP methods?", "The request always returns the same data", "Making the same request multiple times produces the same result as making it once", "The request never modifies data", "The request is always cached", "B"),
            },
            },
        };
    }


}