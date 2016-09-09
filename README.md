# CompetitionPlatform

A website to help manage Projects Competitions.

# AppSettings

Get the template for the appsettings file [here](https://github.com/LykkeCity/CompetitionPlatform/blob/master/appsettings_template.json)

Fill the fields for authentication server:

* AzureStorage - the main connection string that is used to store information about projects, users, etc.
* AzureStorageLog - points to the storage that is used for logging.
* Container - name of the blob container where the general settings file is stored.
* FileName - name of the general settings file ( including extension ).

# General Settings

Will be fetched using AzureStorage connection string ( from appsettings or overwritten in environmental variables ). Should be uploaded to "settings" folder in Azure Blob Container.

Get the template for the settins file [here](https://github.com/LykkeCity/CompetitionPlatform/blob/master/generalsettings_template.json)

Fill the fields for authentication server:

* ClientId - id fields of the application.
* ClientSecret - secret field associated with the clientId.
* PostLogoutRedirectUri - link to the authentication server.
* Authority - link to the authentication server.

