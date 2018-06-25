# LibraryBuddy
A microservice project for any given library using .NET core 

Use following to create Identity, Configuration and PersistedGrant migrations. Run it from VS Package Manager Console. Here -c switch is to generate context file and -o to specify where the migration file will be generated.

  Add-Migration initial_identity_migration -c ApplicationDbContext -o Data\Migrations\AspNetIdentity\ApplicationDb
  
  Add-Migration initial_config_migration -c ConfigurationDbContext -o Data\Migrations\IdentityServer\ConfigurationDb
  
  Add-Migration initial_persister_grant_migration -c PersistedGrantDbContext -o Data\Migrations\IdentityServer\PersistedGrantDb

Create databases using following command. We need to specify "Context" switch as there are more than one context.

  update-database -c ApplicationDbContext
  
  update-database -c ConfigurationDbContext
  
  update-database -c PersistedGrantDbContext
