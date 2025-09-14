var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Hamekoz_Api_Example>("hamekoz-api-example");

builder.AddProject<Projects.Hamekoz_Auth_Service_Api>("hamekoz-auth-service-api");

builder.Build().Run();
