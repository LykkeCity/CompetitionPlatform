# CompetitionPlatform

A website to help manage Projects Competitions.

# How to create the configuration file?

* Add these settings in the environmental variables on the host:

  *  SettingsConnString - Connection string that points to the azure blob that contains the main settings file.
  *  SettingsContainerName - Name of the blob container where the global settings file is stored.
  *  SettingsFileName - Name of the global settings file ( including extension ).

* Create the global settings file using this [template](https://github.com/LykkeCity/CompetitionPlatform/blob/master/generalsettings_template.json)

* Fill the Azure connection settings:

  *  StorageConnString - Connection string that points to azure storage that contains main project files (projects, votes, comments, etc).
  *  StorageLogConnString - Connection string that points to the azure storage that contains logs.

* Fill the fields for the authentication server:

  *  ClientId - Id field of the application.
  *  ClientSecret - Secret field associated with the clientId.
  *  PostLogoutRedirectUri - Link to the authentication server.
  *  Authority - Link to the authentication server.
  
* Fill the fields for notification settings:

  *  EmailsQueueConnString - Connection string that points to the queue which handles email notifications.
  *  SlackQueueConnString - Connection string that points to the queue which handles slack notifications.
  
* Fill the fields for recaptcha settings:

  *  SiteKey - Recaptcha site key.
  *  SecretKey - Recaptcha secret key.

* Upload the global settings file to the server where the AzureStorageConnString is pointing. The filename should equal SettingsFileName and the Blob container name that it is uploaded to should equal SettingsContainerName.
