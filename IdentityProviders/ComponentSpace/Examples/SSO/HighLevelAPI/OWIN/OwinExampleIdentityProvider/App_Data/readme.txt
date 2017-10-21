If publishing to IIS, ensure the application pool process has file write permission to the App_Data folder
so the LocalDb database files may be created. For example, give the DefaultAppPool user or IIS_IUSRS group
write permission.

Also, ensure loadUserProfile and setProfileEnvironment are set to true for the relevant application pool
in the applicationHost.config file under C:\Windows\System32\inetsrv\config.

Refer to the following article for more information.

https://blogs.msdn.microsoft.com/sqlexpress/2011/12/08/using-localdb-with-full-iis-part-1-user-profile/