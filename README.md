# CompetitionPlatform

A website to help manage Projects Competitions.

# How to create the configuration file?

First you have to add these settings in the environmental variables on the host:

* AzureStorageConnString - the main connection string that is used to store information about projects, users, etc.
* AzureStorageLogConnString - points to the storage that is used for logging.
* SettingsContainerName - name of the blob container where the global settings file is stored.
* SettingsFileName - name of the global settings file ( including extension ).

The configuration file will be fetched using AzureStorageConnString connection string. The filename should equal SettingsFileName and the Blob container name that it is uploaded to should equal SettingsContainerName.

Get the template for the settings file [here](https://github.com/LykkeCity/CompetitionPlatform/blob/master/generalsettings_template.json)

Fill the fields for the authentication server:

* ClientId - id fields of the application.
* ClientSecret - secret field associated with the clientId.
* PostLogoutRedirectUri - link to the authentication server.
* Authority - link to the authentication server.
