# LibraryAPI – ASP.NET Core Routing and Model Binding

## 1. Project Overview

This project demonstrates how routing and model binding work in ASP.NET Core Web API using a simple Library API.

The project uses an in-memory collection of books to demonstrate how HTTP requests are mapped to controller actions and how parameters are received from different parts of an HTTP request.

### Concepts Covered

1. Accessing multiple parameters using Conventional Routing.
2. Performing POST operations using Conventional Routing.
3. Accepting multiple values through a single `[FromBody]` parameter.
4. Understanding GET requests with `[FromBody]`.
5. Applying filters using `[FromRoute]` and `[FromQuery]` and comparing both approaches.

---

## 2. Project Configuration – Program.cs

The application is configured using the following code:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Books}/{action=GetAllBooks}/{id?}/{category?}/{author?}/{minPrice?}"
);

app.Run();
```

### Understanding the Route Pattern

The application uses Conventional Routing.

```csharp
"{controller=Books}/{action=GetAllBooks}/{id?}/{category?}/{author?}/{minPrice?}"
```

| Route Parameter | Purpose | Default |
|---|---|---|
| controller | Identifies the controller | Books |
| action | Identifies the action method | GetAllBooks |
| id | Identifies a book | Optional |
| category | Book category | Optional |
| author | Book author | Optional |
| minPrice | Minimum book price | Optional |

The `?` symbol indicates that a route segment is optional.

For example:

```http
GET /Books/GetBook/1/Programming
```

Maps the following values:

- Controller: Books
- Action: GetBook
- ID: 1
- Category: Programming

**Important:** Conventional routing maps URL segments according to their position in the configured route template.

---

## 3. Accessing Multiple Parameters Using Conventional Routing

### Concept

Conventional Routing allows multiple parameters to be passed through URL segments.

ASP.NET Core identifies the controller and action and maps the remaining URL segments to the corresponding route parameters.

### Controller Implementation

```csharp
[HttpGet]
public IActionResult GetBook(int id, string category)
{
    var book = books.FirstOrDefault(
        b => b.Id == id && b.Category == category
    );

    if (book == null)
        return NotFound("Book not found");

    return Ok(book);
}
```

### Postman Request

```http
GET /Books/GetBook/1/Programming
```

### How It Works

1. The request reaches the ASP.NET Core application.
2. Conventional routing identifies `BooksController`.
3. It identifies the `GetBook` action.
4. The route value `1` is mapped to `id`.
5. The route value `Programming` is mapped to `category`.
6. The action searches for a matching book.
7. The API returns the book if found; otherwise, it returns 404 Not Found.

### Key Learning

Multiple action parameters can be accessed using conventional routing without defining individual route templates on every action.

---

## 4. Performing POST Operations Using Conventional Routing

### Concept

Conventional routing is not restricted to GET requests. It can also be used with POST, PUT, and DELETE operations.

Routing identifies the controller and action, while HTTP method attributes determine which HTTP method the action accepts.

### Controller Implementation

```csharp
[HttpPost]
public IActionResult AddBook([FromBody] Book book)
{
    book.Id = books.Count + 1;

    books.Add(book);

    return Ok(book);
}
```

### Postman Request

```http
POST /Books/AddBook
```

Request body – JSON:

```json
{
    "title": "Advanced C#",
    "author": "Riya",
    "category": "Programming",
    "price": 900
}
```

### How It Works

1. The client sends a POST request.
2. Conventional routing identifies the `BooksController`.
3. It identifies the `AddBook` action.
4. `[HttpPost]` restricts the action to POST requests.
5. `[FromBody]` reads the JSON request body and converts it into a `Book` object.
6. The book is added to the in-memory collection.
7. The API returns the added book.

### Key Learning

Routing and HTTP method attributes work together to identify and execute the appropriate controller action.

---

## 5. Accepting Multiple Values Through a Single `[FromBody]` Parameter

### Concept

`[FromBody]` tells ASP.NET Core to read data from the HTTP request body and deserialize it into a .NET object.

A common requirement is to accept multiple pieces of information in one request.

For example, when adding a book, we might need its title, author, category, price, and publishing year.

Instead of using multiple `[FromBody]` parameters, we can group the required information into one DTO.

### Example DTO

```csharp
public class AddBookRequest
{
    public string Title { get; set; }
    public string Author { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public int PublishedYear { get; set; }
}
```

### Example Controller Action

```csharp
[HttpPost]
public IActionResult AddBook([FromBody] AddBookRequest request)
{
    var book = new Book
    {
        Id = books.Count + 1,
        Title = request.Title,
        Author = request.Author,
        Category = request.Category,
        Price = request.Price
    };

    books.Add(book);

    return Ok(new
    {
        Book = book,
        PublishedYear = request.PublishedYear
    });
}
```

### JSON Request

```json
{
    "title": "Advanced C#",
    "author": "Riya",
    "category": "Programming",
    "price": 900,
    "publishedYear": 2025
}
```

### Why Use a DTO?

An action method normally supports only one body-bound parameter because the request body is a single stream consumed by an input formatter.

A DTO allows us to group multiple values into one object.

### Key Learning

DTOs help organize request data, separate API contracts from database entities, and control which properties clients can submit.

**Note:** The DTO example demonstrates the approach conceptually. It is an additional implementation example and is not currently part of the supplied `BooksController`.

---

## 6. GET Request Using `[FromBody]`

### Concept

GET requests are generally used to retrieve resources from the server.

The common ways to pass parameters in GET requests are:

- `[FromRoute]` – Retrieves values from URL path segments.
- `[FromQuery]` – Retrieves values from the query string.

Although HTTP specifications do not universally prohibit content in GET requests, GET request bodies have no generally defined semantics and are not reliably supported across clients, servers, proxies, and caching systems.

ASP.NET Core MVC also does not normally support body binding for GET actions through its default behavior.

### Recommended Approach

Use `[FromQuery]` when retrieving data with search or filtering conditions.

Your existing implementation:

```csharp
[HttpGet]
public IActionResult SearchBooks(string category, string author)
{
    var result = books.Where(
        b => b.Category == category && b.Author == author
    ).ToList();

    return Ok(result);
}
```

### Postman Request

```http
GET /Books/SearchBooks?category=Programming&author=Riya
```

### How It Works

1. The client sends a GET request.
2. ASP.NET Core identifies the `SearchBooks` action.
3. Model binding retrieves `category` and `author` from the query string.
4. The action filters the book collection.
5. The API returns the matching books.

### What If We Need to Send Complex Search Data?

For complex search criteria containing multiple fields or nested objects, use a POST request with a search DTO and `[FromBody]`.

### Key Learning

GET requests should generally use route and query parameters. POST requests are more suitable when submitting complex structured request data.

---

## 7. Filtering Using `[FromRoute]` and `[FromQuery]`

### Concept

Filtering allows clients to retrieve specific records instead of receiving the entire collection.

In this project, we apply filters using:

- Category
- Author
- Minimum Price

We demonstrate two approaches: `[FromRoute]` and `[FromQuery]`.

### A. Filtering Using `[FromRoute]`

`[FromRoute]` retrieves values from the URL path.

Your configured route pattern:

```csharp
"{controller=Books}/{action=GetAllBooks}/{id?}/{category?}/{author?}/{minPrice?}"
```

### Controller Implementation

```csharp
[HttpGet]
public IActionResult FilterBooksByRoute(
    [FromRoute] string? category,
    [FromRoute] string? author,
    [FromRoute] decimal? minPrice)
{
    var result = books.AsQueryable();

    if (!string.IsNullOrEmpty(category))
    {
        result = result.Where(b => b.Category == category);
    }

    if (!string.IsNullOrEmpty(author))
    {
        result = result.Where(b => b.Author == author);
    }

    if (minPrice.HasValue)
    {
        result = result.Where(b => b.Price >= minPrice.Value);
    }

    return Ok(result.ToList());
}
```

### Postman Requests

**Filter by category:**

```http
GET /Books/FilterBooksByRoute/0/Programming
```

**Filter by category and author:**

```http
GET /Books/FilterBooksByRoute/0/Programming/Riya
```

**Filter by category, author, and minimum price:**

```http
GET /Books/FilterBooksByRoute/0/Programming/Riya/500
```

### Why Is `0` Used in the URL?

The configured route pattern contains `id` before `category`.

Therefore, the URL segments must follow this order:

| Segment | Value |
|---|---|
| id | 0 |
| category | Programming |
| author | Riya |
| minPrice | 500 |

We use `0` as a placeholder because the filtering action does not require an ID.

The `category`, `author`, and `minPrice` parameters are explicitly bound using `[FromRoute]`.

### How Does Filtering Work?

1. Initially, all books are available in the collection.
2. If category is provided, only books belonging to that category are selected.
3. If author is provided, the result is further filtered by author.
4. If minimum price is provided, books below that price are excluded.
5. The filtered collection is returned.

---

### B. Filtering Using `[FromQuery]`

`[FromQuery]` retrieves values from the query string.

Unlike route parameters, query parameters are not dependent on their position in the URL.

### Controller Implementation

```csharp
[HttpGet]
public IActionResult FilterBooksByQuery(
    [FromQuery] string? category,
    [FromQuery] string? author,
    [FromQuery] decimal? minPrice)
{
    var result = books.AsQueryable();

    if (!string.IsNullOrEmpty(category))
    {
        result = result.Where(b => b.Category == category);
    }

    if (!string.IsNullOrEmpty(author))
    {
        result = result.Where(b => b.Author == author);
    }

    if (minPrice.HasValue)
    {
        result = result.Where(b => b.Price >= minPrice.Value);
    }

    return Ok(result.ToList());
}
```

### Postman Requests

**Filter by category:**

```http
GET /Books/FilterBooksByQuery?category=Programming
```

**Filter by author:**

```http
GET /Books/FilterBooksByQuery?author=Riya
```

**Filter by category and author:**

```http
GET /Books/FilterBooksByQuery?category=Programming&author=Riya
```

**Filter by minimum price:**

```http
GET /Books/FilterBooksByQuery?minPrice=600
```

**Apply all three filters:**

```http
GET /Books/FilterBooksByQuery?category=Programming&author=Riya&minPrice=500
```

**Retrieve all books without applying filters:**

```http
GET /Books/FilterBooksByQuery
```

### How Does Filtering Work?

1. The client sends the filtering values through the query string.
2. `[FromQuery]` retrieves the corresponding values.
3. The API checks which filters have been provided.
4. Only the applicable filters are applied.
5. The matching books are returned.

### Why Are Nullable Parameters Used?

Consider:

```csharp
[FromQuery] decimal? minPrice
```

The client may or may not provide a minimum price.

If the client doesn't provide it, `minPrice` is `null`.

We use:

```csharp
if (minPrice.HasValue)
{
    result = result.Where(b => b.Price >= minPrice.Value);
}
```

This ensures that the minimum price filter is applied only when the client supplies a value.

---

### C. Difference Between `[FromRoute]` and `[FromQuery]`

| Feature | FromRoute | FromQuery |
|---|---|---|
| Data source | URL path | Query string |
| Example | `/Books/10` | `/Books?category=Programming` |
| Parameter order | Depends on route template | Can be in any order |
| Optional filters | Less convenient | More convenient |
| Multiple filters | Possible | Easy to combine |
| Common purpose | Resource identification | Filtering, searching, sorting, pagination |

### Which One Is Better?

Neither approach is universally better. The choice depends on the purpose of the parameter.

**Use `[FromRoute]` when:**

- Identifying a particular resource.
- Accessing a resource through its unique ID.
- Representing hierarchical resources.

Example:

```http
GET /Books/10
```

**Use `[FromQuery]` when:**

- Applying optional filters.
- Searching records using multiple conditions.
- Implementing sorting.
- Implementing pagination.

Example:

```http
GET /Books?category=Programming&page=1&pageSize=10
```

**Recommendation:** For collection filtering in this Library API, `[FromQuery]` is generally more suitable because filters are optional and can be combined without changing the route structure.

---

## 8. Sample Book Data

The project uses an in-memory collection containing the following records:

| ID | Title | Author | Category | Price |
|---|---|---|---|---|
| 1 | C# Basics | Aman | Programming | 500 |
| 2 | ASP.NET Core | Riya | Programming | 700 |
| 3 | Physics | Ankit | Science | 400 |
| 4 | Advanced C# | Riya | Programming | 900 |
| 5 | Chemistry | Aman | Science | 600 |

The data is stored in a static `List<Book>` inside `BooksController`.

No database is required to run these demonstrations.

---

## 9. Key Learnings

Through this practical, the following concepts are demonstrated:

1. Conventional routing can map multiple URL segments to controller action parameters.
2. Conventional routing supports different HTTP methods when combined with HTTP method attributes.
3. Routing and model binding have different responsibilities.
4. A single `[FromBody]` parameter can receive multiple fields through a DTO.
5. GET request bodies should generally be avoided for interoperability.
6. `[FromRoute]` is useful for resource identification.
7. `[FromQuery]` is useful for optional filtering, sorting, and pagination.
8. Nullable parameters represent optional filter values.
9. HTTP method attributes such as `[HttpGet]` and `[HttpPost]` restrict the HTTP methods accepted by an action.

---

## 10. Technologies Used

- C#
- ASP.NET Core Web API
- Conventional Routing
- Model Binding
- HTTP Methods (GET and POST)
- DTOs (Data Transfer Objects)
- Swagger / OpenAPI
- Postman for API testing

---

## 11. How to Run the Project

1. Clone or download the project.
2. Open the solution in Visual Studio.
3. Restore the required NuGet packages.
4. Build and run the project.
5. Open Swagger, if running in the Development environment.
6. Alternatively, use Postman to test the GET and POST endpoints.
7. Use the example URLs and JSON payloads provided in this README.

---

## Conclusion

This practical demonstrates how routing and model binding work together in ASP.NET Core Web API.

It explains how ASP.NET Core identifies controller actions, maps URL segments, retrieves query parameters, reads request bodies, and applies filtering conditions.

Understanding these concepts provides a strong foundation for developing structured, maintainable, and RESTful Web APIs.
