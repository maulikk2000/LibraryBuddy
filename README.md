# LibraryBuddy
A microservice project for book library using .NET core 

<h3>Run the following commands in Package Manager Console to generate migrations </h3>

<ul>
  <li>Add-Migration initial_identity_migration -c ApplicationDbContext -o Data/Migrations/AspNetIdentity/ApplicationDb</li>
  <li>Add-Migration initial_config_migration -c ConfigurationDbContext -o Data/Migrations/IdentityServer/ConfigurationDb</li>
  <li>Add-Migration initial_persisted_grant_migration -c PersistedGrantDbContext -o Data/Migrations/IdentityServer/PersistedGrantDb</li>
</ul>

<h3>Run the following commands in Package Manager Console to generate tables in the Database</h3>

<ul>
  <li>update-database -c ApplicationDbContext</li>
  <li>update-database -c ConfigurationDbContext</li>
  <li>update-database -c PersistedGrantDbContext</li>
</ul>




