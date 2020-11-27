To re-scaffold the database use the following commands from Package Manager COnsole:

cd .\Authentication   (make sure Default project is set to Authentication)
Scaffold-DbContext 'Data Source=.\SQLEXPRESS;Initial Catalog=Sentinel;Trusted_connection=true' Microsoft.EntityFrameworkCore.SqlServer -context ApplicationContext -tables User,Role,UserRole -OutputDir Models -contextdir Models -force

After re-scaffolding there are two things you need to do:
- Comment out the line in ApplicationContext.cs that tries to set the connection string
- Comment out the default constructor in the generated User.cs class