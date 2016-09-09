# CompetitionPlatform

A website to help manage Projects Competitions.

# AppSettings

appsettings.json is structured like this: 

ConnectionStrings contains AzureStorage and AzureStorageLog connection strings.

AzureStorage is a main connection string that is used to store information about projects, users, etc.

AzureStorageLog points to a storage that is used for logging.

# generalsettings.json

Get the template for the settins file [here](https://github.com/LykkeCity/CompetitionPlatform/blob/master/generalsettings_template.json)

Fill the fields for authentication server:

* ClientId - id fields of the application.
* ClientSecret - secret fields associated with the clientId.
* PostLogoutRedirectUri - link to the authentication server.
* Authority - link to the authentication server.

