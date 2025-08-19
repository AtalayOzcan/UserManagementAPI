var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

/*
 özellik: global exception handler middleware eklendi

- app.UseExceptionHandler ile yakalanmayan hatalar için global hata yakalama eklendi
- Yanıt RFC 7807 standardına uygun ProblemDetails JSON formatında döndürülüyor
- API’nin çökmesi engellendi, 500 hataları artık tutarlı şekilde dönüyor
- Güvenilirlik artırıldı ve hata ayıklama süreci kolaylaştırıldı
*/
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        var problem = Results.Problem(
            title : "Unexpected problem.",
            statusCode:StatusCodes.Status500InternalServerError
            );

        await problem.ExecuteAsync(context);    
    });
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var users = new List<User>
{
    new User {Id = 1, Name = "Atalay", Age = 24, Gender = "Male"},
    new User {Id = 2, Name = "Isilay", Age = 25, Gender = "Female"}
};

app.MapGet("/", () => "Root");


app.MapGet("/users", () =>
{
    if (!users.Any())
    {
        return Results.NotFound("User not found."); 
    }

    return Results.Ok(users); 
});



app.MapGet("/users/{id}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    
    if (user != null)
    {
        return Results.Ok(user);
    }
    else
    {
        return Results.NotFound($"User with ID {id} not found.");
    }
});


app.MapPost("/users", (User newUser) =>
{
    //ID çakışmasını kontrol eder.
    //var existingUser = users.FirstOrDefault(u => u.Id == newUser.Id);
    //if(existingUser != null)
    //{
    //    return Results.Conflict("User already exist."); //Conflict 409 çevirir, sistem üzerinde çakışma olduğunu belirtir.
    //}
    // Doğrulama: Kullanıcı geçerli mi?
    try {

    var check = User.Validate(newUser, isCreate: true);
    if (!check.IsValid)
    {
        return Results.BadRequest(check.Error); // 400 + açıklama
    }

    // Any LINQ Metodu boş mu dolu mu diye kontrol ediyor, users.Any() → Eğer listede kullanıcı varsa||users.Max(u => u.Id) + 1 → Mevcut en büyük ID'nin 1 fazlasını verir
    var newId = users.Any() ? users.Max(u => u.Id) + 1 : 1; // Buradaki : 1 ifadesi ilk kullanıcı liste boş ise.
    newUser.Id = newId; // Yukarıdaki soru işareti koşul belirtir true ise max id + 1, false ise id = 1.

    users.Add(newUser);
    return Results.Created($"/users/{newUser.Id}", newUser); // 201 çevirir, kaynağın verisi.
    }
    catch(Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});


app.MapPut("/users/{id}", (int id,User updatedUser) =>
{
    try
    {

    var existingUser = users.FirstOrDefault(u => u.Id == id);

    if (existingUser == null)
    {
        return Results.NotFound($"User with ID: {id} not found.");
    }
    
    var check = User.Validate(updatedUser, isCreate: false);
    if (!check.IsValid)
        return Results.BadRequest(check.Error);

    existingUser.Name = updatedUser.Name;
    existingUser.Age = updatedUser.Age;
    existingUser.Gender = updatedUser.Gender;

    return Results.Ok(existingUser);
    }
    catch(Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});


app.MapDelete("/users/{id}", (int id) =>
{
    var existingUser = users.FirstOrDefault(u => u.Id == id);
    if (existingUser == null)
    {
        return Results.NotFound($"User with ID: {id} not found.");
    }
    users.Remove(existingUser);

    return Results.NoContent();// 204 başarıyla silindi
});


app.Run();


public class User
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int Age { get; set; }
    public required string Gender { get; set; }

    public static (bool IsValid, string? Error) Validate(User u, bool isCreate) // error mesajı boş dönebilir.
    {
        if (string.IsNullOrWhiteSpace(u.Name))
            return (false, "Name cannot be empty.");
        if (u.Age < 0 || u.Age > 120)
            return (false, "Age must be between 0 and 120.");
        if (string.IsNullOrWhiteSpace(u.Gender))
            return (false, "Gender cannot be empty.");
        if (isCreate && u.Id != 0)
            return (false, "ID should not be provided by the client.");
        return (true, null);
    }

}
