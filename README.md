# CompetitionPlatform

A website to help manage Projects Competitions.

# How to create the configuration file?

* Add these settings in the environmental variables on the host:

  *  AzureStorageConnString - the main connection string that is used to store information about projects, users, etc.
  *  AzureStorageLogConnString - points to the storage that is used for logging.
  *  SettingsContainerName - name of the blob container where the global settings file is stored.
  *  SettingsFileName - name of the global settings file ( including extension ).

* Create the global settings file using this [template](https://github.com/LykkeCity/CompetitionPlatform/blob/master/generalsettings_template.json)

* Fill the fields for the authentication server:

  *  ClientId - id fields of the application.
  *  ClientSecret - secret field associated with the clientId.
  *  PostLogoutRedirectUri - link to the authentication server.
  *  Authority - link to the authentication server.

* Upload the global settings file to the server where the AzureStorageConnString is pointing. The filename should equal SettingsFileName and the Blob container name that it is uploaded to should equal SettingsContainerName.
