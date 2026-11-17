QuizMaster --- ონლაინ ტესტირების სისტემა
QuizMaster არის ონლაინ ტესტირების სისტემა. ლექტორებს შეუძლიათ ტესტების,
კითხვებისა და პასუხების ვარიანტების შექმნა და მართვა, ტესტების
გამოქვეყნება ან დახურვა და სტატისტიკის ნახვა. სტუდენტებს შეუძლიათ
გამოქვეყნებული ტესტების გავლა, პასუხების გაგზავნა, ავტომატურად დათვლილი
შედეგების მიღება, მცდელობების ისტორიის ნახვა და რეიტინგის დათვალიერება.

პროექტის backend შექმნილია C# / ASP.NET Core Web API (.NET 10)
ტექნოლოგიებით, Entity Framework Core-ისა და Microsoft SQL
Server-ის გამოყენებით. Frontend შექმნილია HTML, CSS და
Vanilla JavaScript-ით და განთავსებულია API პროექტის wwwroot
საქაღალდეში.
ძირითადი ფუნქციები
- JWT-ზე დაფუძნებული ავთენტიფიკაცია და როლებზე დაფუძნებული
  ავტორიზაცია.
- მომხმარებლის როლები: Student, Lecturer, Admin და SuperAdmin.
- ლექტორის ანგარიშის დამტკიცების პროცესი, რომელსაც ადმინისტრატორები
  მართავენ.
- ლექტორის მიერ ტესტების მართვა: შექმნა, ნახვა, განახლება, წაშლა,
  გამოქვეყნება და დახურვა.
- კითხვების ტიპები: Single Choice, Multiple Choice და True/False.
- პასუხების ვარიანტების მართვა, მათ შორის სწორი პასუხების მონიშვნა.
- სტუდენტებისთვის გამოქვეყნებულ ტესტებსა და ტესტირების მცდელობებზე
  წვდომა.
- დროის ლიმიტის შემოწმება და ქულის გამოთვლა სერვერის მხარეს.
- სტუდენტისთვის განკუთვნილი კითხვების DTO-ებში სწორი პასუხის
  აღმნიშვნელი IsCorrect ველი არ იგზავნება.
- მცდელობების ისტორია, ტესტების სტატისტიკა და რეიტინგები.
- პროფილის ფოტოს ატვირთვა და პაროლის შეცვლა.
- EF Core migrations და მონაცემთა ბაზის საწყისი მონაცემებით შევსება
  (seeding).
- გლობალური შეცდომების დამუშავება ProblemDetails პასუხებით.
- Unit ტესტები xUnit-ისა და Moq-ის გამოყენებით.
Solution-ის სტრუქტურა
Solution მოიცავს ხუთ პროექტს:
QuizMaster.API/             ASP.NET Core-ის მთავარი პროექტი, controllers,
                            middleware, კონფიგურაცია, seeding, ფოტოების შენახვა
                            და wwwroot-ში განთავსებული frontend
QuizMaster.Domain/          Entities, enums და domain-ის ინტერფეისები
QuizMaster.Service/         ბიზნესლოგიკა, service-ინტერფეისები, წესები და exceptions
QuizMaster.Infrastructure/  EF Core DbContext, migrations, repositories,
                            JWT token service და DTO validators
QuizMaster.Tests/           ცალკეული ბიზნესსერვისების unit ტესტები
არქიტექტურა და მოთხოვნის დამუშავების პროცესი
პროექტში პასუხისმგებლობები ფენებად არის დაყოფილი:
1. API იღებს HTTP მოთხოვნებს controllers-ის საშუალებით, ამოწმებს
   ავთენტიფიკაციასა და ავტორიზაციას და აბრუნებს HTTP პასუხებს.
2. Service ასრულებს ბიზნესლოგიკას და მართავს აპლიკაციის ოპერაციებს.
3. Infrastructure უზრუნველყოფს repository-ებთან მუშაობას, Entity
   Framework Core-ის საშუალებით მონაცემების შენახვას და JWT token-ების
   გენერირებას.
4. Domain შეიცავს ძირითად entities-სა და საერთო domain ტიპებს.
5. Tests ამოწმებს შერჩეული სერვისების მუშაობას mock ობიექტების
   გამოყენებით.
მოთხოვნის ტიპური ნაკადი:
Frontend → Controller → Service → Repository → QuizMasterDbContext → SQL Server
                         ↑
                    Domain entities / DTOs
გამოყენებული ტექნოლოგიები
- C# და .NET 10
- ASP.NET Core Web API
- Entity Framework Core 10
- Microsoft SQL Server
- JWT Bearer Authentication
- FluentValidation
- Swagger / OpenAPI
- HTML, CSS და Vanilla JavaScript
- xUnit, Moq და Coverlet
წინაპირობები
აპლიკაციის გაშვებამდე დააინსტალირეთ:
- .NET 10 SDK
- Microsoft SQL Server (პროექტში გამოყენებული development connection
  string მიუთითებს instance-ზე MSSQLSERVER01)
- SQL Server Management Studio (არასავალდებულოა, მონაცემთა ბაზის
  დასათვალიერებლად)
კონფიგურაცია
1. მონაცემთა ბაზასთან კავშირი
გახსენით QuizMaster.API/appsettings.json და შეამოწმეთ
ConnectionStrings:DefaultConnection. პროექტში ამჟამად მითითებულია
შემდეგი development connection string:
"DefaultConnection": "Server=.\\MSSQLSERVER01;Database=QuizMasterDB;Trusted_Connection=True;TrustServerCertificate=True"
თუ თქვენს კომპიუტერზე SQL Server-ის სხვა instance ან ავტორიზაციის მეთოდი
გამოიყენება, შეცვალეთ შესაბამისი პარამეტრები. დარწმუნდით, რომ SQL Server
გაშვებულია და Trusted_Connection=True-ის გამოყენების შემთხვევაში
Windows Authentication დაშვებულია.
2. JWT signing key
აპლიკაციას სჭირდება მინიმუმ 32 byte სიგრძის JWT გასაღები. რეალური
გასაღები არ შეინახოთ appsettings.json-ში ან source control-ში.
გამოიყენეთ .NET User Secrets და ბრძანება გაუშვით repository-ის root
საქაღალდიდან:
dotnet user-secrets set "Jwt:Key" "REPLACE_WITH_A_RANDOM_SECRET_OF_AT_LEAST_32_BYTES" --project QuizMaster.API
მაგალითში მოცემული მნიშვნელობა მხოლოდ placeholder-ია. გამოიყენეთ პირადი,
შემთხვევითად გენერირებული მნიშვნელობა.
3. SuperAdmin-ის საწყისი მონაცემები
აპლიკაციის გაშვებისას DbSeeder კონფიგურაციიდან კითხულობს
Seed:SuperAdminEmail და Seed:SuperAdminPassword მნიშვნელობებს.
ელფოსტას appsettings.json-ში აქვს development-ისთვის განკუთვნილი
ნაგულისხმევი მნიშვნელობა; პაროლი უსაფრთხოდ დააყენეთ User Secrets-ის
გამოყენებით:
dotnet user-secrets set "Seed:SuperAdminPassword" "YOUR_LOCAL_SUPERADMIN_PASSWORD" --project QuizMaster.API
საჭიროების შემთხვევაში, საწყისი SuperAdmin-ის ელფოსტაც შეგიძლიათ
შეცვალოთ:
dotnet user-secrets set "Seed:SuperAdminEmail" "your-admin-email@example.com" --project QuizMaster.API
არ გამოაქვეყნოთ რეალური პაროლები, JWT გასაღებები ან production გარემოს
connection string-ები.
აპლიკაციის გაშვება
ბრძანებები გაუშვით იმ საქაღალდიდან, სადაც მდებარეობს
QuizMaster.API.slnx:
dotnet restore
dotnet build
dotnet run --project QuizMaster.API
აპლიკაციის გაშვებისას DbSeeder.SeedAsync იძახებს
Database.MigrateAsync()-ს, რის შედეგადაც მონაცემთა ბაზაში ავტომატურად
გამოიყენება არსებული EF Core migrations. ასევე მოწმდება საჭირო როლების
არსებობა და იქმნება ან ახლდება კონფიგურაციაში მითითებული SuperAdmin
ანგარიში.

API-ის გაშვების პროფილები განსაზღვრულია ფაილში
QuizMaster.API/Properties/launchSettings.json. მოწოდებულ პროექტში
გამოიყენება:
- HTTPS: https://localhost:7138
- HTTP: http://localhost:5256
Development გარემოში Swagger UI ხელმისაწვდომია მისამართზე:
https://localhost:7138/swagger
Frontend განთავსებულია QuizMaster.API/wwwroot საქაღალდეში. აპლიკაციის
გაშვების შემდეგ გახსენით API-ის ძირითადი მისამართი.
მონაცემთა ბაზა და ER დიაგრამა
პროექტში გამოყენებული development connection string-ის მიხედვით,
მონაცემთა ბაზის სახელია QuizMasterDB. EF Core Code First migrations
ინახება QuizMaster.Infrastructure/Migrations საქაღალდეში.

მონაცემთა ბაზის ძირითადი ცხრილები/entities:
- Roles
- Users
- Quizzes
- Questions
- AnswerOptions
- QuizAttempts
- StudentAnswers
- QuizResults
ER დიაგრამა, Primary Key-ების, Foreign Key-ებისა და კავშირების ჩათვლით,
აღწერილია ფაილში [`ERD.md`](ERD.md). GitHub ამ Markdown ფაილში Mermaid
ER დიაგრამას ვიზუალურად აჩვენებს.
API-ის მიმოხილვა
ძირითადი API route-ები მოცემულია ქვემოთ. ზუსტი request/response
DTO-ებისა და სქემების სანახავად გამოიყენეთ Swagger.
  განყოფილება       მეთოდი            Route                                                 წვდომა
  ავთენტიფიკაცია    POST              /api/Auth/register                                  საჯარო

  ავთენტიფიკაცია    POST              /api/Auth/login                                     საჯარო

  ავთენტიფიკაცია    POST              /api/Auth/change-password                           ავტორიზებული

  ტესტები           GET               /api/Quiz                                           Lecturer

  ტესტები           POST              /api/Quiz                                           Lecturer

  ტესტები           GET               /api/Quiz/{id}                                      Lecturer

  ტესტები           PUT               /api/Quiz/{id}                                      Lecturer

  ტესტები           DELETE            /api/Quiz/{id}                                      Lecturer

  ტესტები           PATCH             /api/Quiz/{id}/publish                              Lecturer

  ტესტები           PATCH             /api/Quiz/{id}/close                                Lecturer

  კითხვები          GET/POST          /api/Question                                       Lecturer

  კითხვები          GET/PUT/DELETE    /api/Question/{id}                                  Lecturer

  პასუხების         GET/POST          /api/AnswerOption/question/{questionId}             Lecturer
ვარიანტები                                                                                

  პასუხების         PUT/DELETE        /api/AnswerOption/{id}                              Lecturer
ვარიანტები                                                                                

  სტუდენტის ტესტები GET               /api/StudentQuizzes                                 Student

  სტუდენტის ტესტები POST              /api/StudentQuizzes/{quizId}/start                  Student

  მცდელობის         GET               /api/StudentQuizzes/attempt/{attemptId}/questions   Student
კითხვები                                                                                  (მფლობელი)

  პასუხების         POST              /api/StudentAnswers/attempt/{attemptId}/submit      Student
გაგზავნა                                                                                  (მფლობელი)

  მცდელობის         POST              /api/StudentQuizzes/attempt/{attemptId}/finish      Student
დასრულება                                                                                 (მფლობელი)

  მცდელობების       GET               /api/StudentQuizzes/history                         Student
ისტორია                                                                                   

  რეიტინგი          GET               /api/quizzes/{quizId}/leaderboard                   ავტორიზებული

  ტესტის სტატისტიკა GET               /api/quizzes/{quizId}/statistics                    Lecturer

  პროფილი           GET               /api/Profile/me                                     ავტორიზებული

  პროფილის ფოტო     POST/DELETE       /api/Profile/photo                                  ავტორიზებული

  ადმინისტრირება    GET               /api/Admin/users                                    Admin /
SuperAdmin

  ადმინისტრირება    GET               /api/Admin/pending-lecturers                        Admin /
SuperAdmin
SuperAdmin        POST              /api/SuperAdmin/create-admin                        SuperAdmin
უსაფრთხოება და ბიზნესწესები
- API-ის შესაბამის endpoint-ებზე გამოიყენება JWT Bearer ავთენტიფიკაცია
  და როლებზე დაფუძნებული ავტორიზაცია.
- პაროლები ინახება hash-ის სახით ASP.NET Core-ის
  IPasswordHasher<User>-ის გამოყენებით.
- სტუდენტისთვის განკუთვნილი კითხვების DTO-ები არ შეიცავს IsCorrect
  თვისებას.
- მცდელობის მფლობელსა და დროის ლიმიტს backend ამოწმებს; client-side
  შემოწმება სანდო წყაროდ არ ითვლება.
- ტესტის ქულას backend ითვლის. კლიენტის მიერ გაგზავნილი ქულა სანდოდ არ
  მიიჩნევა.
- უნიკალური filtered index უზრუნველყოფს, რომ ერთ სტუდენტს ერთი და
  იმავე ტესტისთვის ერთდროულად მხოლოდ ერთი InProgress მცდელობა
  ჰქონდეს.
- QuizResult.AttemptId-ზე უნიკალური index უზრუნველყოფს, რომ ერთ
  მცდელობაზე მხოლოდ ერთი შედეგი შეიქმნას.
- მომხმარებლის დეაქტივაციისას გამოიყენება IsDeleted ველი და იზრდება
  TokenVersion; token-ის ვალიდაციისას მოწმდება მომხმარებლის
  მიმდინარე ვერსია და წაშლილი/დეაქტივირებული მდგომარეობა.
- კონფიგურაციის საიდუმლო მნიშვნელობები ლოკალურად უნდა მიეწოდოს User
  Secrets-ის ან შესაბამისი secret manager-ის საშუალებით.
Migrations
Migrations ინახება QuizMaster.Infrastructure/Migrations საქაღალდეში.
EF Core მოდელის შეცვლის შემდეგ ახალი migration-ის შესაქმნელად,
solution-ის root საქაღალდიდან გაუშვით:
dotnet ef migrations add MigrationName --project QuizMaster.Infrastructure --startup-project QuizMaster.API --output-dir Migrations
მონაცემთა ბაზის ხელით გასაახლებლად გამოიყენეთ:
dotnet ef database update --project QuizMaster.Infrastructure --startup-project QuizMaster.API
აპლიკაცია გაშვებისას ასევე ავტომატურად იყენებს pending migrations-ს
database seeder-ის საშუალებით.
ტესტები
ყველა unit ტესტის გასაშვებად solution-ის root საქაღალდიდან გამოიყენეთ:
dotnet test
ტესტების პროექტში გამოყენებულია xUnit და Moq. არსებული test classes
ამოწმებს პროფილის ფოტოს, ტესტებისა და ტესტირების მცდელობების სერვისების
შერჩეულ სცენარებს.
დემოს რეკომენდებული თანმიმდევრობა
1. შედით SuperAdmin ანგარიშით და შექმენით ან გადაამოწმეთ Admin
   ანგარიში.
2. შედით Admin ანგარიშით და დაამტკიცეთ მომლოდინე Lecturer ანგარიში.
3. შედით Lecturer ანგარიშით, შექმენით ტესტი და დაამატეთ Single Choice,
   Multiple Choice და True/False ტიპის კითხვები.
4. გამოაქვეყნეთ ტესტი.
5. შედით Student ანგარიშით, დაიწყეთ გამოქვეყნებული ტესტი და გაგზავნეთ
   პასუხები.
6. დაასრულეთ მცდელობა და აჩვენეთ ავტომატურად დათვლილი შედეგი და
   მცდელობების ისტორია.
7. აჩვენეთ რეიტინგი და ლექტორისთვის ხელმისაწვდომი ტესტის სტატისტიკა.
Repository-ის შემადგენლობა
Repository მოიცავს solution-სა და project ფაილებს, source code-ს, EF
Core migrations-ს, seed ლოგიკას, frontend ფაილებსა და unit ტესტებს.
ლოკალური build შედეგები, IDE-ის ფაილები და საიდუმლო კონფიგურაციები
repository-ში არ უნდა აიტვირთოს.